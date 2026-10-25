#nullable enable
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Core;
using AnomalyPath.Core.Validation;

namespace AnomalyPath.Tests
{
    /// <summary>Часть класса <see cref="ValidationErrorTests"/>: ошибки квестов.</summary>
    public sealed partial class ValidationErrorTests
    {
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
    }
}
