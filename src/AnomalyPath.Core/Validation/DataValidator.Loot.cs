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
    /// <summary>Часть класса <see cref="DataValidator"/>: добыча.</summary>
    public sealed partial class DataValidator
    {
        private static void ValidateLoot(LoadedData data, List<ValidationIssue> issues)
        {
            LootRepository loot;
            ItemRepository items;
            ActorRepository actors;
            try
            {
                loot = LootRepository.FromLoadedData(data);
                items = ItemRepository.FromLoadedData(data);
                actors = ActorRepository.FromLoadedData(data);
            }
            catch (DataException)
            {
                return;
            }

            foreach (var table in loot.All)
            {
                foreach (var entry in table.Entries)
                {
                    if (!items.TryGet(entry.ItemId, out _))
                    {
                        issues.Add(new ValidationIssue(
                            table.Id,
                            CoreErrors.LootItemNotFound,
                            CoreText.Format(CoreErrors.LootItemNotFound, entry.ItemId)));
                    }
                }

                ExpandLootTable(
                    table,
                    loot,
                    new HashSet<string>(StringComparer.Ordinal),
                    new HashSet<string>(StringComparer.Ordinal),
                    issues);
            }

            foreach (var actor in actors.All)
            {
                if (actor.LootTableId == null)
                {
                    continue;
                }

                if (actor.Type != ActorType.Enemy)
                {
                    issues.Add(new ValidationIssue(
                        actor.Id,
                        CoreErrors.LootTableOnNonEnemy,
                        CoreText.Format(CoreErrors.LootTableOnNonEnemy, actor.Id)));
                    continue;
                }

                if (!loot.TryGet(actor.LootTableId, out _))
                {
                    issues.Add(new ValidationIssue(
                        actor.Id,
                        CoreErrors.LootTableNotFound,
                        CoreText.Format(CoreErrors.LootTableNotFound, actor.LootTableId)));
                }
            }
        }

        private static void ExpandLootTable(
            LootTable table,
            ILootRepository loot,
            HashSet<string> visiting,
            HashSet<string> seen,
            List<ValidationIssue> issues)
        {
            visiting.Add(table.Id);
            foreach (var sharedId in table.SharedTableIds)
            {
                if (visiting.Contains(sharedId))
                {
                    issues.Add(new ValidationIssue(
                        table.Id,
                        CoreErrors.LootTableCycle,
                        CoreText.Format(CoreErrors.LootTableCycle, sharedId)));
                    continue;
                }

                if (!loot.TryGet(sharedId, out var shared))
                {
                    issues.Add(new ValidationIssue(
                        table.Id,
                        CoreErrors.LootTableNotFound,
                        CoreText.Format(CoreErrors.LootTableNotFound, sharedId)));
                    continue;
                }

                if (!seen.Add(sharedId))
                {
                    issues.Add(new ValidationIssue(
                        table.Id,
                        CoreErrors.LootTableDuplicate,
                        CoreText.Format(CoreErrors.LootTableDuplicate, sharedId)));
                    continue;
                }

                ExpandLootTable(shared, loot, visiting, seen, issues);
            }

            visiting.Remove(table.Id);
        }
    }
}
