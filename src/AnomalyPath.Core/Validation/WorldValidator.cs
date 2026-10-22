#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Validation
{
    /// <summary>
    /// Проверка связности мира: роли, теги, топология, цели переходов и возможность
    /// построить маршрут до каждой точки интереса.
    /// </summary>
    public static class WorldValidator
    {
        /// <summary>Собирает ошибки мира, не прерываясь на первой.</summary>
        /// <param name="data">Загруженные данные.</param>
        /// <returns>Список ошибок; пустой, если мир связен.</returns>
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

            var hubs = Find(locations, "hub");
            var sites = Find(locations, "site");
            var routes = Find(locations, "route");
            if (hubs.Count == 0)
            {
                issues.Add(Error("world", CoreErrors.WorldNoHub));
            }

            if (sites.Count == 0)
            {
                issues.Add(Error("world", CoreErrors.WorldNoSite));
            }

            foreach (var location in locations.All)
            {
                if (location.ApproachLength.HasValue && location.ApproachLength.Value.Max > topology.Topology.MaxBranchDepth)
                {
                    issues.Add(Error(location.Id, CoreErrors.WorldDepth, location.Id));
                }

                foreach (var connection in location.Connections)
                {
                    if (!locations.Contains(connection.ToId))
                    {
                        issues.Add(Error(location.Id, CoreErrors.WorldConnectionTarget, connection.ToId));
                        continue;
                    }

                    var target = locations.Get(connection.ToId);
                    if (!topology.Topology.Allows(location.NodeType, target.NodeType, connection.Via))
                    {
                        issues.Add(Error(
                            location.Id,
                            CoreErrors.WorldTransitionForbidden,
                            location.NodeType,
                            target.NodeType,
                            connection.Via));
                        continue;
                    }

                    if (!ViaRules.Allows(connection.Via, location.NodeType, target.NodeType, location.Tags, target.Tags))
                    {
                        issues.Add(Error(location.Id, CoreErrors.WorldViaTags, connection.Via));
                    }

                    if (location.NodeType == "hub" && target.NodeType == "hub" && !topology.Topology.HubFastTravel)
                    {
                        issues.Add(Error(location.Id, CoreErrors.WorldTransitionForbidden, "hub", "hub", connection.Via));
                    }
                }
            }

            foreach (var site in sites)
            {
                if (hubs.Count == 0)
                {
                    continue;
                }

                if (!HasRoute(hubs[0], site, routes, topology.Topology))
                {
                    issues.Add(Error(site.Id, CoreErrors.WorldRouteUnavailable, site.Id));
                    continue;
                }

                var maxLength = (site.ApproachLength ?? topology.Topology.RouteLength).Max;
                if (maxLength > 1 && !HasRoutePair(routes, topology.Topology))
                {
                    issues.Add(Error(site.Id, CoreErrors.WorldRouteUnavailable, site.Id));
                }
            }

            return issues;
        }

        private static bool HasRoute(
            LocationTemplate hub,
            LocationTemplate site,
            IReadOnlyList<LocationTemplate> routes,
            TopologyTemplate topology)
        {
            var seen = new HashSet<string>();
            var queue = new Queue<LocationTemplate>();
            foreach (var route in routes)
            {
                if (RouteRules.Vias(hub, route, topology).Count > 0 && seen.Add(route.Id))
                {
                    queue.Enqueue(route);
                }
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (RouteRules.Vias(current, site, topology).Count > 0)
                {
                    return true;
                }

                foreach (var next in routes)
                {
                    if (seen.Add(next.Id) && RouteRules.Vias(current, next, topology).Count > 0)
                    {
                        queue.Enqueue(next);
                    }
                }
            }

            return false;
        }

        private static bool HasRoutePair(IReadOnlyList<LocationTemplate> routes, TopologyTemplate topology)
        {
            foreach (var from in routes)
            {
                foreach (var to in routes)
                {
                    if (RouteRules.Vias(from, to, topology).Count > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static List<LocationTemplate> Find(LocationRepository locations, string nodeType)
        {
            var result = new List<LocationTemplate>();
            foreach (var location in locations.All)
            {
                if (location.NodeType == nodeType)
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
