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
    /// <summary>Часть класса <see cref="SaveTests"/>: запись и чтение.</summary>
    public sealed partial class SaveTests
    {
        private void SlotNameTests()
        {
            Check("сейв: имена служебных слотов", () =>
            {
                Equal("manual_save", SaveSlots.Manual, "ручной слот");
                Equal("exit_save", SaveSlots.Exit, "слот выхода");
            });
        }

        private void JsonWriterTests()
        {
            Check("json-писатель: ключи сортируются, строки экранируются", () =>
            {
                var value = JsonValue.NewObject();
                value.Members["b"] = JsonValue.FromString("Привет\n\"мир\"");
                value.Members["a"] = JsonValue.FromNumber(42);
                var text = JsonWriter.Write(value, indented: false);
                Equal("{\"a\":42,\"b\":\"Привет\\n\\\"мир\\\"\"}", text, "текст");
                var parsed = JsonParser.Parse(text, "test");
                Equal("Привет\n\"мир\"", parsed.Get("b").Text, "строка");
                Equal(42.0, parsed.Get("a").Number, "число");
            });

            Check("json-писатель: массив и вложенность", () =>
            {
                var value = JsonValue.NewObject();
                var array = JsonValue.NewArray();
                array.Items.Add(JsonValue.FromBool(true));
                array.Items.Add(JsonValue.Null());
                value.Members["items"] = array;
                var text = JsonWriter.Write(value, indented: false);
                Equal("{\"items\":[true,null]}", text, "текст");
                Equal(2, JsonParser.Parse(text, "test").Get("items").Items.Count, "элементы");
            });
        }

        private void RoundTripTests(IWorldGenerator generator, ITravelService travel, IItemRepository items)
        {
            Check("сейв: мир и состояние восстанавливаются", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    generator.BeginRoute(state);
                    var edge = FirstPaidEdge(map, state.CurrentLocationId);
                    True(travel.Travel(state, edge.EdgeId).Succeeded, "переход");
                    state.SetWeather(WeatherKind.Fog);
                    state.NextLootSerial();
                    state.NextLootSerial();

                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    var createdAt = new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero);
                    service.Save(new GameState(state, null), "slot_1", 123.5, createdAt);

                    var loaded = service.Load("slot_1").World;
                    Equal(state.CurrentLocationId, loaded.CurrentLocationId, "текущая локация");
                    Equal(state.Stamina, loaded.Stamina, "выносливость");
                    Equal(state.StaminaMax, loaded.StaminaMax, "максимум выносливости");
                    Equal(state.Clock.Day, loaded.Clock.Day, "день");
                    Equal(state.Clock.Hours, loaded.Clock.Hours, "часы");
                    Equal(state.Weather, loaded.Weather, "погода");
                    Equal(state.LastHubId, loaded.LastHubId, "последний хаб");
                    Equal(state.ObjectSeed, loaded.ObjectSeed, "seed объектов");
                    Equal(2, loaded.LootSerial, "счётчик бросков лута");
                    Equal(map.Seed, loaded.Map.Seed, "seed");
                    Equal(map.StartLocationId, loaded.Map.StartLocationId, "стартовая локация");
                    Equal(map.Locations.Count, loaded.Map.Locations.Count, "локации");
                    for (var index = 0; index < map.Locations.Count; index++)
                    {
                        Equal(map.Locations[index].Subtype, loaded.Map.Locations[index].Subtype, "роль локации");
                    }

                    Equal(map.Edges.Count, loaded.Map.Edges.Count, "рёбра");
                    Equal(map.RouteEdges, loaded.Map.RouteEdges, "звеньев маршрута");
                    Equal(map.RouteTailId, loaded.Map.RouteTailId, "конец маршрута");
                    Equal(map.RouteLastBranches, loaded.Map.RouteLastBranches, "веток на последнем звене");
                    Equal(map.RouteSerial, loaded.Map.RouteSerial, "номер выхода из хаба");
                    Equal(
                        string.Join(",", map.RouteFrontIds),
                        string.Join(",", loaded.Map.RouteFrontIds),
                        "выходы последнего звена");
                    Equal(map.Locations[0].RouteStep, loaded.Map.Locations[0].RouteStep, "шаг от хаба");
                    True(loaded.CurrentLocation.Visited, "текущая локация посещена");
                    True(loaded.Map.TryGetEdge(edge.EdgeId, out var savedEdge), "ребро сохранено");
                    Equal(edge.Cost.Stamina, savedEdge!.Cost.Stamina, "стоимость ребра");
                    Equal(edge.Length, savedEdge.Length, "длина ребра");
                });
            });

            Check("сейв: исчезнувший противник сохраняется по локации", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    state.CurrentLocation.MarkEnemyGone();
                    var service = new SaveService(new FileSaveStorage(root), items);
                    service.Save(
                        new GameState(state, null),
                        "slot_1",
                        0,
                        new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));

                    var loaded = service.Load("slot_1").World;
                    True(loaded.CurrentLocation.EnemyGone, "текущая локация без противника");
                    foreach (var location in loaded.Map.Locations)
                    {
                        if (location.InstanceId != loaded.CurrentLocationId)
                        {
                            False(location.EnemyGone, "другая локация с противником");
                        }
                    }
                });
            });

            Check("сейв: старый сейв без исчезнувшего противника читается с false", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(
                        new GameState(state, null),
                        "slot_1",
                        0,
                        new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    var text = File.ReadAllText(storage.GetPath("slot_1"))
                        .Replace("\"enemyGone\"", "\"enemyGoneRemoved\"");
                    File.WriteAllText(storage.GetPath("slot_1"), text);

                    False(service.Load("slot_1").World.CurrentLocation.EnemyGone, "противник на месте");
                });
            });

            Check("сейв: старый сейв без счётчика лута читается с нулём", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(new GameState(state, null), "slot_1", 0,
                        new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    var text = File.ReadAllText(storage.GetPath("slot_1"))
                        .Replace("\"lootSerial\"", "\"lootSerialRemoved\"");
                    File.WriteAllText(storage.GetPath("slot_1"), text);

                    Equal(0, service.Load("slot_1").World.LootSerial, "счётчик по умолчанию");
                });
            });

            Check("сейв: список слотов читает метаданные", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var service = new SaveService(new FileSaveStorage(root), items);
                    service.Save(new GameState(state, null), "slot_1", 7, new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));

                    var slots = service.ListSlots();
                    Equal(1, slots.Count, "слоты");
                    Equal("slot_1", slots[0].Slot, "имя");
                    Equal(42, slots[0].Seed, "seed");
                    Equal(state.Clock.Day, slots[0].Day, "день");
                    True(slots[0].IsReadable, "читается");
                    Equal(7.0, slots[0].Playtime, "время");
                    Equal(0, slots[0].Level, "уровень без игрока");
                    Equal(0, slots[0].Money, "деньги без игрока");
                    Equal(string.Empty, slots[0].Title, "название по умолчанию");
                });
            });

            Check("сейв: список слотов показывает название, уровень и деньги", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var service = new SaveService(new FileSaveStorage(root), items);
                    service.Save(
                        new GameState(state, NewPlayer(items)),
                        "slot_1",
                        5,
                        new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero),
                        "Мой сейв");

                    var slots = service.ListSlots();
                    Equal(1, slots.Count, "слоты");
                    Equal("Мой сейв", slots[0].Title, "название");
                    Equal(3, slots[0].Level, "уровень");
                    Equal(1500, slots[0].Money, "деньги");
                });
            });

            Check("сейв: старый сейв без денег читается с нулём", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(new GameState(state, NewPlayer(items)), "slot_1", 0,
                        new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    var text = File.ReadAllText(storage.GetPath("slot_1"))
                        .Replace("\"money\": 1500,", string.Empty);
                    File.WriteAllText(storage.GetPath("slot_1"), text);

                    Equal(0, service.Load("slot_1").Player!.Money, "деньги по умолчанию");
                    Equal(0, service.ListSlots()[0].Money, "деньги в списке");
                });
            });

            Check("сейв: старая версия формата отклоняется", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(
                        new GameState(state, NewPlayer(items), new NewGameSettings(0.4, 1.5, xpLossChance: 0.6)),
                        "slot_1",
                        0,
                        new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    var path = storage.GetPath("slot_1");
                    var text = File.ReadAllText(path)
                        .Replace($"\"schemaVersion\": {SaveGame.CurrentSchemaVersion}", "\"schemaVersion\": 3")
                        .Replace($"\"saveVersion\": {SaveGame.CurrentSaveVersion}", "\"saveVersion\": 3");
                    File.WriteAllText(path, text);

                    Throws<AnomalyPath.Core.Data.DataException>(() => service.Load("slot_1"), "старая версия");
                });
            });

            Check("сейв: отрицательный номер выхода из хаба отклоняется", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    generator.BeginRoute(state);
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(
                        new GameState(state, null),
                        "slot_1",
                        0,
                        new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    var path = storage.GetPath("slot_1");
                    var text = File.ReadAllText(path).Replace("\"routeSerial\": 1", "\"routeSerial\": -1");
                    File.WriteAllText(path, text);

                    Throws<AnomalyPath.Core.Data.DataException>(() => service.Load("slot_1"), "номер маршрута");
                });
            });

            Check("сейв: старый сейв без магазина читается как разряженное оружие", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(new GameState(state, NewPlayer(items)), "slot_1", 0,
                        new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    var text = File.ReadAllText(storage.GetPath("slot_1"))
                        .Replace("\"magazine\": 30", "\"magazine\": null");
                    File.WriteAllText(storage.GetPath("slot_1"), text);

                    var rifle = service.Load("slot_1").Player!.Inventory.Find("inst_rifle")!;
                    Equal(0, rifle.MagazineRounds, "старое оружие разряжено");
                });
            });

            Check("сейв: тайник и счётчики сохраняются", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    state.RegisterStashCheck();
                    state.RegisterStashCheck();
                    state.RegisterStashOpening();
                    state.CurrentLocation.MarkStashRolled();
                    state.CurrentLocation.MarkStashPresent();
                    state.CurrentLocation.MarkStashOpened();
                    var service = new SaveService(new FileSaveStorage(root), items);
                    service.Save(new GameState(state, null), "slot_1", 0,
                        new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));

                    var loaded = service.Load("slot_1").World;
                    Equal(2, loaded.StashChecks, "проверки без тайника");
                    Equal(1, loaded.StashOpenings, "вскрытия без джекпота");
                    True(loaded.CurrentLocation.StashRolled, "локация проверена");
                    True(loaded.CurrentLocation.StashPresent, "тайник есть");
                    True(loaded.CurrentLocation.StashOpened, "тайник вскрыт");
                });
            });

            Check("сейв: старый сейв без тайника читается с false", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    state.CurrentLocation.MarkStashRolled();
                    state.CurrentLocation.MarkStashPresent();
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(new GameState(state, null), "slot_1", 0,
                        new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    var text = File.ReadAllText(storage.GetPath("slot_1"))
                        .Replace("\"stashRolled\"", "\"stashRolledRemoved\"")
                        .Replace("\"stashPresent\"", "\"stashPresentRemoved\"")
                        .Replace("\"stashOpened\"", "\"stashOpenedRemoved\"");
                    File.WriteAllText(storage.GetPath("slot_1"), text);

                    var loaded = service.Load("slot_1").World;
                    False(loaded.CurrentLocation.StashRolled, "проверка не сохранена");
                    False(loaded.CurrentLocation.StashPresent, "тайника нет");
                    False(loaded.CurrentLocation.StashOpened, "тайник не вскрыт");
                });
            });

            Check("сейв: старый сейв без счётчиков тайников читается с нулями", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(new GameState(state, null), "slot_1", 0,
                        new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    var text = File.ReadAllText(storage.GetPath("slot_1"))
                        .Replace("\"stashChecks\"", "\"stashChecksRemoved\"")
                        .Replace("\"stashOpenings\"", "\"stashOpeningsRemoved\"");
                    File.WriteAllText(storage.GetPath("slot_1"), text);

                    var loaded = service.Load("slot_1").World;
                    Equal(0, loaded.StashChecks, "проверки");
                    Equal(0, loaded.StashOpenings, "вскрытия");
                });
            });

            Check("сейв: вскрытый тайник без наличия — ошибка", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    state.CurrentLocation.MarkStashRolled();
                    state.CurrentLocation.MarkStashPresent();
                    state.CurrentLocation.MarkStashOpened();
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(new GameState(state, null), "slot_1", 0,
                        new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    var path = storage.GetPath("slot_1");
                    var game = JsonParser.Parse(File.ReadAllText(path), "slot_1");
                    foreach (var location in game.Get("world").Get("locations").Items)
                    {
                        if (string.Equals(
                                location.Get("instanceId").Text,
                                state.CurrentLocationId,
                                StringComparison.Ordinal))
                        {
                            location.Members["stashPresent"] = JsonValue.FromBool(false);
                        }
                    }

                    File.WriteAllText(path, JsonWriter.Write(game));
                    Throws<DataException>(() => service.Load("slot_1"), "несогласованное состояние");
                });
            });

            Check("сейв: повторная запись создаёт бэкап", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(new GameState(state, null), "slot_1", 1, new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    service.Save(new GameState(state, null), "slot_1", 2, new DateTimeOffset(2026, 10, 22, 10, 1, 0, TimeSpan.Zero));
                    True(File.Exists(storage.GetPath("slot_1")), "файл");
                    True(File.Exists(storage.GetPath("slot_1") + ".bak"), "бэкап");
                    True(service.Load("slot_1").World != null, "загрузка после перезаписи");
                });
            });
        }
    }
}
