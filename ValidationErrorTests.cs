#nullable enable
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Core;
using AnomalyPath.Core.Validation;

namespace AnomalyPath.Tests
{
    public sealed class ValidationErrorTests : TestBase
    {
        private readonly string _dataRoot;
        public ValidationErrorTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        public override void Run()
        {
            ErrorTests(); ErrorBehaviorTests(); TraderErrorTests(); QuestErrorTests(); TalentErrorTests(); LootErrorTests();
        }

        private void TraderErrorTests()
        {
            var cases = new (string Name, string Location, string Actor, string Fragment)[]
            {
                (
                    "ошибка: торговец не привязан к хабу",
                    "{\"schemaVersion\":1,\"kind\":\"locations\",\"id\":\"loc_hub\"," +
                    "\"titleKey\":\"t\",\"locationKind\":\"hub\",\"nodeType\":\"hub\"," +
                    "\"backgrounds\":[\"bg\"],\"objectCount\":[0,0],\"connections\":[]}",
                    "{\"schemaVersion\":1,\"kind\":\"actors\",\"id\":\"trd_x\",\"type\":\"trader\"," +
                    "\"nameKey\":\"n\",\"roles\":[\"trader\"],\"trader\":{\"restockHours\":12," +
                    "\"playerBuyModifier\":1,\"playerSellModifier\":0.5," +
                    "\"assortment\":[{\"itemId\":\"wpn_x\",\"stock\":1}]}}",
                    "не привязан ни к одному хабу"),
                (
                    "ошибка: торговец привязан не к хабу",
                    "{\"schemaVersion\":1,\"kind\":\"locations\",\"id\":\"loc_route\"," +
                    "\"titleKey\":\"t\",\"locationKind\":\"wild\",\"nodeType\":\"route\"," +
                    "\"backgrounds\":[\"bg\"],\"objectCount\":[0,0],\"connections\":[]," +
                    "\"traders\":[\"trd_x\"]}",
                    "{\"schemaVersion\":1,\"kind\":\"actors\",\"id\":\"trd_x\",\"type\":\"trader\"," +
                    "\"nameKey\":\"n\",\"roles\":[\"trader\"],\"trader\":{\"restockHours\":12," +
                    "\"playerBuyModifier\":1,\"playerSellModifier\":0.5," +
                    "\"assortment\":[{\"itemId\":\"wpn_x\",\"stock\":1}]}}",
                    "небезопасной локации"),
                (
                    "ошибка: товар торговца не найден",
                    "{\"schemaVersion\":1,\"kind\":\"locations\",\"id\":\"loc_hub\"," +
                    "\"titleKey\":\"t\",\"locationKind\":\"hub\",\"nodeType\":\"hub\"," +
                    "\"backgrounds\":[\"bg\"],\"objectCount\":[0,0],\"connections\":[]," +
                    "\"traders\":[\"trd_x\"]}",
                    "{\"schemaVersion\":1,\"kind\":\"actors\",\"id\":\"trd_x\",\"type\":\"trader\"," +
                    "\"nameKey\":\"n\",\"roles\":[\"trader\"],\"trader\":{\"restockHours\":12," +
                    "\"playerBuyModifier\":1,\"playerSellModifier\":0.5," +
                    "\"assortment\":[{\"itemId\":\"missing\",\"stock\":1}]}}",
                    "не найден среди предметов"),
                (
                    "ошибка: у торговца нет блока торговли",
                    "{\"schemaVersion\":1,\"kind\":\"locations\",\"id\":\"loc_hub\"," +
                    "\"titleKey\":\"t\",\"locationKind\":\"hub\",\"nodeType\":\"hub\"," +
                    "\"backgrounds\":[\"bg\"],\"objectCount\":[0,0],\"connections\":[]}",
                    "{\"schemaVersion\":1,\"kind\":\"actors\",\"id\":\"trd_x\",\"type\":\"trader\"," +
                    "\"nameKey\":\"n\",\"roles\":[\"trader\"]}",
                    "нет блока 'trader'"),
                (
                    "ошибка: роль торговца у не-торговца",
                    "{\"schemaVersion\":1,\"kind\":\"locations\",\"id\":\"loc_hub\"," +
                    "\"titleKey\":\"t\",\"locationKind\":\"hub\",\"nodeType\":\"hub\"," +
                    "\"backgrounds\":[\"bg\"],\"objectCount\":[0,0],\"connections\":[]}",
                    "{\"schemaVersion\":1,\"kind\":\"actors\",\"id\":\"npc_x\",\"type\":\"npc\"," +
                    "\"nameKey\":\"n\",\"roles\":[\"trader\"]}",
                    "Роль 'trader' задана"),
            };

            foreach (var testCase in cases)
            {
                Check(testCase.Name, () =>
                {
                    WithFixture(
                        new Dictionary<string, string>
                        {
                            ["catalog.json"] = Catalog("actors/a.json", "locations/loc.json"),
                            ["actors/a.json"] = testCase.Actor,
                            ["locations/loc.json"] = testCase.Location,
                        },
                        root =>
                        {
                            var report = Preflight.Run(root).Report;
                            True(HasIssue(report, "trd_x", testCase.Fragment) ||
                                 HasIssue(report, "actors/a.json", testCase.Fragment) ||
                                 HasIssue(report, "loc_hub", testCase.Fragment) ||
                                 HasIssue(report, "loc_route", testCase.Fragment),
                                $"нет ошибки: {testCase.Fragment}");
                        });
                });
            }
        }

        private void QuestErrorTests()
        {
            var cases = new (string Name, string Quest, string ExtraFile, string ExtraJson, string Fragment)[]
            {
                (
                    "ошибка: цель квеста — неизвестный предмет",
                    "{\"schemaVersion\":1,\"kind\":\"quests\",\"id\":\"qst_x\",\"questKind\":\"repeatable\"," +
                    "\"questType\":\"findItems\",\"titleKey\":\"t\",\"descriptionKey\":\"d\"," +
                    "\"target\":{\"itemPool\":[\"missing\"],\"itemKinds\":1,\"itemCount\":1," +
                    "\"npcPool\":[\"enemy_x\"],\"npcCount\":1,\"poolSize\":1,\"minTier\":0,\"maxTier\":2}," +
                    "\"rewards\":{\"money\":1}}",
                    "actors/enemy.json",
                    "{\"schemaVersion\":1,\"kind\":\"actors\",\"id\":\"enemy_x\",\"type\":\"enemy\"," +
                    "\"nameKey\":\"n\",\"roles\":[\"human\"],\"combat\":{\"tier\":1,\"level\":1," +
                    "\"health\":10,\"ap\":1,\"xp\":1,\"ai\":\"aggressive\",\"accuracy\":0.5," +
                    "\"attack\":{\"damage\":1,\"damageTypeId\":\"dmg_bullet\"}}}",
                    "не найден"),
                (
                    "ошибка: цель квеста — неизвестный актёр",
                    "{\"schemaVersion\":1,\"kind\":\"quests\",\"id\":\"qst_x\",\"questKind\":\"repeatable\"," +
                    "\"questType\":\"killMutants\",\"titleKey\":\"t\",\"descriptionKey\":\"d\"," +
                    "\"target\":{\"count\":1,\"poolSize\":1,\"actorPool\":[\"mutant_missing\"]," +
                    "\"minTier\":0,\"maxTier\":2},\"rewards\":{\"money\":1}}",
                    "actors/enemy.json",
                    "{\"schemaVersion\":1,\"kind\":\"actors\",\"id\":\"enemy_x\",\"type\":\"enemy\"," +
                    "\"nameKey\":\"n\",\"roles\":[\"mutant\"],\"combat\":{\"tier\":1,\"level\":1," +
                    "\"health\":10,\"ap\":1,\"xp\":1,\"ai\":\"aggressive\",\"accuracy\":0.5," +
                    "\"attack\":{\"damage\":1,\"damageTypeId\":\"dmg_bullet\"}}}",
                    "Актёр не найден"),
                (
                    "ошибка: цель зачистки не точка интереса",
                    "{\"schemaVersion\":1,\"kind\":\"quests\",\"id\":\"qst_x\",\"questKind\":\"repeatable\"," +
                    "\"questType\":\"clearLocation\",\"titleKey\":\"t\",\"descriptionKey\":\"d\"," +
                    "\"target\":{\"locationPool\":[\"loc_route\"],\"minTier\":0,\"maxTier\":2}," +
                    "\"rewards\":{\"money\":1}}",
                    "locations/loc_route.json",
                    "{\"schemaVersion\":1,\"kind\":\"locations\",\"id\":\"loc_route\",\"titleKey\":\"t\"," +
                    "\"locationKind\":\"wild\",\"nodeType\":\"route\",\"tags\":[\"road\"],\"tier\":1," +
                    "\"backgrounds\":[\"bg\"],\"objectCount\":[0,0],\"connections\":[]}",
                    "дикой точкой интереса"),
                (
                    "ошибка: пул целей квеста пуст",
                    "{\"schemaVersion\":1,\"kind\":\"quests\",\"id\":\"qst_x\",\"questKind\":\"repeatable\"," +
                    "\"questType\":\"killMutants\",\"titleKey\":\"t\",\"descriptionKey\":\"d\"," +
                    "\"target\":{\"count\":1,\"poolSize\":1,\"minTier\":9,\"maxTier\":9}," +
                    "\"rewards\":{\"money\":1}}",
                    "actors/enemy.json",
                    "{\"schemaVersion\":1,\"kind\":\"actors\",\"id\":\"enemy_x\",\"type\":\"enemy\"," +
                    "\"nameKey\":\"n\",\"roles\":[\"mutant\"],\"combat\":{\"tier\":1,\"level\":1," +
                    "\"health\":10,\"ap\":1,\"xp\":1,\"ai\":\"aggressive\",\"accuracy\":0.5," +
                    "\"attack\":{\"damage\":1,\"damageTypeId\":\"dmg_bullet\"}}}",
                    "не дал ни одного кандидата"),
                (
                    "ошибка: предмет награды квеста не найден",
                    "{\"schemaVersion\":1,\"kind\":\"quests\",\"id\":\"qst_x\",\"questKind\":\"repeatable\"," +
                    "\"questType\":\"killMutants\",\"titleKey\":\"t\",\"descriptionKey\":\"d\"," +
                    "\"target\":{\"count\":1,\"poolSize\":1,\"minTier\":0,\"maxTier\":2}," +
                    "\"rewards\":{\"money\":1,\"items\":[{\"itemId\":\"missing\",\"count\":1}]}}",
                    "actors/enemy.json",
                    "{\"schemaVersion\":1,\"kind\":\"actors\",\"id\":\"enemy_x\",\"type\":\"enemy\"," +
                    "\"nameKey\":\"n\",\"roles\":[\"mutant\"],\"combat\":{\"tier\":1,\"level\":1," +
                    "\"health\":10,\"ap\":1,\"xp\":1,\"ai\":\"aggressive\",\"accuracy\":0.5," +
                    "\"attack\":{\"damage\":1,\"damageTypeId\":\"dmg_bullet\"}}}",
                    "Предмет награды квеста не найден"),
            };

            foreach (var testCase in cases)
            {
                Check(testCase.Name, () =>
                {
                    WithFixture(
                        new Dictionary<string, string>
                        {
                            ["catalog.json"] = Catalog("quests/qst_x.json", testCase.ExtraFile),
                            ["quests/qst_x.json"] = testCase.Quest,
                            [testCase.ExtraFile] = testCase.ExtraJson,
                        },
                        root =>
                        {
                            var report = Preflight.Run(root).Report;
                            True(
                                HasIssue(report, "qst_x", testCase.Fragment),
                                $"нет ошибки: {testCase.Fragment}; {Describe(report)}");
                        });
                });
            }
        }

        private void TalentErrorTests()
        {
            var cases = new (string Name, string Talent, string Fragment)[]
            {
                (
                    "ошибка: талант без максимального ранга",
                    "{\"schemaVersion\":1,\"kind\":\"talents\",\"id\":\"talents\",\"titleKey\":\"t\"," +
                    "\"descriptionKey\":\"d\",\"nodes\":[{\"id\":\"tal_a\",\"titleKey\":\"t\"," +
                    "\"descriptionKey\":\"d\",\"row\":0,\"column\":0,\"maxRank\":0,\"requiredPoints\":0," +
                    "\"effects\":[{\"stat\":\"accuracy\",\"perRank\":0.01}]}]}",
                    "максимальный ранг"),
                (
                    "ошибка: талант без эффектов",
                    "{\"schemaVersion\":1,\"kind\":\"talents\",\"id\":\"talents\",\"titleKey\":\"t\"," +
                    "\"descriptionKey\":\"d\",\"nodes\":[{\"id\":\"tal_a\",\"titleKey\":\"t\"," +
                    "\"descriptionKey\":\"d\",\"row\":0,\"column\":0,\"maxRank\":1,\"requiredPoints\":0," +
                    "\"effects\":[]}]}",
                    "нужен хотя бы один эффект"),
                (
                    "ошибка: неизвестная характеристика таланта",
                    "{\"schemaVersion\":1,\"kind\":\"talents\",\"id\":\"talents\",\"titleKey\":\"t\"," +
                    "\"descriptionKey\":\"d\",\"nodes\":[{\"id\":\"tal_a\",\"titleKey\":\"t\"," +
                    "\"descriptionKey\":\"d\",\"row\":0,\"column\":0,\"maxRank\":1,\"requiredPoints\":0," +
                    "\"effects\":[{\"stat\":\"mana\",\"perRank\":1}]}]}",
                    "Неизвестная характеристика"),
                (
                    "ошибка: дробная прибавка целой характеристики",
                    "{\"schemaVersion\":1,\"kind\":\"talents\",\"id\":\"talents\",\"titleKey\":\"t\"," +
                    "\"descriptionKey\":\"d\",\"nodes\":[{\"id\":\"tal_a\",\"titleKey\":\"t\"," +
                    "\"descriptionKey\":\"d\",\"row\":0,\"column\":0,\"maxRank\":1,\"requiredPoints\":0," +
                    "\"effects\":[{\"stat\":\"apMax\",\"perRank\":1.5}]}]}",
                    "положительным целым"),
                (
                    "ошибка: повторный эффект таланта",
                    "{\"schemaVersion\":1,\"kind\":\"talents\",\"id\":\"talents\",\"titleKey\":\"t\"," +
                    "\"descriptionKey\":\"d\",\"nodes\":[{\"id\":\"tal_a\",\"titleKey\":\"t\"," +
                    "\"descriptionKey\":\"d\",\"row\":0,\"column\":0,\"maxRank\":1,\"requiredPoints\":0," +
                    "\"effects\":[{\"stat\":\"apMax\",\"perRank\":1},{\"stat\":\"apMax\",\"perRank\":1}]}]}",
                    "указан дважды"),
                (
                    "ошибка: повторный id таланта",
                    "{\"schemaVersion\":1,\"kind\":\"talents\",\"id\":\"talents\",\"titleKey\":\"t\"," +
                    "\"descriptionKey\":\"d\",\"nodes\":[{\"id\":\"tal_a\",\"titleKey\":\"t\"," +
                    "\"descriptionKey\":\"d\",\"row\":0,\"column\":0,\"maxRank\":1,\"requiredPoints\":0," +
                    "\"effects\":[{\"stat\":\"apMax\",\"perRank\":1}]},{\"id\":\"tal_a\",\"titleKey\":\"t\"," +
                    "\"descriptionKey\":\"d\",\"row\":1,\"column\":0,\"maxRank\":1,\"requiredPoints\":0," +
                    "\"effects\":[{\"stat\":\"apMax\",\"perRank\":1}]}]}",
                    "повторяется"),
                (
                    "ошибка: позиция таланта занята",
                    "{\"schemaVersion\":1,\"kind\":\"talents\",\"id\":\"talents\",\"titleKey\":\"t\"," +
                    "\"descriptionKey\":\"d\",\"nodes\":[{\"id\":\"tal_a\",\"titleKey\":\"t\"," +
                    "\"descriptionKey\":\"d\",\"row\":0,\"column\":0,\"maxRank\":1,\"requiredPoints\":0," +
                    "\"effects\":[{\"stat\":\"apMax\",\"perRank\":1}]},{\"id\":\"tal_b\",\"titleKey\":\"t\"," +
                    "\"descriptionKey\":\"d\",\"row\":0,\"column\":0,\"maxRank\":1,\"requiredPoints\":0," +
                    "\"effects\":[{\"stat\":\"apMax\",\"perRank\":1}]}]}",
                    "уже занята"),
                (
                    "ошибка: талант требует больше очков, чем доступно",
                    "{\"schemaVersion\":1,\"kind\":\"talents\",\"id\":\"talents\",\"titleKey\":\"t\"," +
                    "\"descriptionKey\":\"d\",\"nodes\":[{\"id\":\"tal_a\",\"titleKey\":\"t\"," +
                    "\"descriptionKey\":\"d\",\"row\":0,\"column\":0,\"maxRank\":1,\"requiredPoints\":0," +
                    "\"effects\":[{\"stat\":\"apMax\",\"perRank\":1}]},{\"id\":\"tal_b\",\"titleKey\":\"t\"," +
                    "\"descriptionKey\":\"d\",\"row\":1,\"column\":0,\"maxRank\":1,\"requiredPoints\":5," +
                    "\"effects\":[{\"stat\":\"apMax\",\"perRank\":1}]}]}",
                    "требует 5 вложенных очков"),
            };

            foreach (var testCase in cases)
            {
                Check(testCase.Name, () =>
                {
                    WithFixture(
                        new Dictionary<string, string>
                        {
                            ["catalog.json"] = Catalog("talents.json"),
                            ["talents.json"] = testCase.Talent
                        },
                        root =>
                        {
                            var report = Preflight.Run(root).Report;
                            True(
                                HasIssue(report, "talents", testCase.Fragment),
                                $"нет ошибки: {testCase.Fragment}; {Describe(report)}");
                        });
                });
            }
        }

        private void LootErrorTests()
        {
            const string settings = "{\"schemaVersion\":1,\"kind\":\"loot\",\"id\":\"loot\",\"tierBands\":[" +
                                    "{\"offset\":-1,\"chance\":0.2},{\"offset\":0,\"chance\":0.6}," +
                                    "{\"offset\":1,\"chance\":0.2}]}";

            var cases = new (string Name, Dictionary<string, string> Files, string Fragment)[]
            {
                (
                    "ошибка: неизвестная редкость предмета",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json"),
                        ["items/a.json"] = "{ \"kind\":\"items\",\"id\":\"itm_a\",\"nameKey\":\"n\"," +
                                           "\"category\":\"trophy\",\"footprint\":[1,1],\"maxStack\":1," +
                                           "\"tier\":1,\"rarity\":\"mythic\",\"icon\":\"i\" }"
                    },
                    "Неизвестная редкость"),
                (
                    "ошибка: таблица лута у NPC",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("actors/a.json"),
                        ["actors/a.json"] = "{ \"kind\":\"actors\",\"id\":\"npc_x\",\"type\":\"npc\"," +
                                            "\"nameKey\":\"n\",\"lootTable\":\"loot_x\" }"
                    },
                    "только у врага"),
                (
                    "ошибка: предмет записи лута не найден",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("loot.json", "loot/loot_x.json", "items/a.json"),
                        ["loot.json"] = settings,
                        ["loot/loot_x.json"] = "{ \"kind\":\"lootTables\",\"id\":\"loot_x\",\"entries\":[" +
                                               "{\"itemId\":\"itm_missing\",\"chance\":0.5}] }",
                        ["items/a.json"] = Weapon("wpn_a")
                    },
                    "Предмет записи лута не найден"),
                (
                    "ошибка: цикл общих таблиц лута",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("loot.json", "loot/loot_a.json", "loot/loot_b.json"),
                        ["loot.json"] = settings,
                        ["loot/loot_a.json"] = "{ \"kind\":\"lootTables\",\"id\":\"loot_a\"," +
                                               "\"sharedTableIds\":[\"loot_b\"] }",
                        ["loot/loot_b.json"] = "{ \"kind\":\"lootTables\",\"id\":\"loot_b\"," +
                                               "\"sharedTableIds\":[\"loot_a\"] }"
                    },
                    "ссылается на себя"),
                (
                    "ошибка: таблица лута включена повторно",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("loot.json", "loot/loot_a.json", "loot/loot_b.json"),
                        ["loot.json"] = settings,
                        ["loot/loot_a.json"] = "{ \"kind\":\"lootTables\",\"id\":\"loot_a\"," +
                                               "\"sharedTableIds\":[\"loot_b\",\"loot_b\"] }",
                        ["loot/loot_b.json"] = "{ \"kind\":\"lootTables\",\"id\":\"loot_b\" }"
                    },
                    "включена повторно"),
                (
                    "ошибка: шанс записи лута вне диапазона",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("loot.json", "loot/loot_x.json"),
                        ["loot.json"] = settings,
                        ["loot/loot_x.json"] = "{ \"kind\":\"lootTables\",\"id\":\"loot_x\",\"entries\":[" +
                                               "{\"itemId\":\"itm_a\",\"chance\":1.5}] }"
                    },
                    "шанс должен быть в диапазоне"),
                (
                    "ошибка: сумма полос тира не равна единице",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("loot.json", "loot/loot_x.json"),
                        ["loot.json"] = "{\"kind\":\"loot\",\"id\":\"loot\",\"tierBands\":[" +
                                        "{\"offset\":0,\"chance\":0.5}]}",
                        ["loot/loot_x.json"] = "{ \"kind\":\"lootTables\",\"id\":\"loot_x\" }"
                    },
                    "Сумма вкладов полос"),
            };

            foreach (var testCase in cases)
            {
                Check(testCase.Name, () =>
                {
                    WithFixture(testCase.Files, root =>
                    {
                        var error = Throws<ValidationException>(() => Bootstrap.Build(root), testCase.Name);
                        True(
                            HasFragment(error.Report, testCase.Fragment),
                            $"нет ошибки: {testCase.Fragment}; {Describe(error.Report)}");
                    });
                });
            }
        }

        private static bool HasFragment(ValidationReport report, string fragment)
        {
            foreach (var issue in report.Errors)
            {
                if (issue.Message.IndexOf(fragment, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void ErrorBehaviorTests()
        {
            WithFixture(BuildBrokenFixture(_dataRoot), root =>
            {
                var result = Preflight.Run(root);

                Check("поведение: отчёт содержит все виды ошибок", () =>
                {
                    var expected = new (string Name, string Path, string Fragment)[]
                    {
                        ("синтаксис", "items/broken_syntax.json", "конец документа"),
                        ("нет файла из каталога", "topologies/missing.json", "Файл не найден"),
                        ("нет файла из коллекции", "items/absent.json", "Файл не найден"),
                        ("дубль id", "items/b.json", "Дублирующийся id"),
                        ("файл в двух коллекциях", "items/a.json", "подключён дважды"),
                        ("цикл коллекций", "items/col_cycle.json", "Цикл"),
                        ("weapon без блока", "items/bad.json", "без блока weapon"),
                        ("extends на несуществующий файл", "_base/missing.json", "Файл не найден"),
                        ("extends на другой вид", "items/mismatch.json", "extends на другой вид")
                    };

                    foreach (var item in expected)
                    {
                        True(
                            HasIssue(result.Report, item.Path, item.Fragment),
                            $"нет ошибки «{item.Name}»: {item.Path} / {item.Fragment}");
                    }
                });

                Check("поведение: предупреждения не блокируют, но видны", () =>
                {
                    True(HasWarning(result.Report, "items/orphan.json", "не используется"), "нет предупреждения о недостижимом файле");
                    True(HasWarning(result.Report, "items/translation", "Нет перевода"), "нет предупреждения о переводе");
                });

                Check("поведение: игра не стартует и сервисы не создаются", () =>
                {
                    False(result.Report.IsValid, "отчёт должен быть невалидным");
                    False(result.CanStart, "старт запрещён");
                    True(result.Services == null, "сервисы не должны создаваться");
                });

                Check("поведение: Bootstrap.Build бросает ValidationException с отчётом", () =>
                {
                    var error = Throws<ValidationException>(() => Bootstrap.Build(root), "Build");
                    False(error.Report.IsValid, "отчёт в исключении невалиден");
                    Equal(result.Report.Errors.Count, error.Report.Errors.Count, "число ошибок");
                    Contains(error.Message, "не пройдена", "сводка в сообщении");
                });

                Check("поведение: повторный прогон даёт тот же результат", () =>
                {
                    var second = Preflight.Run(root);
                    Equal(result.Report.Errors.Count, second.Report.Errors.Count, "ошибки");
                    Equal(result.Report.Warnings.Count, second.Report.Warnings.Count, "предупреждения");
                });
            });
        }

        private void ErrorTests()
        {
            var cases = new List<(string Name, Dictionary<string, string> Files, string Fragment)>
            {
                (
                    "ошибка: дубль id в коллекции",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/col.json"),
                        ["items/col.json"] = Collection("items/a.json", "items/a.json"),
                        ["items/a.json"] = Weapon("wpn_ak74"),
                    },
                    "подключён дважды"),
                (
                    "ошибка: extends на несуществующий файл",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json"),
                        ["items/a.json"] = Weapon("wpn_ak74", extends: "_base/missing.json"),
                    },
                    "Файл не найден"),
                (
                    "ошибка: extends на другой вид",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json"),
                        ["_base/loc.json"] = "{\"schemaVersion\":1,\"kind\":\"locations\",\"abstract\":true}",
                        ["items/a.json"] = Weapon("wpn_ak74", extends: "_base/loc.json"),
                    },
                    "extends на другой вид"),
                (
                    "ошибка: битый json",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json"),
                        ["items/a.json"] = "{ \"kind\": \"items\", ",
                    },
                    "Ожидалось имя поля"),
                (
                    "ошибка: неизвестный материал брони",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json"),
                        ["items/a.json"] = "{ \"kind\":\"items\",\"id\":\"arm_wood\",\"nameKey\":\"n\",\"category\":\"armor\",\"footprint\":[1,1],\"maxStack\":1,\"tier\":1,\"icon\":\"i\",\"durabilityMax\":10,\"material\":\"wood\",\"durabilityFactor\":1.0 }",
                    },
                    "Неизвестный материал"),
                (
                    "ошибка: неизвестный тип урона в защите",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json"),
                        ["items/a.json"] = "{ \"kind\":\"items\",\"id\":\"arm_wood\",\"nameKey\":\"n\",\"category\":\"armor\",\"footprint\":[1,1],\"maxStack\":1,\"tier\":1,\"icon\":\"i\",\"durabilityMax\":10,\"material\":\"light\",\"durabilityFactor\":1.0,\"quality\":1,\"protection\":{\"dmg_wood\":1} }",
                    },
                    "Неизвестный тип урона"),
                (
                    "ошибка: путь выходит за корень",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("../secret.json"),
                    },
                    "выходит за корень"),
                (
                    "ошибка: weapon без блока weapon",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json"),
                        ["items/a.json"] = "{ \"kind\":\"items\",\"id\":\"wpn_x\",\"nameKey\":\"n\",\"category\":\"weapon\",\"footprint\":[1,1],\"maxStack\":1,\"tier\":1,\"icon\":\"i\" }",
                    },
                    "без блока weapon"),
                (
                    "ошибка: у врага нет опыта",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("actors/a.json"),
                        ["actors/a.json"] = "{ \"kind\":\"actors\",\"id\":\"enemy_x\",\"type\":\"enemy\",\"nameKey\":\"n\"," +
                                             "\"combat\":{\"tier\":1,\"level\":1,\"health\":10,\"ap\":1,\"ai\":\"aggressive\",\"accuracy\":0.5} }",
                    },
                    "xp"),
                (
                    "ошибка: магазин оружия меньше одного патрона",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json"),
                        ["items/a.json"] = "{ \"kind\":\"items\",\"id\":\"wpn_x\",\"nameKey\":\"n\",\"category\":\"weapon\",\"footprint\":[1,1],\"maxStack\":1,\"tier\":1,\"icon\":\"i\",\"weapon\":{\"classId\":\"w\",\"damageTypeId\":\"dmg_bullet\",\"damage\":1,\"accuracy\":0.5,\"magazine\":0,\"ammoCaliber\":\"c\",\"reloadTime\":1,\"armorPenetration\":0,\"durabilityMax\":10,\"durabilityPerShot\":0.1}}",
                    },
                    "вместимость магазина"),
                (
                    "ошибка: у врага нет блока attack",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("actors/a.json"),
                        ["actors/a.json"] = "{ \"kind\":\"actors\",\"id\":\"enemy_x\",\"type\":\"enemy\",\"nameKey\":\"n\"," +
                                             "\"combat\":{\"tier\":1,\"level\":1,\"health\":10,\"ap\":1,\"xp\":1,\"ai\":\"aggressive\",\"accuracy\":0.5} }",
                    },
                    "блока attack"),
                (
                    "ошибка: блок атаки у не-врага",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("actors/a.json"),
                        ["actors/a.json"] = "{ \"kind\":\"actors\",\"id\":\"npc_x\",\"type\":\"npc\",\"nameKey\":\"n\"," +
                                             "\"combat\":{\"tier\":1,\"level\":1,\"health\":10,\"ap\":1,\"ai\":\"passive\",\"accuracy\":0.5," +
                                             "\"attack\":{\"damage\":1,\"damageTypeId\":\"dmg_rupture\"}} }",
                    },
                    "только у врага"),
                (
                    "ошибка: в атаке смешаны оружие и естественная атака",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("actors/a.json"),
                        ["actors/a.json"] = "{ \"kind\":\"actors\",\"id\":\"enemy_x\",\"type\":\"enemy\",\"nameKey\":\"n\"," +
                                             "\"combat\":{\"tier\":1,\"level\":1,\"health\":10,\"ap\":1,\"xp\":1,\"ai\":\"aggressive\",\"accuracy\":0.5," +
                                             "\"attack\":{\"weaponId\":\"wpn_x\",\"damage\":5}} }",
                    },
                    "смешаны"),
                (
                    "ошибка: тип урона атаки не найден",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("actors/a.json", "combat.json", "equipment.json", "topology.json"),
                        ["combat.json"] = CombatJson(),
                        ["equipment.json"] = EquipmentJson(),
                        ["topology.json"] = TopologyJson(),
                        ["actors/a.json"] = "{ \"kind\":\"actors\",\"id\":\"enemy_x\",\"type\":\"enemy\",\"nameKey\":\"n\"," +
                                             "\"combat\":{\"tier\":1,\"level\":1,\"health\":10,\"ap\":1,\"xp\":1,\"ai\":\"aggressive\",\"accuracy\":0.5," +
                                             "\"attack\":{\"damage\":1,\"damageTypeId\":\"dmg_missing\"}} }",
                    },
                    "Неизвестный тип урона"),
                (
                    "ошибка: оружие атаки не найдено",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("actors/a.json", "combat.json", "equipment.json", "topology.json"),
                        ["combat.json"] = CombatJson(),
                        ["equipment.json"] = EquipmentJson(),
                        ["topology.json"] = TopologyJson(),
                        ["actors/a.json"] = "{ \"kind\":\"actors\",\"id\":\"enemy_x\",\"type\":\"enemy\",\"nameKey\":\"n\"," +
                                             "\"combat\":{\"tier\":1,\"level\":1,\"health\":10,\"ap\":1,\"xp\":1,\"ai\":\"aggressive\",\"accuracy\":0.5," +
                                             "\"attack\":{\"weaponId\":\"wpn_missing\"}} }",
                    },
                    "Предмет не найден"),
                (
                    "ошибка: оружие атаки без блока weapon",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json", "actors/a.json", "combat.json", "equipment.json", "topology.json"),
                        ["combat.json"] = CombatJson(),
                        ["equipment.json"] = EquipmentJson(),
                        ["topology.json"] = TopologyJson(),
                        ["items/a.json"] = "{ \"kind\":\"items\",\"id\":\"wpn_x\",\"nameKey\":\"n\",\"category\":\"trophy\",\"footprint\":[1,1],\"maxStack\":1,\"tier\":1,\"icon\":\"i\" }",
                        ["actors/a.json"] = "{ \"kind\":\"actors\",\"id\":\"enemy_x\",\"type\":\"enemy\",\"nameKey\":\"n\"," +
                                             "\"combat\":{\"tier\":1,\"level\":1,\"health\":10,\"ap\":1,\"xp\":1,\"ai\":\"aggressive\",\"accuracy\":0.5," +
                                             "\"attack\":{\"weaponId\":\"wpn_x\"}} }",
                    },
                    "без блока weapon"),
            };

            foreach (var testCase in cases)
            {
                Check(testCase.Name, () =>
                {
                    WithFixture(testCase.Files, root =>
                    {
                        var error = Throws<ValidationException>(() => Bootstrap.Build(root), testCase.Name);
                        Contains(error.Message, testCase.Fragment, "причина");
                    });
                });
            }
        }
    }
}
