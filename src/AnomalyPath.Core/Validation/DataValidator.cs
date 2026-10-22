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
using AnomalyPath.Core.Nicks;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Rules;
using AnomalyPath.Core.Templates;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Validation
{
    /// <summary>
    /// Проверка данных до старта игры на уже разобранных файлах. Собирает все ошибки
    /// за один проход: синтаксис, наследование, дубли id, коллекции, каталог и переводы.
    /// </summary>
    public sealed class DataValidator : IDataValidator
    {
        private readonly IDataSource _source;
        private readonly IReadOnlyDictionary<string, JsonValue> _parsed;
        private readonly IReadOnlyList<ValidationIssue> _syntaxIssues;

        /// <summary>Создаёт валидатор поверх источника и разобранных файлов.</summary>
        /// <param name="source">Источник данных.</param>
        /// <param name="parsed">Кэш «путь → разобранный JSON».</param>
        /// <param name="syntaxIssues">Ошибки разбора, найденные при чтении файлов.</param>
        public DataValidator(
            IDataSource source,
            IReadOnlyDictionary<string, JsonValue> parsed,
            IReadOnlyList<ValidationIssue> syntaxIssues)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _parsed = parsed ?? throw new ArgumentNullException(nameof(parsed));
            _syntaxIssues = syntaxIssues ?? throw new ArgumentNullException(nameof(syntaxIssues));
        }

        /// <inheritdoc />
        public ValidationReport Validate() => Validate(out _);

        /// <summary>Проверяет данные и отдаёт загруженные сущности, чтобы не грузить их повторно.</summary>
        /// <param name="data">Загруженные данные или <c>null</c>, если есть ошибки.</param>
        /// <returns>Отчёт со всеми найденными ошибками и предупреждениями.</returns>
        public ValidationReport Validate(out LoadedData? data)
        {
            var issues = new List<ValidationIssue>(_syntaxIssues);
            var loader = new DataLoader(_source, _parsed);
            var entityFiles = ValidateEntities(loader, issues);
            var reachable = ValidateCatalogAndReachability(issues);

            if (reachable != null)
            {
                foreach (var file in entityFiles)
                {
                    if (!reachable.Contains(file))
                    {
                        issues.Add(new ValidationIssue(
                            file,
                            CoreErrors.UnreachableFile,
                            CoreText.Get(CoreErrors.UnreachableFile),
                            isWarning: true));
                    }
                }
            }

            data = null;
            try
            {
                data = loader.Load();
                var loaded = data;
                Try(() => ItemRepository.FromLoadedData(loaded), issues);
                Try(() => DamageTypeRepository.FromLoadedData(loaded), issues);
                ValidateProtectionKeys(loaded, issues);
                ValidateRules(loaded, issues);
                ValidateLoadouts(loaded, issues);
                ValidateSettings(loaded, issues);
                AddWorldIssues(loaded, issues);
            }
            catch (DataException error)
            {
                issues.Add(new ValidationIssue(error.Path, error.Code, error.Reason));
                data = null;
            }

            ValidateTextSets(issues);
            ValidateGameTexts(issues);

            var report = new ValidationReport(Distinct(issues));
            if (!report.IsValid)
            {
                data = null;
            }

            return report;
        }

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
        }

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

        private static void AddWorldIssues(LoadedData data, List<ValidationIssue> issues)
        {
            foreach (var issue in WorldValidator.Validate(data))
            {
                issues.Add(issue);
            }
        }

        private static void ValidateCombatReferences(LoadedData data, List<ValidationIssue> issues)
        {
            try
            {
                var damageTypes = DamageTypeRepository.FromLoadedData(data);
                var formulas = FormulaRepository.FromLoadedData(data);
                var actors = ActorRepository.FromLoadedData(data);
                var balanceRecords = data.All("balance");
                var constants = balanceRecords.Count > 0
                    ? BalanceTemplate.FromJson(balanceRecords[0]).Constants
                    : BalanceTemplate.Empty.Constants;

                foreach (var formula in formulas.All)
                {
                    var declared = new HashSet<string>(formula.Variables, StringComparer.Ordinal);
                    foreach (var name in formula.UsedNames)
                    {
                        if (!declared.Contains(name) && !constants.ContainsKey(name))
                        {
                            issues.Add(new ValidationIssue(
                                formula.Id,
                                CoreErrors.FormulaUnknownVariable,
                                CoreText.Format(CoreErrors.FormulaUnknownVariable, name)));
                        }
                    }
                }

                var combat = data.All("combat");
                if (combat.Count > 1)
                {
                    issues.Add(new ValidationIssue("combat", CoreErrors.DuplicateIdFiles, "combat"));
                }

                if (combat.Count == 1)
                {
                    var settings = CombatSettingsTemplate.FromJson(combat[0], "combat");
                    if (!formulas.Contains(settings.DamageFormulaId))
                    {
                        issues.Add(new ValidationIssue("combat", CoreErrors.FormulaNotFound, settings.DamageFormulaId));
                    }

                    if (settings.InitiativeFormulaId != null && !formulas.Contains(settings.InitiativeFormulaId))
                    {
                        issues.Add(new ValidationIssue("combat", CoreErrors.FormulaNotFound, settings.InitiativeFormulaId));
                    }
                }

                foreach (var actor in actors.All)
                {
                    var block = actor.Combat;
                    if (block == null)
                    {
                        continue;
                    }

                    foreach (var pair in block.Protection)
                    {
                        if (!damageTypes.Contains(pair.Key))
                        {
                            issues.Add(new ValidationIssue(
                                actor.Id,
                                CoreErrors.UnknownDamageType,
                                CoreText.Format(CoreErrors.UnknownDamageType, pair.Key)));
                        }
                    }
                }
            }
            catch (DataException)
            {
                // Ошибки схемы уже собраны отдельными проходами.
            }
        }

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
            }

            return entityFiles;
        }

        private HashSet<string>? ValidateCatalogAndReachability(List<ValidationIssue> issues)
        {
            if (!_parsed.TryGetValue("catalog.json", out var catalog))
            {
                issues.Add(new ValidationIssue("catalog.json", CoreErrors.FileNotFound, CoreText.Get(CoreErrors.FileNotFound)));
                return null;
            }

            if (!catalog.TryGet("files", out var files) || !files.IsArray)
            {
                issues.Add(new ValidationIssue(
                    "catalog.json",
                    CoreErrors.MissingArrayField,
                    CoreText.Format(CoreErrors.MissingArrayField, "files")));
                return null;
            }

            var reachable = new HashSet<string>(StringComparer.Ordinal);
            var referredBy = new Dictionary<string, string>(StringComparer.Ordinal);
            var queue = new Queue<string>();

            foreach (var item in files.Items)
            {
                if (item.Kind != JsonKind.String)
                {
                    issues.Add(new ValidationIssue(
                        "catalog.json",
                        CoreErrors.ArrayItemNotString,
                        CoreText.Format(CoreErrors.ArrayItemNotString, "files")));
                    continue;
                }

                if (!_parsed.ContainsKey(item.Text))
                {
                    issues.Add(new ValidationIssue(item.Text, CoreErrors.FileNotFound, CoreText.Get(CoreErrors.FileNotFound)));
                    continue;
                }

                queue.Enqueue(item.Text);
            }

            DetectCollectionCycles(issues);

            while (queue.Count > 0)
            {
                var path = queue.Dequeue();
                if (!reachable.Add(path) || !_parsed.TryGetValue(path, out var value) || !value.IsObject)
                {
                    continue;
                }

                if (!IsFlag(value, "collection"))
                {
                    continue;
                }

                foreach (var entry in CollectionItems(value, "entries", path, issues))
                {
                    if (!_parsed.ContainsKey(entry))
                    {
                        issues.Add(new ValidationIssue(entry, CoreErrors.FileNotFound, CoreText.Get(CoreErrors.FileNotFound)));
                        continue;
                    }

                    if (referredBy.TryGetValue(entry, out var first))
                    {
                        issues.Add(new ValidationIssue(
                            entry,
                            CoreErrors.DuplicateEntity,
                            CoreText.Format(CoreErrors.DuplicateEntity, entry) + $" ({first})"));
                    }
                    else
                    {
                        referredBy[entry] = path;
                    }

                    queue.Enqueue(entry);
                }

                foreach (var include in CollectionItems(value, "include", path, issues))
                {
                    if (!_parsed.ContainsKey(include))
                    {
                        issues.Add(new ValidationIssue(include, CoreErrors.FileNotFound, CoreText.Get(CoreErrors.FileNotFound)));
                        continue;
                    }

                    queue.Enqueue(include);
                }
            }

            return reachable;
        }

        private void DetectCollectionCycles(List<ValidationIssue> issues)
        {
            var visiting = new HashSet<string>(StringComparer.Ordinal);
            var done = new HashSet<string>(StringComparer.Ordinal);

            foreach (var path in _parsed.Keys)
            {
                if (_parsed[path].IsObject && IsFlag(_parsed[path], "collection"))
                {
                    WalkCollections(path, visiting, done, issues);
                }
            }
        }

        private void WalkCollections(string path, HashSet<string> visiting, HashSet<string> done, List<ValidationIssue> issues)
        {
            if (done.Contains(path) || !visiting.Add(path))
            {
                if (visiting.Contains(path) && !done.Contains(path))
                {
                    issues.Add(new ValidationIssue(path, CoreErrors.InheritanceCycle, CoreText.Get(CoreErrors.InheritanceCycle)));
                }

                return;
            }

            if (_parsed.TryGetValue(path, out var value) && value.IsObject)
            {
                foreach (var include in CollectionItems(value, "include", path, issues))
                {
                    if (_parsed.ContainsKey(include))
                    {
                        WalkCollections(include, visiting, done, issues);
                    }
                }
            }

            visiting.Remove(path);
            done.Add(path);
        }

        private IEnumerable<string> CollectionItems(JsonValue value, string field, string path, List<ValidationIssue> issues)
        {
            var result = new List<string>();
            if (!value.TryGet(field, out var items) || !items.IsArray)
            {
                if (field == "entries")
                {
                    issues.Add(new ValidationIssue(
                        path,
                        CoreErrors.MissingArrayField,
                        CoreText.Format(CoreErrors.MissingArrayField, field)));
                }

                return result;
            }

            foreach (var item in items.Items)
            {
                if (item.Kind != JsonKind.String)
                {
                    issues.Add(new ValidationIssue(
                        path,
                        CoreErrors.ArrayItemNotString,
                        CoreText.Format(CoreErrors.ArrayItemNotString, field)));
                    continue;
                }

                result.Add(item.Text);
            }

            return result;
        }

        private void ValidateTextSets(List<ValidationIssue> issues)
        {
            var ru = TryReadCatalog("Localization/errors.ru.json", issues);
            var en = TryReadCatalog("Localization/errors.en.json", issues);
            if (ru == null || en == null)
            {
                return;
            }

            foreach (var key in ru.Keys)
            {
                if (!en.Contains(key))
                {
                    issues.Add(new ValidationIssue(
                        "Localization/errors.en.json",
                        CoreErrors.TranslationMissing,
                        CoreText.Format(CoreErrors.TranslationMissing, key, "en"),
                        isWarning: true));
                }
            }

            foreach (var key in en.Keys)
            {
                if (!ru.Contains(key))
                {
                    issues.Add(new ValidationIssue(
                        "Localization/errors.ru.json",
                        CoreErrors.TranslationMissing,
                        CoreText.Format(CoreErrors.TranslationMissing, key, "ru"),
                        isWarning: true));
                }
            }
        }

        private void ValidateGameTexts(List<ValidationIssue> issues)
        {
            var ru = TryReadCatalog(Bootstrap.DefaultGameTextCatalog, issues);
            if (ru == null)
            {
                issues.Add(new ValidationIssue(
                    Bootstrap.DefaultGameTextCatalog,
                    CoreErrors.GameTextsMissing,
                    CoreText.Format(CoreErrors.GameTextsMissing, Bootstrap.DefaultGameTextCatalog),
                    isWarning: true));
                return;
            }

            var en = TryReadCatalog(Bootstrap.FallbackGameTextCatalog, issues);

            foreach (var file in _parsed)
            {
                if (file.Key.StartsWith("Localization/", StringComparison.OrdinalIgnoreCase) ||
                    file.Key.StartsWith("Localization\\", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (var key in CollectTextKeys(file.Value))
                {
                    if (!ru.Contains(key))
                    {
                        issues.Add(new ValidationIssue(
                            file.Key,
                            CoreErrors.TranslationMissing,
                            CoreText.Format(CoreErrors.TranslationMissing, key, "ru")));
                    }
                    else if (en != null && !en.Contains(key))
                    {
                        issues.Add(new ValidationIssue(
                            file.Key,
                            CoreErrors.TranslationMissing,
                            CoreText.Format(CoreErrors.TranslationMissing, key, "en"),
                            isWarning: true));
                    }
                }
            }

            if (en == null)
            {
                return;
            }

            foreach (var key in ru.Keys)
            {
                if (!en.Contains(key))
                {
                    issues.Add(new ValidationIssue(
                        Bootstrap.DefaultGameTextCatalog,
                        CoreErrors.TranslationMissing,
                        CoreText.Format(CoreErrors.TranslationMissing, key, "en"),
                        isWarning: true));
                }
            }

            foreach (var key in en.Keys)
            {
                if (!ru.Contains(key))
                {
                    issues.Add(new ValidationIssue(
                        Bootstrap.FallbackGameTextCatalog,
                        CoreErrors.TranslationMissing,
                        CoreText.Format(CoreErrors.TranslationMissing, key, "ru"),
                        isWarning: true));
                }
            }
        }

        private static IEnumerable<string> CollectTextKeys(JsonValue value)
        {
            if (value.Kind == JsonKind.Object)
            {
                foreach (var pair in value.Entries)
                {
                    if (pair.Key.EndsWith("Key", StringComparison.Ordinal) &&
                        pair.Value.Kind == JsonKind.String &&
                        pair.Value.Text.Length > 0)
                    {
                        yield return pair.Value.Text;
                        continue;
                    }

                    foreach (var nested in CollectTextKeys(pair.Value))
                    {
                        yield return nested;
                    }
                }
            }
            else if (value.Kind == JsonKind.Array)
            {
                foreach (var item in value.Items)
                {
                    foreach (var nested in CollectTextKeys(item))
                    {
                        yield return nested;
                    }
                }
            }
        }

        private JsonStringCatalog? TryReadCatalog(string path, List<ValidationIssue> issues)
        {
            if (!_parsed.TryGetValue(path, out var value))
            {
                return null;
            }

            try
            {
                return JsonStringCatalog.FromValue(value, path);
            }
            catch (DataException error)
            {
                issues.Add(new ValidationIssue(error.Path, error.Code, error.Reason));
                return null;
            }
        }

        private static void Try(Action body, List<ValidationIssue> issues)
        {
            try
            {
                body();
            }
            catch (DataException error)
            {
                issues.Add(new ValidationIssue(error.Path, error.Code, error.Reason));
            }
        }

        private static bool IsFlag(JsonValue value, string field) =>
            value.TryGet(field, out var flag) && flag.Kind == JsonKind.Bool && flag.Bool;

        private static IEnumerable<ValidationIssue> Distinct(List<ValidationIssue> issues)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var result = new List<ValidationIssue>();
            foreach (var issue in issues)
            {
                if (seen.Add(issue.Path + "|" + issue.Message))
                {
                    result.Add(issue);
                }
            }

            return result;
        }
    }
}
