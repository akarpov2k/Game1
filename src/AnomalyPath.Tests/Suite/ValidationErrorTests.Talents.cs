#nullable enable
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Core;
using AnomalyPath.Core.Validation;

namespace AnomalyPath.Tests
{
    /// <summary>Часть класса <see cref="ValidationErrorTests"/>: ошибки талантов.</summary>
    public sealed partial class ValidationErrorTests
    {
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
    }
}
