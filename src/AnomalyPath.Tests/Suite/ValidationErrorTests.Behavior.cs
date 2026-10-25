#nullable enable
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Core;
using AnomalyPath.Core.Validation;

namespace AnomalyPath.Tests
{
    /// <summary>Часть класса <see cref="ValidationErrorTests"/>: поведение валидатора.</summary>
    public sealed partial class ValidationErrorTests
    {
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
    }
}
