#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Loot;
using AnomalyPath.Core.Quests;
using AnomalyPath.Core.Talents;

namespace AnomalyPath.Tests
{
    public sealed class FileFormatTests : TestBase
    {



        public override void Run()
        {
            Check("формат: сущность прямо из каталога", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/wpn_x.json"),
                        ["items/wpn_x.json"] = Weapon("wpn_x")
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("items").Count, "items");
                    });
            });

            Check("формат: коллекция с вложенной коллекцией", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/col_a.json"),
                        ["items/col_a.json"] = CollectionWithInclude("items/a.json", "items/col_b.json"),
                        ["items/col_b.json"] = Collection("items/b.json"),
                        ["items/a.json"] = Weapon("wpn_a"),
                        ["items/b.json"] = Weapon("wpn_b")
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(2, data.All("items").Count, "items");
                    });
            });

            Check("формат: абстрактный шаблон не регистрируется, но наследуется", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/wpn_x.json"),
                        ["_base/base.json"] = Abstract("{\"sellable\":true}"),
                        ["items/wpn_x.json"] = Weapon("wpn_x", extends: "_base/base.json")
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("items").Count, "items");
                        True(data.OfKind("items")["wpn_x"].Get("sellable").Bool, "sellable из базы");
                        False(data.TryGet("items", "base", out _), "абстрактный файл не должен регистрироваться");
                    });
            });

            Check("формат: признак unlosable наследуется и перекрывается", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/wpn_a.json", "items/wpn_b.json"),
                        ["_base/base.json"] = Abstract("{\"unlosable\":true}"),
                        ["items/wpn_a.json"] = Weapon("wpn_a", extends: "_base/base.json"),
                        ["items/wpn_b.json"] = Weapon("wpn_b", extends: "_base/base.json", extra: ",\"unlosable\":false")
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        True(data.OfKind("items")["wpn_a"].Get("unlosable").Bool, "унаследован");
                        False(data.OfKind("items")["wpn_b"].Get("unlosable").Bool, "перекрыт");
                    });
            });

            Check("формат: null удаляет унаследованное поле", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/wpn_x.json"),
                        ["_base/base.json"] = Abstract("{\"sellable\":true}"),
                        ["items/wpn_x.json"] = Weapon("wpn_x", extends: "_base/base.json", extra: ",\"sellable\":null")
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        False(data.OfKind("items")["wpn_x"].TryGet("sellable", out _), "sellable должен исчезнуть");
                    });
            });

            Check("формат: массивы заменяются, а не сливаются", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/wpn_x.json"),
                        ["_base/base.json"] = Abstract("{\"tags\":[\"a\",\"b\"]}"),
                        ["items/wpn_x.json"] = Weapon("wpn_x", extends: "_base/base.json", extra: ",\"tags\":[\"c\"]")
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        var tags = data.OfKind("items")["wpn_x"].Get("tags");
                        Equal(1, tags.Items.Count, "tags");
                        Equal("c", tags.Items[0].Text, "tag");
                    });
            });

            Check("формат: тип урона грузится как сущность damageTypes", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("damage/dmg_x.json"),
                        ["damage/dmg_x.json"] = "{ \"kind\":\"damageTypes\",\"id\":\"dmg_x\",\"nameKey\":\"n\",\"damageKind\":\"anomaly\",\"stackable\":true }"
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("damageTypes").Count, "damageTypes");
                        Equal("anomaly", data.OfKind("damageTypes")["dmg_x"].Get("damageKind").Text, "вид");
                    });
            });

            Check("формат: формула грузится как сущность formulas", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("formulas/f.json"),
                        ["formulas/f.json"] = "{ \"kind\":\"formulas\",\"id\":\"f\",\"value\":7,\"variables\":[\"x\"],\"result\":\"int\" }"
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("formulas").Count, "formulas");
                    });
            });

            Check("формат: локация грузится как сущность locations", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("locations/loc_x.json"),
                        ["locations/loc_x.json"] = "{\"schemaVersion\":1,\"kind\":\"locations\",\"id\":\"loc_x\"," +
                                                   "\"titleKey\":\"t\",\"locationKind\":\"wild\",\"nodeType\":\"route\"," +
                                                   "\"tags\":[\"road\",\"outdoor\"],\"tier\":0,\"backgrounds\":[\"bg\"]," +
                                                   "\"objectCount\":[1,2],\"danger\":0.2,\"connections\":[]}"
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("locations").Count, "locations");
                        Equal("route", data.OfKind("locations")["loc_x"].Get("nodeType").Text, "роль");
                    });
            });

            Check("формат: топология грузится как сущность topology", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("topology.json"),
                        ["topology.json"] = "{\"schemaVersion\":1,\"kind\":\"topology\",\"id\":\"topology\"," +
                                            "\"transitions\":[{\"from\":\"hub\",\"to\":\"route\",\"via\":[\"road\"]}]," +
                                            "\"routeLength\":[1,2],\"edgeLength\":[1,2]," +
                                            "\"costs\":{\"road\":{\"stamina\":2,\"hours\":1}}," +
                                            "\"maxBranchDepth\":4,\"initialStamina\":10," +
                                            "\"dayStartHour\":6,\"nightStartHour\":22}"
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("topology").Count, "topology");
                        Equal(2, data.OfKind("topology")["topology"].Get("edgeLength").Items[1].Number, "edgeLength");
                    });
            });

            Check("формат: набор грузится как сущность loadouts", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("loadouts/loadout_x.json"),
                        ["loadouts/loadout_x.json"] = "{\"schemaVersion\":1,\"kind\":\"loadouts\"," +
                                                       "\"id\":\"loadout_x\",\"nameKey\":\"n\"," +
                                                       "\"items\":[{\"itemId\":\"wpn_pm\",\"count\":1}]," +
                                                       "\"equipment\":{\"main\":\"wpn_pm\"}}"
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("loadouts").Count, "loadouts");
                        Equal("loadouts", data.OfKind("loadouts")["loadout_x"].Get("kind").Text, "kind");
                    });
            });

            Check("формат: правила грузятся как сущность rules", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("rules.json"),
                        ["rules.json"] = "{\"schemaVersion\":1,\"kind\":\"rules\",\"id\":\"rules\"," +
                                         "\"insuranceChance\":0.15," +
                                         "\"insuranceStep\":0.05,\"baseRows\":4,\"tierCount\":4}"
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("rules").Count, "rules");
                        Equal(0.05, data.OfKind("rules")["rules"].Get("insuranceStep").Number, "insuranceStep");
                    });
            });

            Check("формат: торговец грузится как сущность actors", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("actors/trd_x.json"),
                        ["actors/trd_x.json"] = "{\"schemaVersion\":1,\"kind\":\"actors\",\"id\":\"trd_x\"," +
                                                "\"type\":\"trader\",\"nameKey\":\"n\"," +
                                                "\"portrait\":\"portraits/trd_x\",\"portraitMode\":\"waist\"," +
                                                "\"roles\":[\"trader\"],\"trader\":{\"restockHours\":12," +
                                                "\"playerBuyModifier\":1.1,\"playerSellModifier\":0.5," +
                                                "\"assortment\":[{\"itemId\":\"wpn_x\",\"stock\":2," +
                                                "\"requirements\":{\"quests\":[\"quest_a\"]}}]}}"
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("actors").Count, "actors");
                        Equal("trader", data.OfKind("actors")["trd_x"].Get("type").Text, "тип");
                    });
            });

            Check("формат: атака врага — оружие и естественное оружие", () =>
            {
                var armed = AnomalyPath.Core.Combat.ActorTemplate.FromJson(
                    AnomalyPath.Core.Json.JsonParser.Parse(
                        "{\"kind\":\"actors\",\"id\":\"enemy_a\",\"type\":\"enemy\",\"nameKey\":\"n\"," +
                        "\"combat\":{\"tier\":1,\"level\":1,\"health\":10,\"ap\":5,\"xp\":10," +
                        "\"ai\":\"aggressive\",\"accuracy\":0.7,\"attack\":{\"weaponId\":\"wpn_akm\"}}}",
                        "test"),
                    "test");
                Equal("wpn_akm", armed.Combat!.Attack!.WeaponId, "оружие");
                False(armed.Combat.Attack.IsNatural, "атака оружием");

                var natural = AnomalyPath.Core.Combat.ActorTemplate.FromJson(
                    AnomalyPath.Core.Json.JsonParser.Parse(
                        "{\"kind\":\"actors\",\"id\":\"enemy_b\",\"type\":\"enemy\",\"nameKey\":\"n\"," +
                        "\"combat\":{\"tier\":1,\"level\":1,\"health\":10,\"ap\":5,\"xp\":10," +
                        "\"ai\":\"aggressive\",\"accuracy\":0.7," +
                        "\"attack\":{\"damage\":8,\"damageTypeId\":\"dmg_rupture\",\"range\":\"melee\"}}}",
                        "test"),
                    "test");
                True(natural.Combat!.Attack!.IsNatural, "естественная атака");
                Equal(8.0, natural.Combat.Attack.Damage, "урон");
                Equal("dmg_rupture", natural.Combat.Attack.DamageTypeId, "тип урона");
                Equal(AnomalyPath.Core.Combat.DistanceZone.Melee, natural.Combat.Attack.Range, "дистанция");
            });

            Check("формат: локация с привязкой торговцев", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("locations/loc_hub.json"),
                        ["locations/loc_hub.json"] = "{\"schemaVersion\":1,\"kind\":\"locations\",\"id\":\"loc_hub\"," +
                                                     "\"titleKey\":\"t\",\"locationKind\":\"hub\",\"nodeType\":\"hub\"," +
                                                     "\"tags\":[\"hub\"],\"tier\":0,\"backgrounds\":[\"bg\"]," +
                                                     "\"objectCount\":[0,0],\"connections\":[]," +
                                                     "\"traders\":[\"trd_x\",\"trd_y\"]}"
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(2, data.OfKind("locations")["loc_hub"].Get("traders").Items.Count, "traders");
                    });
            });

            Check("формат: экономика грузится как сущность economy", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("economy.json"),
                        ["economy.json"] = "{\"schemaVersion\":1,\"kind\":\"economy\",\"id\":\"economy\"," +
                                           "\"rounding\":5,\"minPrice\":10}"
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("economy").Count, "economy");
                        Equal(5.0, data.OfKind("economy")["economy"].Get("rounding").Number, "rounding");
                    });
            });

            Check("формат: квест грузится как сущность quests", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("quests/qst_x.json"),
                        ["quests/qst_x.json"] = "{\"schemaVersion\":1,\"kind\":\"quests\",\"id\":\"qst_x\"," +
                                                "\"questKind\":\"repeatable\",\"questType\":\"killMutants\"," +
                                                "\"titleKey\":\"t\",\"descriptionKey\":\"d\",\"difficulty\":1," +
                                                "\"target\":{\"count\":2,\"poolSize\":1,\"minTier\":1,\"maxTier\":1}," +
                                                "\"rewards\":{\"money\":100}}"
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("quests").Count, "quests");
                        var template = QuestTemplate.FromJson(
                            data.OfKind("quests")["qst_x"],
                            "quests/qst_x.json");
                        Equal(QuestType.KillMutants, template.Type, "тип");
                        Equal(2, template.KillMutants!.Count, "количество");
                        Equal(100, template.Rewards.Money, "награда");
                    });
            });

            Check("формат: дерево талантов с вложенными эффектами", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("talents.json"),
                        ["talents.json"] = "{\"schemaVersion\":1,\"kind\":\"talents\",\"id\":\"talents\"," +
                                           "\"titleKey\":\"t\",\"descriptionKey\":\"d\",\"nodes\":[" +
                                           "{\"id\":\"tal_a\",\"titleKey\":\"a\",\"descriptionKey\":\"b\"," +
                                           "\"icon\":\"icons/tal_a\",\"row\":0,\"column\":0,\"maxRank\":2," +
                                           "\"requiredPoints\":0," +
                                           "\"effects\":[{\"stat\":\"apRestore\",\"perRank\":1}]}]}"
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("talents").Count, "talents");
                        var tree = TalentTreeTemplate.FromJson(data.OfKind("talents")["talents"], "talents");
                        Equal(1, tree.Nodes.Count, "талантов");
                        Equal(2, tree.Nodes[0].MaxRank, "рангов");
                        Equal("icons/tal_a", tree.Nodes[0].Icon, "картинка");
                        Equal(TalentStat.ApRestore, tree.Nodes[0].Effects[0].Stat, "характеристика");
                        Equal(1.0, tree.Nodes[0].Effects[0].PerRank, "прибавка");
                    });
            });

            Check("формат: таблица лута и настройка полос", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("loot.json", "loot/loot_tbl.json"),
                        ["loot.json"] = "{\"schemaVersion\":1,\"kind\":\"loot\",\"id\":\"loot\"," +
                                        "\"tierBands\":[{\"offset\":-1,\"chance\":0.2}," +
                                        "{\"offset\":0,\"chance\":0.6},{\"offset\":1,\"chance\":0.2}]}",
                        ["loot/loot_tbl.json"] = "{\"schemaVersion\":1,\"kind\":\"lootTables\",\"id\":\"loot_tbl\"," +
                                                 "\"sharedTableIds\":[\"loot_common\"],\"entries\":[" +
                                                 "{\"itemId\":\"itm_a\",\"chance\":0.5,\"count\":[1,3]}," +
                                                 "{\"itemId\":\"itm_b\",\"always\":true}]}"
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("lootTables").Count, "lootTables");
                        var table = LootTable.FromJson(data.OfKind("lootTables")["loot_tbl"], "loot");
                        Equal("loot_tbl", table.Id, "id таблицы");
                        Equal(1, table.SharedTableIds.Count, "общих таблиц");
                        Equal(2, table.Entries.Count, "записей");
                        Equal("itm_a", table.Entries[0].ItemId, "предмет");
                        Equal(3, table.Entries[0].CountMax, "максимум стопки");
                        True(table.Entries[1].Always, "always");
                        var settings = LootSettings.FromJson(data.OfKind("loot")["loot"], "loot");
                        Equal(3, settings.Bands.Count, "полос");
                    });
            });

            Check("формат: повторная коллекция — ошибка", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/col_a.json"),
                        ["items/col_a.json"] = CollectionWithInclude("items/a.json", "items/col_a.json"),
                        ["items/a.json"] = Weapon("wpn_a")
                    },
                    root => Throws<DataException>(() => new DataLoader(new FileSystemDataSource(root)).Load(), "цикл"));
            });
        }
    }
}
