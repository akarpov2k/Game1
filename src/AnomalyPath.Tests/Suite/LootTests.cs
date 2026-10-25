#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Loot;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Services;
using AnomalyPath.Core.Templates;

namespace AnomalyPath.Tests
{
    /// <summary>Лут: редкость, полосы тира, таблицы, детерминированный бросок и выдача.</summary>
    public sealed class LootTests : TestBase
    {
        /// <inheritdoc />
        public override void Run()
        {
            RarityTests();
            SettingsTests();
            EntryTests();
            TableTests();
            ServiceTests();
        }

        private void RarityTests()
        {
            Check("лут: редкость — числовые значения ступеней", () =>
            {
                Equal(1, (int)Rarity.Common, "обычная");
                Equal(2, (int)Rarity.Rare, "редкая");
                Equal(3, (int)Rarity.Special, "особая");
                Equal(4, (int)Rarity.Epic, "эпическая");
                Equal(5, (int)Rarity.Legendary, "легендарная");
                Equal(6, (int)Rarity.Unique, "уникальная");
            });

            Check("лут: код редкости читается и пишется", () =>
            {
                True(SchemaEnum.TryParse("epic", out Rarity rarity) && rarity == Rarity.Epic, "epic");
                Equal("legendary", SchemaEnum.ToCode(Rarity.Legendary), "код");
                False(SchemaEnum.TryParse<Rarity>("mythic", out _), "неизвестный код");
            });

            Check("лут: у предмета редкость по умолчанию обычная", () =>
            {
                var template = Template(ItemJson("itm_a", 1));
                Equal(Rarity.Common, template.Rarity, "по умолчанию");
            });

            Check("лут: редкость предмета читается из данных", () =>
            {
                var template = Template(ItemJson("itm_a", 2, "special"));
                Equal(Rarity.Special, template.Rarity, "редкость");
            });

            Check("лут: неизвестная редкость предмета — ошибка", () =>
            {
                Throws<DataException>(() => Template(ItemJson("itm_a", 1, "mythic")), "редкость");
            });

            Check("лут: у врага читаются редкость и таблица лута", () =>
            {
                var actor = ActorTemplate.FromJson(JsonParser.Parse(EnemyJson("epic", "loot_x"), "actors"), "actors");
                Equal(Rarity.Epic, actor.Rarity, "редкость");
                Equal("loot_x", actor.LootTableId, "таблица");
            });

            Check("лут: таблица лута у не-врага — ошибка", () =>
            {
                var json = "{\"kind\":\"actors\",\"id\":\"npc_x\",\"type\":\"npc\",\"nameKey\":\"n\",\"lootTable\":\"loot_x\"}";
                Throws<DataException>(
                    () => ActorTemplate.FromJson(JsonParser.Parse(json, "actors"), "actors"),
                    "таблица у NPC");
            });

            Check("лут: редкость врага по умолчанию обычная", () =>
            {
                var actor = ActorTemplate.FromJson(JsonParser.Parse(EnemyJson(null, null), "actors"), "actors");
                Equal(Rarity.Common, actor.Rarity, "по умолчанию");
                True(actor.LootTableId == null, "таблицы нет");
            });
        }

        private void SettingsTests()
        {
            Check("лут: настройка полос читается из данных", () =>
            {
                var settings = LootSettings.FromJson(JsonParser.Parse(SettingsJson("loot"), "loot"), "loot");
                Equal(3, settings.Bands.Count, "полос");
                Equal(-1, settings.Bands[0].Offset, "сдвиг");
                Near(0.2, settings.Bands[0].Chance, "вклад");
            });

            Check("лут: полосы — тир 1 локации даёт 80/20", () =>
            {
                var settings = Bands();
                Near(0.8, settings.ChanceForTier(1, 1, 1, 4), "тир 1");
                Near(0.2, settings.ChanceForTier(2, 1, 1, 4), "тир 2");
                Near(0.0, settings.ChanceForTier(3, 1, 1, 4), "тир 3");
            });

            Check("лут: полосы — тир 2 локации даёт 20/60/20", () =>
            {
                var settings = Bands();
                Near(0.2, settings.ChanceForTier(1, 2, 1, 4), "тир 1");
                Near(0.6, settings.ChanceForTier(2, 2, 1, 4), "тир 2");
                Near(0.2, settings.ChanceForTier(3, 2, 1, 4), "тир 3");
                Near(0.0, settings.ChanceForTier(4, 2, 1, 4), "тир 4");
            });

            Check("лут: полосы — верхний край шкалы даёт 80/20", () =>
            {
                var settings = Bands();
                Near(0.2, settings.ChanceForTier(3, 4, 1, 4), "тир 3");
                Near(0.8, settings.ChanceForTier(4, 4, 1, 4), "тир 4");
            });

            Check("лут: полосы — хаб (тир 0) упирается в минимальный тир", () =>
            {
                var settings = Bands();
                Near(1.0, settings.ChanceForTier(1, 0, 1, 4), "тир 1");
                Near(0.0, settings.ChanceForTier(2, 0, 1, 4), "тир 2");
            });

            Check("лут: настройка полос — пустой список запрещён", () =>
            {
                Throws<ArgumentException>(() => new LootSettings(Array.Empty<TierBand>()), "пусто");
            });

            Check("лут: настройка полос — сумма не 1 запрещена", () =>
            {
                Throws<ArgumentException>(
                    () => new LootSettings(new[] { new TierBand(0, 0.5) }),
                    "сумма");
                Throws<DataException>(
                    () => LootSettings.FromJson(JsonParser.Parse(SettingsJson("loot", 0.5, 0.2, 0.2), "loot"), "loot"),
                    "сумма в данных");
            });

            Check("лут: настройка полос — шанс вне 0..1 запрещён", () =>
            {
                Throws<DataException>(
                    () => LootSettings.FromJson(JsonParser.Parse(SettingsJson("loot", 1.5, -0.5, 0.0), "loot"), "loot"),
                    "шанс");
            });

            Check("лут: настройка полос — id должен быть loot", () =>
            {
                Throws<DataException>(
                    () => LootSettings.FromJson(JsonParser.Parse(SettingsJson("other"), "loot"), "loot"),
                    "id");
            });

            Check("лут: настройка полос — пустой массив запрещён", () =>
            {
                var json = "{\"kind\":\"loot\",\"id\":\"loot\",\"tierBands\":[]}";
                Throws<DataException>(() => LootSettings.FromJson(JsonParser.Parse(json, "loot"), "loot"), "пусто");
            });

            Check("лут: пустая настройка не даёт ни одного шанса", () =>
            {
                Near(0.0, LootSettings.Empty.ChanceForTier(1, 1, 1, 4), "шанс");
                Equal(0, LootSettings.Empty.Bands.Count, "полос");
            });
        }

        private void EntryTests()
        {
            Check("лут: запись always падает без шанса", () =>
            {
                var entry = LootEntry.FromJson(
                    JsonParser.Parse("{\"itemId\":\"itm_a\",\"always\":true}", "loot"),
                    "loot");
                True(entry.Always, "always");
                Near(0.0, entry.Chance, "шанс не задан");
                Equal(1, entry.CountMin, "минимум по умолчанию");
                Equal(1, entry.CountMax, "максимум по умолчанию");
            });

            Check("лут: запись без always и без chance — ошибка", () =>
            {
                Throws<DataException>(
                    () => LootEntry.FromJson(JsonParser.Parse("{\"itemId\":\"itm_a\"}", "loot"), "loot"),
                    "шанс обязателен");
            });

            Check("лут: шанс записи вне 0..1 — ошибка", () =>
            {
                Throws<DataException>(
                    () => LootEntry.FromJson(
                        JsonParser.Parse("{\"itemId\":\"itm_a\",\"chance\":1.5}", "loot"),
                        "loot"),
                    "шанс");
            });

            Check("лут: диапазон количества записи проверяется", () =>
            {
                Throws<DataException>(
                    () => LootEntry.FromJson(
                        JsonParser.Parse("{\"itemId\":\"itm_a\",\"chance\":0.5,\"count\":[0,2]}", "loot"),
                        "loot"),
                    "минимум");
                Throws<DataException>(
                    () => LootEntry.FromJson(
                        JsonParser.Parse("{\"itemId\":\"itm_a\",\"chance\":0.5,\"count\":[3,2]}", "loot"),
                        "loot"),
                    "порядок");
            });

            Check("лут: количество и флаг полосы читаются из данных", () =>
            {
                var entry = LootEntry.FromJson(
                    JsonParser.Parse(
                        "{\"itemId\":\"itm_a\",\"chance\":0.4,\"count\":[2,4],\"ignoreTierBand\":true}",
                        "loot"),
                    "loot");
                Equal(2, entry.CountMin, "минимум");
                Equal(4, entry.CountMax, "максимум");
                True(entry.IgnoreTierBand, "полоса отключена");
                Near(0.4, entry.Chance, "шанс");
            });
        }

        private void TableTests()
        {
            Check("лут: таблица читает общие таблицы и записи", () =>
            {
                var json = "{\"kind\":\"lootTables\",\"id\":\"loot_x\",\"sharedTableIds\":[\"loot_common\"]," +
                           "\"entries\":[{\"itemId\":\"itm_a\",\"chance\":0.5}]}";
                var table = LootTable.FromJson(JsonParser.Parse(json, "loot"), "loot");
                Equal("loot_x", table.Id, "id");
                Equal(1, table.SharedTableIds.Count, "общих таблиц");
                Equal(1, table.Entries.Count, "записей");
            });

            Check("лут: таблица без записей допустима", () =>
            {
                var table = LootTable.FromJson(
                    JsonParser.Parse("{\"kind\":\"lootTables\",\"id\":\"loot_x\"}", "loot"),
                    "loot");
                Equal(0, table.Entries.Count, "записей");
                Equal(0, table.SharedTableIds.Count, "общих таблиц");
            });

            Check("лут: записи таблицы должны быть массивом", () =>
            {
                Throws<DataException>(
                    () => LootTable.FromJson(
                        JsonParser.Parse("{\"kind\":\"lootTables\",\"id\":\"loot_x\",\"entries\":{}}", "loot"),
                        "loot"),
                    "массив");
            });

            Check("лут: репозиторий отклоняет повторный id таблицы", () =>
            {
                var table = Table("loot_x", Array.Empty<string>(), Entry("itm_a", 0.5));
                Throws<DataException>(() => new LootRepository(new[] { table, table }, Bands()), "дубль");
            });

            Check("лут: репозиторий ищет таблицу по id", () =>
            {
                var repository = new LootRepository(new[] { Table("loot_x", Array.Empty<string>(), Entry("itm_a", 0.5)) }, Bands());
                True(repository.TryGet("loot_x", out var table), "найдена");
                Equal("loot_x", table.Id, "id");
                False(repository.TryGet("loot_missing", out _), "чужая таблица");
                Throws<DataException>(() => repository.Get("loot_missing"), "Get чужой таблицы");
            });
        }

        private void ServiceTests()
        {
            Check("лут: запись always падает при любом seed", () =>
            {
                var service = Service(Table("loot_x", Array.Empty<string>(), Entry("itm_a", 0.0, always: true)));
                for (var seed = 1; seed <= 5; seed++)
                {
                    var loot = service.Roll(Context("loot_x", Rarity.Common, 1, seed));
                    Equal(1, loot.Count, "seed " + seed);
                    Equal("itm_a", loot[0].ItemId, "предмет");
                }
            });

            Check("лут: шанс выше 100% даёт ровно один экземпляр", () =>
            {
                var service = Service(Table("loot_x", Array.Empty<string>(), Entry("itm_a", 0.5)));
                for (var seed = 1; seed <= 10; seed++)
                {
                    var loot = service.Roll(Context("loot_x", Rarity.Epic, 1, seed));
                    Equal(1, loot.Count, "seed " + seed);
                    Equal(1, loot[0].Stack, "стопка");
                }
            });

            Check("лут: шанс умножается на редкость противника", () =>
            {
                var service = Service(Table("loot_x", Array.Empty<string>(), Entry("itm_a", 0.25, ignoreTierBand: true)));
                var common = CountDrops(service, Rarity.Common, 100);
                var legendary = CountDrops(service, Rarity.Legendary, 100);
                True(common > 0 && common < 100, "обычный: часть бросков проходит, " + common);
                Equal(100, legendary, "легендарный: 0.25 × 5 гарантирует предмет");
            });

            Check("лут: нулевой шанс не даёт предмета", () =>
            {
                var service = Service(Table("loot_x", Array.Empty<string>(), Entry("itm_a", 0.0)));
                Equal(0, CountDrops(service, Rarity.Unique, 20), "ни одного");
            });

            Check("лут: полоса тира отсекает предмет чужого тира", () =>
            {
                var items = Catalog(("itm_low", 1), ("itm_top", 4));
                var service = Service(
                    Table("loot_x", Array.Empty<string>(), Entry("itm_top", 1.0)),
                    items);
                Equal(0, CountDrops(service, Rarity.Common, 20), "тир 4 на локации тира 1");
            });

            Check("лут: полоса тира пропускает предмет соседнего тира", () =>
            {
                var items = Catalog(("itm_low", 1), ("itm_mid", 2));
                var service = Service(
                    Table("loot_x", Array.Empty<string>(), Entry("itm_mid", 1.0)),
                    items);
                var drops = CountDrops(service, Rarity.Common, 200);
                True(drops > 0 && drops < 200, "часть бросков проходит: " + drops);
            });

            Check("лут: общая таблица входит в пул", () =>
            {
                var common = Table("loot_common", Array.Empty<string>(), Entry("itm_a", 0.0, always: true));
                var table = Table("loot_x", new[] { "loot_common" }, Entry("itm_b", 0.0));
                var service = Service(common, table);
                var loot = service.Roll(Context("loot_x", Rarity.Common, 1, 1));
                Equal(1, loot.Count, "предмет из общей таблицы");
                Equal("itm_a", loot[0].ItemId, "id");
            });

            Check("лут: количество берётся из диапазона записи", () =>
            {
                var service = Service(Table("loot_x", Array.Empty<string>(), Entry("itm_a", 0.0, always: true, min: 2, max: 3)));
                for (var seed = 1; seed <= 10; seed++)
                {
                    var loot = service.Roll(Context("loot_x", Rarity.Common, 1, seed));
                    True(loot[0].Stack >= 2 && loot[0].Stack <= 3, "стопка " + loot[0].Stack);
                }
            });

            Check("лут: неизвестный предмет записи пропускается", () =>
            {
                var service = Service(Table("loot_x", Array.Empty<string>(), Entry("itm_missing", 1.0)));
                Equal(0, service.Roll(Context("loot_x", Rarity.Common, 1, 1)).Count, "предмета нет в каталоге");
            });

            Check("лут: отсутствующая таблица даёт пустой список", () =>
            {
                var service = Service(Table("loot_x", Array.Empty<string>(), Entry("itm_a", 1.0)));
                Equal(0, service.Roll(Context("loot_missing", Rarity.Common, 1, 1)).Count, "чужой id");
                Equal(0, service.Roll(new LootContext("enemy_x", Rarity.Common, null, 1, 1)).Count, "нет таблицы");
            });

            Check("лут: один seed даёт один и тот же лут", () =>
            {
                var service = Service(Table(
                    "loot_x",
                    Array.Empty<string>(),
                    Entry("itm_a", 0.5),
                    Entry("itm_b", 0.5)));
                var first = Describe(service.Roll(Context("loot_x", Rarity.Rare, 2, 7)));
                var second = Describe(service.Roll(Context("loot_x", Rarity.Rare, 2, 7)));
                Equal(first, second, "повтор с тем же seed");
            });

            Check("лут: разные seed дают разные наборы", () =>
            {
                var service = Service(Table(
                    "loot_x",
                    Array.Empty<string>(),
                    Entry("itm_a", 0.4, ignoreTierBand: true),
                    Entry("itm_b", 0.4, ignoreTierBand: true),
                    Entry("itm_c", 0.4, ignoreTierBand: true)));
                var seen = new HashSet<string>(StringComparer.Ordinal);
                for (var seed = 1; seed <= 30; seed++)
                {
                    seen.Add(Describe(service.Roll(Context("loot_x", Rarity.Common, 2, seed))));
                }

                True(seen.Count > 1, "наборы различаются");
            });

            Check("лут: игнорирование полосы тира пропускает чужой тир", () =>
            {
                var items = Catalog(("itm_top", 4));
                var service = Service(
                    Table("loot_x", Array.Empty<string>(), Entry("itm_top", 0.0, always: true, ignoreTierBand: true)),
                    items);
                Equal(1, service.Roll(Context("loot_x", Rarity.Common, 1, 1)).Count, "always без полосы");
            });
        }

        private int CountDrops(LootService service, Rarity rarity, int attempts)
        {
            var drops = 0;
            for (var seed = 1; seed <= attempts; seed++)
            {
                drops += service.Roll(Context("loot_x", rarity, 1, seed)).Count;
            }

            return drops;
        }

        private string Describe(IReadOnlyList<ItemInstance> loot)
        {
            var parts = new List<string>();
            foreach (var item in loot)
            {
                parts.Add(item.ItemId + ":" + item.Stack);
            }

            return string.Join(",", parts);
        }

        private LootService Service(params LootTable[] tables) =>
            Service(tables, Catalog(("itm_a", 1), ("itm_b", 1), ("itm_c", 1)));

        private LootService Service(LootTable table, ItemRepository items) => Service(new[] { table }, items);

        private LootService Service(LootTable[] tables, ItemRepository items) =>
            new LootService(new LootRepository(tables, Bands()), items, new SequentialInstanceIdGenerator());

        private ItemRepository Catalog(params (string Id, int Tier)[] items)
        {
            var templates = new List<ItemTemplate>(items.Length);
            foreach (var item in items)
            {
                templates.Add(Template(ItemJson(item.Id, item.Tier)));
            }

            return new ItemRepository(templates);
        }

        private static LootSettings Bands() =>
            new LootSettings(new[]
            {
                new TierBand(-1, 0.2),
                new TierBand(0, 0.6),
                new TierBand(1, 0.2)
            });

        private static LootContext Context(string tableId, Rarity rarity, int tier, int seed) =>
            new LootContext("enemy_x", rarity, tableId, tier, seed);

        private static LootEntry Entry(
            string itemId,
            double chance,
            bool always = false,
            int min = 1,
            int max = 1,
            bool ignoreTierBand = false) =>
            new LootEntry(itemId, chance, always, min, max, ignoreTierBand);

        private static LootTable Table(string id, IReadOnlyList<string> shared, params LootEntry[] entries) =>
            new LootTable(id, shared, entries);

        private static void Near(double expected, double actual, string what)
        {
            if (Math.Abs(expected - actual) > 1e-9)
            {
                throw new InvalidOperationException(what + ": ожидалось " + expected + ", получено " + actual);
            }
        }

        private static string ItemJson(string id, int tier, string? rarity = null)
        {
            var rarityPart = rarity == null ? string.Empty : "\"rarity\":\"" + rarity + "\",";
            return "{\"kind\":\"items\",\"id\":\"" + id + "\",\"nameKey\":\"n\",\"category\":\"trophy\"," +
                   "\"footprint\":[1,1],\"maxStack\":1,\"tier\":" + tier + "," + rarityPart + "\"icon\":\"i\"}";
        }

        private static string EnemyJson(string? rarity, string? lootTable)
        {
            var rarityPart = rarity == null ? string.Empty : "\"rarity\":\"" + rarity + "\",";
            var lootPart = lootTable == null ? string.Empty : "\"lootTable\":\"" + lootTable + "\",";
            return "{\"kind\":\"actors\",\"id\":\"enemy_x\",\"type\":\"enemy\",\"nameKey\":\"n\"," +
                   rarityPart + lootPart +
                   "\"combat\":{\"tier\":1,\"level\":1,\"health\":10,\"ap\":1,\"xp\":1,\"ai\":\"aggressive\",\"accuracy\":0.5," +
                   "\"attack\":{\"damage\":1,\"damageTypeId\":\"dmg_bullet\"}}}";
        }

        private static string SettingsJson(string id, double a = 0.2, double b = 0.6, double c = 0.2) =>
            "{\"kind\":\"loot\",\"id\":\"" + id + "\",\"tierBands\":[" +
            "{\"offset\":-1,\"chance\":" + a.ToString(CultureInfo.InvariantCulture) + "}," +
            "{\"offset\":0,\"chance\":" + b.ToString(CultureInfo.InvariantCulture) + "}," +
            "{\"offset\":1,\"chance\":" + c.ToString(CultureInfo.InvariantCulture) + "}]}";
    }
}
