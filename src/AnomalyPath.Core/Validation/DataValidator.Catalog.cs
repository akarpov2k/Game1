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
    /// <summary>Часть класса <see cref="DataValidator"/>: каталог и достижимость.</summary>
    public sealed partial class DataValidator
    {
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
    }
}
