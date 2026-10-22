#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Nicks;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.Session;
using EquipmentState = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Tests
{
    public sealed class NicknameTests : TestBase
    {
        public override void Run()
        {
            Check("ники: каталог требует непустой список", () =>
            {
                var error = Throws<DataException>(
                    () => NicknameCatalogTemplate.FromJson(
                        JsonParser.Parse("{\"kind\":\"nicks\",\"id\":\"nicks\"}", "nicks"), "nicks"),
                    "без names");
                Equal(CoreErrors.MissingArrayField, error.Code, "код ошибки");
            });

            Check("ники: каталог обрезает имена", () =>
            {
                var template = NicknameCatalogTemplate.FromJson(
                    JsonParser.Parse("{\"kind\":\"nicks\",\"id\":\"nicks\",\"names\":[\" Ворон \",\"Дым \"]}", "nicks"),
                    "nicks");

                Equal("nicks", template.Id, "id");
                Equal(2, template.Names.Count, "имен");
                Equal("Ворон", template.Names[0], "первое имя");
                Equal("Дым", template.Names[1], "второе имя");
            });

            Check("ники: генератор детерминирован", () =>
            {
                var generator = new NicknameGenerator(new[] { "А", "Б", "В" });

                Equal("А", generator.Generate(0), "seed 0");
                Equal("Б", generator.Generate(1), "seed 1");
                Equal("В", generator.Generate(2), "seed 2");
                Equal("А", generator.Generate(3), "seed 3");
                Equal("Б", generator.Generate(1), "повтор seed 1");
            });

            Check("ники: пустой список даёт пустую строку", () =>
            {
                var generator = new NicknameGenerator(Array.Empty<string>());
                Equal(string.Empty, generator.Generate(42), "ник");
                Equal(0, generator.Names.Count, "имен");
            });

            Check("игрок: ник обрезается, пустой заменяется id", () =>
            {
                Equal("Ворон", NewPlayer("  Ворон  ").Name, "обрезанный ник");
                Equal("player", NewPlayer("   ").Name, "пустой ник");
                Equal("player", NewPlayer(null).Name, "ник не задан");
            });

            Check("настройки: ник обрезается и ограничен длиной", () =>
            {
                var settings = new NewGameSettings(0.5, 1, " Тихий ");
                Equal("Тихий", settings.Nickname, "ник");
                Throws<ArgumentOutOfRangeException>(
                    () => new NewGameSettings(0.5, 1, new string('x', 25), 24),
                    "слишком длинный ник");
            });

            Check("сейв: ник сохраняется и читается", () =>
            {
                var data = NewSaveData("Ворон");
                var json = data.ToJson();
                var restored = PlayerSaveData.FromJson(json, "save");
                Equal("Ворон", restored.Name, "ник после чтения");
            });

            Check("сейв: старый снимок без ника читается", () =>
            {
                var json = NewSaveData("Ворон").ToJson();
                json.Members.Remove("name");
                var restored = PlayerSaveData.FromJson(json, "save");
                Equal(string.Empty, restored.Name, "ник по умолчанию");
            });

            Check("данные: вид nicks загружается", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("nicks.json"),
                        ["nicks.json"] = "{\"schemaVersion\":1,\"kind\":\"nicks\",\"id\":\"nicks\",\"names\":[\"Ворон\",\"Дым\"]}"
                    },
                    root =>
                    {
                        var data = Load(root);
                        Equal(1, data.All("nicks").Count, "записей вида");
                    });
            });
        }

        private static PlayerState NewPlayer(string? name)
        {
            return new PlayerState(
                "player",
                1,
                0,
                100,
                10,
                new GridContainer("player", 8, 4),
                new EquipmentState("player", 4),
                Array.Empty<string>(),
                new NeedsState(100, 100, 100, 100),
                name);
        }

        private static PlayerSaveData NewSaveData(string name)
        {
            return new PlayerSaveData(
                "player",
                name,
                1,
                0,
                100,
                10,
                Array.Empty<ItemSaveData>(),
                new InventorySaveData("player", 8, 4, Array.Empty<PlacedItemSaveData>()),
                new EquipmentSaveData("player", Array.Empty<EquipmentSlotSaveData>(), Array.Empty<string?>(), Array.Empty<string?>()),
                Array.Empty<string>(),
                new NeedsSaveData(100, 100, 100, 100));
        }
    }
}
