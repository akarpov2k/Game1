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
    /// Детерминированный генератор маршрута: хаб, звенья пути с ветками в здания и местности.
    /// Один seed и одно состояние мира дают один маршрут; звено строится из своего seed,
    /// поэтому достройка после загрузки продолжает тот же маршрут. Ветки крепятся к разным
    /// локациям звена, их концы ведут в единственное следующее звено, а конец звена пути
    /// всегда имеет переход. Шаг считается внутри звена, поэтому цена возврата в хаб
    /// на первом шаге нового звена сбрасывается до базы.
    /// </summary>
    public sealed partial class WorldGenerator : IWorldGenerator
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
            var hub = RequireHub();
            var random = new SeededWorldRandom(seed);
            var hubInstance = NewInstance(hub, random, routeStep: 0, routeEdge: 0, "loc_hub");
            var locations = new List<LocationInstance> { hubInstance };
            var edges = new List<WorldEdge>();

            var map = new WorldMap(
                seed,
                locations,
                edges,
                hubInstance.InstanceId,
                topology.InitialStamina,
                topology.DayStartHour,
                topology.NightStartHour);

            BuildForward(map, hubInstance, hub, topology.Route.LookaheadEdges);
            return map;
        }

        /// <inheritdoc />
        public void BeginRoute(WorldState world)
        {
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }

            var topology = _topology.Topology;
            var hub = RequireHub();
            var map = world.Map;
            if (world.CurrentLocation.Subtype != LocationSubtype.Hub)
            {
                throw new DataException(world.CurrentLocation.InstanceId, CoreErrors.WorldRouteNotFromHub);
            }

            var stale = new List<string>();
            foreach (var location in map.Locations)
            {
                if (location.RouteEdge >= 1)
                {
                    stale.Add(location.InstanceId);
                }
            }

            foreach (var id in stale)
            {
                map.RemoveLocation(id);
            }

            map.BeginRoute();
            var hubInstance = map.GetLocation(map.StartLocationId);
            BuildForward(map, hubInstance, hub, topology.Route.LookaheadEdges);
        }

        /// <inheritdoc />
        public void Extend(WorldState world)
        {
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }

            var topology = _topology.Topology;
            var needed = world.CurrentLocation.RouteEdge + topology.Route.LookaheadEdges;
            var built = world.RouteEdges;
            if (built >= needed)
            {
                return;
            }

            var tailId = world.RouteTailId;
            if (tailId == null)
            {
                var hub = RequireHub();
                var hubInstance = world.Map.StartLocationId == null
                    ? throw new DataException("world", CoreErrors.WorldNoHub)
                    : world.Map.GetLocation(world.Map.StartLocationId);
                BuildForward(world.Map, hubInstance, hub, needed);
                return;
            }

            var tail = world.Map.GetLocation(tailId);
            var tailTemplate = _locations.Get(tail.TemplateId);
            BuildForward(world.Map, tail, tailTemplate, needed - built);
        }

        private void BuildForward(WorldMap map, LocationInstance from, LocationTemplate fromTemplate, int edgesToBuild)
        {
            var topology = _topology.Topology;
            var current = from;
            var currentTemplate = fromTemplate;
            for (var index = 0; index < edgesToBuild; index++)
            {
                var edgeNumber = map.RouteEdges + 1;
                current = BuildEdge(map, current, currentTemplate, edgeNumber, topology);
                currentTemplate = _locations.Get(current.TemplateId);
            }
        }

        private LocationInstance BuildEdge(
            WorldMap map,
            LocationInstance from,
            LocationTemplate fromTemplate,
            int edgeNumber,
            TopologyTemplate topology)
        {
            var paths = Select(LocationSubtype.Path);
            if (paths.Count == 0)
            {
                throw new DataException("locations", CoreErrors.WorldNoPath);
            }

            var random = new SeededWorldRandom(StableHash.Combine(map.RouteSeed, "edge:" + edgeNumber.ToString(CultureInfo.InvariantCulture)));
            var pathTemplate = paths[random.NextInt(0, paths.Count)];
            var count = pathTemplate.ChainLength.Sample(random);
            var previousFront = map.RouteFrontIds;
            var chain = new List<LocationInstance>(count);
            var previous = from;
            var previousTemplate = fromTemplate;

            for (var index = 0; index < count; index++)
            {
                var node = NewInstance(
                    pathTemplate,
                    random,
                    index + 1,
                    edgeNumber,
                    InstanceId(edgeNumber, "path", index));
                map.AddLocation(node);
                var via = PickVia(previousTemplate, pathTemplate, topology, random);
                AddPair(map, previous.InstanceId, node.InstanceId, via, topology, EdgeId(edgeNumber, "path", index));
                if (index == 0)
                {
                    ConnectFront(map, previousFront, node, pathTemplate, edgeNumber, topology, random);
                }

                previous = node;
                previousTemplate = pathTemplate;
                chain.Add(node);
            }

            var branches = BranchCount(map.RouteLastBranches, edgeNumber, topology.Route, random);
            var front = new List<string>(branches);
            for (var index = 0; index < branches; index++)
            {
                var attach = chain[BranchAttachIndex(chain.Count, index)];
                var exit = BuildBranch(map, attach, pathTemplate, edgeNumber, index, topology, random);
                if (exit != null)
                {
                    front.Add(exit.InstanceId);
                }
            }

            map.MarkBranchCount(edgeNumber, branches);
            map.MarkRoute(edgeNumber, previous.InstanceId, branches, front);
            return previous;
        }

        /// <summary>
        /// Связывает выходы предыдущего звена с первой локацией нового: из любой ветки можно перейти
        /// в путь следующего звена, а следующее звено строится одно.
        /// </summary>
        private void ConnectFront(
            WorldMap map,
            IReadOnlyList<string> front,
            LocationInstance target,
            LocationTemplate targetTemplate,
            int edgeNumber,
            TopologyTemplate topology,
            IWorldRandom random)
        {
            for (var index = 0; index < front.Count; index++)
            {
                if (!map.TryGetLocation(front[index], out var source) || source == null)
                {
                    continue;
                }

                var via = PickVia(_locations.Get(source.TemplateId), targetTemplate, topology, random);
                AddPair(map, source.InstanceId, target.InstanceId, via, topology, EdgeId(edgeNumber, "front", index));
            }
        }

        private static string PickVia(
            LocationTemplate from,
            LocationTemplate to,
            TopologyTemplate topology,
            IWorldRandom random)
        {
            var vias = RouteRules.Vias(from, to, topology);
            if (vias.Count == 0)
            {
                throw new DataException(from.Id, CoreErrors.WorldTransitionForbidden, to.Id);
            }

            return vias[random.NextInt(0, vias.Count)];
        }

        private static void AddPair(
            WorldMap map,
            string fromId,
            string toId,
            string via,
            TopologyTemplate topology,
            string edgeId)
        {
            var cost = topology.GetCost(via);
            map.AddEdge(new WorldEdge(edgeId + ".out", fromId, toId, via, 1, cost, false, false));
            map.AddEdge(new WorldEdge(edgeId + ".in", toId, fromId, via, 1, cost, false, false));
        }

        private LocationTemplate RequireHub()
        {
            var hubs = Select(LocationSubtype.Hub);
            if (hubs.Count == 0)
            {
                throw new DataException("world", CoreErrors.WorldNoHub);
            }

            return hubs[0];
        }

        private static LocationInstance NewInstance(
            LocationTemplate template,
            IWorldRandom random,
            int routeStep,
            int routeEdge,
            string instanceId)
        {
            var seed = random.NextInt(1, int.MaxValue);
            var background = template.Backgrounds[random.NextInt(0, template.Backgrounds.Count)];
            return new LocationInstance(
                instanceId,
                template.Id,
                seed,
                background,
                template.Subtype,
                template.Tier,
                template.Danger,
                routeStep,
                routeEdge);
        }

        private static string InstanceId(int edgeNumber, string part, int index) =>
            string.Format(
                CultureInfo.InvariantCulture,
                "loc_e{0}_{1}_{2}",
                edgeNumber,
                part,
                index);

        private static string EdgeId(int edgeNumber, string part, int index) =>
            string.Format(
                CultureInfo.InvariantCulture,
                "edge_e{0}_{1}_{2}",
                edgeNumber,
                part,
                index);

        private List<LocationTemplate> Select(LocationSubtype subtype)
        {
            var result = new List<LocationTemplate>();
            foreach (var template in _locations.All)
            {
                if (template.Subtype == subtype)
                {
                    result.Add(template);
                }
            }

            result.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));
            return result;
        }
    }
}
