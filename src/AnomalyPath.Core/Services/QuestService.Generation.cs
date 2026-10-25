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
    /// <summary>Часть класса <see cref="QuestService"/>: генерация предложений.</summary>
    public sealed partial class QuestService
    {
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
    }
}
