#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.World;

namespace AnomalyPath.Tests
{
    /// <summary>Часть класса <see cref="WorldTests"/>: выход из хаба и обрезка пройденного маршрута.</summary>
    public sealed partial class WorldTests
    {
        private void DepartureTests(IWorldGenerator generator)
        {
            Check("маршрут: выход из хаба строит новое окно и нумерует выходы", () =>
            {
                var map = generator.Generate(42);
                var state = WorldState.Start(map);
                Equal(0, map.RouteSerial, "номер до выхода");
                var before = Signature(map);

                generator.BeginRoute(state);

                Equal(1, map.RouteSerial, "первый выход");
                Equal(3, map.RouteEdges, "окно звеньев");
                True(Signature(map) != before, "маршрут перестроен");
                var first = FirstPathOfEdge(map, 1);
                True(first != null, "первое звено от хаба");
                Equal(1, first!.RouteStep, "первый шаг звена");
            });

            Check("маршрут: один seed и один выход дают один маршрут", () =>
            {
                var first = generator.Generate(42);
                var second = generator.Generate(42);
                generator.BeginRoute(WorldState.Start(first));
                generator.BeginRoute(WorldState.Start(second));

                Equal(Signature(first), Signature(second), "маршруты совпадают");
            });

            Check("маршрут: каждый выход из хаба даёт другой маршрут", () =>
            {
                var map = generator.Generate(42);
                var state = WorldState.Start(map);
                generator.BeginRoute(state);
                var first = RouteSignature(map);

                generator.BeginRoute(state);

                Equal(2, map.RouteSerial, "второй выход");
                True(RouteSignature(map) != first, "маршрут должен отличаться");
            });

            Check("маршрут: новый выход снимает прежние звенья, хаб остаётся", () =>
            {
                var map = generator.Generate(42);
                var state = WorldState.Start(map);
                var old = new LocationInstance("locinst_old", "loc_path", 99, "bg", LocationSubtype.Path, 0, 0, 1, 3);
                map.AddLocation(old);
                True(map.ContainsLocation(old.InstanceId), "старая локация была");

                generator.BeginRoute(state);

                False(map.ContainsLocation(old.InstanceId), "старая локация снята");
                True(map.ContainsLocation(map.StartLocationId), "хаб на месте");
                Equal(3, map.RouteEdges, "окно звеньев построено заново");
            });

            Check("маршрут: выйти в новый маршрут можно только из хаба", () =>
            {
                var map = generator.Generate(42);
                var state = WorldState.Start(map);
                var path = OfSubtype(map, LocationSubtype.Path)[0];
                state.MoveTo(path.InstanceId);

                Throws<DataException>(() => generator.BeginRoute(state), "не из хаба");
            });
        }

        private void PruneTests()
        {
            Check("маршрут: пройденное и недоступные ветки снимаются, хаб и будущее остаются", () =>
            {
                var map = PruneFixture();
                var current = map.GetLocation("locinst_now");

                var removed = map.PrunePassed(current.InstanceId);

                Equal(2, removed, "снято локаций");
                False(map.ContainsLocation("locinst_back"), "пройденный шаг снят");
                False(map.ContainsLocation("locinst_branch"), "недоступная ветка снята");
                True(map.ContainsLocation("locinst_hub"), "хаб остался");
                True(map.ContainsLocation("locinst_now"), "текущая локация осталась");
                True(map.ContainsLocation("locinst_ahead"), "следующее звено осталось");
                True(map.ContainsLocation("locinst_quest"), "цель квеста не трогается");
                False(map.TryGetEdge("edge_hub_back", out _), "ребро к пройденному шагу снято");
            });

            Check("маршрут: обрезка повторно ничего не меняет", () =>
            {
                var map = PruneFixture();
                map.PrunePassed("locinst_now");
                var count = map.Locations.Count;

                Equal(0, map.PrunePassed("locinst_now"), "снимать нечего");
                Equal(count, map.Locations.Count, "локации не изменились");
            });

            Check("маршрут: ребро между пройденной и текущей локацией удаляется в обе стороны", () =>
            {
                var map = PruneFixture();

                Equal(2, map.RemoveEdgesBetween("locinst_back", "locinst_now"), "оба направления");
                False(map.TryGetEdge("edge_back_now", out _), "прямое ребро");
                False(map.TryGetEdge("edge_now_back", out _), "обратное ребро");
            });
        }

        private static WorldMap PruneFixture()
        {
            var hub = new LocationInstance("locinst_hub", "loc_hub", 1, "bg", LocationSubtype.Hub, 0, 0, 0, 0);
            var back = new LocationInstance("locinst_back", "loc_path", 2, "bg", LocationSubtype.Path, 0, 0, 1, 1);
            var current = new LocationInstance("locinst_now", "loc_path", 3, "bg", LocationSubtype.Path, 0, 0, 2, 1);
            var branch = new LocationInstance("locinst_branch", "loc_building", 4, "bg", LocationSubtype.Building, 0, 0, 1, 1);
            var ahead = new LocationInstance("locinst_ahead", "loc_path", 6, "bg", LocationSubtype.Path, 0, 0, 1, 2);
            var quest = new LocationInstance("locinst_quest", "loc_building", 7, "bg", LocationSubtype.Building, 0, 0);

            var locations = new List<LocationInstance> { hub, back, current, branch, ahead, quest };
            var edges = new List<WorldEdge>
            {
                Edge("edge_hub_back", hub, back),
                Edge("edge_back_branch", back, branch),
                Edge("edge_back_now", back, current),
                Edge("edge_now_back", current, back),
                Edge("edge_now_ahead", current, ahead)
            };

            return new WorldMap(42, locations, edges, hub.InstanceId, 10, 6, 22);
        }

        private static WorldEdge Edge(string id, LocationInstance from, LocationInstance to) =>
            new WorldEdge(id, from.InstanceId, to.InstanceId, "path", 1, new TravelCost(2, 1), false, false);

        private static LocationInstance? FirstPathOfEdge(WorldMap map, int edge)
        {
            foreach (var location in map.Locations)
            {
                if (location.Subtype == LocationSubtype.Path && location.RouteEdge == edge)
                {
                    return location;
                }
            }

            return null;
        }

        private static string RouteSignature(WorldMap map)        {
            var parts = new List<string>();
            foreach (var location in map.Locations)
            {
                if (location.RouteEdge >= 1)
                {
                    parts.Add($"{location.InstanceId}:{location.TemplateId}:{location.Seed}");
                }
            }

            parts.Sort(StringComparer.Ordinal);
            return string.Join("|", parts);
        }
    }
}
