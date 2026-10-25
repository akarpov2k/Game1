#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.Services;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.Trade;
using AnomalyPath.Core.World;
using InventoryEquipment = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Tests
{
    /// <summary>Часть класса <see cref="SaveTests"/>: игрок.</summary>
    public sealed partial class SaveTests
    {
        private void PlayerTests(IWorldGenerator generator, IItemRepository items)
        {
            Check("потребности: значения зажимаются в 0..max", () =>
            {
                var needs = new NeedsState(5, 5, 5, 10);
                needs.SetHunger(99);
                needs.SetThirst(-1);
                needs.SetSleep(7);
                Equal(10, needs.Hunger, "верхняя граница");
                Equal(0, needs.Thirst, "нижняя граница");
                Equal(7, needs.Sleep, "середина");
            });

            Check("сейв: игрок, инвентарь и экипировка восстанавливаются", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var player = NewPlayer(
                        items,
                        2,
                        new Dictionary<string, int> { ["tal_steady_hand"] = 3 },
                        levelHealthBonus: 6);
                    var service = new SaveService(new FileSaveStorage(root), items);
                    service.Save(
                        new GameState(state, player),
                        "slot_1",
                        42,
                        new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));

                    var loaded = service.Load("slot_1");
                    True(loaded.Player != null, "игрок восстановлен");
                    var restored = loaded.Player!;
                    Equal(3, restored.Level, "уровень");
                    Equal(120, restored.Xp, "опыт");
                    Equal(90, restored.Health, "здоровье");
                    Equal(10, restored.ApMax, "ОД");
                    Equal(1500, restored.Money, "деньги");
                    Equal(2, restored.TalentPoints, "очки талантов");
                    Equal(6, restored.LevelHealthBonus, "прибавка здоровья за уровни");
                    Equal(3, restored.GetTalentRank("tal_steady_hand"), "ранг таланта");
                    Equal(80, restored.Needs.Hunger, "сытость");
                    Equal(1, restored.Inventory.Items.Count, "предметов в сетке");
                    var rifle = restored.Inventory.Find("inst_rifle")!;
                    Equal(0, rifle.X, "колонка");
                    Equal(0, rifle.Y, "строка");
                    Equal(80, rifle.Durability, "прочность");
                    Equal(30, rifle.Magazine, "магазин");
                    Equal("ak74_variant", rifle.VariantKey, "вариант");
                    Equal("wpn_pm", restored.Equipment.Get(EquipmentSlot.Main)!.ItemId, "основное оружие");
                    Equal("wpn_ak74", restored.Equipment.GetQuick(0)!.ItemId, "быстрый слот");
                    Equal("art_fire_ember", restored.Equipment.Artifacts[0]!.ItemId, "артефакт");
                });
            });

            Check("сейв: старый сейв без талантов читается с нулём очков", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(
                        new GameState(state, NewPlayer(items, 2, new Dictionary<string, int> { ["tal_steady_hand"] = 3 })),
                        "slot_1",
                        0,
                        new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    var path = storage.GetPath("slot_1");
                    var game = JsonParser.Parse(File.ReadAllText(path), "slot_1");
                    var player = game.Get("player");
                    player.Members.Remove("talentPoints");
                    player.Members.Remove("talents");
                    player.Members.Remove("levelHealthBonus");
                    File.WriteAllText(path, JsonWriter.Write(game));

                    var restored = service.Load("slot_1").Player!;
                    Equal(0, restored.TalentPoints, "очки по умолчанию");
                    Equal(0, restored.TalentRanks.Count, "ранги по умолчанию");
                    Equal(0, restored.LevelHealthBonus, "прибавка здоровья по умолчанию");
                });
            });

            Check("сейв: неизвестный предмет возвращается в переполнение", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var grid = new GridContainer("player", 8, 4);
                    grid.AddOverflow(new ItemInstance("inst_unknown", "item_missing", 1));
                    var player = new PlayerState(
                        "player",
                        1,
                        0,
                        10,
                        10,
                        grid,
                        new InventoryEquipment("player", 4),
                        new[] { "skill_test" },
                        new NeedsState(10, 10, 10, 100));
                    var service = new SaveService(new FileSaveStorage(root), items);
                    service.Save(
                        new GameState(state, player),
                        "slot_1",
                        0,
                        new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));

                    var restored = service.Load("slot_1").Player!;
                    var unknown = restored.Inventory.Find("inst_unknown")!;
                    True(unknown.Overflow, "в переполнении");
                    Equal("item_missing", unknown.ItemId, "id сохранён");
                });
            });

            Check("сейв: старый world-only сейв грузится без игрока", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(new GameState(state, null), "slot_1", 0, new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    var text = File.ReadAllText(storage.GetPath("slot_1"))
                        .Replace($"\"schemaVersion\": {SaveGame.CurrentSchemaVersion}", "\"schemaVersion\": 1")
                        .Replace($"\"saveVersion\": {SaveGame.CurrentSaveVersion}", "\"saveVersion\": 1");
                    File.WriteAllText(storage.GetPath("slot_1"), text);

                    True(service.Load("slot_1").Player == null, "игрока нет");
                });
            });

            Check("сейв: повреждённая ссылка на предмет", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var player = NewPlayer(items);
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(
                        new GameState(state, player),
                        "slot_1",
                        0,
                        new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    var text = File.ReadAllText(storage.GetPath("slot_1"));
                    var marker = "\"instanceId\": \"inst_pistol\"";
                    var index = text.IndexOf(marker, StringComparison.Ordinal);
                    text = text.Remove(index, marker.Length).Insert(index, "\"instanceId\": \"inst_missing\"");
                    File.WriteAllText(storage.GetPath("slot_1"), text);
                    var error = Throws<DataException>(() => service.Load("slot_1"), "ссылка");
                    Equal("error.save_missing_field", error.Code, "код");
                });
            });
        }
    }
}
