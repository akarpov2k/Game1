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
using AnomalyPath.Core.Progression;
using AnomalyPath.Core.Quests;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Rules;
using AnomalyPath.Core.Talents;
using AnomalyPath.Core.Templates;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Validation
{
    /// <summary>Часть класса <see cref="DataValidator"/>: настройки игры.</summary>
    public sealed partial class DataValidator
    {
        private static void ValidateSettings(LoadedData data, List<ValidationIssue> issues)
        {
            var combat = data.All("combat");
            if (combat.Count == 0)
            {
                issues.Add(new ValidationIssue("combat", CoreErrors.FileNotFound, CoreText.Get(CoreErrors.FileNotFound)));
            }
            else
            {
                Try(() => CombatSettingsTemplate.FromJson(combat[0], "combat"), issues);
            }

            var equipment = data.All("equipment");
            if (equipment.Count == 0)
            {
                issues.Add(new ValidationIssue("equipment", CoreErrors.FileNotFound, CoreText.Get(CoreErrors.FileNotFound)));
            }

            if (data.All("topology").Count == 0)
            {
                issues.Add(new ValidationIssue("topology", CoreErrors.FileNotFound, CoreText.Get(CoreErrors.FileNotFound)));
            }

            var economy = data.All("economy");
            if (economy.Count > 0)
            {
                Try(() => EconomySettings.FromJson(economy[0], "economy"), issues);
            }

            var loot = data.All(LootSettings.RecordId);
            if (loot.Count == 0 && data.All("lootTables").Count > 0)
            {
                issues.Add(new ValidationIssue(
                    LootSettings.RecordId,
                    CoreErrors.FileNotFound,
                    CoreText.Get(CoreErrors.FileNotFound)));
            }
            else if (loot.Count > 1)
            {
                issues.Add(new ValidationIssue(
                    LootSettings.RecordId,
                    CoreErrors.LootSettingsMultiple,
                    CoreText.Format(CoreErrors.LootSettingsMultiple, loot.Count)));
            }
            else if (loot.Count == 1)
            {
                Try(() => LootSettings.FromJson(loot[0], LootSettings.RecordId), issues);
            }

            var progression = data.All(ProgressionSettings.RecordId);
            if (progression.Count == 0)
            {
                issues.Add(new ValidationIssue(
                    ProgressionSettings.RecordId,
                    CoreErrors.ProgressionMissing,
                    CoreText.Get(CoreErrors.ProgressionMissing)));
            }
            else if (progression.Count > 1)
            {
                issues.Add(new ValidationIssue(
                    ProgressionSettings.RecordId,
                    CoreErrors.ProgressionSingleRecord,
                    CoreText.Format(CoreErrors.ProgressionSingleRecord, progression.Count)));
            }
            else
            {
                Try(() => ProgressionSettings.FromJson(progression[0], ProgressionSettings.RecordId), issues);
            }
        }
    }
}
