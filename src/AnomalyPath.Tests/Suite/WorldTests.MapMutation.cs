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
    /// <summary>Часть класса <see cref="WorldTests"/>: изменение карты.</summary>
    public sealed partial class WorldTests
    {
        private void MapMutationTests(IWorldGenerator generator)
        {
            Check("карта: локация и ребро добавляются во время игры", () =>
            {
                var map = generator.Generate(42);
                var hub = map.StartLocationId;
                var location = new LocationInstance(
                    "locinst_test",
                    "loc_site_house",
                    7,
                    "bg_site_house",
                    LocationSubtype.Building,
                    1,
                    0.2);
                map.AddLocation(location);
                True(map.ContainsLocation("locinst_test"), "локация добавлена");
                var edge = new WorldEdge(
                    "edge_test",
                    hub,
                    "locinst_test",
                    "path",
                    1,
                    new TravelCost(3, 1.5),
                    oneWay: false,
                    hidden: false);
                map.AddEdge(edge);
                var found = false;
                foreach (var candidate in map.EdgesFrom(hub))
                {
                    if (candidate.EdgeId == "edge_test")
                    {
                        found = true;
                    }
                }

                True(found, "ребро добавлено");
                Throws<DataException>(() => map.AddLocation(location), "дубль локации");
                Throws<DataException>(() => map.AddEdge(edge), "дубль ребра");
            });

            Check("карта: пройденная локация удаляется вместе с рёбрами", () =>
            {
                var map = generator.Generate(42);
                var location = new LocationInstance(
                    "locinst_gone",
                    "loc_site_house",
                    7,
                    "bg_site_house",
                    LocationSubtype.Building,
                    1,
                    0.2);
                map.AddLocation(location);
                map.AddEdge(new WorldEdge(
                    "edge_gone",
                    map.StartLocationId,
                    "locinst_gone",
                    "path",
                    1,
                    new TravelCost(3, 1.5),
                    oneWay: false,
                    hidden: false));

                map.RemoveLocation("locinst_gone");
                False(map.ContainsLocation("locinst_gone"), "локация удалена");
                False(map.TryGetEdge("edge_gone", out _), "ребро удалено");
                Throws<DataException>(() => map.RemoveLocation("locinst_gone"), "повторное удаление");
            });
        }
    }
}
