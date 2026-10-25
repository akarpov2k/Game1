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
    /// <summary>Часть класса <see cref="QuestService"/>: предложения и сдача.</summary>
    public sealed partial class QuestService
    {
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
    }
}
