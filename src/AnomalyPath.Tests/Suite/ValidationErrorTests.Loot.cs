#nullable enable
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Core;
using AnomalyPath.Core.Validation;

namespace AnomalyPath.Tests
{
    /// <summary>Часть класса <see cref="ValidationErrorTests"/>: ошибки добычи.</summary>
    public sealed partial class ValidationErrorTests
    {
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
    }
}
