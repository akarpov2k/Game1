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
            ErrorTests(); ErrorBehaviorTests();
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
