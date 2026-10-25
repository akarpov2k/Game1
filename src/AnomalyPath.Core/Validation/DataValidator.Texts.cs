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
    /// <summary>Часть класса <see cref="DataValidator"/>: переводы.</summary>
    public sealed partial class DataValidator
    {
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
    }
}
