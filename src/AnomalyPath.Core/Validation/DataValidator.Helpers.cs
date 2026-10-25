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
    /// <summary>Часть класса <see cref="DataValidator"/>: служебные помощники.</summary>
    public sealed partial class DataValidator
    {
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
