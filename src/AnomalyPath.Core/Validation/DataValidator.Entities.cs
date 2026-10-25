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
using AnomalyPath.Core.Spawns;
using AnomalyPath.Core.Talents;
using AnomalyPath.Core.Templates;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Validation
{
    /// <summary>Часть класса <see cref="DataValidator"/>: сущности.</summary>
    public sealed partial class DataValidator
    {
        private List<string> ValidateEntities(DataLoader loader, List<ValidationIssue> issues)
        {
            var entityFiles = new List<string>();
            var declared = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var pair in _parsed)
            {
                var value = pair.Value;
                if (!value.IsObject)
                {
                    issues.Add(new ValidationIssue(pair.Key, CoreErrors.JsonNotObject, CoreText.Get(CoreErrors.JsonNotObject)));
                    continue;
                }

                if (IsFlag(value, "collection"))
                {
                    continue;
                }

                var kind = value.TryGet("kind", out var kindValue) ? kindValue.Text : null;
                if (string.IsNullOrWhiteSpace(kind))
                {
                    // Каталог и файлы текстов вида не имеют.
                    continue;
                }

                JsonValue resolved;
                try
                {
                    resolved = loader.ResolveEntity(pair.Key);
                }
                catch (DataException error)
                {
                    issues.Add(new ValidationIssue(error.Path, error.Code, error.Reason));
                    continue;
                }

                if (IsFlag(resolved, "abstract"))
                {
                    continue;
                }

                entityFiles.Add(pair.Key);

                if (!resolved.TryGet("id", out var idValue) || idValue.Kind != JsonKind.String ||
                    string.IsNullOrWhiteSpace(idValue.Text))
                {
                    issues.Add(new ValidationIssue(
                        pair.Key,
                        CoreErrors.MissingStringField,
                        CoreText.Format(CoreErrors.MissingStringField, "id")));
                    continue;
                }

                var id = idValue.Text;
                var key = kind + "|" + id;
                if (declared.TryGetValue(key, out var firstPath))
                {
                    issues.Add(new ValidationIssue(
                        pair.Key,
                        CoreErrors.DuplicateIdFiles,
                        CoreText.Format(CoreErrors.DuplicateIdFiles, id, kind, firstPath)));
                }
                else
                {
                    declared[key] = pair.Key;
                }

                if (kind == "items")
                {
                    Try(() => ItemTemplate.FromJson(resolved, pair.Key), issues);
                }
                else if (kind == "equipment")
                {
                    Try(() => Equipment.EquipmentSettings.FromJson(resolved, pair.Key), issues);
                }
                else if (kind == "locations")
                {
                    Try(() => LocationTemplate.FromJson(resolved, pair.Key), issues);
                }
                else if (kind == "topology")
                {
                    Try(() => TopologyTemplate.FromJson(resolved, pair.Key), issues);
                }
                else if (kind == "rules")
                {
                    Try(() => GameRulesTemplate.FromJson(resolved, pair.Key), issues);
                }
                else if (kind == "loadouts")
                {
                    Try(() => LoadoutTemplate.FromJson(resolved, pair.Key), issues);
                }
                else if (kind == "nicks")
                {
                    Try(() => NicknameCatalogTemplate.FromJson(resolved, pair.Key), issues);
                }
                else if (kind == "actors")
                {
                    Try(() => ActorTemplate.FromJson(resolved, pair.Key), issues);
                }
                else if (kind == "economy")
                {
                    Try(() => EconomySettings.FromJson(resolved, pair.Key), issues);
                }
                else if (kind == "quests")
                {
                    Try(() => QuestTemplate.FromJson(resolved, pair.Key), issues);
                }
                else if (kind == "talents")
                {
                    Try(() => TalentTreeTemplate.FromJson(resolved, pair.Key), issues);
                }
                else if (kind == "lootTables")
                {
                    Try(() => LootTable.FromJson(resolved, pair.Key), issues);
                }
                else if (kind == "spawns")
                {
                    Try(() => SpawnEntry.FromJson(resolved, pair.Key), issues);
                }
                else if (kind == LootSettings.RecordId)
                {
                    Try(() => LootSettings.FromJson(resolved, pair.Key), issues);
                }
                else if (kind == ProgressionSettings.RecordId)
                {
                    Try(() => ProgressionSettings.FromJson(resolved, pair.Key), issues);
                }
            }

            return entityFiles;
        }
    }
}
