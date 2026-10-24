#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Quests;
using AnomalyPath.Core.Rules;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.Templates;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Services
{
    /// <summary>
    /// Повторяемые квесты: случайные предложения у торговца, принятие и отмена,
    /// прогресс по событиям и сдача с наградой.
    /// </summary>
    public sealed class QuestService : IQuestService
    {
        private readonly IQuestRepository _quests;
        private readonly ITradeService _trade;
        private readonly IActorRepository _actors;
        private readonly ILocationRepository _locations;
        private readonly ITopologyRepository _topology;
        private readonly IItemRepository _items;
        private readonly IInventoryService _inventory;
        private readonly IGameRulesRepository _rules;

        /// <summary>Создаёт сервис квестов.</summary>
        /// <param name="quests">Шаблоны квестов.</param>
        /// <param name="trade">Доступность торговцев.</param>
        /// <param name="actors">Актёры: пулы мутантов и NPC.</param>
        /// <param name="locations">Шаблоны локаций: пул целей и маршрут.</param>
        /// <param name="topology">Топология: способы перехода.</param>
        /// <param name="items">Предметы: пулы артефактов, оружия и брони.</param>
        /// <param name="inventory">Инвентарь: проверка и выдача предметов.</param>
        /// <param name="rules">Правила: число предложений, откат и предел активных.</param>
        /// <exception cref="ArgumentNullException">Зависимость не задана.</exception>
        public QuestService(
            IQuestRepository quests,
            ITradeService trade,
            IActorRepository actors,
            ILocationRepository locations,
            ITopologyRepository topology,
            IItemRepository items,
            IInventoryService inventory,
            IGameRulesRepository rules)
        {
            _quests = quests ?? throw new ArgumentNullException(nameof(quests));
            _trade = trade ?? throw new ArgumentNullException(nameof(trade));
            _actors = actors ?? throw new ArgumentNullException(nameof(actors));
            _locations = locations ?? throw new ArgumentNullException(nameof(locations));
            _topology = topology ?? throw new ArgumentNullException(nameof(topology));
            _items = items ?? throw new ArgumentNullException(nameof(items));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        /// <inheritdoc />
        public IReadOnlyList<QuestOffer> GetOffers(GameSession session, string traderActorId)
        {
            var result = new List<QuestOffer>();
            if (CheckTrader(session, traderActorId, out _) != QuestResultStatus.Ok)
            {
                return result;
            }

            var player = session.State.Player!;
            var rules = _rules.Require();
            var now = session.State.World.Clock.AbsoluteHours;
            var slots = Math.Max(1, rules.QuestOffersPerTrader);
            var trader = session.State.GetOrCreateTrader(traderActorId);
            trader.EnsureQuestSlots(slots);
            var bySlot = new QuestState?[slots];
            foreach (var quest in session.State.Quests)
            {
                if (quest.Status == QuestStatus.Offered &&
                    string.Equals(quest.GiverActorId, traderActorId, StringComparison.Ordinal) &&
                    quest.Slot >= 0 &&
                    quest.Slot < slots)
                {
                    bySlot[quest.Slot] = quest;
                }
            }

            for (var slot = 0; slot < slots; slot++)
            {
                if (bySlot[slot] == null && now >= trader.GetQuestSlotAvailableAt(slot))
                {
                    var generated = GenerateOffer(session, traderActorId, slot, player, now);
                    if (generated != null)
                    {
                        session.State.AddQuest(generated);
                        bySlot[slot] = generated;
                    }
                }

                var offer = bySlot[slot];
                if (offer != null && _quests.TryGet(offer.QuestId, out var template))
                {
                    result.Add(new QuestOffer(template, offer));
                }
            }

            return result;
        }

        /// <inheritdoc />
        public QuestResult Accept(GameSession session, string traderActorId, string questId)
        {
            var status = CheckTrader(session, traderActorId, out _);
            if (status != QuestResultStatus.Ok)
            {
                return QuestResult.Error(status, questId);
            }

            if (string.IsNullOrWhiteSpace(questId) || !_quests.TryGet(questId, out var template))
            {
                return QuestResult.Error(QuestResultStatus.QuestNotFound, questId);
            }

            var state = session.State;
            var offered = FindOffered(session, traderActorId, questId);
            if (offered == null)
            {
                return QuestResult.Error(QuestResultStatus.NotOffered, questId);
            }

            if (template.QuestKind != QuestKind.Repeatable)
            {
                return QuestResult.Error(QuestResultStatus.NotRepeatable, questId);
            }

            var player = state.Player!;
            if (!template.Requirements.IsMet(player))
            {
                return QuestResult.Error(QuestResultStatus.RequirementsNotMet, questId);
            }

            var now = state.World.Clock.AbsoluteHours;
            if (IsActive(session, questId))
            {
                return QuestResult.Error(QuestResultStatus.AlreadyActive, questId);
            }

            if (CountActive(session) >= _rules.Require().MaxActiveQuests)
            {
                return QuestResult.Error(QuestResultStatus.ActiveLimit, questId);
            }

            if (template.Type == QuestType.ClearLocation)
            {
                if (offered.LocationTemplateId == null ||
                    !_locations.Contains(offered.LocationTemplateId))
                {
                    return QuestResult.Error(QuestResultStatus.NoTargets, questId);
                }

                var serial = state.NextQuestSerial();
                var random = new SeededWorldRandom(StableHash.Combine(
                    state.World.Map.Seed,
                    questId + "#" + serial));
                if (!EnsureQuestLocation(
                        session,
                        _locations.Get(offered.LocationTemplateId),
                        offered,
                        random,
                        serial))
                {
                    return QuestResult.Error(QuestResultStatus.NoTargets, questId);
                }
            }

            offered.Activate(now);
            var trader = state.GetOrCreateTrader(traderActorId);
            trader.SetQuestSlotAvailableAt(
                offered.Slot,
                now + Math.Max(0, _rules.Require().QuestOfferCooldownHours));
            return QuestResult.Ok(questId);
        }

        /// <inheritdoc />
        public QuestResult CancelOffer(GameSession session, string traderActorId, string questId)
        {
            var status = CheckTrader(session, traderActorId, out _);
            if (status != QuestResultStatus.Ok)
            {
                return QuestResult.Error(status, questId);
            }

            var offered = FindOffered(session, traderActorId, questId);
            if (offered == null)
            {
                return QuestResult.Error(QuestResultStatus.NotOffered, questId);
            }

            var now = session.State.World.Clock.AbsoluteHours;
            offered.MarkCancelled(now);
            var trader = session.State.GetOrCreateTrader(traderActorId);
            trader.SetQuestSlotAvailableAt(
                offered.Slot,
                now + Math.Max(0, _rules.Require().QuestOfferCooldownHours));
            return QuestResult.Ok(questId);
        }

        /// <inheritdoc />
        public QuestResult CancelQuest(GameSession session, string questId)
        {
            if (session == null || string.IsNullOrWhiteSpace(questId))
            {
                return QuestResult.Error(QuestResultStatus.QuestNotFound, questId);
            }

            if (!session.State.TryGetActiveQuest(questId, out var quest) || quest == null)
            {
                return QuestResult.Error(QuestResultStatus.QuestNotFound, questId);
            }

            var now = session.State.World.Clock.AbsoluteHours;
            quest.MarkCancelled(now);
            var trader = session.State.GetOrCreateTrader(quest.GiverActorId);
            trader.SetQuestSlotAvailableAt(
                quest.Slot,
                now + Math.Max(0, _rules.Require().QuestOfferCooldownHours));
            return QuestResult.Ok(questId);
        }

        /// <inheritdoc />
        public QuestResult TurnIn(GameSession session, string traderActorId, string questId)
        {
            var status = CheckTrader(session, traderActorId, out _);
            if (status != QuestResultStatus.Ok)
            {
                return QuestResult.Error(status, questId);
            }

            if (!session.State.TryGetActiveQuest(questId, out var quest) || quest == null)
            {
                return QuestResult.Error(QuestResultStatus.QuestNotFound, questId);
            }

            if (!string.Equals(quest.GiverActorId, traderActorId, StringComparison.Ordinal))
            {
                return QuestResult.Error(QuestResultStatus.WrongGiver, questId);
            }

            if (!_quests.TryGet(questId, out var template))
            {
                return QuestResult.Error(QuestResultStatus.QuestNotFound, questId);
            }

            var progress = GetProgress(session, quest);
            if (!progress.Ready)
            {
                return QuestResult.Error(QuestResultStatus.NotReady, questId);
            }

            var player = session.State.Player!;
            if (!CanFitRewards(player, template.Rewards))
            {
                return QuestResult.Error(QuestResultStatus.NoRoom, questId);
            }

            if (!RemoveRequiredItems(player, template, quest))
            {
                return QuestResult.Error(QuestResultStatus.NotReady, questId);
            }

            if (template.Rewards.Money > 0)
            {
                player.AddMoney(template.Rewards.Money);
            }

            foreach (var reward in template.Rewards.Items)
            {
                int? durability = null;
                if (_items.TryGet(reward.ItemId, out var rewardTemplate))
                {
                    durability = NewDurability(rewardTemplate);
                }

                _inventory.TryAdd(
                    player.Inventory,
                    reward.ItemId,
                    reward.Count,
                    durability,
                    null,
                    out _);
            }

            foreach (var unlock in template.Rewards.Unlocks)
            {
                player.AddUnlock(unlock);
            }

            var cooldownHours = template.CooldownHours > 0
                ? template.CooldownHours
                : _rules.Require().QuestCooldownHours;
            quest.MarkCompleted(session.State.World.Clock.AbsoluteHours, cooldownHours);
            player.CompleteQuest(questId);
            if (template.Type == QuestType.ClearLocation && quest.LocationInstanceId != null &&
                session.State.World.Map.TryGetLocation(quest.LocationInstanceId, out var location) &&
                location != null)
            {
                location.MarkCleared();
            }

            return QuestResult.Ok(questId, template.Rewards.Money, template.Rewards.Items);
        }

        /// <inheritdoc />
        public bool TryGetTemplate(string questId, out QuestTemplate template) =>
            _quests.TryGet(questId, out template);

        /// <inheritdoc />
        public IReadOnlyList<QuestState> GetActive(GameSession session)
        {
            var result = new List<QuestState>();
            if (session == null)
            {
                return result;
            }

            foreach (var quest in session.State.Quests)
            {
                if (quest.Status == QuestStatus.Active)
                {
                    result.Add(quest);
                }
            }

            return result;
        }

        /// <inheritdoc />
        public QuestProgress GetProgress(GameSession session, QuestState quest)
        {
            if (session == null || quest == null)
            {
                return new QuestProgress(QuestType.ClearLocation, 0, 1, false);
            }

            if (!_quests.TryGet(quest.QuestId, out var template))
            {
                return new QuestProgress(QuestType.ClearLocation, 0, 1, false);
            }

            var player = session.State.Player;
            if (player == null)
            {
                return new QuestProgress(template.Type, 0, quest.RequiredCount, false);
            }

            switch (template.Type)
            {
                case QuestType.ClearLocation:
                {
                    var cleared = quest.Cleared;
                    if (!cleared && quest.LocationInstanceId != null &&
                        session.State.World.Map.TryGetLocation(quest.LocationInstanceId, out var location) &&
                        location != null)
                    {
                        cleared = location.Cleared;
                    }

                    return new QuestProgress(template.Type, cleared ? 1 : 0, 1, cleared);
                }

                case QuestType.KillMutants:
                    return new QuestProgress(
                        template.Type,
                        quest.KillCount,
                        quest.RequiredCount,
                        quest.KillCount >= quest.RequiredCount);

                case QuestType.GatherArtifacts:
                {
                    var items = new List<QuestItemProgress>();
                    var total = 0;
                    foreach (var target in quest.Items)
                    {
                        var current = CountInventory(player, target.ItemId);
                        total += current;
                        items.Add(new QuestItemProgress(target.ItemId, current, target.Count));
                    }

                    return new QuestProgress(
                        template.Type,
                        total,
                        quest.RequiredCount,
                        total >= quest.RequiredCount,
                        items);
                }

                case QuestType.FindItems:
                {
                    var items = new List<QuestItemProgress>();
                    var itemsReady = true;
                    foreach (var target in quest.Items)
                    {
                        var current = CountInventory(player, target.ItemId);
                        var met = current >= target.Count;
                        itemsReady &= met;
                        items.Add(new QuestItemProgress(target.ItemId, current, target.Count));
                    }

                    var npcReady = quest.KillCount >= quest.RequiredCount;
                    return new QuestProgress(
                        template.Type,
                        quest.KillCount,
                        quest.RequiredCount,
                        itemsReady && npcReady,
                        items,
                        quest.KillCount,
                        quest.RequiredCount);
                }

                default:
                    return new QuestProgress(template.Type, 0, quest.RequiredCount, false);
            }
        }

        /// <inheritdoc />
        public void NotifyActorKilled(GameSession session, string actorId)
        {
            if (session == null || string.IsNullOrWhiteSpace(actorId))
            {
                return;
            }

            foreach (var quest in session.State.Quests)
            {
                if (quest.Status != QuestStatus.Active || !Contains(quest.ActorIds, actorId))
                {
                    continue;
                }

                if (!_quests.TryGet(quest.QuestId, out var template))
                {
                    continue;
                }

                if ((template.Type == QuestType.KillMutants || template.Type == QuestType.FindItems) &&
                    quest.KillCount < quest.RequiredCount)
                {
                    quest.AddKill();
                }
            }
        }

        /// <inheritdoc />
        public void NotifyLocationCleared(GameSession session, string locationInstanceId)
        {
            if (session == null || string.IsNullOrWhiteSpace(locationInstanceId))
            {
                return;
            }

            if (session.State.World.Map.TryGetLocation(locationInstanceId, out var location) && location != null)
            {
                location.MarkCleared();
            }

            foreach (var quest in session.State.Quests)
            {
                if (quest.Status != QuestStatus.Active || quest.Cleared)
                {
                    continue;
                }

                if (!_quests.TryGet(quest.QuestId, out var template) ||
                    template.Type != QuestType.ClearLocation)
                {
                    continue;
                }

                if (string.Equals(quest.LocationInstanceId, locationInstanceId, StringComparison.Ordinal))
                {
                    quest.MarkCleared();
                }
            }
        }

        private QuestResultStatus CheckTrader(
            GameSession session,
            string traderActorId,
            out ActorTemplate? actor)
        {
            actor = null;
            if (session == null || session.State.Player == null)
            {
                return QuestResultStatus.Unavailable;
            }

            if (string.IsNullOrWhiteSpace(traderActorId) || !_actors.Contains(traderActorId))
            {
                return QuestResultStatus.TraderNotFound;
            }

            actor = _actors.Get(traderActorId);
            if (actor.Type != ActorType.Trader || actor.Trader == null)
            {
                return QuestResultStatus.NotTrader;
            }

            if (_trade.IsAvailable(session, traderActorId))
            {
                return QuestResultStatus.Ok;
            }

            var location = _locations.Get(session.State.World.CurrentLocation.TemplateId);
            if (location.LocationKind != LocationKind.Hub || !Contains(location.Traders, traderActorId))
            {
                return QuestResultStatus.WrongLocation;
            }

            return QuestResultStatus.Unavailable;
        }

        private static QuestState? FindOffered(GameSession session, string traderActorId, string questId)
        {
            foreach (var quest in session.State.Quests)
            {
                if (quest.Status == QuestStatus.Offered &&
                    string.Equals(quest.GiverActorId, traderActorId, StringComparison.Ordinal) &&
                    string.Equals(quest.QuestId, questId, StringComparison.Ordinal))
                {
                    return quest;
                }
            }

            return null;
        }

        private QuestState? GenerateOffer(
            GameSession session,
            string traderActorId,
            int slot,
            PlayerState player,
            double now)
        {
            var candidates = new List<QuestTemplate>();
            foreach (var template in _quests.All)
            {
                if (template.QuestKind != QuestKind.Repeatable ||
                    !template.Requirements.IsMet(player) ||
                    IsActive(session, template.Id) ||
                    IsOnCooldown(session, template.Id, now))
                {
                    continue;
                }

                candidates.Add(template);
            }

            var serial = session.State.NextQuestSerial();
            var random = new SeededWorldRandom(StableHash.Combine(
                session.State.World.Map.Seed,
                traderActorId + "#" + slot + "#" + serial));
            while (candidates.Count > 0)
            {
                var index = random.NextInt(0, candidates.Count);
                var template = candidates[index];
                candidates.RemoveAt(index);
                var quest = new QuestState(
                    template.Id,
                    traderActorId,
                    QuestStatus.Offered,
                    0,
                    0,
                    0,
                    requiredCount: RequiredCount(template),
                    slot: slot);
                if (BuildOfferTarget(template, quest, random))
                {
                    return quest;
                }
            }

            return null;
        }

        private bool BuildOfferTarget(
            QuestTemplate template,
            QuestState quest,
            IWorldRandom random)
        {
            switch (template.Type)
            {
                case QuestType.ClearLocation:
                {
                    var target = template.ClearLocation;
                    if (target == null)
                    {
                        return false;
                    }

                    var chosen = SelectLocation(target, random);
                    if (chosen == null)
                    {
                        return false;
                    }

                    quest.LocationTemplateId = chosen.Id;
                    return true;
                }

                case QuestType.KillMutants:
                {
                    var target = template.KillMutants;
                    if (target == null)
                    {
                        return false;
                    }

                    var candidates = ActorCandidates(target.ActorPool, ActorRoles.Mutant, target.MinTier, target.MaxTier);
                    if (candidates.Count == 0)
                    {
                        return false;
                    }

                    var picked = PickDistinct(candidates, target.PoolSize, random);
                    foreach (var actorId in picked)
                    {
                        quest.AddActor(actorId);
                    }

                    return picked.Count > 0;
                }

                case QuestType.GatherArtifacts:
                {
                    var target = template.GatherArtifacts;
                    if (target == null)
                    {
                        return false;
                    }

                    var candidates = ItemCandidates(target.ItemPool, ItemCategory.Artifact, target.MinTier, target.MaxTier);
                    if (candidates.Count == 0)
                    {
                        return false;
                    }

                    var picked = PickDistinct(candidates, target.PoolSize, random);
                    foreach (var itemId in picked)
                    {
                        quest.AddItem(new QuestItemTarget(itemId, 1));
                    }

                    return picked.Count > 0;
                }

                case QuestType.FindItems:
                {
                    var target = template.FindItems;
                    if (target == null)
                    {
                        return false;
                    }

                    var itemCandidates = FindItemCandidates(target);
                    if (itemCandidates.Count == 0)
                    {
                        return false;
                    }

                    var pickedItems = PickDistinct(itemCandidates, target.ItemKinds, random);
                    foreach (var itemId in pickedItems)
                    {
                        quest.AddItem(new QuestItemTarget(itemId, target.ItemCount));
                    }

                    var npcCandidates = ActorCandidates(target.NpcPool, ActorRoles.Human, target.MinTier, target.MaxTier);
                    if (npcCandidates.Count == 0)
                    {
                        return false;
                    }

                    var pickedNpcs = PickDistinct(npcCandidates, target.PoolSize, random);
                    foreach (var actorId in pickedNpcs)
                    {
                        quest.AddActor(actorId);
                    }

                    return pickedItems.Count > 0 && pickedNpcs.Count > 0;
                }

                default:
                    return false;
            }
        }

        private LocationTemplate? SelectLocation(ClearLocationTarget target, IWorldRandom random)
        {
            var candidates = new List<LocationTemplate>();
            foreach (var template in _locations.All)
            {
                if (template.NodeType != LocationNodeType.Site || template.LocationKind != LocationKind.Wild ||
                    template.Unique)
                {
                    continue;
                }

                if (template.Tier < target.MinTier || template.Tier > target.MaxTier)
                {
                    continue;
                }

                if (target.LocationPool.Count > 0 && !Contains(target.LocationPool, template.Id))
                {
                    continue;
                }

                if (template.SpawnChance <= 0)
                {
                    continue;
                }

                candidates.Add(template);
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            var total = 0.0;
            foreach (var candidate in candidates)
            {
                total += candidate.SpawnChance;
            }

            if (total <= 0)
            {
                return candidates[random.NextInt(0, candidates.Count)];
            }

            var roll = random.NextDouble() * total;
            foreach (var candidate in candidates)
            {
                roll -= candidate.SpawnChance;
                if (roll <= 0)
                {
                    return candidate;
                }
            }

            return candidates[candidates.Count - 1];
        }

        private List<string> ActorCandidates(
            IReadOnlyList<string> pool,
            string role,
            int minTier,
            int maxTier)
        {
            var result = new List<string>();
            if (pool.Count > 0)
            {
                foreach (var actorId in pool)
                {
                    if (!_actors.Contains(actorId))
                    {
                        continue;
                    }

                    var actor = _actors.Get(actorId);
                    if (actor.Type != ActorType.Enemy || !actor.HasRole(role) || actor.Combat == null)
                    {
                        continue;
                    }

                    if (actor.Combat.Tier < minTier || actor.Combat.Tier > maxTier)
                    {
                        continue;
                    }

                    if (!Contains(result, actorId))
                    {
                        result.Add(actorId);
                    }
                }

                return result;
            }

            foreach (var actor in _actors.All)
            {
                if (actor.Type != ActorType.Enemy || !actor.HasRole(role) || actor.Combat == null)
                {
                    continue;
                }

                if (actor.Combat.Tier < minTier || actor.Combat.Tier > maxTier)
                {
                    continue;
                }

                result.Add(actor.Id);
            }

            return result;
        }

        private List<string> ItemCandidates(
            IReadOnlyList<string> pool,
            ItemCategory category,
            int minTier,
            int maxTier)
        {
            var result = new List<string>();
            if (pool.Count > 0)
            {
                foreach (var itemId in pool)
                {
                    if (!_items.TryGet(itemId, out var template) || template.Category != category)
                    {
                        continue;
                    }

                    if (template.Tier < minTier || template.Tier > maxTier)
                    {
                        continue;
                    }

                    if (!Contains(result, itemId))
                    {
                        result.Add(itemId);
                    }
                }

                return result;
            }

            foreach (var item in _items.All)
            {
                if (item.Category != category || item.Tier < minTier || item.Tier > maxTier)
                {
                    continue;
                }

                result.Add(item.Id);
            }

            return result;
        }

        private List<string> FindItemCandidates(FindItemsTarget target)
        {
            var result = new List<string>();
            if (target.ItemPool.Count > 0)
            {
                foreach (var itemId in target.ItemPool)
                {
                    if (!_items.TryGet(itemId, out var template))
                    {
                        continue;
                    }

                    if (!ContainsCategory(target.Categories, template.Category))
                    {
                        continue;
                    }

                    if (template.Tier < target.MinTier || template.Tier > target.MaxTier)
                    {
                        continue;
                    }

                    if (!Contains(result, itemId))
                    {
                        result.Add(itemId);
                    }
                }

                return result;
            }

            foreach (var item in _items.All)
            {
                if (!ContainsCategory(target.Categories, item.Category) ||
                    item.Tier < target.MinTier || item.Tier > target.MaxTier)
                {
                    continue;
                }

                result.Add(item.Id);
            }

            return result;
        }

        private bool EnsureQuestLocation(
            GameSession session,
            LocationTemplate target,
            QuestState quest,
            IWorldRandom random,
            int serial)
        {
            var map = session.State.World.Map;
            foreach (var existing in map.Locations)
            {
                if (!existing.Cleared &&
                    string.Equals(existing.TemplateId, target.Id, StringComparison.Ordinal))
                {
                    quest.LocationInstanceId = existing.InstanceId;
                    return true;
                }
            }

            var anchorId = session.State.World.CurrentLocationId;
            if (session.State.World.CurrentLocation.NodeType != LocationNodeType.Hub)
            {
                anchorId = map.StartLocationId;
            }

            var anchor = _locations.Get(map.GetLocation(anchorId).TemplateId);
            if (anchor.NodeType != LocationNodeType.Hub)
            {
                return false;
            }

            var route = SelectRoute(anchor, target, random);
            if (route == null)
            {
                return false;
            }

            var routeVias = RouteRules.Vias(anchor, route, _topology.Topology);
            var siteVias = RouteRules.Vias(route, target, _topology.Topology);
            if (routeVias.Count == 0 || siteVias.Count == 0)
            {
                return false;
            }

            var routeInstanceId = "locinst_q" + serial.ToString("D4") + "_r";
            var siteInstanceId = "locinst_q" + serial.ToString("D4") + "_s";
            if (map.ContainsLocation(routeInstanceId) || map.ContainsLocation(siteInstanceId))
            {
                return false;
            }

            var routeInstance = NewInstance(route, routeInstanceId, random);
            var siteInstance = NewInstance(target, siteInstanceId, random);
            map.AddLocation(routeInstance);
            map.AddLocation(siteInstance);

            var viaToRoute = routeVias[random.NextInt(0, routeVias.Count)];
            var viaToSite = siteVias[random.NextInt(0, siteVias.Count)];
            AddEdge(
                map,
                "edge_q" + serial.ToString("D4") + "_a",
                anchorId,
                routeInstanceId,
                viaToRoute);
            AddEdge(
                map,
                "edge_q" + serial.ToString("D4") + "_b",
                routeInstanceId,
                siteInstanceId,
                viaToSite);
            quest.LocationInstanceId = siteInstanceId;
            return true;
        }

        private LocationTemplate? SelectRoute(
            LocationTemplate anchor,
            LocationTemplate target,
            IWorldRandom random)
        {
            var candidates = new List<LocationTemplate>();
            foreach (var template in _locations.All)
            {
                if (template.NodeType != LocationNodeType.Route)
                {
                    continue;
                }

                if (RouteRules.Vias(anchor, template, _topology.Topology).Count == 0 ||
                    RouteRules.Vias(template, target, _topology.Topology).Count == 0)
                {
                    continue;
                }

                candidates.Add(template);
            }

            return candidates.Count == 0
                ? null
                : candidates[random.NextInt(0, candidates.Count)];
        }

        private static LocationInstance NewInstance(LocationTemplate template, string instanceId, IWorldRandom random)
        {
            var seed = random.NextInt(1, int.MaxValue);
            var background = template.Backgrounds[random.NextInt(0, template.Backgrounds.Count)];
            return new LocationInstance(
                instanceId,
                template.Id,
                seed,
                background,
                template.NodeType,
                template.Tier,
                template.Danger);
        }

        private void AddEdge(
            WorldMap map,
            string edgeId,
            string fromId,
            string toId,
            string via)
        {
            var cost = _topology.Topology.GetCost(via).Scale(1);
            map.AddEdge(new WorldEdge(edgeId, fromId, toId, via, 1, cost, oneWay: false, hidden: false));
            map.AddEdge(new WorldEdge(edgeId + "_back", toId, fromId, via, 1, cost, oneWay: false, hidden: false));
        }

        private bool CanFitRewards(PlayerState player, QuestRewardTemplate rewards)
        {
            if (rewards.Items.Count == 0)
            {
                return true;
            }

            var probes = new List<ItemInstance>();
            foreach (var item in rewards.Items)
            {
                if (!_items.TryGet(item.ItemId, out var template))
                {
                    return false;
                }

                probes.Add(new ItemInstance(
                    "quest-probe",
                    item.ItemId,
                    item.Count,
                    NewDurability(template),
                    null));
            }

            return _inventory.CanFit(player.Inventory, probes);
        }

        private bool RemoveRequiredItems(PlayerState player, QuestTemplate template, QuestState quest)
        {
            switch (template.Type)
            {
                case QuestType.GatherArtifacts:
                {
                    var remaining = quest.RequiredCount;
                    foreach (var target in quest.Items)
                    {
                        remaining -= RemoveInventory(player, target.ItemId, remaining);
                        if (remaining <= 0)
                        {
                            return true;
                        }
                    }

                    return remaining <= 0;
                }

                case QuestType.FindItems:
                {
                    foreach (var target in quest.Items)
                    {
                        if (RemoveInventory(player, target.ItemId, target.Count) < target.Count)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                default:
                    return true;
            }
        }

        private static int RemoveInventory(PlayerState player, string itemId, int count)
        {
            var remaining = count;
            var items = player.Inventory.Items;
            for (var index = items.Count - 1; index >= 0 && remaining > 0; index--)
            {
                var item = items[index];
                if (!string.Equals(item.ItemId, itemId, StringComparison.Ordinal))
                {
                    continue;
                }

                var take = Math.Min(item.Stack, remaining);
                item.Stack -= take;
                remaining -= take;
                if (item.Stack <= 0)
                {
                    player.Inventory.Remove(item);
                }
            }

            return count - remaining;
        }

        private static int CountInventory(PlayerState player, string itemId)
        {
            var total = 0;
            foreach (var item in player.Inventory.Items)
            {
                if (string.Equals(item.ItemId, itemId, StringComparison.Ordinal))
                {
                    total += item.Stack;
                }
            }

            return total;
        }

        private static int? NewDurability(ItemTemplate template)
        {
            if (template.Weapon != null)
            {
                return template.Weapon.DurabilityMax;
            }

            return template.ArmorDurabilityMax > 0 ? template.ArmorDurabilityMax : null;
        }

        private static int RequiredCount(QuestTemplate template)
        {
            switch (template.Type)
            {
                case QuestType.ClearLocation:
                    return 1;
                case QuestType.KillMutants:
                    return template.KillMutants?.Count ?? 1;
                case QuestType.GatherArtifacts:
                    return template.GatherArtifacts?.Count ?? 1;
                case QuestType.FindItems:
                    return template.FindItems?.NpcCount ?? 1;
                default:
                    return 1;
            }
        }

        private static bool IsActive(GameSession session, string questId) =>
            session.State.TryGetActiveQuest(questId, out _);

        private bool IsOnCooldown(GameSession session, string questId, double now)
        {
            foreach (var quest in session.State.Quests)
            {
                if (string.Equals(quest.QuestId, questId, StringComparison.Ordinal) &&
                    quest.Status == QuestStatus.Completed &&
                    now < quest.CooldownUntil)
                {
                    return true;
                }
            }

            return false;
        }

        private static int CountActive(GameSession session)
        {
            var count = 0;
            foreach (var quest in session.State.Quests)
            {
                if (quest.Status == QuestStatus.Active)
                {
                    count++;
                }
            }

            return count;
        }

        private static List<string> PickDistinct(
            IReadOnlyList<string> candidates,
            int count,
            IWorldRandom random)
        {
            var remaining = new List<string>(candidates);
            var result = new List<string>();
            var take = Math.Min(Math.Max(1, count), remaining.Count);
            for (var index = 0; index < take; index++)
            {
                var pick = random.NextInt(0, remaining.Count);
                result.Add(remaining[pick]);
                remaining.RemoveAt(pick);
            }

            return result;
        }

        private static bool Contains(IReadOnlyList<string> values, string value)
        {
            foreach (var item in values)
            {
                if (string.Equals(item, value, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsCategory(IReadOnlyList<ItemCategory> values, ItemCategory value)
        {
            foreach (var item in values)
            {
                if (item == value)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
