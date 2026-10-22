#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Services
{
    /// <summary>
    /// Детерминированная генерация мира: один хаб, независимая цепочка отрезков до каждой точки интереса,
    /// случайная длина ребра и двери внутрь зданий.
    /// </summary>
    public sealed class WorldGenerator : IWorldGenerator
    {
        private readonly ILocationRepository _locations;
        private readonly ITopologyRepository _topology;

        /// <summary>Создаёт генератор.</summary>
        /// <param name="locations">Шаблоны локаций.</param>
        /// <param name="topology">Топология мира.</param>
        /// <exception cref="ArgumentNullException">Зависимость не задана.</exception>
        public WorldGenerator(ILocationRepository locations, ITopologyRepository topology)
        {
            _locations = locations ?? throw new ArgumentNullException(nameof(locations));
            _topology = topology ?? throw new ArgumentNullException(nameof(topology));
        }

        /// <inheritdoc />
        public WorldMap Generate(int seed)
        {
            var topology = _topology.Topology;
            var hubs = Select("hub");
            var sites = Select("site");
            var routes = Select("route");
            if (hubs.Count == 0)
            {
                throw new DataException("world", CoreErrors.WorldNoHub);
            }

            if (sites.Count == 0)
            {
                throw new DataException("world", CoreErrors.WorldNoSite);
            }

            var random = new SeededWorldRandom(seed);
            var locations = new List<LocationInstance>();
            var edges = new List<WorldEdge>();
            var instanceCounter = 0;
            var edgeCounter = 0;

            var rootTemplate = hubs[0];
            var root = NewInstance(rootTemplate, random, ref instanceCounter);
            locations.Add(root);

            foreach (var site in sites)
            {
                if (!site.Unique && random.NextDouble() > site.SpawnChance)
                {
                    continue;
                }

                var siteInstance = NewInstance(site, random, ref instanceCounter);
                locations.Add(siteInstance);
                BuildRoute(root, rootTemplate, siteInstance, site, routes, topology, random, locations, edges, ref instanceCounter, ref edgeCounter);
                AttachInteriors(siteInstance, site, topology, random, locations, edges, ref instanceCounter, ref edgeCounter);
            }

            return new WorldMap(
                seed,
                locations,
                edges,
                root.InstanceId,
                topology.InitialStamina,
                topology.DayStartHour,
                topology.NightStartHour);
        }

        private void BuildRoute(
            LocationInstance root,
            LocationTemplate rootTemplate,
            LocationInstance siteInstance,
            LocationTemplate site,
            IReadOnlyList<LocationTemplate> routes,
            TopologyTemplate topology,
            IWorldRandom random,
            List<LocationInstance> locations,
            List<WorldEdge> edges,
            ref int instanceCounter,
            ref int edgeCounter)
        {
            var count = (site.ApproachLength ?? topology.RouteLength).Sample(random);
            if (count < 1)
            {
                count = 1;
            }

            if (count > topology.MaxBranchDepth)
            {
                throw new DataException(site.Id, CoreErrors.WorldDepth, site.Id);
            }

            var current = root;
            var currentTemplate = rootTemplate;
            for (var step = 0; step < count; step++)
            {
                var candidates = Candidates(currentTemplate, routes, topology);
                if (candidates.Count == 0)
                {
                    throw new DataException(site.Id, CoreErrors.WorldRouteUnavailable, site.Id);
                }

                var choice = candidates[random.NextInt(0, candidates.Count)];
                var length = topology.EdgeLength.Sample(random);
                var routeInstance = NewInstance(choice.Template, random, ref instanceCounter);
                locations.Add(routeInstance);
                AddEdges(current.InstanceId, routeInstance.InstanceId, choice.Via, length, null, topology, edges, ref edgeCounter);
                current = routeInstance;
                currentTemplate = choice.Template;
            }

            var finalVias = RouteRules.Vias(currentTemplate, site, topology);
            if (finalVias.Count == 0)
            {
                throw new DataException(site.Id, CoreErrors.WorldRouteUnavailable, site.Id);
            }

            var finalVia = finalVias[random.NextInt(0, finalVias.Count)];
            var finalLength = topology.EdgeLength.Sample(random);
            AddEdges(current.InstanceId, siteInstance.InstanceId, finalVia, finalLength, null, topology, edges, ref edgeCounter);
        }

        private void AttachInteriors(
            LocationInstance siteInstance,
            LocationTemplate site,
            TopologyTemplate topology,
            IWorldRandom random,
            List<LocationInstance> locations,
            List<WorldEdge> edges,
            ref int instanceCounter,
            ref int edgeCounter)
        {
            foreach (var connection in site.Connections)
            {
                if (!_locations.Contains(connection.ToId))
                {
                    throw new DataException(site.Id, CoreErrors.WorldConnectionTarget, connection.ToId);
                }

                var interiorTemplate = _locations.Get(connection.ToId);
                if (interiorTemplate.NodeType != "interior")
                {
                    continue;
                }

                var interior = NewInstance(interiorTemplate, random, ref instanceCounter);
                locations.Add(interior);
                var length = connection.Length ?? 1;
                AddEdges(siteInstance.InstanceId, interior.InstanceId, connection.Via, length, connection, topology, edges, ref edgeCounter);
            }
        }

        private List<RouteChoice> Candidates(
            LocationTemplate from,
            IReadOnlyList<LocationTemplate> routes,
            TopologyTemplate topology)
        {
            var result = new List<RouteChoice>();
            foreach (var route in routes)
            {
                foreach (var via in RouteRules.Vias(from, route, topology))
                {
                    result.Add(new RouteChoice(route, via));
                }
            }

            return result;
        }

        private static void AddEdges(
            string fromId,
            string toId,
            string via,
            int length,
            ConnectionTemplate? connection,
            TopologyTemplate topology,
            List<WorldEdge> edges,
            ref int edgeCounter)
        {
            var baseCost = connection?.Cost ?? topology.GetCost(via);
            var cost = baseCost.Scale(length);
            var hidden = connection?.Hidden ?? false;
            var oneWay = connection?.OneWay ?? false;
            edges.Add(new WorldEdge(NextEdgeId(ref edgeCounter), fromId, toId, via, length, cost, oneWay, hidden));
            if (!oneWay)
            {
                edges.Add(new WorldEdge(NextEdgeId(ref edgeCounter), toId, fromId, via, length, cost, false, hidden));
            }
        }

        private static LocationInstance NewInstance(LocationTemplate template, IWorldRandom random, ref int counter)
        {
            counter++;
            var instanceId = "locinst_" + counter.ToString("D4", CultureInfo.InvariantCulture);
            var seed = random.NextInt(1, int.MaxValue);
            var background = template.Backgrounds[random.NextInt(0, template.Backgrounds.Count)];
            return new LocationInstance(instanceId, template.Id, seed, background, template.NodeType, template.Tier, template.Danger);
        }

        private static string NextEdgeId(ref int counter)
        {
            counter++;
            return "edge_" + counter.ToString("D4", CultureInfo.InvariantCulture);
        }

        private List<LocationTemplate> Select(string nodeType)
        {
            var result = new List<LocationTemplate>();
            foreach (var template in _locations.All)
            {
                if (template.NodeType == nodeType)
                {
                    result.Add(template);
                }
            }

            result.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));
            return result;
        }

        private readonly struct RouteChoice
        {
            public RouteChoice(LocationTemplate template, string via)
            {
                Template = template;
                Via = via;
            }

            public LocationTemplate Template { get; }

            public string Via { get; }
        }
    }
}
