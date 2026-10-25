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
        private Dictionary<string, string> BaseWorldFixture() =>
            new Dictionary<string, string>
            {
                ["catalog.json"] = Catalog("topology.json", "locations/hub.json", "locations/route.json", "locations/site.json"),
                ["topology.json"] = WorldTopologyJson(),
                ["locations/hub.json"] = LocationJson("loc_hub", "hub", "hub", "[\"hub\",\"outdoor\",\"road\"]"),
                ["locations/route.json"] = LocationJson("loc_route", "wild", "path", "[\"road\",\"outdoor\"]"),
                ["locations/site.json"] = LocationJson("loc_site", "wild", "building", "[\"indoor\",\"outdoor\"]")
            };

        private static string WorldTopologyJson() =>
            "{\"schemaVersion\":1,\"kind\":\"topology\",\"id\":\"topology\",\"route\":{\"transitions\":[" +
            "{\"from\":\"hub\",\"to\":\"path\",\"via\":[\"road\",\"path\"]}," +
            "{\"from\":\"path\",\"to\":\"path\",\"via\":[\"road\",\"path\"]}," +
            "{\"from\":\"path\",\"to\":\"building\",\"via\":[\"path\"]}," +
            "{\"from\":\"building\",\"to\":\"building\",\"via\":[\"corridor\"]}," +
            "{\"from\":\"building\",\"to\":\"path\",\"via\":[\"path\"]}]," +
            "\"lookaheadEdges\":2,\"hubReturn\":{\"stamina\":2,\"hours\":0.5}," +
            "\"branches\":{\"oddRange\":[1,2],\"evenGrowth\":1,\"maxChains\":2}},\"costs\":{" +
            "\"road\":{\"stamina\":2,\"hours\":1},\"path\":{\"stamina\":3,\"hours\":1.5}," +
            "\"corridor\":{\"stamina\":0,\"hours\":0}},\"initialStamina\":10," +
            "\"dayStartHour\":6,\"nightStartHour\":22}";

        private static string LocationJson(
            string id,
            string locationKind,
            string subtype,
            string tags,
            string extra = "")
        {
            var suffix = string.IsNullOrWhiteSpace(extra) ? string.Empty : "," + extra.TrimStart(',');
            return "{\"schemaVersion\":1,\"kind\":\"locations\",\"id\":\"" + id + "\",\"titleKey\":\"t\"," +
                   "\"locationKind\":\"" + locationKind + "\",\"subtype\":\"" + subtype + "\"," +
                   "\"tags\":" + tags + ",\"tier\":0,\"backgrounds\":[\"bg\"],\"objectCount\":[0,0]," +
                   "\"danger\":0,\"chainLength\":[1,1],\"rarity\":\"common\"," +
                   "\"spawnChance\":1,\"unique\":false" + suffix + "}";
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
                if (location.Subtype == LocationSubtype.Path)
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
                if (location.Subtype == LocationSubtype.Path)
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
