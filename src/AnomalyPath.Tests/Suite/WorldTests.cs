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
    /// <summary>Мир: шаблоны, топология, генерация маршрутов, переходы и время.</summary>
    public sealed class WorldTests : TestBase
    {
        private readonly string _dataRoot;

        /// <summary>Создаёт категорию тестов мира.</summary>
        /// <param name="dataRoot">Корень данных проекта.</param>
        public WorldTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        /// <inheritdoc />
        public override void Run()
        {
            var services = Bootstrap.Build(_dataRoot);
            var generator = services.Resolve<IWorldGenerator>();
            var travel = services.Resolve<ITravelService>();
            var locations = services.Resolve<ILocationRepository>();
            var topology = services.Resolve<ITopologyRepository>().Topology;

            TemplateTests();
            GenerationTests(generator, locations, topology);
            MapMutationTests(generator);
            TravelTests(generator, travel);
            ValidationTests();
        }

        private void TemplateTests()
        {
            Check("шаблон: неизвестный тег локации — ошибка схемы", () =>
            {
                var json = LocationJson("loc_x", "wild", "site", "[\"swamp\"]");
                var error = Throws<DataException>(
                    () => LocationTemplate.FromJson(JsonParser.Parse(json, "test"), "test"),
                    "тег");
                Equal(CoreErrors.WorldUnknownLocationTag, error.Code, "код");
                Contains(error.Message, "swamp", "значение");
            });

            Check("схема: способы перехода читаются как enum", () =>
            {
                True(SchemaEnum.TryParse("tunnel", out TravelVia via) && via == TravelVia.Tunnel, "tunnel");
                True(ViaRules.IsKnown("door"), "door известен");
                False(ViaRules.IsKnown("teleport"), "teleport неизвестен");
            });

            Check("шаблон: unique без rarity unique — ошибка", () =>
            {
                var json = LocationJson("loc_x", "wild", "site", "[\"outdoor\"]", ",\"unique\":true");
                var error = Throws<DataException>(() => LocationTemplate.FromJson(JsonParser.Parse(json, "test"), "test"), "unique");
                Equal("error.world_unique_rarity", error.Code, "код");
            });

            Check("схема: коды значений читаются только канонически", () =>
            {
                True(SchemaEnum.TryParse("hub", out LocationKind kind) && kind == LocationKind.Hub, "тип hub");
                True(SchemaEnum.TryParse("interior", out LocationNodeType nodeType) &&
                     nodeType == LocationNodeType.Interior, "роль interior");
                True(SchemaEnum.TryParse("unique", out LocationRarity rarity) &&
                     rarity == LocationRarity.Unique, "редкость unique");
                Equal("special", SchemaEnum.ToCode(LocationKind.Special), "код special");
                False(SchemaEnum.TryParse("Hub", out LocationKind _), "верхний регистр");
                False(SchemaEnum.TryParse("1", out LocationKind _), "число");
                False(SchemaEnum.TryParse("hub,route", out LocationNodeType _), "список значений");
                False(SchemaEnum.TryParse(string.Empty, out LocationKind _), "пустая строка");
            });

            Check("шаблон: неизвестный тип локации — ошибка схемы", () =>
            {
                var json = LocationJson("loc_x", "swamp", "site", "[\"outdoor\"]");
                var error = Throws<DataException>(
                    () => LocationTemplate.FromJson(JsonParser.Parse(json, "test"), "test"),
                    "тип");
                Equal(CoreErrors.WorldUnknownLocationKind, error.Code, "код");
                Contains(error.Message, "swamp", "значение");
            });

            Check("шаблон: неизвестная роль локации — ошибка схемы", () =>
            {
                var json = LocationJson("loc_x", "wild", "bridge", "[\"outdoor\"]");
                var error = Throws<DataException>(
                    () => LocationTemplate.FromJson(JsonParser.Parse(json, "test"), "test"),
                    "роль");
                Equal(CoreErrors.WorldUnknownNodeType, error.Code, "код");
                Contains(error.Message, "bridge", "значение");
            });

            Check("шаблон: неизвестная редкость локации — ошибка схемы", () =>
            {
                var json = LocationJson("loc_x", "wild", "site", "[\"outdoor\"]")
                    .Replace("\"common\"", "\"legendary\"");
                var error = Throws<DataException>(
                    () => LocationTemplate.FromJson(JsonParser.Parse(json, "test"), "test"),
                    "редкость");
                Equal(CoreErrors.WorldUnknownRarity, error.Code, "код");
                Contains(error.Message, "legendary", "значение");
            });

            Check("шаблон: редкость по умолчанию — common", () =>
            {
                var json = LocationJson("loc_x", "wild", "site", "[\"outdoor\"]")
                    .Replace("\"rarity\":\"common\",", string.Empty);
                var template = LocationTemplate.FromJson(JsonParser.Parse(json, "test"), "test");
                Equal(LocationRarity.Common, template.Rarity, "редкость");
            });

            Check("шаблон: пустой id — текст из каталога ресурсов", () =>
            {
                var error = Throws<ArgumentException>(
                    () => new LocationTemplate(
                        string.Empty,
                        "t",
                        LocationKind.Wild,
                        LocationNodeType.Site,
                        Array.Empty<LocationTag>(),
                        0,
                        new[] { "bg" },
                        new IntRange(0, 0),
                        0,
                        Array.Empty<ConnectionTemplate>(),
                        null,
                        Array.Empty<string>(),
                        LocationRarity.Common,
                        1,
                        false),
                    "id");
                Equal("id", error.ParamName, "параметр");
                Contains(error.Message, CoreText.Format(CoreErrors.RequiredField, "id"), "текст");
            });

            Check("шаблон: неопределённая роль — ошибка с текстом из ресурсов", () =>
            {
                var error = Throws<ArgumentException>(
                    () => new LocationTemplate(
                        "loc_x",
                        "t",
                        LocationKind.Wild,
                        (LocationNodeType)42,
                        Array.Empty<LocationTag>(),
                        0,
                        new[] { "bg" },
                        new IntRange(0, 0),
                        0,
                        Array.Empty<ConnectionTemplate>(),
                        null,
                        Array.Empty<string>(),
                        LocationRarity.Common,
                        1,
                        false),
                    "роль");
                Equal("nodeType", error.ParamName, "параметр");
                Contains(error.Message, "42", "значение");
            });

            Check("топология: для способа перехода нужна цена", () =>
            {
                var json = WorldTopologyJson().Replace("\"road\":{\"stamina\":2,\"hours\":1},", string.Empty);
                var error = Throws<DataException>(() => TopologyTemplate.FromJson(JsonParser.Parse(json, "test"), "test"), "цена");
                Equal("error.world_cost", error.Code, "код");
            });

            Check("ребро: length 0 — ошибка", () =>
            {
                var json = LocationJson(
                    "loc_x",
                    "wild",
                    "site",
                    "[\"outdoor\"]",
                    ",\"connections\":[{\"toId\":\"loc_y\",\"via\":\"door\",\"length\":0}]");
                var error = Throws<DataException>(() => LocationTemplate.FromJson(JsonParser.Parse(json, "test"), "test"), "length");
                Equal("error.world_range", error.Code, "код");
            });

            Check("диапазон: min больше max — ошибка", () =>
            {
                var json = "{\"value\":[3,1]}";
                var error = Throws<DataException>(
                    () => IntRange.FromJson(JsonParser.Parse(json, "test"), "value", "test"),
                    "диапазон");
                Equal("error.world_range", error.Code, "код");
            });

            Check("диапазон: выбор попадает в границы", () =>
            {
                var range = new IntRange(1, 3);
                var random = new SeededWorldRandom(7);
                for (var i = 0; i < 20; i++)
                {
                    var value = range.Sample(random);
                    True(value >= 1 && value <= 3, "значение в границах");
                }
            });

            Check("переходы: хаб соединяется с хабом дорогой", () =>
            {
                var tags = new[] { LocationTag.Hub, LocationTag.Road, LocationTag.Outdoor };
                True(ViaRules.Allows("road", LocationNodeType.Hub, LocationNodeType.Hub, tags, tags), "дорога между хабами");
                False(ViaRules.Allows("door", LocationNodeType.Hub, LocationNodeType.Hub, tags, tags), "дверь между хабами");
            });
        }

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
                    LocationNodeType.Site,
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
        }


        private void TravelTests(IWorldGenerator generator, ITravelService travel)
        {
            Check("переход: не списывает выносливость и двигает время", () =>
            {
                var map = generator.Generate(42);
                var state = WorldState.Start(map);
                var edge = FirstPaidEdge(map, state.CurrentLocationId);
                var stamina = state.Stamina;
                var hours = state.Clock.Hours;
                var result = travel.Travel(state, edge.EdgeId);
                True(result.Succeeded, "переход выполнен");
                Equal(edge.ToId, state.CurrentLocationId, "новая локация");
                Equal(stamina, state.Stamina, "выносливость не тратится");
                Equal(hours + edge.Cost.Hours, state.Clock.Hours, "часы");
            });

            Check("переход: доступен без выносливости", () =>
            {
                var map = generator.Generate(42);
                var state = new WorldState(map, map.StartLocationId, 0, map.StartingStamina, new WorldClock(1, 8, 6, 22));
                var edge = FirstPaidEdge(map, state.CurrentLocationId);
                True(travel.CanTravel(state, edge.EdgeId), "CanTravel");
                True(travel.Travel(state, edge.EdgeId).Succeeded, "переход выполнен");
                Equal(0, state.Stamina, "выносливость осталась нулевой");
            });

            Check("переход: из чужой локации запрещён", () =>
            {
                var map = generator.Generate(42);
                var state = WorldState.Start(map);
                var foreign = FirstPaidEdge(map, OfTemplate(map, "loc_site_anomaly")[0].InstanceId);
                Equal(TravelStatus.WrongLocation, travel.Travel(state, foreign.EdgeId).Status, "статус");
            });

            Check("переход: неизвестное ребро", () =>
            {
                var map = generator.Generate(42);
                var state = WorldState.Start(map);
                Equal(TravelStatus.EdgeNotFound, travel.Travel(state, "edge_9999").Status, "статус");
            });

            Check("переход: список видимых рёбер не пуст в хабе", () =>
            {
                var map = generator.Generate(42);
                var state = WorldState.Start(map);
                True(travel.GetAvailableEdges(state).Count > 0, "рёбра из хаба");
            });

            Check("время: переход через полночь меняет день", () =>
            {
                var clock = new WorldClock(1, 23, 6, 22);
                clock.Advance(2);
                Equal(2, clock.Day, "день");
                Equal(1.0, clock.Hours, "часы");
                Equal(TimePhase.Night, clock.Phase, "фаза");
            });

            Check("время: день и ночь считаются от порогов", () =>
            {
                Equal(TimePhase.Day, new WorldClock(1, 6, 6, 22).Phase, "06:00");
                Equal(TimePhase.Night, new WorldClock(1, 22, 6, 22).Phase, "22:00");
                Equal(TimePhase.Night, new WorldClock(1, 0, 6, 22).Phase, "00:00");
            });

            Check("время: FromState отвергает 24 часа", () =>
            {
                Throws<DataException>(() => WorldClock.FromState(1, 24, 6, 22), "24 часа");
            });

            Check("состояние: восстановление не выше максимума", () =>
            {
                var map = generator.Generate(42);
                var state = new WorldState(map, map.StartLocationId, 5, 10, new WorldClock(1, 8, 6, 22));
                state.RestoreStamina(10);
                Equal(10, state.Stamina, "максимум");
                Throws<ArgumentOutOfRangeException>(() => state.RestoreStamina(-1), "отрицательное восстановление");
            });

            Check("состояние: погода меняется", () =>
            {
                var map = generator.Generate(42);
                var state = WorldState.Start(map);
                state.SetWeather(WeatherKind.Fog);
                Equal(WeatherKind.Fog, state.Weather, "погода");
            });

            Check("локация: отметки посещения и зачистки", () =>
            {
                var map = generator.Generate(42);
                var location = OfTemplate(map, "loc_site_house")[0];
                False(location.Visited, "до отметки");
                location.MarkVisited();
                location.MarkCleared();
                True(location.Visited, "посещена");
                True(location.Cleared, "зачищена");
            });

            Check("хаб: последний посещённый хаб обновляется и восстанавливается", () =>
            {
                var map = new WorldMap(
                    1,
                    new List<LocationInstance>
                    {
                        new LocationInstance("hub_a", "loc_hub", 1, "bg", LocationNodeType.Hub, 0, 0),
                        new LocationInstance("hub_b", "loc_hub", 2, "bg", LocationNodeType.Hub, 0, 0),
                        new LocationInstance("route", "loc_route", 3, "bg", LocationNodeType.Route, 1, 0.2)
                    },
                    new List<WorldEdge>(),
                    "hub_a",
                    10,
                    6,
                    22);
                var state = WorldState.Start(map);
                Equal("hub_a", state.LastHubId, "стартовый хаб");

                state.MoveTo("hub_b");
                Equal("hub_b", state.LastHubId, "новый хаб");
                state.MoveTo("route");
                Equal("hub_b", state.LastHubId, "дикая локация хаб не меняет");

                var restored = new WorldState(
                    map, "route", 5, 10, new WorldClock(1, 8, 6, 22), WeatherKind.Clear, "hub_a");
                Equal("hub_a", restored.LastHubId, "хаб из сейва");

                var fallback = new WorldState(map, "route", 5, 10, new WorldClock(1, 8, 6, 22));
                Equal("hub_a", fallback.LastHubId, "старый сейв без хаба");

                Throws<DataException>(
                    () => new WorldState(map, "route", 5, 10, new WorldClock(1, 8, 6, 22), WeatherKind.Clear, "route"),
                    "сохранённый хаб не хаб");
            });

            Check("лут: счётчик бросков растёт и восстанавливается из сейва", () =>
            {
                var map = new WorldMap(
                    1,
                    new List<LocationInstance>
                    {
                        new LocationInstance("hub_a", "loc_hub", 1, "bg", LocationNodeType.Hub, 0, 0)
                    },
                    new List<WorldEdge>(),
                    "hub_a",
                    10,
                    6,
                    22);
                var state = WorldState.Start(map);
                Equal(0, state.LootSerial, "новый мир");
                Equal(1, state.NextLootSerial(), "первый бросок");
                Equal(2, state.NextLootSerial(), "второй бросок");
                Equal(2, state.LootSerial, "счётчик после двух бросков");

                var restored = new WorldState(
                    map, "hub_a", 10, 10, new WorldClock(1, 8, 6, 22), WeatherKind.Clear, null, 7);
                Equal(7, restored.LootSerial, "счётчик из сейва");

                Throws<DataException>(
                    () => new WorldState(
                        map, "hub_a", 10, 10, new WorldClock(1, 8, 6, 22), WeatherKind.Clear, null, -1),
                    "отрицательный счётчик");
            });
        }

        private void ValidationTests()
        {
            Check("валидация: переход на несуществующую локацию", () =>
            {
                WithFixture(WorldFixture(",\"connections\":[{\"toId\":\"loc_missing\",\"via\":\"road\"}]"), root =>
                {
                    var report = Preflight.Validate(root);
                    True(HasIssue(report, "loc_hub", "несуществующую"), "нет ошибки target");
                });
            });

            Check("валидация: запрещённый топологией переход", () =>
            {
                WithFixture(
                    WorldFixture(
                        string.Empty,
                        routeExtra: ",\"connections\":[{\"toId\":\"loc_interior\",\"via\":\"door\"}]"),
                    root =>
                    {
                        var report = Preflight.Validate(root);
                        True(HasIssue(report, "loc_route", "запрещён"), "нет ошибки перехода");
                    });
            });

            Check("валидация: мир без хаба", () =>
            {
                var files = BaseWorldFixture();
                files.Remove("locations/hub.json");
                files["catalog.json"] = Catalog("topology.json", "locations/route.json", "locations/site.json", "locations/interior.json");
                WithFixture(files, root =>
                {
                    var report = Preflight.Validate(root);
                    True(HasIssue(report, "world", "нет ни одного хаба"), "нет ошибки хаба");
                });
            });

            Check("валидация: нет маршрута до точки интереса", () =>
            {
                var files = BaseWorldFixture();
                files["locations/site.json"] = LocationJson("loc_site", "wild", "site", "[\"indoor\"]");
                WithFixture(files, root =>
                {
                    var report = Preflight.Validate(root);
                    True(HasIssue(report, "loc_site", "нельзя построить маршрут"), "нет ошибки маршрута");
                });
            });
        }

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
