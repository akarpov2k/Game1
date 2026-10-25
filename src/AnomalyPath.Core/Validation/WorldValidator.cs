#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Validation
{
    /// <summary>
    /// Проверка связности маршрута: хаб и путь есть, переходы описаны, роли существуют,
    /// ветки достижимы из пути.
    /// </summary>
    public static class WorldValidator
    {
        /// <summary>Собирает ошибки мира, не прерываясь на первой.</summary>
        /// <param name="data">Загруженные данные.</param>
        /// <returns>Список ошибок; пустой, если маршрут связен.</returns>
        public static IReadOnlyList<ValidationIssue> Validate(LoadedData data)
        {
            var issues = new List<ValidationIssue>();
            LocationRepository locations;
            TopologyRepository topology;
            try
            {
                locations = LocationRepository.FromLoadedData(data);
                topology = TopologyRepository.FromLoadedData(data);
            }
            catch (DataException)
            {
                return issues;
            }

            if (locations.All.Count == 0)
            {
                return issues;
            }

            var hubs = Find(locations, LocationSubtype.Hub);
            var paths = Find(locations, LocationSubtype.Path);
            if (hubs.Count == 0)
            {
                issues.Add(Error("world", CoreErrors.WorldNoHub));
            }

            if (paths.Count == 0)
            {
                issues.Add(Error("world", CoreErrors.WorldNoPath));
            }

            ValidateTransitions(locations, topology.Topology, issues);
            ValidateBranchExits(paths, locations, topology.Topology, issues);
            ValidateChains(locations, topology.Topology, issues);
            ValidateReachability(paths, locations, topology.Topology, issues);
            return issues;
        }

        private static void ValidateTransitions(
            LocationRepository locations,
            TopologyTemplate topology,
            List<ValidationIssue> issues)
        {
            foreach (var rule in topology.Route.Transitions)
            {
                if (rule.From == LocationSubtype.Hub &&
                    rule.To != LocationSubtype.Path &&
                    rule.To != LocationSubtype.Hub)
                {
                    issues.Add(Error("topology", CoreErrors.WorldHubAccess));
                }

                if (Find(locations, rule.From).Count == 0)
                {
                    issues.Add(Error(
                        "topology",
                        CoreErrors.WorldUnknownSubtype,
                        SchemaEnum.ToCode(rule.From)));
                }

                if (Find(locations, rule.To).Count == 0)
                {
                    issues.Add(Error(
                        "topology",
                        CoreErrors.WorldUnknownSubtype,
                        SchemaEnum.ToCode(rule.To)));
                }

                if (rule.FromRole != null && !HasRole(Find(locations, rule.From), rule.FromRole))
                {
                    issues.Add(Error("topology", CoreErrors.WorldUnknownRole, rule.FromRole));
                }

                if (rule.ToRole != null && !HasRole(Find(locations, rule.To), rule.ToRole))
                {
                    issues.Add(Error("topology", CoreErrors.WorldUnknownRole, rule.ToRole));
                }
            }
        }

        /// <summary>Проверяет, что из каждой цепочки ветки можно выйти на путь следующего звена.</summary>
        private static void ValidateBranchExits(
            IReadOnlyList<LocationTemplate> paths,
            LocationRepository locations,
            TopologyTemplate topology,
            List<ValidationIssue> issues)
        {
            if (paths.Count == 0)
            {
                return;
            }

            foreach (var template in locations.All)
            {
                if (template.Subtype == LocationSubtype.Hub ||
                    template.Subtype == LocationSubtype.Path)
                {
                    continue;
                }

                var canExit = false;
                foreach (var path in paths)
                {
                    if (RouteRules.Vias(template, path, topology).Count > 0)
                    {
                        canExit = true;
                        break;
                    }
                }

                if (!canExit)
                {
                    issues.Add(Error(template.Id, CoreErrors.WorldBranchExit, template.Id));
                }
            }
        }

        /// <summary>Проверяет, что локацию с длиной цепочки больше одной можно повторить подряд.</summary>
        private static void ValidateChains(
            LocationRepository locations,
            TopologyTemplate topology,
            List<ValidationIssue> issues)
        {
            foreach (var template in locations.All)
            {
                if (template.Subtype == LocationSubtype.Hub || template.ChainLength.Range.Max <= 1)
                {
                    continue;
                }

                if (RouteRules.Vias(template, template, topology).Count == 0)
                {
                    issues.Add(Error(template.Id, CoreErrors.WorldChainLoop, template.Id));
                }
            }
        }

        private static void ValidateReachability(
            IReadOnlyList<LocationTemplate> paths,
            LocationRepository locations,
            TopologyTemplate topology,
            List<ValidationIssue> issues)
        {
            if (paths.Count == 0)
            {
                return;
            }

            var reachable = new HashSet<string>();
            var queue = new Queue<LocationTemplate>();
            foreach (var path in paths)
            {
                if (reachable.Add(path.Id))
                {
                    queue.Enqueue(path);
                }
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var template in locations.All)
                {
                    if (template.Subtype == LocationSubtype.Hub ||
                        reachable.Contains(template.Id))
                    {
                        continue;
                    }

                    if (RouteRules.Vias(current, template, topology).Count == 0)
                    {
                        continue;
                    }

                    reachable.Add(template.Id);
                    queue.Enqueue(template);
                }
            }

            foreach (var template in locations.All)
            {
                if (template.Subtype == LocationSubtype.Hub)
                {
                    continue;
                }

                if (!reachable.Contains(template.Id))
                {
                    issues.Add(Error(template.Id, CoreErrors.WorldRouteUnavailable, template.Id));
                }
            }
        }

        private static bool HasRole(IReadOnlyList<LocationTemplate> templates, string role)
        {
            foreach (var template in templates)
            {
                if (string.Equals(template.Role, role, System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static List<LocationTemplate> Find(LocationRepository locations, LocationSubtype subtype)
        {
            var result = new List<LocationTemplate>();
            foreach (var location in locations.All)
            {
                if (location.Subtype == subtype)
                {
                    result.Add(location);
                }
            }

            result.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));
            return result;
        }

        private static ValidationIssue Error(string path, string code, params object[] args) =>
            new ValidationIssue(path, code, CoreText.Format(code, args));
    }
}
