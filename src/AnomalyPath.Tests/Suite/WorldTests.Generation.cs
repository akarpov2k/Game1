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
    /// <summary>Часть класса <see cref="WorldTests"/>: генерация маршрута.</summary>
    public sealed partial class WorldTests
    {
        private void GenerationTests(IWorldGenerator generator, ILocationRepository locations, TopologyTemplate topology)
        {
            Check("мир: шаблоны и топология загружены", () =>
            {
                Equal(10, locations.All.Count, "локации");
                Equal(3, topology.Route.LookaheadEdges, "окно звеньев");
                Equal(1, topology.Route.BranchOddRange.Min, "минимум веток на нечётном звене");
                Equal(3, topology.Route.BranchOddRange.Max, "максимум веток на нечётном звене");
                Equal(10, topology.InitialStamina, "стартовая выносливость");
                Equal(2, topology.Route.BranchMaxChains, "цепочек в ветке");
            });

            Check("мир: один seed даёт одну карту", () =>
            {
                var first = generator.Generate(42);
                var second = generator.Generate(42);
                Equal(Signature(first), Signature(second), "сигнатура мира");
            });

            Check("мир: разные seed дают разные карты", () =>
            {
                var first = generator.Generate(1);
                var second = generator.Generate(2);
                True(Signature(first) != Signature(second), "карты должны отличаться");
            });

            Check("мир: старт в хабе, первое звено примыкает к нему", () =>
            {
                var map = generator.Generate(42);
                var state = WorldState.Start(map);
                True(state.CurrentLocation.Subtype == LocationSubtype.Hub, "старт в хабе");
                var paths = OfSubtype(map, LocationSubtype.Path);
                True(paths.Count > 0, "есть локации пути");
                var firstEdge = FindEdge(map, map.StartLocationId, paths[0].InstanceId);
                True(firstEdge != null, "ребро хаб → путь");
                Equal(1, paths[0].RouteEdge, "первое звено");
                Equal(1, paths[0].RouteStep, "первый шаг");
            });

            Check("мир: на каждом звене столько локаций пути, сколько задаёт chainLength", () =>
            {
                var map = generator.Generate(42);
                var perEdge = CountPathPerEdge(map);
                True(perEdge.ContainsKey(1), "звено 1 построено");
                foreach (var pair in perEdge)
                {
                    True(pair.Value >= 1 && pair.Value <= 4, $"звено {pair.Key}: {pair.Value} локаций пути");
                }
            });

            Check("мир: ветки растут по правилу чётных и нечётных звеньев", () =>
            {
                var map = generator.Generate(42);
                var first = map.BranchCount(1);
                True(first >= 1 && first <= 3, $"звено 1: {first} веток");
                Equal(first + 1, map.BranchCount(2), "звено 2: прошлое + 1");
                var third = map.BranchCount(3);
                True(third >= 1 && third <= 3, $"звено 3: {third} веток");
            });

            Check("мир: ветки уходят в здания и местности, а не в путь", () =>
            {
                var map = generator.Generate(42);
                foreach (var edge in map.Edges)
                {
                    var from = map.GetLocation(edge.FromId);
                    var to = map.GetLocation(edge.ToId);
                    if (from.Subtype == LocationSubtype.Path && to.Subtype == LocationSubtype.Hub)
                    {
                        continue;
                    }

                    if (from.Subtype == LocationSubtype.Path &&
                        to.Subtype == LocationSubtype.Path)
                    {
                        True(edge.Via == "road" || edge.Via == "path",
                            $"путь продолжается путём: {edge.Via}");
                    }

                    True(to.Subtype != LocationSubtype.Hub || from.Subtype == LocationSubtype.Path,
                        "в хаб ведёт только путь");
                }
            });

            Check("мир: ветка не длиннее разрешённого числа цепочек", () =>
            {
                var map = generator.Generate(42);
                var maxChain = 0;
                foreach (var template in locations.All)
                {
                    if (template.ChainLength.Range.Max > maxChain)
                    {
                        maxChain = template.ChainLength.Range.Max;
                    }
                }

                var limit = topology.Route.BranchMaxChains * maxChain;
                for (var edge = 1; edge <= map.RouteEdges; edge++)
                {
                    var branches = map.BranchCount(edge);
                    var inBranches = 0;
                    foreach (var location in map.Locations)
                    {
                        if (location.RouteEdge == edge &&
                            (location.Subtype == LocationSubtype.Building || location.Subtype == LocationSubtype.Terrain))
                        {
                            inBranches++;
                        }
                    }

                    True(inBranches <= branches * limit,
                        $"звено {edge}: {inBranches} локаций в {branches} ветках при пределе {branches * limit}");
                }
            });

            Check("мир: здание ведёт в здание коридором, помещения не строятся", () =>
            {
                var found = false;
                for (var seed = 1; seed <= 40 && !found; seed++)
                {
                    var map = generator.Generate(seed);
                    foreach (var edge in map.Edges)
                    {
                        var from = map.GetLocation(edge.FromId);
                        var to = map.GetLocation(edge.ToId);
                        if (from.Subtype != LocationSubtype.Building || to.Subtype != LocationSubtype.Building)
                        {
                            continue;
                        }

                        Equal("corridor", edge.Via, "здание → здание идёт коридором");
                        found = true;
                        break;
                    }
                }

                True(found, "нашлась пара зданий подряд");
            });

            Check("мир: шаг считается внутри звена", () =>
            {
                var map = generator.Generate(42);
                var steps = StepsPerEdge(map);
                True(steps.Count > 0, "звенья построены");
                foreach (var pair in steps)
                {
                    pair.Value.Sort();
                    Equal(1, pair.Value[0], $"звено {pair.Key}: первый шаг");
                    for (var index = 0; index < pair.Value.Count; index++)
                    {
                        Equal(index + 1, pair.Value[index], $"звено {pair.Key}: шаги подряд с единицы");
                    }
                }
            });

            Check("мир: ветки крепятся к разным локациям звена", () =>
            {
                var map = generator.Generate(42);
                var pathPerEdge = CountPathPerEdge(map);
                foreach (var pair in pathPerEdge)
                {
                    var branches = map.BranchCount(pair.Key);
                    True(branches >= 1, $"звено {pair.Key}: ветка есть");
                    var attach = new HashSet<string>(StringComparer.Ordinal);
                    foreach (var location in map.Locations)
                    {
                        if (location.Subtype != LocationSubtype.Path || location.RouteEdge != pair.Key)
                        {
                            continue;
                        }

                        foreach (var edge in map.EdgesFrom(location.InstanceId))
                        {
                            var target = map.GetLocation(edge.ToId);
                            if (target.RouteEdge == pair.Key &&
                                target.Subtype != LocationSubtype.Path)
                            {
                                attach.Add(location.InstanceId);
                            }
                        }
                    }

                    Equal(
                        Math.Min(branches, pair.Value),
                        attach.Count,
                        $"звено {pair.Key}: локаций с ветками при {branches} ветках и {pair.Value} локациях");
                }
            });

            Check("мир: конец звена связан с веткой и следующим звеном", () =>
            {
                var map = generator.Generate(42);
                var steps = StepsPerEdge(map);
                foreach (var pair in steps)
                {
                    var tail = Tail(map, pair.Key);
                    var branch = false;
                    var next = false;
                    foreach (var edge in map.EdgesFrom(tail.InstanceId))
                    {
                        var target = map.GetLocation(edge.ToId);
                        if (target.RouteEdge == pair.Key &&
                            target.Subtype != LocationSubtype.Path)
                        {
                            branch = true;
                        }

                        if (target.RouteEdge == pair.Key + 1 && target.Subtype == LocationSubtype.Path)
                        {
                            next = true;
                        }
                    }

                    True(branch, $"звено {pair.Key}: у конца звена есть ветка");
                    if (pair.Key < map.RouteEdges)
                    {
                        True(next, $"звено {pair.Key}: конец звена связан со следующим");
                    }
                }
            });

            Check("мир: конец каждой ветки ведёт в следующее звено", () =>
            {
                var map = generator.Generate(42);
                for (var edge = 1; edge < map.RouteEdges; edge++)
                {
                    foreach (var location in map.Locations)
                    {
                        if (location.RouteEdge != edge ||
                            location.Subtype == LocationSubtype.Path ||
                            location.Subtype == LocationSubtype.Hub)
                        {
                            continue;
                        }

                        True(
                            ReachesEdge(map, location, edge + 1),
                            $"{location.InstanceId}: ветка звена {edge} не ведёт в звено {edge + 1}");
                    }
                }
            });

            Check("мир: достройка связывает выходы прошлого звена с новым", () =>
            {
                var map = generator.Generate(42);
                var front = new List<string>(map.RouteFrontIds);
                True(front.Count > 0, "выходы прошлого звена сохранены");
                var before = map.RouteEdges;
                var state = WorldState.Start(map);
                state.MoveTo(Tail(map, before).InstanceId);

                generator.Extend(state);

                True(map.RouteEdges > before, "звенья достроены");
                foreach (var id in front)
                {
                    if (!map.ContainsLocation(id))
                    {
                        continue;
                    }

                    var linked = false;
                    foreach (var edge in map.EdgesFrom(id))
                    {
                        var target = map.GetLocation(edge.ToId);
                        if (target.Subtype == LocationSubtype.Path && target.RouteEdge > before)
                        {
                            linked = true;
                        }
                    }

                    True(linked, $"{id}: выход связан с новым звеном");
                }
            });

            Check("мир: достройка маршрута добавляет звенья и повторно ничего не меняет", () =>
            {
                var map = generator.Generate(42);
                var state = WorldState.Start(map);
                var before = CountPathPerEdge(map);
                generator.Extend(state);
                var afterFirst = CountPathPerEdge(map);
                generator.Extend(state);
                var afterSecond = CountPathPerEdge(map);
                Equal(map.RouteEdges, afterSecond.Count, "все звенья посчитаны");
                True(afterFirst.Count >= before.Count, "достройка не убирает звенья");
                Equal(afterFirst.Count, afterSecond.Count, "повторная достройка ничего не добавляет");
            });

            Check("мир: достройка детерминирована seed", () =>
            {
                var firstMap = generator.Generate(7);
                var secondMap = generator.Generate(7);
                generator.Extend(WorldState.Start(firstMap));
                generator.Extend(WorldState.Start(secondMap));
                Equal(Signature(firstMap), Signature(secondMap), "маршруты совпадают");
            });
        }

        private static Dictionary<int, int> CountPathPerEdge(WorldMap map)
        {
            var result = new Dictionary<int, int>();
            foreach (var location in map.Locations)
            {
                if (location.Subtype != LocationSubtype.Path)
                {
                    continue;
                }

                result.TryGetValue(location.RouteEdge, out var count);
                result[location.RouteEdge] = count + 1;
            }

            return result;
        }

        private static Dictionary<int, List<int>> StepsPerEdge(WorldMap map)
        {
            var result = new Dictionary<int, List<int>>();
            foreach (var location in map.Locations)
            {
                if (location.Subtype != LocationSubtype.Path)
                {
                    continue;
                }

                if (!result.TryGetValue(location.RouteEdge, out var steps))
                {
                    steps = new List<int>();
                    result[location.RouteEdge] = steps;
                }

                steps.Add(location.RouteStep);
            }

            return result;
        }

        private static LocationInstance Tail(WorldMap map, int edgeNumber)
        {
            LocationInstance? tail = null;
            foreach (var location in map.Locations)
            {
                if (location.Subtype == LocationSubtype.Path &&
                    location.RouteEdge == edgeNumber &&
                    (tail == null || location.RouteStep > tail.RouteStep))
                {
                    tail = location;
                }
            }

            return tail ?? throw new InvalidOperationException("Нет локации пути в звене " + edgeNumber);
        }

        private static bool ReachesEdge(WorldMap map, LocationInstance start, int edgeNumber)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal) { start.InstanceId };
            var queue = new Queue<LocationInstance>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var edge in map.EdgesFrom(current.InstanceId))
                {
                    var target = map.GetLocation(edge.ToId);
                    if (target.Subtype == LocationSubtype.Path)
                    {
                        if (target.RouteEdge == edgeNumber)
                        {
                            return true;
                        }

                        continue;
                    }

                    if (target.RouteEdge != current.RouteEdge)
                    {
                        continue;
                    }

                    if (seen.Add(target.InstanceId))
                    {
                        queue.Enqueue(target);
                    }
                }
            }

            return false;
        }

        private static List<LocationInstance> OfSubtype(WorldMap map, LocationSubtype subtype)
        {
            var result = new List<LocationInstance>();
            foreach (var location in map.Locations)
            {
                if (location.Subtype == subtype)
                {
                    result.Add(location);
                }
            }

            return result;
        }
    }
}
