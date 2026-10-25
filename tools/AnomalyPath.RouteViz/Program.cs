#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.World;

namespace AnomalyPath.RouteViz
{
    /// <summary>
    /// Инструмент визуализации: гоняет настоящий <see cref="WorldGenerator"/> на данных
    /// <c>Assets/Data</c> и выгружает маршруты в JSON для отрисовки. В игру не входит.
    /// </summary>
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var dataRoot = args.Length > 0 ? args[0] : "Assets/Data";
            var outDir = args.Length > 1 ? args[1] : ".build/route-viz";
            var seed = args.Length > 2 ? ParseInt(args[2], "seed") : 11;

            var specs = new List<(string Name, int Edges)>();
            for (var index = 3; index < args.Length; index++)
            {
                var parts = args[index].Split(':');
                if (parts.Length != 2)
                {
                    Console.Error.WriteLine("спецификация маршрута: имя:число_звеньев");
                    return 1;
                }

                specs.Add((parts[0], ParseInt(parts[1], parts[0])));
            }

            if (specs.Count == 0)
            {
                specs.Add(("small", 3));
                specs.Add(("medium", 6));
                specs.Add(("far", 10));
            }

            var services = Bootstrap.Build(dataRoot);
            var generator = services.Resolve<IWorldGenerator>();
            var templates = services.Resolve<ILocationRepository>();
            var topology = services.Resolve<ITopologyRepository>().Topology;
            Directory.CreateDirectory(outDir);

            foreach (var spec in specs)
            {
                var map = Build(generator, topology.Route.LookaheadEdges, seed, spec.Edges);
                var path = Path.Combine(outDir, "route-" + spec.Name + ".json");
                WriteJson(path, spec.Name, seed, map, templates, topology);
                Console.WriteLine(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}: seed {1}, звеньев {2}, локаций {3}, шагов {4}, веток {5}",
                        spec.Name,
                        seed,
                        map.RouteEdges,
                        map.Locations.Count,
                        MaxStep(map),
                        BranchCounts(map, topology.Route.LookaheadEdges)));
            }

            return 0;
        }

        /// <summary>Строит маршрут заданной длины: первое окно генератора и достройка при «переходе».</summary>
        private static WorldMap Build(IWorldGenerator generator, int lookahead, int seed, int edges)
        {
            var map = generator.Generate(seed);
            var state = WorldState.Start(map);
            generator.BeginRoute(state);
            if (edges <= map.RouteEdges)
            {
                return map;
            }

            var safety = edges + 4;
            while (map.RouteEdges < edges && safety-- > 0)
            {
                // Окно генератора: Extend строит до «текущее звено + окно», поэтому шагаем по одному звену.
                var wanted = Math.Max(1, Math.Min(edges - lookahead, map.RouteEdges - lookahead + 1));
                var node = LastSpineOfEdge(Spine(map), wanted);
                if (node != null && node != state.CurrentLocationId)
                {
                    state.MoveTo(node);
                }

                var before = map.RouteEdges;
                generator.Extend(state);
                if (map.RouteEdges == before)
                {
                    break;
                }
            }

            return map;
        }

        private static void WriteJson(
            string path,
            string name,
            int seed,
            WorldMap map,
            ILocationRepository templates,
            TopologyTemplate topology)
        {
            using var stream = File.Create(path);
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            writer.WriteStartObject();
            writer.WriteString("name", name);
            writer.WriteNumber("seed", seed);
            writer.WriteNumber("lookahead", topology.Route.LookaheadEdges);
            writer.WriteNumber("routeEdges", map.RouteEdges);
            writer.WriteNumber("startingStamina", map.StartingStamina);
            writer.WriteNumber("hubReturnStamina", topology.Route.HubReturn.Stamina);
            writer.WriteNumber("hubReturnHours", topology.Route.HubReturn.Hours);
            writer.WriteNumber("maxStep", MaxStep(map));

            writer.WriteStartArray("branchCounts");
            for (var edge = 1; edge <= map.RouteEdges; edge++)
            {
                writer.WriteNumberValue(map.BranchCount(edge));
            }

            writer.WriteEndArray();

            var locations = new List<LocationInstance>(map.Locations);
            locations.Sort((left, right) => string.CompareOrdinal(left.InstanceId, right.InstanceId));

            writer.WriteStartArray("nodes");
            foreach (var location in locations)
            {
                var template = templates.Get(location.TemplateId);
                writer.WriteStartObject();
                writer.WriteString("id", location.InstanceId);
                writer.WriteString("template", location.TemplateId);
                writer.WriteString("subtype", Subtype(location.Subtype));
                writer.WriteString("role", template.Role);
                writer.WriteString("titleKey", template.TitleKey);
                writer.WriteNumber("tier", location.Tier);
                writer.WriteNumber("danger", location.Danger);
                writer.WriteNumber("step", location.RouteStep);
                writer.WriteNumber("edge", location.RouteEdge);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();

            writer.WriteStartArray("edges");
            foreach (var edge in map.Edges)
            {
                if (string.CompareOrdinal(edge.FromId, edge.ToId) > 0)
                {
                    continue;
                }

                writer.WriteStartObject();
                writer.WriteString("id", edge.EdgeId);
                writer.WriteString("from", edge.FromId);
                writer.WriteString("to", edge.ToId);
                writer.WriteString("via", edge.Via);
                writer.WriteNumber("stamina", edge.Cost.Stamina);
                writer.WriteNumber("hours", edge.Cost.Hours);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        private static List<(int Edge, int Index, string Id)> Spine(WorldMap map)
        {
            var result = new List<(int Edge, int Index, string Id)>();
            foreach (var location in map.Locations)
            {
                var parts = location.InstanceId.Split('_');
                if (parts.Length != 4 ||
                    parts[0] != "loc" ||
                    parts[2] != "path" ||
                    parts[1].Length < 2 ||
                    parts[1][0] != 'e')
                {
                    continue;
                }

                if (int.TryParse(parts[1].Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out var edge) &&
                    int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
                {
                    result.Add((edge, index, location.InstanceId));
                }
            }

            result.Sort((left, right) => left.Edge != right.Edge
                ? left.Edge.CompareTo(right.Edge)
                : left.Index.CompareTo(right.Index));
            return result;
        }

        private static string? LastSpineOfEdge(List<(int Edge, int Index, string Id)> spine, int edge)
        {
            string? result = null;
            foreach (var item in spine)
            {
                if (item.Edge == edge)
                {
                    result = item.Id;
                }
            }

            return result;
        }

        private static int MaxStep(WorldMap map)
        {
            var max = 0;
            foreach (var location in map.Locations)
            {
                if (location.RouteStep > max)
                {
                    max = location.RouteStep;
                }
            }

            return max;
        }

        private static string BranchCounts(WorldMap map, int lookahead)
        {
            var parts = new List<string>();
            for (var edge = 1; edge <= map.RouteEdges; edge++)
            {
                parts.Add(map.BranchCount(edge).ToString(CultureInfo.InvariantCulture));
            }

            return string.Join("/", parts);
        }

        private static string Subtype(LocationSubtype subtype) => subtype.ToString().ToLowerInvariant();

        private static int ParseInt(string text, string what)
        {
            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                throw new ArgumentException("не число: " + what);
            }

            return value;
        }
    }
}
