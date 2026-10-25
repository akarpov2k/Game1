#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core;
using AnomalyPath.Core.Validation;

namespace AnomalyPath.Tests
{
    /// <summary>Часть класса <see cref="ValidationErrorTests"/>: объекты локаций.</summary>
    public sealed partial class ValidationErrorTests
    {
        private void SpawnErrorTests()
        {
            var cases = new (string Name, Dictionary<string, string> Files, string Fragment)[]
            {
                (
                    "ошибка: таблица лута объекта не найдена",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("spawns/a.json"),
                        ["spawns/a.json"] = "{\"kind\":\"spawns\",\"id\":\"spawn_a\",\"titleKey\":\"n\"," +
                                            "\"objectKind\":\"loot\",\"weight\":1,\"lootTable\":\"loot_missing\"}"
                    },
                    "Таблица лута не найдена"),
                (
                    "ошибка: актёр объекта не найден",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("spawns/a.json"),
                        ["spawns/a.json"] = "{\"kind\":\"spawns\",\"id\":\"spawn_a\",\"titleKey\":\"n\"," +
                                            "\"objectKind\":\"npc\",\"weight\":1,\"actorId\":\"npc_missing\"}"
                    },
                    "Актёр не найден"),
                (
                    "ошибка: objectCount локации больше одного",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("locations/a.json"),
                        ["locations/a.json"] = "{\"kind\":\"locations\",\"id\":\"loc_x\",\"titleKey\":\"n\"," +
                                               "\"locationKind\":\"wild\",\"subtype\":\"building\"," +
                                               "\"tags\":[\"outdoor\"],\"tier\":1,\"backgrounds\":[\"bg\"]," +
                                               "\"objectCount\":[1,2]}"
                    },
                    "objectCount")
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
