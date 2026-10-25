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
    /// <summary>Часть класса <see cref="SaveTests"/>: хранилище.</summary>
    public sealed partial class SaveTests
    {
        private void StorageTests(IWorldGenerator generator, IItemRepository items)
        {
            Check("сейв: отсутствующий слот — ошибка", () =>
            {
                WithSaveRoot(root =>
                {
                    var service = new SaveService(new FileSaveStorage(root), items);
                    var error = Throws<DataException>(() => service.Load("missing"), "загрузка");
                    Equal("error.save_not_found", error.Code, "код");
                });
            });

            Check("сейв: ячейки экрана сохранения", () =>
            {
                Equal("save_1", SaveSlots.Slot(1), "первая");
                Equal("save_6", SaveSlots.Slot(6), "последняя на странице");
                Equal("save_99", SaveSlots.Slot(99), "слоты не ограничены страницей");
                Throws<ArgumentOutOfRangeException>(() => SaveSlots.Slot(0), "ноль");
            });

            Check("сейв: недопустимое имя слота", () =>
            {
                WithSaveRoot(root =>
                {
                    var storage = new FileSaveStorage(root);
                    var error = Throws<DataException>(() => storage.GetPath("../escape"), "слот");
                    Equal("error.save_slot_invalid", error.Code, "код");
                });
            });

            Check("сейв: битый JSON — ошибка разбора", () =>
            {
                WithSaveRoot(root =>
                {
                    var storage = new FileSaveStorage(root);
                    File.WriteAllText(storage.GetPath("slot_1"), "{");
                    var service = new SaveService(storage, items);
                    Throws<DataException>(() => service.Load("slot_1"), "битый сейв");
                });
            });

            Check("сейв: неподдерживаемая версия", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(new GameState(state, null), "slot_1", 0, new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    var text = File.ReadAllText(storage.GetPath("slot_1"));
                    File.WriteAllText(
                        storage.GetPath("slot_1"),
                        text.Replace($"\"schemaVersion\": {SaveGame.CurrentSchemaVersion}", "\"schemaVersion\": 99"));
                    var error = Throws<DataException>(() => service.Load("slot_1"), "версия");
                    Equal("error.save_unsupported_version", error.Code, "код");
                });
            });

            Check("сейв: неизвестная погода", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(new GameState(state, null), "slot_1", 0, new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    var text = File.ReadAllText(storage.GetPath("slot_1"));
                    File.WriteAllText(storage.GetPath("slot_1"), text.Replace("\"weather\": \"clear\"", "\"weather\": \"snow\""));
                    var error = Throws<DataException>(() => service.Load("slot_1"), "погода");
                    Equal("error.save_unknown_weather", error.Code, "код");
                });
            });

            Check("сейв: неизвестный способ перехода", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(new GameState(state, null), "slot_1", 0, new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    var text = File.ReadAllText(storage.GetPath("slot_1"));
                    text = text.Replace("\"via\": \"road\"", "\"via\": \"teleport\"")
                        .Replace("\"via\": \"path\"", "\"via\": \"teleport\"");
                    File.WriteAllText(storage.GetPath("slot_1"), text);
                    var error = Throws<DataException>(() => service.Load("slot_1"), "via");
                    Equal("error.world_unknown_via", error.Code, "код");
                });
            });

            Check("сейв: неизвестный подтип локации", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(new GameState(state, null), "slot_1", 0, new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    var text = File.ReadAllText(storage.GetPath("slot_1"));
                    text = text.Replace("\"subtype\": \"hub\"", "\"subtype\": \"bridge\"");
                    File.WriteAllText(storage.GetPath("slot_1"), text);
                    var error = Throws<DataException>(() => service.Load("slot_1"), "subtype");
                    Equal("error.world_unknown_subtype", error.Code, "код");
                });
            });

            Check("сейв: повреждённый слот виден в списке", () =>
            {
                WithSaveRoot(root =>
                {
                    var storage = new FileSaveStorage(root);
                    File.WriteAllText(storage.GetPath("slot_bad"), "{");
                    var service = new SaveService(storage, items);
                    var slots = service.ListSlots();
                    Equal(1, slots.Count, "слоты");
                    False(slots[0].IsReadable, "повреждён");
                });
            });

            Check("сейв: удаление слота", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(new GameState(state, null), "slot_1", 0, new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    service.Delete("slot_1");
                    False(service.Exists("slot_1"), "удалён");
                    Equal(0, service.ListSlots().Count, "список");
                });
            });

            Check("сейв: переход после загрузки работает", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(new GameState(state, null), "slot_1", 0, new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    var loaded = service.Load("slot_1").World;
                    var edge = FirstPaidEdge(loaded.Map, loaded.CurrentLocationId);
                    var travel = new TravelService();
                    True(travel.Travel(loaded, edge.EdgeId).Succeeded, "переход");
                });
            });

            Check("сейв: запас торговца и время пополнения восстанавливаются", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var game = new GameState(state, NewPlayer(items));
                    var trader = game.GetOrCreateTrader("trd_hub_smuggler");
                    trader.SetStock("wpn_pm", 2);
                    trader.MarkRestocked(12.5);
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(game, "slot_1", 0, new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));

                    var loaded = service.Load("slot_1");
                    True(loaded.TryGetTrader("trd_hub_smuggler", out var restored), "торговец в сейве");
                    Equal(2, restored!.Count("wpn_pm"), "остаток");
                    Equal(12.5, restored.LastRestockAt, "время пополнения");
                });
            });
        }
    }
}
