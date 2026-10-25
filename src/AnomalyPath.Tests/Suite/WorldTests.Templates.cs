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
    }
}
