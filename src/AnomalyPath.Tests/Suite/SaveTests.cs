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
using AnomalyPath.Core.World;
using InventoryEquipment = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Tests
{
    /// <summary>Сохранения: JSON-писатель, round-trip мира и игрока, слоты и ошибки.</summary>
    public sealed class SaveTests : TestBase
    {
        private readonly string _dataRoot;

        /// <summary>Создаёт категорию тестов сохранений.</summary>
        /// <param name="dataRoot">Корень данных проекта.</param>
        public SaveTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        /// <inheritdoc />
        public override void Run()
        {
            JsonWriterTests();
            var services = Bootstrap.Build(_dataRoot);
            var generator = services.Resolve<IWorldGenerator>();
            var travel = services.Resolve<ITravelService>();
            var items = services.Resolve<IItemRepository>();
            SlotNameTests();
            RoundTripTests(generator, travel, items);
            PlayerTests(generator, items);
            StorageTests(generator, items);
        }

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
                    var edge = FirstPaidEdge(map, state.CurrentLocationId);
                    True(travel.Travel(state, edge.EdgeId).Succeeded, "переход");
                    state.SetWeather(WeatherKind.Fog);

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
                    Equal(map.Seed, loaded.Map.Seed, "seed");
                    Equal(map.StartLocationId, loaded.Map.StartLocationId, "стартовая локация");
                    Equal(map.Locations.Count, loaded.Map.Locations.Count, "локации");
                    for (var index = 0; index < map.Locations.Count; index++)
                    {
                        Equal(map.Locations[index].NodeType, loaded.Map.Locations[index].NodeType, "роль локации");
                    }

                    Equal(map.Edges.Count, loaded.Map.Edges.Count, "рёбра");
                    True(loaded.CurrentLocation.Visited, "текущая локация посещена");
                    True(loaded.Map.TryGetEdge(edge.EdgeId, out var savedEdge), "ребро сохранено");
                    Equal(edge.Cost.Stamina, savedEdge!.Cost.Stamina, "стоимость ребра");
                    Equal(edge.Length, savedEdge.Length, "длина ребра");
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
                    var player = NewPlayer(items);
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
                        .Replace("\"schemaVersion\": 3", "\"schemaVersion\": 1")
                        .Replace("\"saveVersion\": 3", "\"saveVersion\": 1");
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
                    File.WriteAllText(storage.GetPath("slot_1"), text.Replace("\"schemaVersion\": 3", "\"schemaVersion\": 99"));
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

            Check("сейв: неизвестная роль локации", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = generator.Generate(42);
                    var state = WorldState.Start(map);
                    var storage = new FileSaveStorage(root);
                    var service = new SaveService(storage, items);
                    service.Save(new GameState(state, null), "slot_1", 0, new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
                    var text = File.ReadAllText(storage.GetPath("slot_1"));
                    text = text.Replace("\"nodeType\": \"hub\"", "\"nodeType\": \"bridge\"");
                    File.WriteAllText(storage.GetPath("slot_1"), text);
                    var error = Throws<DataException>(() => service.Load("slot_1"), "nodeType");
                    Equal("error.world_unknown_node_type", error.Code, "код");
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
        }

        private PlayerState NewPlayer(IItemRepository items)
        {
            var grid = new GridContainer("player", 8, 4);
            var rifle = new ItemInstance("inst_rifle", "wpn_ak74", 1, 80, 100, "ak74_variant");
            rifle.EnsureMagazine(30);
            grid.Place(rifle, items.Get("wpn_ak74"), 0, 0, false);

            var equipment = new InventoryEquipment("player", 4);
            equipment.Set(EquipmentSlot.Main, new ItemInstance("inst_pistol", "wpn_pm", 1, 100, 100));
            equipment.SetQuick(0, new ItemInstance("inst_quick", "wpn_ak74", 1, 100, 100));
            equipment.SetArtifact(0, new ItemInstance("inst_artifact", "art_fire_ember", 1));

            return new PlayerState(
                "player",
                3,
                120,
                90,
                10,
                grid,
                equipment,
                new[] { "skill_test" },
                new NeedsState(80, 70, 90, 100),
                "Игрок",
                1500);
        }

        private void WithSaveRoot(Action<string> body)
        {
            var root = Path.Combine(Path.GetTempPath(), "anomalypath-saves", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                body(root);
            }
            finally
            {
                try
                {
                    Directory.Delete(root, recursive: true);
                }
                catch (IOException)
                {
                    // временная папка не критична для результата теста
                }
            }
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
