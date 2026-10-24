#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Di;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Loadouts;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Services;
using AnomalyPath.Core.Session;

namespace AnomalyPath.Tests
{
    /// <summary>Наборы снаряжения: загрузка, применение и ошибки данных.</summary>
    public sealed class LoadoutTests : TestBase
    {
        private readonly string _dataRoot;

        /// <summary>Создаёт категорию тестов наборов.</summary>
        /// <param name="dataRoot">Корень данных проекта.</param>
        public LoadoutTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        /// <inheritdoc />
        public override void Run()
        {
            var services = Bootstrap.Build(_dataRoot);
            RepositoryTests(services);
            CreateTests(services);
            PreviewTests(services);
            ParsingTests();
            ValidatorTests();
        }

        private void RepositoryTests(ServiceContainer services)
        {
            Check("наборы: загружаются из данных", () =>
            {
                var repository = services.Resolve<ILoadoutRepository>();
                Equal(2, repository.All.Count, "наборов");
                var scout = repository.Get("loadout_scout");
                Equal("loadout_scout", scout.Id, "id");
                Equal("wpn_pm", scout.Equipment[EquipmentSlot.Main], "main");
                Equal("itm_armor_jacket", scout.Equipment[EquipmentSlot.Armor], "armor");
                Equal("itm_helmet_light", scout.Equipment[EquipmentSlot.Helmet], "helmet");
                Equal("itm_backpack_small", scout.Equipment[EquipmentSlot.Backpack], "backpack");
                Equal("itm_belt_artifact", scout.Equipment[EquipmentSlot.Belt], "belt");
                Equal(2, scout.Items.Count, "предметы");
                Equal(1000, scout.StartingMoney ?? -1, "стартовые деньги");
            });
        }

        private void CreateTests(ServiceContainer services)
        {
            Check("набор: новая игра применяет броню, оружие и расходники", () =>
            {
                var newGame = services.Resolve<INewGameService>();
                var inventory = services.Resolve<IInventoryService>();
                var items = services.Resolve<IItemRepository>();
                var state = newGame.Create(42, "loadout_scout", new NewGameSettings(0.5, 1));

                True(state.Player != null, "игрок");
                var player = state.Player!;
                Equal(8, player.Inventory.Width, "ширина сетки");
                Equal(6, player.Inventory.Rows, "строки");
                Equal(100, player.Needs.Max, "потребности");
                Equal("wpn_pm", player.Equipment.Get(EquipmentSlot.Main)?.ItemId, "оружие");
                var pistol = player.Equipment.Get(EquipmentSlot.Main)!;
                Equal(
                    items.Get(pistol.ItemId).Weapon!.Magazine,
                    pistol.MagazineRounds,
                    "стартовое оружие заряжено полностью");
                Equal("itm_armor_jacket", player.Equipment.Get(EquipmentSlot.Armor)?.ItemId, "броня");
                Equal("itm_helmet_light", player.Equipment.Get(EquipmentSlot.Helmet)?.ItemId, "шлем");
                Equal("itm_backpack_small", player.Equipment.Get(EquipmentSlot.Backpack)?.ItemId, "рюкзак");
                Equal("itm_belt_artifact", player.Equipment.Get(EquipmentSlot.Belt)?.ItemId, "пояс");
                Equal(60, inventory.Count(player.Inventory, "ammo_9x18"), "патроны 9x18");
                Equal(30, inventory.Count(player.Inventory, "ammo_545"), "патроны 5.45");
                Equal(1000, player.Money, "деньги на старте");
                True(state.Settings != null, "настройки");
            });

            Check("набор: новая игра детерминирована", () =>
            {
                var newGame = services.Resolve<INewGameService>();
                var settings = new NewGameSettings(0.5, 1);
                var first = newGame.Create(42, "loadout_scout", settings);
                var second = newGame.Create(42, "loadout_scout", settings);

                Equal(first.World.Map.StartLocationId, second.World.Map.StartLocationId, "стартовая локация");
                Equal(
                    first.Player!.Equipment.Get(EquipmentSlot.Main)?.ItemId,
                    second.Player!.Equipment.Get(EquipmentSlot.Main)?.ItemId,
                    "оружие");
            });

            Check("набор: второй набор применяется", () =>
            {
                var newGame = services.Resolve<INewGameService>();
                var items = services.Resolve<IItemRepository>();
                var state = newGame.Create(42, "loadout_rifleman", new NewGameSettings(0.5, 1));

                Equal("wpn_ak74", state.Player!.Equipment.Get(EquipmentSlot.Main)?.ItemId, "оружие");
                Equal("wpn_pm", state.Player.Equipment.Get(EquipmentSlot.Secondary)?.ItemId, "второе оружие");
                Equal("itm_armor_plate", state.Player.Equipment.Get(EquipmentSlot.Armor)?.ItemId, "броня");
                Equal(6, state.Player.Inventory.Rows, "строки с рюкзаком");
                var main = state.Player.Equipment.Get(EquipmentSlot.Main)!;
                var secondary = state.Player.Equipment.Get(EquipmentSlot.Secondary)!;
                Equal(items.Get(main.ItemId).Weapon!.Magazine, main.MagazineRounds, "автомат заряжен");
                Equal(items.Get(secondary.ItemId).Weapon!.Magazine, secondary.MagazineRounds, "пистолет заряжен");
            });

            Check("набор: стартовые деньги берутся из набора", () =>
            {
                var loadout = new LoadoutTemplate(
                    "loadout_rich",
                    "n",
                    null,
                    new List<LoadoutItem>(),
                    new Dictionary<EquipmentSlot, string>(),
                    2500);
                var service = new NewGameService(
                    services.Resolve<IGameRulesRepository>(),
                    services.Resolve<IWorldGenerator>(),
                    services.Resolve<IActorRepository>(),
                    NewPreviewService(services, new[] { loadout }),
                    services.Resolve<INicknameGenerator>());

                var state = service.Create(42, "loadout_rich", new NewGameSettings(0.5, 1));

                Equal(2500, state.Player!.Money, "деньги из набора");
            });

            Check("набор: без startingMoney деньги берутся из правил", () =>
            {
                var loadout = new LoadoutTemplate(
                    "loadout_plain",
                    "n",
                    null,
                    new List<LoadoutItem>(),
                    new Dictionary<EquipmentSlot, string>());
                var service = new NewGameService(
                    services.Resolve<IGameRulesRepository>(),
                    services.Resolve<IWorldGenerator>(),
                    services.Resolve<IActorRepository>(),
                    NewPreviewService(services, new[] { loadout }),
                    services.Resolve<INicknameGenerator>());

                var state = service.Create(42, "loadout_plain", new NewGameSettings(0.5, 1));

                Equal(1000, state.Player!.Money, "деньги из правил");
            });

            Check("набор: переполнение стартового инвентаря — ошибка", () =>
            {
                var items = new List<LoadoutItem>();
                for (var index = 0; index < 10; index++)
                {
                    items.Add(new LoadoutItem("itm_armor_plate", 1));
                }

                var loadout = new LoadoutTemplate(
                    "loadout_overflow",
                    "n",
                    null,
                    items,
                    new Dictionary<EquipmentSlot, string>());
                var service = new NewGameService(
                    services.Resolve<IGameRulesRepository>(),
                    services.Resolve<IWorldGenerator>(),
                    services.Resolve<IActorRepository>(),
                    NewPreviewService(services, new[] { loadout }),
                    services.Resolve<INicknameGenerator>());

                var error = Throws<DataException>(
                    () => service.Create(42, "loadout_overflow", new NewGameSettings(0.5, 1)),
                    "переполнение");
                Equal(CoreErrors.LoadoutOverflow, error.Code, "код");
            });

            Check("набор: неизвестный набор — ошибка", () =>
            {
                var newGame = services.Resolve<INewGameService>();
                Throws<DataException>(
                    () => newGame.Create(42, "loadout_missing", new NewGameSettings(0.5, 1)),
                    "набор");
            });

            Check("набор: настройки обязательны", () =>
            {
                var newGame = services.Resolve<INewGameService>();
                Throws<ArgumentNullException>(
                    () => newGame.Create(42, "loadout_scout", null!),
                    "настройки");
            });
        }

        private void PreviewTests(ServiceContainer services)
        {
            Check("набор: предпросмотр раскладывает оружие, броню и расходники", () =>
            {
                var preview = services.Resolve<ILoadoutPreviewService>().Build("loadout_scout");
                var inventory = services.Resolve<IInventoryService>();

                True(preview.IsComplete, "набор без потерь");
                Equal("loadout_scout", preview.Loadout.Id, "id набора");
                Equal(8, preview.Inventory.Width, "ширина сетки");
                Equal(6, preview.Inventory.Rows, "строки с рюкзаком");
                Equal("wpn_pm", preview.Equipment.Get(EquipmentSlot.Main)?.ItemId, "оружие");
                Equal(
                    services.Resolve<IItemRepository>().Get("wpn_pm").Weapon!.Magazine,
                    preview.Equipment.Get(EquipmentSlot.Main)!.MagazineRounds,
                    "оружие в наборе заряжено полностью");
                Equal("itm_armor_jacket", preview.Equipment.Get(EquipmentSlot.Armor)?.ItemId, "броня");
                Equal("itm_belt_artifact", preview.Equipment.Get(EquipmentSlot.Belt)?.ItemId, "пояс");
                Equal(60, inventory.Count(preview.Inventory, "ammo_9x18"), "патроны 9x18");
                Equal(30, inventory.Count(preview.Inventory, "ammo_545"), "патроны 5.45");
            });

            Check("набор: сетка показа содержит все предметы набора как обычные экземпляры", () =>
            {
                var preview = services.Resolve<ILoadoutPreviewService>().Build("loadout_rifleman");
                var inventory = services.Resolve<IInventoryService>();

                True(preview.IsDisplayComplete, "все предметы поместились");
                Equal(preview.Inventory.Width, preview.Display.Width, "ширина сетки показа");
                Equal(preview.Inventory.Rows, preview.Display.Rows, "строки сетки показа");
                Equal(26, preview.Display.UsedCells, "занятые клетки показа");
                Equal(8, preview.Display.Items.Count, "предметов в показе");
                True(HasItem(preview.Display, "wpn_ak74"), "автомат как обычный предмет");
                True(HasItem(preview.Display, "itm_armor_plate"), "броня как обычный предмет");
                True(HasItem(preview.Display, "itm_belt_artifact"), "пояс как обычный предмет");
                Equal(120, inventory.Count(preview.Display, "ammo_545"), "патроны 5.45 в показе");
                Equal(30, inventory.Count(preview.Display, "ammo_9x18"), "патроны 9x18 в показе");
            });

            Check("набор: показ не подменяет стартовый инвентарь", () =>
            {
                var preview = services.Resolve<ILoadoutPreviewService>().Build("loadout_rifleman");

                Equal(2, preview.Inventory.Items.Count, "в стартовом инвентаре только расходники");
                False(HasItem(preview.Inventory, "wpn_ak74"), "оружие не лежит в стартовом инвентаре");
                Equal("wpn_ak74", preview.Equipment.Get(EquipmentSlot.Main)?.ItemId, "оружие надето");
                Equal(7, services.Resolve<ILoadoutPreviewService>().Build("loadout_scout").Display.Items.Count,
                    "предметов в показе второго набора");
            });

            Check("набор: предпросмотр повторяем: та же раскладка", () =>
            {
                var previews = services.Resolve<ILoadoutPreviewService>();
                var first = previews.Build("loadout_rifleman");
                var second = previews.Build("loadout_rifleman");

                Equal(first.Inventory.UsedCells, second.Inventory.UsedCells, "занятые клетки");
                Equal(first.Inventory.Items.Count, second.Inventory.Items.Count, "экземпляры");
                for (var index = 0; index < first.Inventory.Items.Count; index++)
                {
                    var left = first.Inventory.Items[index];
                    var right = second.Inventory.Items[index];
                    Equal(left.ItemId, right.ItemId, "предмет " + index);
                    Equal(left.X, right.X, "x " + index);
                    Equal(left.Y, right.Y, "y " + index);
                }
            });

            Check("набор: у сетки показа нет лимита ячеек", () =>
            {
                var items = new List<LoadoutItem>();
                for (var index = 0; index < 10; index++)
                {
                    items.Add(new LoadoutItem("itm_armor_plate", 1));
                }

                var loadout = new LoadoutTemplate(
                    "loadout_heavy",
                    "n",
                    null,
                    items,
                    new Dictionary<EquipmentSlot, string>());
                var preview = NewPreviewService(services, new[] { loadout }).Build("loadout_heavy");

                True(!preview.IsComplete, "стартовый инвентарь переполнен");
                Equal(0, preview.DisplayOverflow.Count, "сетка показа без переполнения");
                True(preview.Display.Rows > 6, "сетка показа выросла по рядам");
                Equal(10, preview.Display.Items.Count, "все предметы отрисованы");
            });

            Check("набор: предпросмотр не падает на переполнении", () =>
            {
                var items = new List<LoadoutItem>();
                for (var index = 0; index < 10; index++)
                {
                    items.Add(new LoadoutItem("itm_armor_plate", 1));
                }

                var loadout = new LoadoutTemplate(
                    "loadout_overflow",
                    "n",
                    null,
                    items,
                    new Dictionary<EquipmentSlot, string>());
                var preview = NewPreviewService(services, new[] { loadout }).Build("loadout_overflow");

                True(!preview.IsComplete, "набор с потерей");
                True(preview.Overflow.Count > 0, "есть переполнение");
                Equal(0, preview.Rejected.Count, "слоты");
            });

            Check("набор: предпросмотр сообщает о неверном слоте", () =>
            {
                var equipment = new Dictionary<EquipmentSlot, string>
                {
                    [EquipmentSlot.Helmet] = "wpn_pm"
                };
                var loadout = new LoadoutTemplate(
                    "loadout_bad_slot",
                    "n",
                    null,
                    new List<LoadoutItem>(),
                    equipment);
                var preview = NewPreviewService(services, new[] { loadout }).Build("loadout_bad_slot");

                Equal(1, preview.Rejected.Count, "отвергнутых");
                Equal(EquipmentSlot.Helmet, preview.Rejected[0].Key, "слот");
                Equal("wpn_pm", preview.Rejected[0].Value, "предмет");
                Equal(0, preview.Overflow.Count, "переполнение");
            });

            Check("набор: предпросмотр неизвестного набора — ошибка", () =>
            {
                var error = Throws<DataException>(
                    () => services.Resolve<ILoadoutPreviewService>().Build("loadout_missing"),
                    "набор");
                Equal(CoreErrors.LoadoutNotFound, error.Code, "код");
            });
        }

        private static bool HasItem(GridContainer container, string itemId)
        {
            foreach (var item in container.Items)
            {
                if (item.ItemId == itemId)
                {
                    return true;
                }
            }

            return false;
        }

        private static ILoadoutPreviewService NewPreviewService(
            ServiceContainer services,
            IEnumerable<LoadoutTemplate> loadouts) =>
            new LoadoutPreviewService(
                services.Resolve<IGameRulesRepository>(),
                new LoadoutRepository(loadouts),
                services.Resolve<IItemRepository>(),
                services.Resolve<IInventoryService>(),
                services.Resolve<IEquipmentService>(),
                services.Resolve<IEquipmentSettings>(),
                services.Resolve<IInstanceIdGenerator>());

        private void ParsingTests()
        {
            Check("набор: явные поля слотов и расходников", () =>
            {
                var json = "{\"schemaVersion\":1,\"kind\":\"loadouts\",\"id\":\"loadout_x\",\"nameKey\":\"n\"," +
                           "\"startingMoney\":1500," +
                           "\"weapon\":\"wpn_pm\",\"secondaryWeapon\":\"wpn_pm\",\"armor\":\"itm_armor_jacket\"," +
                           "\"helmet\":\"itm_helmet_light\",\"backpack\":\"itm_backpack_small\"," +
                           "\"belt\":\"itm_belt_artifact\",\"consumables\":[{\"itemId\":\"ammo_9x18\",\"count\":1}]}";
                var loadout = LoadoutTemplate.FromJson(JsonParser.Parse(json, "loadout_x"), "loadout_x");

                Equal(1500, loadout.StartingMoney ?? -1, "startingMoney");
                Equal("wpn_pm", loadout.Equipment[EquipmentSlot.Main], "main");
                Equal("wpn_pm", loadout.Equipment[EquipmentSlot.Secondary], "secondary");
                Equal("itm_armor_jacket", loadout.Equipment[EquipmentSlot.Armor], "armor");
                Equal("itm_helmet_light", loadout.Equipment[EquipmentSlot.Helmet], "helmet");
                Equal("itm_backpack_small", loadout.Equipment[EquipmentSlot.Backpack], "backpack");
                Equal("itm_belt_artifact", loadout.Equipment[EquipmentSlot.Belt], "belt");
                Equal(1, loadout.Items.Count, "consumables");
            });

            Check("набор: без startingMoney сумма не задана", () =>
            {
                var json = "{\"schemaVersion\":1,\"kind\":\"loadouts\",\"id\":\"loadout_plain\",\"nameKey\":\"n\",\"items\":[],\"equipment\":{}}";
                var loadout = LoadoutTemplate.FromJson(JsonParser.Parse(json, "loadout_plain"), "loadout_plain");

                Equal(-1, loadout.StartingMoney ?? -1, "нет стартовых денег");
            });

            Check("набор: неизвестный слот — ошибка", () =>
            {
                var json = "{\"schemaVersion\":1,\"kind\":\"loadouts\",\"id\":\"loadout_bad\",\"nameKey\":\"n\",\"items\":[],\"equipment\":{\"head\":\"wpn_pm\"}}";
                var error = Throws<DataException>(
                    () => LoadoutTemplate.FromJson(JsonParser.Parse(json, "loadout_bad"), "loadout_bad"),
                    "слот");
                Equal(CoreErrors.LoadoutSlot, error.Code, "код");
            });

            Check("набор: количество меньше 1 — ошибка", () =>
            {
                var json = "{\"schemaVersion\":1,\"kind\":\"loadouts\",\"id\":\"loadout_bad\",\"nameKey\":\"n\",\"items\":[{\"itemId\":\"wpn_pm\",\"count\":0}],\"equipment\":{}}";
                var error = Throws<DataException>(
                    () => LoadoutTemplate.FromJson(JsonParser.Parse(json, "loadout_bad"), "loadout_bad"),
                    "количество");
                Equal(CoreErrors.LoadoutCount, error.Code, "код");
            });

            Check("набор: отрицательные стартовые деньги — ошибка", () =>
            {
                var json = "{\"schemaVersion\":1,\"kind\":\"loadouts\",\"id\":\"loadout_bad\",\"nameKey\":\"n\",\"startingMoney\":-1,\"items\":[],\"equipment\":{}}";
                var error = Throws<DataException>(
                    () => LoadoutTemplate.FromJson(JsonParser.Parse(json, "loadout_bad"), "loadout_bad"),
                    "деньги");
                Equal(CoreErrors.NonNegative, error.Code, "код");
            });
        }

        private void ValidatorTests()
        {
            Check("набор: валидатор ловит отсутствующий предмет", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("loadouts/loadout_bad.json"),
                        ["loadouts/loadout_bad.json"] =
                            "{\"schemaVersion\":1,\"kind\":\"loadouts\",\"id\":\"loadout_bad\",\"nameKey\":\"n\",\"items\":[{\"itemId\":\"missing\",\"count\":1}],\"equipment\":{}}"
                    },
                    root =>
                    {
                        var report = Preflight.Validate(root);
                        True(HasIssue(report, "loadout_bad", "не найден"), "нет ошибки");
                    });
            });

            Check("набор: валидатор ловит неверный слот", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/wpn_pm.json", "loadouts/loadout_bad.json"),
                        ["items/wpn_pm.json"] = Weapon("wpn_pm"),
                        ["loadouts/loadout_bad.json"] =
                            "{\"schemaVersion\":1,\"kind\":\"loadouts\",\"id\":\"loadout_bad\",\"nameKey\":\"n\",\"items\":[],\"equipment\":{\"helmet\":\"wpn_pm\"}}"
                    },
                    root =>
                    {
                        var report = Preflight.Validate(root);
                        True(HasIssue(report, "loadout_bad", "нельзя надеть"), "нет ошибки");
                    });
            });
        }
    }
}
