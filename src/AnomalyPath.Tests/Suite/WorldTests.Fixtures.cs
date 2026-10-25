#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.World;

namespace AnomalyPath.Tests
{
    /// <summary>Часть класса <see cref="WorldTests"/>: фикстуры и помощники.</summary>
    public sealed partial class WorldTests
    {
        private Dictionary<string, string> WorldFixture(string hubExtra = "", string routeExtra = "")
        {
            var files = BaseWorldFixture();
            if (!string.IsNullOrWhiteSpace(hubExtra))
            {
                files["locations/hub.json"] = LocationJson("loc_hub", "hub", "hub", "[\"hub\",\"outdoor\",\"road\"]", hubExtra);
            }

            if (!string.IsNullOrWhiteSpace(routeExtra))
            {
                files["locations/route.json"] = LocationJson("loc_route", "wild", "route", "[\"road\",\"outdoor\"]", routeExtra);
            }

            return files;
        }

        private Dictionary<string, string> BaseWorldFixture() =>
            new Dictionary<string, string>
            {
                ["catalog.json"] = Catalog("topology.json", "locations/hub.json", "locations/route.json", "locations/site.json", "locations/interior.json"),
                ["topology.json"] = WorldTopologyJson(),
                ["locations/hub.json"] = LocationJson("loc_hub", "hub", "hub", "[\"hub\",\"outdoor\",\"road\"]"),
                ["locations/route.json"] = LocationJson("loc_route", "wild", "route", "[\"road\",\"outdoor\"]"),
                ["locations/site.json"] = LocationJson(
                    "loc_site",
                    "wild",
                    "site",
                    "[\"outdoor\"]",
                    ",\"approachLength\":[1,1],\"connections\":[{\"toId\":\"loc_interior\",\"via\":\"door\"}]"),
                ["locations/interior.json"] = LocationJson("loc_interior", "special", "interior", "[\"indoor\"]")
            };

        private static string WorldTopologyJson() =>
            "{\"schemaVersion\":1,\"kind\":\"topology\",\"id\":\"topology\"," +
            "\"transitions\":[" +
            "{\"from\":\"hub\",\"to\":\"route\",\"via\":[\"road\",\"path\"]}," +
            "{\"from\":\"route\",\"to\":\"route\",\"via\":[\"road\",\"path\"]}," +
            "{\"from\":\"route\",\"to\":\"site\",\"via\":[\"path\"]}," +
            "{\"from\":\"site\",\"to\":\"interior\",\"via\":[\"door\"]}]," +
            "\"routeLength\":[1,2],\"edgeLength\":[1,2],\"costs\":{" +
            "\"road\":{\"stamina\":2,\"hours\":1},\"path\":{\"stamina\":3,\"hours\":1.5}," +
            "\"door\":{\"stamina\":0,\"hours\":0}},\"maxBranchDepth\":4,\"initialStamina\":10," +
            "\"dayStartHour\":6,\"nightStartHour\":22}";

        private static string LocationJson(
            string id,
            string locationKind,
            string nodeType,
            string tags,
            string extra = "")
        {
            var suffix = string.IsNullOrWhiteSpace(extra) ? string.Empty : "," + extra.TrimStart(',');
            return "{\"schemaVersion\":1,\"kind\":\"locations\",\"id\":\"" + id + "\",\"titleKey\":\"t\"," +
                   "\"locationKind\":\"" + locationKind + "\",\"nodeType\":\"" + nodeType + "\"," +
                   "\"tags\":" + tags + ",\"tier\":0,\"backgrounds\":[\"bg\"],\"objectCount\":[0,0]," +
                   "\"danger\":0,\"connections\":[],\"rarity\":\"common\",\"spawnChance\":1,\"unique\":false" + suffix + "}";
        }

        private static string Signature(WorldMap map)
        {
            var parts = new List<string>();
            foreach (var location in map.Locations)
            {
                parts.Add($"{location.InstanceId}:{location.TemplateId}:{location.Seed}:{location.Background}");
            }

            foreach (var edge in map.Edges)
            {
                parts.Add($"{edge.FromId}>{edge.ToId}:{edge.Via}:{edge.Length}");
            }

            return string.Join("|", parts);
        }

        private static List<LocationInstance> OfTemplate(WorldMap map, string templateId)
        {
            var result = new List<LocationInstance>();
            foreach (var location in map.Locations)
            {
                if (location.TemplateId == templateId)
                {
                    result.Add(location);
                }
            }

            return result;
        }

        private static List<LocationInstance> PathTo(WorldMap map, string targetId)
        {
            var parents = new Dictionary<string, string>(StringComparer.Ordinal);
            var queue = new Queue<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal) { map.StartLocationId };
            queue.Enqueue(map.StartLocationId);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == targetId)
                {
                    break;
                }

                foreach (var edge in map.EdgesFrom(current))
                {
                    if (seen.Add(edge.ToId))
                    {
                        parents[edge.ToId] = current;
                        queue.Enqueue(edge.ToId);
                    }
                }
            }

            var path = new List<LocationInstance>();
            if (!parents.ContainsKey(targetId))
            {
                return path;
            }

            var node = targetId;
            while (node != map.StartLocationId)
            {
                path.Add(map.GetLocation(node));
                node = parents[node];
            }

            path.Add(map.GetLocation(map.StartLocationId));
            path.Reverse();
            return path;
        }

        private static int CountRoutes(IReadOnlyList<LocationInstance> path)
        {
            var count = 0;
            foreach (var location in path)
            {
                if (location.NodeType == LocationNodeType.Route)
                {
                    count++;
                }
            }

            return count;
        }

        private static List<string> RouteIds(IReadOnlyList<LocationInstance> path)
        {
            var result = new List<string>();
            foreach (var location in path)
            {
                if (location.NodeType == LocationNodeType.Route)
                {
                    result.Add(location.InstanceId);
                }
            }

            return result;
        }

        private static WorldEdge? FindEdge(WorldMap map, string fromId, string toId)
        {
            foreach (var edge in map.EdgesFrom(fromId))
            {
                if (edge.ToId == toId)
                {
                    return edge;
                }
            }

            return null;
        }

        private static WorldEdge FirstPaidEdge(WorldMap map, string fromId)
        {
            foreach (var edge in map.EdgesFrom(fromId))
            {
                if (edge.Cost.Stamina > 0)
                {
                    return edge;
                }
            }

            throw new InvalidOperationException("Нет платного ребра из " + fromId);
        }
    }
}
