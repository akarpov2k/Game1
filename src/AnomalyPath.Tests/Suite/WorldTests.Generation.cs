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
    /// <summary>Часть класса <see cref="WorldTests"/>: генерация карты.</summary>
    public sealed partial class WorldTests
    {
        private void GenerationTests(IWorldGenerator generator, ILocationRepository locations, TopologyTemplate topology)
        {
            Check("мир: шаблоны и топология загружены", () =>
            {
                Equal(6, locations.All.Count, "локации");
                Equal(1, topology.RouteLength.Min, "минимум отрезков");
                Equal(2, topology.EdgeLength.Max, "максимум длины ребра");
                Equal(10, topology.InitialStamina, "стартовая выносливость");
            });

            Check("мир: один seed даёт одну карту", () =>
            {
                var first = generator.Generate(42);
                var second = generator.Generate(42);
                Equal(Signature(first), Signature(second), "сигнатура мира");
            });

            Check("мир: разные seed дают разные маршруты", () =>
            {
                var first = generator.Generate(1);
                var second = generator.Generate(2);
                True(Signature(first) != Signature(second), "карты должны отличаться");
            });

            Check("мир: маршрут есть до каждой точки интереса", () =>
            {
                var map = generator.Generate(42);
                foreach (var site in OfTemplate(map, "loc_site_house"))
                {
                    True(PathTo(map, site.InstanceId).Count > 0, "путь до дома");
                }

                foreach (var site in OfTemplate(map, "loc_site_anomaly"))
                {
                    True(PathTo(map, site.InstanceId).Count > 0, "путь до аномалии");
                }
            });

            Check("мир: число отрезков берётся из approachLength", () =>
            {
                var map = generator.Generate(42);
                var house = OfTemplate(map, "loc_site_house")[0];
                var anomaly = OfTemplate(map, "loc_site_anomaly")[0];
                var houseRoutes = CountRoutes(PathTo(map, house.InstanceId));
                var anomalyRoutes = CountRoutes(PathTo(map, anomaly.InstanceId));
                True(houseRoutes >= 1 && houseRoutes <= 2, $"дом: {houseRoutes}");
                True(anomalyRoutes >= 2 && anomalyRoutes <= 3, $"аномалия: {anomalyRoutes}");
            });

            Check("мир: у разных локаций разные маршруты", () =>
            {
                var map = generator.Generate(42);
                var house = OfTemplate(map, "loc_site_house")[0];
                var anomaly = OfTemplate(map, "loc_site_anomaly")[0];
                var houseRoutes = RouteIds(PathTo(map, house.InstanceId));
                var anomalyRoutes = RouteIds(PathTo(map, anomaly.InstanceId));
                foreach (var id in houseRoutes)
                {
                    False(anomalyRoutes.Contains(id), "маршруты не должны пересекаться");
                }
            });

            Check("мир: длина ребра в вилке и цена считается по длине", () =>
            {
                var map = generator.Generate(42);
                foreach (var edge in map.Edges)
                {
                    True(edge.Length >= topology.EdgeLength.Min && edge.Length <= topology.EdgeLength.Max, "длина ребра");
                    var baseCost = topology.GetCost(edge.Via);
                    Equal(baseCost.Stamina * edge.Length, edge.Cost.Stamina, "выносливость ребра");
                    Equal(baseCost.Hours * edge.Length, edge.Cost.Hours, "часы ребра");
                }
            });

            Check("мир: длина ребра меняется от seed", () =>
            {
                var lengths = new HashSet<int>();
                for (var seed = 1; seed <= 20; seed++)
                {
                    foreach (var edge in generator.Generate(seed).Edges)
                    {
                        lengths.Add(edge.Length);
                    }
                }

                True(lengths.Contains(1), "должна встречаться длина 1");
                True(lengths.Contains(2), "должна встречаться длина 2");
            });

            Check("мир: внутрь дома ведёт дверь", () =>
            {
                var map = generator.Generate(42);
                var house = OfTemplate(map, "loc_site_house")[0];
                var interior = OfTemplate(map, "loc_interior_room")[0];
                var edge = FindEdge(map, house.InstanceId, interior.InstanceId);
                True(edge != null, "ребро дом → комната");
                Equal("door", edge!.Via, "способ перехода");
                True(FindEdge(map, interior.InstanceId, house.InstanceId) != null, "обратная дверь");
            });

            Check("мир: старт в хабе с полной выносливостью", () =>
            {
                var map = generator.Generate(42);
                var state = WorldState.Start(map);
                Equal(map.StartLocationId, state.CurrentLocationId, "стартовая локация");
                Equal(map.StartingStamina, state.Stamina, "выносливость");
                True(state.CurrentLocation.NodeType == LocationNodeType.Hub, "старт в хабе");
            });
        }
    }
}
