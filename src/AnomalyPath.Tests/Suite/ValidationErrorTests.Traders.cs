#nullable enable
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Core;
using AnomalyPath.Core.Validation;

namespace AnomalyPath.Tests
{
    /// <summary>Часть класса <see cref="ValidationErrorTests"/>: ошибки торговцев.</summary>
    public sealed partial class ValidationErrorTests
    {
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
    }
}
