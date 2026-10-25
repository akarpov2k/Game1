#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Loot;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Services;
using AnomalyPath.Core.Templates;
using AnomalyPath.Core.World;

namespace AnomalyPath.Tests
{
    /// <summary>Джекпот тайника: отдельный шанс, пул вне тиров, число стопок и детерминизм.</summary>
    public sealed class JackpotTests : TestBase
    {
        private const string PoolTable = "loot_pool_test";
        private const string JackpotTable = "loot_jackpot_test";
        private const string PoolItem = "itm_pool";
        private const string JackpotItem = "itm_jackpot";
        private const string JackpotItemTwo = "itm_jackpot_two";

        /// <inheritdoc />
        public override void Run()
        {
            ChanceTests();
            LootTests();
            DeterminismTests();
        }

        private void ChanceTests()
        {
            Check("джекпот: нулевой шанс не срабатывает", () =>
            {
                var (world, location) = NewState();
                location.MarkStashPresent();
                var service = NewService(Stashes(0, 0, 1.0));
                var result = service.Open(world, location);
                False(result.Jackpot, "джекпота нет");
                Equal(PoolTable, result.LootTableId, "обычный пул");
            });

            Check("джекпот: гарантированный шанс заменяет пул", () =>
            {
                var (world, location) = NewState();
                location.MarkStashPresent();
                var service = NewService(Stashes(1.0, 0, 1.0));
                var result = service.Open(world, location);
                True(result.Jackpot, "джекпот");
                Equal(JackpotTable, result.LootTableId, "пул джекпота");
            });

            Check("джекпот: срабатывание сбрасывает прогресс", () =>
            {
                var (world, location) = NewState();
                location.MarkStashPresent();
                var service = NewService(Stashes(1.0, 0.1, 1.0));
                True(service.Open(world, location).Jackpot, "джекпот");
                Equal(0, world.StashOpenings, "прогресс сброшен");
            });

            Check("джекпот: шанс растёт с каждым вскрытием", () =>
            {
                var (world, first, second) = NewStatePair();
                first.MarkStashPresent();
                second.MarkStashPresent();
                var service = NewService(Stashes(0, 1.0, 1.0));
                False(service.Open(world, first).Jackpot, "первое вскрытие");
                Equal(1, world.StashOpenings, "прогресс");
                True(service.Open(world, second).Jackpot, "второе вскрытие");
                Equal(0, world.StashOpenings, "прогресс сброшен");
            });
        }

        private void LootTests()
        {
            Check("джекпот: пул не привязан к тиру локации", () =>
            {
                var (world, location) = NewState();
                location.MarkStashPresent();
                var service = NewService(Stashes(1.0, 0, 1.0));
                var result = service.Open(world, location);
                Equal(2, result.Loot.Count, "стопки джекпота");
                var ids = new HashSet<string>(StringComparer.Ordinal);
                foreach (var item in result.Loot)
                {
                    ids.Add(item.ItemId);
                    Equal(1, item.Stack, "по одному предмету в стопке");
                }

                True(ids.Contains(JackpotItem), "первый предмет");
                True(ids.Contains(JackpotItemTwo), "второй предмет");
            });

            Check("джекпот: без таблицы лут пуст, но тайник вскрыт", () =>
            {
                var (world, location) = NewState();
                location.MarkStashPresent();
                var service = NewService(Stashes(1.0, 0, 1.0), includeJackpotTable: false);
                var result = service.Open(world, location);
                True(result.Jackpot, "джекпот");
                Equal(0, result.Loot.Count, "лута нет");
                True(location.StashOpened, "тайник вскрыт");
            });

            Check("джекпот: обычный пул не зависит от шанса джекпота", () =>
            {
                var (world, location) = NewState();
                location.MarkStashPresent();
                var service = NewService(Stashes(0, 0, 1.0));
                var result = service.Open(world, location);
                Equal(1, result.Loot.Count, "одна стопка пула");
                Equal(PoolItem, result.Loot[0].ItemId, "предмет пула");
            });
        }

        private void DeterminismTests()
        {
            Check("джекпот: одинаковое состояние даёт одинаковый результат", () =>
            {
                var first = Snapshot();
                var second = Snapshot();
                Equal(first, second, "повтор");
            });

            Check("джекпот: seed броска входит в номер броска лута", () =>
            {
                var (world, location) = NewState();
                location.MarkStashPresent();
                var service = NewService(Stashes(1.0, 0, 1.0));
                service.Open(world, location);
                Equal(1, world.LootSerial, "номер вырос");
            });
        }

        private string Snapshot()
        {
            var (world, location) = NewState();
            location.MarkStashPresent();
            var service = NewService(Stashes(1.0, 0, 1.0));
            var result = service.Open(world, location);
            var parts = new List<string>();
            foreach (var item in result.Loot)
            {
                parts.Add(item.ItemId + ":" + item.Stack);
            }

            return (result.Jackpot ? "jackpot:" : "normal:") + string.Join(",", parts);
        }

        private StashService NewService(StashSettings stashes, bool includeJackpotTable = true)
        {
            var settings = new LootSettings(Bands(), stashes);
            var repository = new LootRepository(Tables(includeJackpotTable), settings);
            var items = Catalog();
            var loot = new LootService(repository, items, new SequentialInstanceIdGenerator());
            return new StashService(repository, loot);
        }

        private static LootTable[] Tables(bool includeJackpotTable)
        {
            var tables = new List<LootTable>
            {
                new LootTable(
                    PoolTable,
                    Array.Empty<string>(),
                    new[] { new LootEntry(PoolItem, 0, true, 1, 1, true) })
            };
            if (includeJackpotTable)
            {
                tables.Add(new LootTable(
                    JackpotTable,
                    Array.Empty<string>(),
                    new[]
                    {
                        new LootEntry(JackpotItem, 0, true, 1, 1, true),
                        new LootEntry(JackpotItemTwo, 0, true, 1, 1, true)
                    }));
            }

            return tables.ToArray();
        }

        private static StashSettings Stashes(
            double jackpotBase,
            double jackpotGrowth,
            double jackpotCap) =>
            new StashSettings(
                0.05,
                0.02,
                0.5,
                jackpotBase,
                jackpotGrowth,
                jackpotCap,
                new[] { new StashTierPool(0, PoolTable) },
                JackpotTable);

        private ItemRepository Catalog()
        {
            var templates = new List<ItemTemplate>
            {
                Template(ItemJson(PoolItem, 0)),
                Template(ItemJson(JackpotItem, 4)),
                Template(ItemJson(JackpotItemTwo, 4))
            };
            return new ItemRepository(templates);
        }

        private static (WorldState World, LocationInstance Location) NewState() =>
            (NewWorld(), new LocationInstance("loc_1", "loc_1", 11, "bg", LocationSubtype.Path, 0, 0.2));

        private static (WorldState World, LocationInstance First, LocationInstance Second) NewStatePair()
        {
            var world = NewWorld();
            return (
                world,
                new LocationInstance("loc_1", "loc_1", 11, "bg", LocationSubtype.Path, 0, 0.2),
                new LocationInstance("loc_2", "loc_2", 12, "bg", LocationSubtype.Path, 0, 0.2));
        }

        private static WorldState NewWorld()
        {
            var hub = new LocationInstance("loc_hub", "loc_hub", 1, "bg", LocationSubtype.Hub, 0, 0.1);
            var map = new WorldMap(
                42,
                new[] { hub },
                Array.Empty<WorldEdge>(),
                "loc_hub",
                10,
                6,
                22);
            return new WorldState(map, "loc_hub", 10, 10, new WorldClock(1, 12, 6, 22));
        }

        private static TierBand[] Bands() =>
            new[]
            {
                new TierBand(-1, 0.2),
                new TierBand(0, 0.6),
                new TierBand(1, 0.2)
            };

        private static string ItemJson(string id, int tier)
        {
            return "{\"kind\":\"items\",\"id\":\"" + id + "\",\"nameKey\":\"n\"," +
                   "\"category\":\"trophy\",\"footprint\":[1,1],\"maxStack\":1,\"tier\":" +
                   tier + ",\"icon\":\"i\"}";
        }
    }
}
