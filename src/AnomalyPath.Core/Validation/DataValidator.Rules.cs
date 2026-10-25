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
    /// <summary>Часть класса <see cref="DataValidator"/>: правила, снаряжение и защита.</summary>
    public sealed partial class DataValidator
    {
        private static void ValidateProtectionKeys(LoadedData data, List<ValidationIssue> issues)        {
            DamageTypeRepository damageTypes;
            ItemRepository items;
            try
            {
                damageTypes = DamageTypeRepository.FromLoadedData(data);
                items = ItemRepository.FromLoadedData(data);
            }
            catch (DataException)
            {
                return;
            }

            foreach (var item in items.All)
            {
                foreach (var pair in item.Protection)
                {
                    if (!damageTypes.Contains(pair.Key))
                    {
                        issues.Add(new ValidationIssue(
                            item.Id,
                            CoreErrors.UnknownDamageType,
                            CoreText.Format(CoreErrors.UnknownDamageType, pair.Key)));
                    }
                }
            }
        }

        private static void ValidateRules(LoadedData data, List<ValidationIssue> issues)
        {
            var records = data.All("rules");
            if (records.Count > 1)
            {
                issues.Add(new ValidationIssue(
                    "rules",
                    CoreErrors.RulesSingleRecord,
                    CoreText.Format(CoreErrors.RulesSingleRecord, records.Count)));
            }

            if (records.Count != 1)
            {
                return;
            }

            try
            {
                var rules = GameRulesTemplate.FromJson(records[0], "rules");
                var actors = ActorRepository.FromLoadedData(data);
                if (!actors.Contains(rules.PlayerActorId))
                {
                    issues.Add(new ValidationIssue(
                        "rules",
                        CoreErrors.ActorNotFound,
                        CoreText.Format(CoreErrors.ActorNotFound, rules.PlayerActorId)));
                }
            }
            catch (DataException)
            {
                // Ошибки схемы уже собраны в ValidateEntities.
            }
        }

        private static void ValidateLoadouts(LoadedData data, List<ValidationIssue> issues)
        {
            ItemRepository items;
            LoadoutRepository loadouts;
            try
            {
                items = ItemRepository.FromLoadedData(data);
                loadouts = LoadoutRepository.FromLoadedData(data);
            }
            catch (DataException)
            {
                return;
            }

            foreach (var loadout in loadouts.All)
            {
                foreach (var item in loadout.Items)
                {
                    if (!items.TryGet(item.ItemId, out _))
                    {
                        issues.Add(new ValidationIssue(
                            loadout.Id,
                            CoreErrors.LoadoutItemNotFound,
                            CoreText.Format(CoreErrors.LoadoutItemNotFound, item.ItemId)));
                    }
                }

                foreach (var pair in loadout.Equipment)
                {
                    if (!items.TryGet(pair.Value, out var template))
                    {
                        issues.Add(new ValidationIssue(
                            loadout.Id,
                            CoreErrors.LoadoutItemNotFound,
                            CoreText.Format(CoreErrors.LoadoutItemNotFound, pair.Value)));
                        continue;
                    }

                    var slotId = EquipmentSlots.Id(pair.Key);
                    if (!AllowsSlot(template.EquipSlots, slotId))
                    {
                        issues.Add(new ValidationIssue(
                            loadout.Id,
                            CoreErrors.LoadoutSlotMismatch,
                            CoreText.Format(CoreErrors.LoadoutSlotMismatch, pair.Value, slotId)));
                    }
                }
            }
        }

        private static bool AllowsSlot(IReadOnlyList<string> allowed, string slotId)
        {
            foreach (var slot in allowed)
            {
                if (string.Equals(slot, slotId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
