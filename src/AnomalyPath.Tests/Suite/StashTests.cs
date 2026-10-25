#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Loot;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Services;
using AnomalyPath.Core.Templates;
using AnomalyPath.Core.World;

namespace AnomalyPath.Tests
{
    /// <summary>Тайники: настройки, появление с растущим шансом, вскрытие и состояние в сейве.</summary>
    public sealed class StashTests : TestBase
    {
        private const string PoolTable = "loot_pool_test";
        private const string JackpotTable = "loot_jackpot_test";
        private const string PoolItem = "itm_pool";
        private const string JackpotItem = "itm_jackpot";

        private readonly string _dataRoot;

        /// <summary>Создаёт категорию.</summary>
        /// <param name="dataRoot">Корень данных.</param>
        public StashTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        /// <inheritdoc />
        public override void Run()
        {
            SettingsTests();
            RollTests();
            OpenTests();
            DataTests();
        }

        private void SettingsTests()
        {
            Check("тайник: блок настроек читается из данных", () =>
            {
                var settings = LootSettings.FromJson(
                    JsonParser.Parse(LootJson(StashJson()), "loot"),
                    "loot");
                True(settings.Stashes != null, "блок тайников");
                var stashes = settings.Stashes!;
                Near(0.05, stashes.BaseChance, "базовый шанс");
                Near(0.02, stashes.ChanceGrowth, "рост шанса");
                Near(0.5, stashes.ChanceCap, "предел шанса");
                Near(0.01, stashes.JackpotBaseChance, "шанс джекпота");
                Equal(2, stashes.Pools.Count, "пулов");
                Equal(JackpotTable, stashes.JackpotLootTableId, "таблица джекпота");
            });

            Check("тайник: без блока настроек тайников нет", () =>
            {
                var settings = LootSettings.FromJson(
                    JsonParser.Parse(LootJson(null), "loot"),
                    "loot");
                True(settings.Stashes == null, "блока нет");
                var repository = new LootRepository(Tables(), settings);
                var loot = new LootService(repository, Catalog((PoolItem, 1)), new SequentialInstanceIdGenerator());
                False(new StashService(repository, loot).Enabled, "сервис выключен");
            });

            Check("тайник: шанс вне 0..1 запрещён", () =>
            {
                Throws<DataException>(
                    () => LootSettings.FromJson(
                        JsonParser.Parse(LootJson(StashJson(baseChance: 1.5)), "loot"),
                        "loot"),
                    "базовый шанс");
            });

            Check("тайник: рост вне 0..1 запрещён", () =>
            {
                Throws<DataException>(
                    () => LootSettings.FromJson(
                        JsonParser.Parse(LootJson(StashJson(chanceGrowth: -0.1)), "loot"),
                        "loot"),
                    "рост");
            });

            Check("тайник: пустой список пулов запрещён", () =>
            {
                var json = "{\"kind\":\"loot\",\"id\":\"loot\",\"tierBands\":[" +
                           "{\"offset\":-1,\"chance\":0.2},{\"offset\":0,\"chance\":0.6}," +
                           "{\"offset\":1,\"chance\":0.2}],\"stashes\":{\"baseChance\":0.05," +
                           "\"chanceGrowth\":0.02,\"chanceCap\":0.5,\"jackpotBaseChance\":0.01," +
                           "\"jackpotChanceGrowth\":0.01,\"jackpotChanceCap\":1.0,\"pools\":[]," +
                           "\"jackpotLootTableId\":\"loot_jackpot_test\"}}";
                Throws<DataException>(() => LootSettings.FromJson(JsonParser.Parse(json, "loot"), "loot"), "пулы");
            });

            Check("тайник: повторный тир пула запрещён", () =>
            {
                var json = "{\"kind\":\"loot\",\"id\":\"loot\",\"tierBands\":[" +
                           "{\"offset\":-1,\"chance\":0.2},{\"offset\":0,\"chance\":0.6}," +
                           "{\"offset\":1,\"chance\":0.2}],\"stashes\":{\"baseChance\":0.05," +
                           "\"chanceGrowth\":0.02,\"chanceCap\":0.5,\"jackpotBaseChance\":0.01," +
                           "\"jackpotChanceGrowth\":0.01,\"jackpotChanceCap\":1.0,\"pools\":[" +
                           "{\"tier\":1,\"lootTableId\":\"loot_pool_test\"}," +
                           "{\"tier\":1,\"lootTableId\":\"loot_pool_test\"}]," +
                           "\"jackpotLootTableId\":\"loot_jackpot_test\"}}";
                Throws<DataException>(() => LootSettings.FromJson(JsonParser.Parse(json, "loot"), "loot"), "тир");
            });

            Check("тайник: шанс растёт и упирается в предел", () =>
            {
                var settings = Stashes(0.05, 0.02, 0.5, 0.01, 0.01, 1.0);
                Near(0.05, settings.Chance(0), "база");
                Near(0.07, settings.Chance(1), "одна проверка");
                Near(0.5, settings.Chance(100), "предел");
            });

            Check("тайник: шанс джекпота растёт и упирается в предел", () =>
            {
                var settings = Stashes(0.05, 0.02, 0.5, 0.01, 0.01, 1.0);
                Near(0.01, settings.JackpotChance(0), "база");
                Near(0.02, settings.JackpotChance(1), "одно вскрытие");
                Near(1.0, settings.JackpotChance(1000), "предел");
            });

            Check("тайник: пул выбирается по точному тиру, иначе по ближайшему", () =>
            {
                var settings = Stashes(0.05, 0.02, 0.5, 0.01, 0.01, 1.0);
                Equal(PoolTable, settings.PoolForTier(1), "точный тир");
                Equal("loot_pool_2_test", settings.PoolForTier(5), "ближайший больший");
                Equal(PoolTable, settings.PoolForTier(-3), "ближайший меньший");
            });
        }

        private void RollTests()
        {
            Check("тайник: в хабе не появляется", () =>
            {
                var (world, hub, _, _) = NewWorld();
                var service = NewService(Stashes(1.0, 0, 1.0, 0, 0, 1.0));
                False(service.RollOnEntry(world, hub), "хаб");
                False(hub.StashRolled, "хаб не проверяется");
                False(hub.StashPresent, "тайника нет");
                Equal(0, world.StashChecks, "счётчик не вырос");
            });

            Check("тайник: нулевой шанс не даёт находки и растит счётчик", () =>
            {
                var (world, _, first, _) = NewWorld();
                var service = NewService(Stashes(0.0, 0.02, 0.5, 0, 0, 1.0));
                False(service.RollOnEntry(world, first), "промах");
                Equal(1, world.StashChecks, "одна проверка");
                True(first.StashRolled, "локация проверена");
                False(service.RollOnEntry(world, first), "повторная проверка");
                Equal(1, world.StashChecks, "счётчик не вырос повторно");
            });

            Check("тайник: гарантированный шанс оставляет его на локации и сбрасывает счётчик", () =>
            {
                var (world, _, first, _) = NewWorld();
                var service = NewService(Stashes(1.0, 0.02, 1.0, 0, 0, 1.0));
                True(service.RollOnEntry(world, first), "находка");
                True(first.StashPresent, "тайник есть");
                Equal(0, world.StashChecks, "счётчик сброшен");
            });

            Check("тайник: шанс растёт с каждой проверкой", () =>
            {
                var (world, _, first, second) = NewWorld();
                var service = NewService(Stashes(0.0, 1.0, 1.0, 0, 0, 1.0));
                False(service.RollOnEntry(world, first), "первая проверка — 0%");
                Equal(1, world.StashChecks, "одна проверка");
                True(service.RollOnEntry(world, second), "вторая проверка — 100%");
                Equal(0, world.StashChecks, "счётчик сброшен");
            });
        }

        private void OpenTests()
        {
            Check("тайник: вскрытие без тайника — NoStash", () =>
            {
                var (world, _, first, _) = NewWorld();
                var service = NewService(Stashes(0.05, 0.02, 0.5, 0.01, 0.01, 1.0));
                var result = service.Open(world, first);
                Equal(StashOpenStatus.NoStash, result.Status, "статус");
                Equal(0, world.LootSerial, "номер броска не тронут");
            });

            Check("тайник: вскрытие берёт пул тира локации", () =>
            {
                var (world, _, first, _) = NewWorld();
                first.MarkStashPresent();
                var service = NewService(Stashes(0.05, 0.02, 0.5, 0, 0, 1.0));
                var result = service.Open(world, first);
                Equal(StashOpenStatus.Opened, result.Status, "статус");
                False(result.Jackpot, "джекпот не сработал");
                Equal(PoolTable, result.LootTableId, "пул");
                Equal(1, result.Loot.Count, "лут");
                Equal(PoolItem, result.Loot[0].ItemId, "предмет пула");
                True(first.StashOpened, "тайник вскрыт");
                Equal(1, world.StashOpenings, "счётчик вскрытий");
                Equal(1, world.LootSerial, "номер броска");
            });

            Check("тайник: второй раз вскрыть нельзя", () =>
            {
                var (world, _, first, _) = NewWorld();
                first.MarkStashPresent();
                first.MarkStashOpened();
                var service = NewService(Stashes(0.05, 0.02, 0.5, 0, 0, 1.0));
                var result = service.Open(world, first);
                Equal(StashOpenStatus.AlreadyOpened, result.Status, "статус");
                Equal(0, world.LootSerial, "номер броска не тронут");
            });

            Check("тайник: пул выбирается по тиру локации", () =>
            {
                var (world, _, _, second) = NewWorld();
                second.MarkStashPresent();
                var service = NewService(Stashes(0.05, 0.02, 0.5, 0, 0, 1.0));
                var result = service.Open(world, second);
                Equal("loot_pool_2_test", result.LootTableId, "пул второго тира");
                Equal(1, result.Loot.Count, "лут");
            });

            Check("тайник: одинаковое состояние даёт одинаковый лут", () =>
            {
                var first = OpenSnapshot();
                var second = OpenSnapshot();
                Equal(first, second, "повтор");
            });

            Check("тайник: счётчик вскрытий растёт и сбрасывается джекпотом", () =>
            {
                var (world, _, first, second) = NewWorld();
                first.MarkStashPresent();
                second.MarkStashPresent();
                var service = NewService(Stashes(0.05, 0.02, 0.5, 0, 1.0, 1.0));
                False(service.Open(world, first).Jackpot, "первое вскрытие без джекпота");
                Equal(1, world.StashOpenings, "счётчик вскрытий");
                True(service.Open(world, second).Jackpot, "второе вскрытие — джекпот");
                Equal(0, world.StashOpenings, "счётчик сброшен");
            });
        }

        private void DataTests()
        {
            Check("тайник: в данных есть настройки, пулы всех тиров и пул джекпота", () =>
            {
                var data = Load(_dataRoot);
                var loot = LootRepository.FromLoadedData(data);
                var items = ItemRepository.FromLoadedData(data);
                var stashes = loot.Settings.Stashes;
                True(stashes != null, "блок настроек");
                Equal(3, stashes!.Pools.Count, "пулов по тирам");
                Equal(0, stashes.Pools[0].Tier, "первый тир");
                Equal(2, stashes.Pools[2].Tier, "последний тир");
                True(loot.TryGet(stashes.JackpotLootTableId, out var jackpot), "таблица джекпота");
                foreach (var entry in jackpot.Entries)
                {
                    True(items.TryGet(entry.ItemId, out var template), "предмет " + entry.ItemId);
                    True(template.Rarity >= Rarity.Special, "редкость джекпота " + entry.ItemId);
                }
            });
        }

        private string OpenSnapshot()
        {
            var (world, _, first, _) = NewWorld();
            first.MarkStashPresent();
            var service = NewService(Stashes(0.05, 0.02, 0.5, 0, 0, 1.0));
            var result = service.Open(world, first);
            var parts = new List<string>();
            foreach (var item in result.Loot)
            {
                parts.Add(item.ItemId + ":" + item.Stack);
            }

            return string.Join(",", parts);
        }

        private StashService NewService(StashSettings stashes)
        {
            var settings = new LootSettings(Bands(), stashes);
            var repository = new LootRepository(Tables(), settings);
            var items = Catalog((PoolItem, 1), (JackpotItem, 4));
            var loot = new LootService(repository, items, new SequentialInstanceIdGenerator());
            return new StashService(repository, loot);
        }

        private static LootTable[] Tables() =>
            new[]
            {
                new LootTable(
                    PoolTable,
                    Array.Empty<string>(),
                    new[] { new LootEntry(PoolItem, 0, true, 1, 1, true) }),
                new LootTable(
                    "loot_pool_2_test",
                    Array.Empty<string>(),
                    new[] { new LootEntry(PoolItem, 0, true, 1, 1, true) }),
                new LootTable(
                    JackpotTable,
                    Array.Empty<string>(),
                    new[] { new LootEntry(JackpotItem, 0, true, 1, 1, true) })
            };

        private static StashSettings Stashes(
            double baseChance,
            double growth,
            double cap,
            double jackpotBase,
            double jackpotGrowth,
            double jackpotCap) =>
            new StashSettings(
                baseChance,
                growth,
                cap,
                jackpotBase,
                jackpotGrowth,
                jackpotCap,
                new[]
                {
                    new StashTierPool(1, PoolTable),
                    new StashTierPool(2, "loot_pool_2_test")
                },
                JackpotTable);

        private static (WorldState World, LocationInstance Hub, LocationInstance First, LocationInstance Second)
            NewWorld()
        {
            var hub = new LocationInstance("loc_hub", "loc_hub", 1, "bg", LocationSubtype.Hub, 0, 0.1);
            var first = new LocationInstance("loc_1", "loc_1", 11, "bg", LocationSubtype.Path, 1, 0.2);
            var second = new LocationInstance("loc_2", "loc_2", 12, "bg", LocationSubtype.Building, 2, 0.3);
            var map = new WorldMap(
                42,
                new[] { hub, first, second },
                Array.Empty<WorldEdge>(),
                "loc_hub",
                10,
                6,
                22);
            var world = new WorldState(map, "loc_hub", 10, 10, new WorldClock(1, 12, 6, 22));
            return (world, hub, first, second);
        }

        private ItemRepository Catalog(params (string Id, int Tier)[] items)
        {
            var templates = new List<ItemTemplate>(items.Length);
            foreach (var item in items)
            {
                templates.Add(Template(ItemJson(item.Id, item.Tier)));
            }

            return new ItemRepository(templates);
        }

        private static TierBand[] Bands() =>
            new[]
            {
                new TierBand(-1, 0.2),
                new TierBand(0, 0.6),
                new TierBand(1, 0.2)
            };

        private static string LootJson(string? stashes)
        {
            var block = stashes == null ? string.Empty : ",\"stashes\":" + stashes;
            return "{\"kind\":\"loot\",\"id\":\"loot\",\"tierBands\":[" +
                   "{\"offset\":-1,\"chance\":0.2},{\"offset\":0,\"chance\":0.6}," +
                   "{\"offset\":1,\"chance\":0.2}]" + block + "}";
        }

        private static string StashJson(
            double baseChance = 0.05,
            double chanceGrowth = 0.02)
        {
            return "{\"baseChance\":" + Number(baseChance) +
                   ",\"chanceGrowth\":" + Number(chanceGrowth) +
                   ",\"chanceCap\":0.5,\"jackpotBaseChance\":0.01," +
                   "\"jackpotChanceGrowth\":0.01,\"jackpotChanceCap\":1.0," +
                   "\"pools\":[{\"tier\":1,\"lootTableId\":\"" + PoolTable + "\"}," +
                   "{\"tier\":2,\"lootTableId\":\"loot_pool_2_test\"}]," +
                   "\"jackpotLootTableId\":\"" + JackpotTable + "\"}";
        }

        private static string Number(double value) => value.ToString(CultureInfo.InvariantCulture);

        private static string ItemJson(string id, int tier)
        {
            return "{\"kind\":\"items\",\"id\":\"" + id + "\",\"nameKey\":\"n\"," +
                   "\"category\":\"trophy\",\"footprint\":[1,1],\"maxStack\":1,\"tier\":" +
                   tier + ",\"icon\":\"i\"}";
        }

        private static void Near(double expected, double actual, string what)
        {
            if (Math.Abs(expected - actual) > 1e-9)
            {
                throw new InvalidOperationException(what + ": ожидалось " + expected + ", получено " + actual);
            }
        }
    }
}
