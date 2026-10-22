#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Di;
using AnomalyPath.Core.Equipment;
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
            });
        }

        private void CreateTests(ServiceContainer services)
        {
            Check("набор: новая игра применяет броню, оружие и расходники", () =>
            {
                var newGame = services.Resolve<INewGameService>();
                var inventory = services.Resolve<IInventoryService>();
                var state = newGame.Create(42, "loadout_scout", new NewGameSettings(0.5, 1));

                True(state.Player != null, "игрок");
                var player = state.Player!;
                Equal(8, player.Inventory.Width, "ширина сетки");
                Equal(6, player.Inventory.Rows, "строки");
                Equal(100, player.Needs.Max, "потребности");
                Equal("wpn_pm", player.Equipment.Get(EquipmentSlot.Main)?.ItemId, "оружие");
                Equal("itm_armor_jacket", player.Equipment.Get(EquipmentSlot.Armor)?.ItemId, "броня");
                Equal("itm_helmet_light", player.Equipment.Get(EquipmentSlot.Helmet)?.ItemId, "шлем");
                Equal("itm_backpack_small", player.Equipment.Get(EquipmentSlot.Backpack)?.ItemId, "рюкзак");
                Equal("itm_belt_artifact", player.Equipment.Get(EquipmentSlot.Belt)?.ItemId, "пояс");
                Equal(60, inventory.Count(player.Inventory, "ammo_9x18"), "патроны 9x18");
                Equal(30, inventory.Count(player.Inventory, "ammo_545"), "патроны 5.45");
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
                var state = newGame.Create(42, "loadout_rifleman", new NewGameSettings(0.5, 1));

                Equal("wpn_ak74", state.Player!.Equipment.Get(EquipmentSlot.Main)?.ItemId, "оружие");
                Equal("wpn_pm", state.Player.Equipment.Get(EquipmentSlot.Secondary)?.ItemId, "второе оружие");
                Equal("itm_armor_plate", state.Player.Equipment.Get(EquipmentSlot.Armor)?.ItemId, "броня");
                Equal(6, state.Player.Inventory.Rows, "строки с рюкзаком");
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
                    new LoadoutRepository(new[] { loadout }),
                    services.Resolve<IItemRepository>(),
                    services.Resolve<IInventoryService>(),
                    services.Resolve<IEquipmentService>(),
                    services.Resolve<IEquipmentSettings>(),
                    services.Resolve<IInstanceIdGenerator>());

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

        private void ParsingTests()
        {
            Check("набор: явные поля слотов и расходников", () =>
            {
                var json = "{\"schemaVersion\":1,\"kind\":\"loadouts\",\"id\":\"loadout_x\",\"nameKey\":\"n\"," +
                           "\"weapon\":\"wpn_pm\",\"secondaryWeapon\":\"wpn_pm\",\"armor\":\"itm_armor_jacket\"," +
                           "\"helmet\":\"itm_helmet_light\",\"backpack\":\"itm_backpack_small\"," +
                           "\"belt\":\"itm_belt_artifact\",\"consumables\":[{\"itemId\":\"ammo_9x18\",\"count\":1}]}";
                var loadout = LoadoutTemplate.FromJson(JsonParser.Parse(json, "loadout_x"), "loadout_x");

                Equal("wpn_pm", loadout.Equipment[EquipmentSlot.Main], "main");
                Equal("wpn_pm", loadout.Equipment[EquipmentSlot.Secondary], "secondary");
                Equal("itm_armor_jacket", loadout.Equipment[EquipmentSlot.Armor], "armor");
                Equal("itm_helmet_light", loadout.Equipment[EquipmentSlot.Helmet], "helmet");
                Equal("itm_backpack_small", loadout.Equipment[EquipmentSlot.Backpack], "backpack");
                Equal("itm_belt_artifact", loadout.Equipment[EquipmentSlot.Belt], "belt");
                Equal(1, loadout.Items.Count, "consumables");
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
