#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Loadouts;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Loot;
using AnomalyPath.Core.Nicks;
using AnomalyPath.Core.Quests;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Rules;
using AnomalyPath.Core.Talents;
using AnomalyPath.Core.Templates;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Validation
{
    /// <summary>Часть класса <see cref="DataValidator"/>: цели квестов.</summary>
    public sealed partial class DataValidator
    {
        private static void ValidateClearQuest(
            QuestTemplate quest,
            LocationRepository locations,
            List<ValidationIssue> issues)
        {
            var target = quest.ClearLocation;
            if (target == null)
            {
                issues.Add(new ValidationIssue(
                    quest.Id,
                    CoreErrors.QuestTargetPool,
                    CoreText.Format(CoreErrors.QuestTargetPool, quest.Id)));
                return;
            }

            var candidates = 0;
            foreach (var location in locations.All)
            {
                var inPool = target.LocationPool.Count == 0 || Contains(target.LocationPool, location.Id);
                if (inPool && !locations.Contains(location.Id))
                {
                    continue;
                }

                if (!inPool)
                {
                    continue;
                }

                if (location.LocationKind != LocationKind.Wild ||
                    location.Subtype == LocationSubtype.Path ||
                    location.Subtype == LocationSubtype.Hub ||
                    location.Unique)
                {
                    issues.Add(new ValidationIssue(
                        quest.Id,
                        CoreErrors.QuestLocationNotSite,
                        CoreText.Format(CoreErrors.QuestLocationNotSite, location.Id)));
                    continue;
                }

                if (location.Tier < target.MinTier || location.Tier > target.MaxTier)
                {
                    continue;
                }

                candidates++;
            }

            foreach (var locationId in target.LocationPool)
            {
                if (!locations.Contains(locationId))
                {
                    issues.Add(new ValidationIssue(
                        quest.Id,
                        CoreErrors.WorldInstanceNotFound,
                        CoreText.Format(CoreErrors.WorldInstanceNotFound, locationId)));
                }
            }

            if (candidates == 0)
            {
                issues.Add(new ValidationIssue(
                    quest.Id,
                    CoreErrors.QuestTargetPool,
                    CoreText.Format(CoreErrors.QuestTargetPool, quest.Id)));
            }
        }

        private static void ValidateKillQuest(
            QuestTemplate quest,
            ActorRepository actors,
            List<ValidationIssue> issues)
        {
            var target = quest.KillMutants;
            if (target == null)
            {
                issues.Add(new ValidationIssue(
                    quest.Id,
                    CoreErrors.QuestTargetPool,
                    CoreText.Format(CoreErrors.QuestTargetPool, quest.Id)));
                return;
            }

            foreach (var actorId in target.ActorPool)
            {
                if (!actors.Contains(actorId))
                {
                    issues.Add(new ValidationIssue(
                        quest.Id,
                        CoreErrors.ActorNotFound,
                        CoreText.Format(CoreErrors.ActorNotFound, actorId)));
                    continue;
                }

                var actor = actors.Get(actorId);
                if (actor.Type != ActorType.Enemy || !actor.HasRole(ActorRoles.Mutant) || actor.Combat == null)
                {
                    issues.Add(new ValidationIssue(
                        quest.Id,
                        CoreErrors.QuestActorRole,
                        CoreText.Format(CoreErrors.QuestActorRole, ActorRoles.Mutant, actorId)));
                }
            }

            var candidates = 0;
            foreach (var actor in actors.All)
            {
                if (actor.Type != ActorType.Enemy || !actor.HasRole(ActorRoles.Mutant) || actor.Combat == null)
                {
                    continue;
                }

                if (target.ActorPool.Count > 0 && !Contains(target.ActorPool, actor.Id))
                {
                    continue;
                }

                if (actor.Combat.Tier < target.MinTier || actor.Combat.Tier > target.MaxTier)
                {
                    continue;
                }

                candidates++;
            }

            if (candidates == 0)
            {
                issues.Add(new ValidationIssue(
                    quest.Id,
                    CoreErrors.QuestTargetPool,
                    CoreText.Format(CoreErrors.QuestTargetPool, quest.Id)));
            }
        }

        private static void ValidateGatherQuest(
            QuestTemplate quest,
            ItemRepository items,
            List<ValidationIssue> issues)
        {
            var target = quest.GatherArtifacts;
            if (target == null)
            {
                issues.Add(new ValidationIssue(
                    quest.Id,
                    CoreErrors.QuestTargetPool,
                    CoreText.Format(CoreErrors.QuestTargetPool, quest.Id)));
                return;
            }

            foreach (var itemId in target.ItemPool)
            {
                if (!items.TryGet(itemId, out var item))
                {
                    issues.Add(new ValidationIssue(
                        quest.Id,
                        CoreErrors.ItemNotFound,
                        CoreText.Format(CoreErrors.ItemNotFound, itemId)));
                    continue;
                }

                if (item.Category != ItemCategory.Artifact)
                {
                    issues.Add(new ValidationIssue(
                        quest.Id,
                        CoreErrors.QuestItemCategory,
                        CoreText.Format(CoreErrors.QuestItemCategory, "artifact", itemId)));
                }
            }

            var candidates = 0;
            foreach (var item in items.All)
            {
                if (item.Category != ItemCategory.Artifact)
                {
                    continue;
                }

                if (target.ItemPool.Count > 0 && !Contains(target.ItemPool, item.Id))
                {
                    continue;
                }

                if (item.Tier < target.MinTier || item.Tier > target.MaxTier)
                {
                    continue;
                }

                candidates++;
            }

            if (candidates == 0)
            {
                issues.Add(new ValidationIssue(
                    quest.Id,
                    CoreErrors.QuestTargetPool,
                    CoreText.Format(CoreErrors.QuestTargetPool, quest.Id)));
            }
        }

        private static void ValidateFindQuest(
            QuestTemplate quest,
            ActorRepository actors,
            ItemRepository items,
            List<ValidationIssue> issues)
        {
            var target = quest.FindItems;
            if (target == null)
            {
                issues.Add(new ValidationIssue(
                    quest.Id,
                    CoreErrors.QuestTargetPool,
                    CoreText.Format(CoreErrors.QuestTargetPool, quest.Id)));
                return;
            }

            foreach (var itemId in target.ItemPool)
            {
                if (!items.TryGet(itemId, out var item))
                {
                    issues.Add(new ValidationIssue(
                        quest.Id,
                        CoreErrors.ItemNotFound,
                        CoreText.Format(CoreErrors.ItemNotFound, itemId)));
                    continue;
                }

                if (!Contains(target.Categories, item.Category))
                {
                    issues.Add(new ValidationIssue(
                        quest.Id,
                        CoreErrors.QuestItemCategory,
                        CoreText.Format(CoreErrors.QuestItemCategory, item.Category.ToString(), itemId)));
                }
            }

            foreach (var actorId in target.NpcPool)
            {
                if (!actors.Contains(actorId))
                {
                    issues.Add(new ValidationIssue(
                        quest.Id,
                        CoreErrors.ActorNotFound,
                        CoreText.Format(CoreErrors.ActorNotFound, actorId)));
                    continue;
                }

                var actor = actors.Get(actorId);
                if (actor.Type != ActorType.Enemy || !actor.HasRole(ActorRoles.Human) || actor.Combat == null)
                {
                    issues.Add(new ValidationIssue(
                        quest.Id,
                        CoreErrors.QuestActorRole,
                        CoreText.Format(CoreErrors.QuestActorRole, ActorRoles.Human, actorId)));
                }
            }

            var itemCandidates = 0;
            foreach (var item in items.All)
            {
                if (!Contains(target.Categories, item.Category) ||
                    item.Tier < target.MinTier || item.Tier > target.MaxTier)
                {
                    continue;
                }

                if (target.ItemPool.Count > 0 && !Contains(target.ItemPool, item.Id))
                {
                    continue;
                }

                itemCandidates++;
            }

            var npcCandidates = 0;
            foreach (var actor in actors.All)
            {
                if (actor.Type != ActorType.Enemy || !actor.HasRole(ActorRoles.Human) || actor.Combat == null)
                {
                    continue;
                }

                if (actor.Combat.Tier < target.MinTier || actor.Combat.Tier > target.MaxTier)
                {
                    continue;
                }

                if (target.NpcPool.Count > 0 && !Contains(target.NpcPool, actor.Id))
                {
                    continue;
                }

                npcCandidates++;
            }

            if (itemCandidates == 0 || npcCandidates == 0)
            {
                issues.Add(new ValidationIssue(
                    quest.Id,
                    CoreErrors.QuestTargetPool,
                    CoreText.Format(CoreErrors.QuestTargetPool, quest.Id)));
            }
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

        private static bool Contains(IReadOnlyList<ItemCategory> values, ItemCategory value)
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
