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
    /// <summary>Часть класса <see cref="WorldTests"/>: шаблоны мира.</summary>
    public sealed partial class WorldTests
    {
        private void TemplateTests()
        {
            Check("веса: минимум диапазона выпадает реже середины", () =>
            {
                var range = new WeightedRange(
                    new IntRange(1, 3),
                    new Dictionary<int, double> { [1] = 0.1, [2] = 0.45, [3] = 0.45 });
                var random = new SeededWorldRandom(11);
                var counts = new int[4];
                for (var i = 0; i < 4000; i++)
                {
                    counts[range.Sample(random)]++;
                }

                True(counts[1] < counts[2], $"минимум {counts[1]} против середины {counts[2]}");
                True(counts[1] < counts[3], $"минимум {counts[1]} против верха {counts[3]}");
                True(counts[2] > 0 && counts[3] > 0, "середина и верх выпадают");
            });

            Check("веса: без весов выбор равномерный", () =>
            {
                var range = new WeightedRange(new IntRange(1, 2));
                var random = new SeededWorldRandom(5);
                var counts = new int[3];
                for (var i = 0; i < 400; i++)
                {
                    counts[range.Sample(random)]++;
                }

                True(counts[1] > 100 && counts[2] > 100, "оба значения встречаются");
            });

            Check("веса: неполное покрытие диапазона — ошибка", () =>
            {
                var json = "{\"chainLength\":[1,3],\"chainWeights\":{\"1\":0.5,\"2\":0.5}}";
                var error = Throws<DataException>(
                    () => WeightedRange.Optional(
                        JsonParser.Parse(json, "test"),
                        "chainLength",
                        "chainWeights",
                        "test",
                        new IntRange(1, 1)),
                    "веса");
                Equal(CoreErrors.WorldWeights, error.Code, "код");
            });

            Check("веса: значение вне диапазона — ошибка", () =>
            {
                var json = "{\"chainLength\":[1,2],\"chainWeights\":{\"1\":0.5,\"2\":0.4,\"3\":0.1}}";
                var error = Throws<DataException>(
                    () => WeightedRange.Optional(
                        JsonParser.Parse(json, "test"),
                        "chainLength",
                        "chainWeights",
                        "test",
                        new IntRange(1, 1)),
                    "веса");
                Equal(CoreErrors.WorldWeights, error.Code, "код");
            });

            Check("топология: без блока route — ошибка", () =>
            {
                var json = "{\"id\":\"topology\",\"costs\":{\"road\":{\"stamina\":1,\"hours\":1}}," +
                           "\"initialStamina\":10,\"dayStartHour\":6,\"nightStartHour\":22}";
                Throws<DataException>(
                    () => TopologyTemplate.FromJson(JsonParser.Parse(json, "test"), "test"),
                    "route");
            });

            Check("топология: окно вперёд не меньше звена", () =>
            {
                var json = WorldTopologyJson().Replace("\"lookaheadEdges\":2", "\"lookaheadEdges\":0");
                var error = Throws<DataException>(
                    () => TopologyTemplate.FromJson(JsonParser.Parse(json, "test"), "test"),
                    "окно");
                Equal(CoreErrors.WorldRange, error.Code, "код");
            });

            Check("топология: роль перехода должна встречаться в данных", () =>
            {
                var files = BaseWorldFixture();
                files["topology.json"] = WorldTopologyJson()
                    .Replace("\"to\":\"building\",\"via\":[\"path\"]", "\"to\":\"building\",\"toRole\":\"laboratory\",\"via\":[\"path\"]");
                WithFixture(files, root =>
                {
                    var report = Preflight.Validate(root);
                    True(HasIssue(report, "topology", "не встречается"), "нет ошибки роли");
                });
            });

            Check("шаблон: неизвестный тег локации — ошибка схемы", () =>
            {
                var json = LocationJson("loc_x", "wild", "building", "[\"swamp\"]");
                var error = Throws<DataException>(
                    () => LocationTemplate.FromJson(JsonParser.Parse(json, "test"), "test"),
                    "тег");
                Equal(CoreErrors.WorldUnknownLocationTag, error.Code, "код");
                Contains(error.Message, "swamp", "значение");
            });

            Check("схема: способы перехода читаются как enum", () =>
            {
                True(SchemaEnum.TryParse("tunnel", out TravelVia via) && via == TravelVia.Tunnel, "tunnel");
                True(ViaRules.IsKnown("corridor"), "corridor известен");
                False(ViaRules.IsKnown("door"), "дверь больше не способ перехода");
                False(ViaRules.IsKnown("teleport"), "teleport неизвестен");
            });

            Check("шаблон: unique без rarity unique — ошибка", () =>
            {
                var json = LocationJson("loc_x", "wild", "building", "[\"outdoor\"]", ",\"unique\":true");
                var error = Throws<DataException>(() => LocationTemplate.FromJson(JsonParser.Parse(json, "test"), "test"), "unique");
                Equal("error.world_unique_rarity", error.Code, "код");
            });

            Check("схема: коды значений читаются только канонически", () =>
            {
                True(SchemaEnum.TryParse("hub", out LocationKind kind) && kind == LocationKind.Hub, "тип hub");
                True(SchemaEnum.TryParse("terrain", out LocationSubtype nodeType) &&
                     nodeType == LocationSubtype.Terrain, "подтип terrain");
                False(SchemaEnum.TryParse("interior", out LocationSubtype _), "помещений больше нет");
                True(SchemaEnum.TryParse("unique", out LocationRarity rarity) &&
                     rarity == LocationRarity.Unique, "редкость unique");
                Equal("special", SchemaEnum.ToCode(LocationKind.Special), "код special");
                False(SchemaEnum.TryParse("Hub", out LocationKind _), "верхний регистр");
                False(SchemaEnum.TryParse("1", out LocationKind _), "число");
                False(SchemaEnum.TryParse("hub,route", out LocationSubtype _), "список значений");
                False(SchemaEnum.TryParse(string.Empty, out LocationKind _), "пустая строка");
            });

            Check("шаблон: неизвестный тип локации — ошибка схемы", () =>
            {
                var json = LocationJson("loc_x", "swamp", "building", "[\"outdoor\"]");
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
                Equal(CoreErrors.WorldUnknownSubtype, error.Code, "код");
                Contains(error.Message, "bridge", "значение");
            });

            Check("шаблон: неизвестная редкость локации — ошибка схемы", () =>
            {
                var json = LocationJson("loc_x", "wild", "building", "[\"outdoor\"]")
                    .Replace("\"common\"", "\"legendary\"");
                var error = Throws<DataException>(
                    () => LocationTemplate.FromJson(JsonParser.Parse(json, "test"), "test"),
                    "редкость");
                Equal(CoreErrors.WorldUnknownRarity, error.Code, "код");
                Contains(error.Message, "legendary", "значение");
            });

            Check("шаблон: редкость по умолчанию — common", () =>
            {
                var json = LocationJson("loc_x", "wild", "building", "[\"outdoor\"]")
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
                        LocationSubtype.Building,
                        null,
                        Array.Empty<LocationTag>(),
                        0,
                        new[] { "bg" },
                        new IntRange(0, 0),
                        0,
                        new WeightedRange(new IntRange(1, 1)),
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
                        (LocationSubtype)42,
                        null,
                        Array.Empty<LocationTag>(),
                        0,
                        new[] { "bg" },
                        new IntRange(0, 0),
                        0,
                        new WeightedRange(new IntRange(1, 1)),
                        Array.Empty<string>(),
                        LocationRarity.Common,
                        1,
                        false),
                    "роль");
                Equal("subtype", error.ParamName, "параметр");
                Contains(error.Message, "42", "значение");
            });

            Check("топология: для способа перехода нужна цена", () =>
            {
                var json = WorldTopologyJson().Replace("\"road\":{\"stamina\":2,\"hours\":1},", string.Empty);
                var error = Throws<DataException>(() => TopologyTemplate.FromJson(JsonParser.Parse(json, "test"), "test"), "цена");
                Equal("error.world_cost", error.Code, "код");
            });

            Check("ребро мира: length 0 — ошибка", () =>
            {
                Throws<ArgumentOutOfRangeException>(
                    () => new WorldEdge("edge", "loc_a", "loc_b", "path", 0, new TravelCost(0, 0), false, false),
                    "длина ребра");
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

            Check("переходы: хаб соединяется с хабом дорогой, здания — коридором", () =>
            {
                var tags = new[] { LocationTag.Hub, LocationTag.Road, LocationTag.Outdoor };
                True(ViaRules.Allows("road", LocationSubtype.Path, LocationSubtype.Path, tags, tags), "дорога между отрезками пути");
                False(ViaRules.Allows("door", LocationSubtype.Path, LocationSubtype.Path, tags, tags), "дверь больше не способ перехода");
                var indoor = new[] { LocationTag.Indoor, LocationTag.Outdoor };
                True(
                    ViaRules.Allows("corridor", LocationSubtype.Building, LocationSubtype.Building, indoor, indoor),
                    "коридор между зданиями");
                False(
                    ViaRules.Allows("corridor", LocationSubtype.Path, LocationSubtype.Building, indoor, indoor),
                    "коридор только между зданиями");
            });
        }
    }
}
