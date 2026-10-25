#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Validation;

namespace AnomalyPath.Tests
{
    public sealed class PreflightTests : TestBase
    {
        private readonly string _dataRoot;
        public PreflightTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        public override void Run()
        {
            Check("preflight: данные проекта проходят проверку", () =>
            {
                var result = Preflight.Run(_dataRoot);
                True(result.Report.IsValid, "ошибки: " + Describe(result.Report));
                Equal(0, result.Report.Warnings.Count, "предупреждения");
                Contains(result.Report.Summary, "пройдена", "сводка");
                True(result.CanStart, "игра может стартовать");
                Equal(21, result.Services!.Resolve<IItemRepository>().All.Count, "предметы из сервисов");
            });

            Check("preflight: данные читаются один раз", () =>
            {
                var source = new TestData.CountingSource(new FileSystemDataSource(_dataRoot));
                var result = Preflight.Run(source);

                True(result.CanStart, "данные должны быть валидны");
                var jsonFiles = 0;
                foreach (var file in source.EnumerateFiles())
                {
                    if (file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        jsonFiles++;
                    }
                }

                Equal(jsonFiles, source.Reads, "чтений файлов");
            });

            Check("preflight: собирает все ошибки, а не падает на первой", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json", "combat.json", "equipment.json", "topology.json"),
                        ["items/a.json"] = "{",
                        ["items/b.json"] = "[1,",
                        ["items/c.json"] = "{\"kind\":}"
                    },
                    root =>
                    {
                        var report = Preflight.Validate(root);
                        False(report.IsValid, "отчёт должен быть невалидным");
                        True(report.Errors.Count >= 3, $"собрано ошибок: {report.Errors.Count}");
                        True(HasIssue(report, "items/a.json"), "нет ошибки по a.json");
                        True(HasIssue(report, "items/b.json"), "нет ошибки по b.json");
                        True(HasIssue(report, "items/c.json"), "нет ошибки по c.json");
                    });
            });

            Check("preflight: отсутствие каталога — ошибка", () =>
            {
                WithFixture(
                    new Dictionary<string, string> { ["items/a.json"] = Weapon("wpn_a") },
                    root =>
                    {
                        var report = Preflight.Validate(root);
                        False(report.IsValid, "без каталога данных быть не должно");
                        True(HasIssue(report, "catalog.json"), "нет ошибки про каталог");
                    });
            });

            Check("preflight: расхождение переводов — предупреждение, не ошибка", () =>
            {
                WithFixture(
                    BuildTranslationFixture(_dataRoot),
                    root =>
                    {
                        var report = Preflight.Validate(root);
                        True(report.IsValid, "расхождение переводов не блокирует старт: " + Describe(report));
                        Equal(1, report.Warnings.Count, "предупреждения");
                        Contains(report.Warnings[0].Message, "extra.key", "имя ключа");
                    });
            });

            Check("preflight: собирает все семантические ошибки сразу", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/col.json", "combat.json", "equipment.json", "topology.json"),
                        ["items/col.json"] = Collection("items/a.json", "items/b.json", "items/c.json", "items/missing.json"),
                        ["items/a.json"] = Weapon("wpn_dup"),
                        ["items/b.json"] = Weapon("wpn_dup"),
                        ["items/c.json"] = "{\"kind\":\"items\",\"id\":\"wpn_c\",\"category\":\"weapon\",\"footprint\":[1,1],\"maxStack\":1,\"tier\":1,\"icon\":\"i\",\"weapon\":{\"classId\":\"wclass_rifle\",\"damageTypeId\":\"dmg_bullet\",\"damage\":1,\"accuracy\":0.5,\"magazine\":10,\"ammoCaliber\":\"5.45x39\",\"reloadTime\":2,\"armorPenetration\":0,\"durabilityMax\":10,\"durabilityPerShot\":0.1}}"
                    },
                    root =>
                    {
                        var report = Preflight.Validate(root);
                        False(report.IsValid, "отчёт должен быть невалидным");
                        True(report.Errors.Count >= 3, $"собрано ошибок: {report.Errors.Count}");
                        True(HasIssue(report, "items/b.json"), "нет ошибки дубля id");
                        True(HasIssue(report, "items/missing.json"), "нет ошибки отсутствующего файла");
                        True(HasIssue(report, "items/c.json"), "нет ошибки отсутствующего поля");
                    });
            });

            Check("preflight: файл в двух коллекциях — ошибка", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/col_a.json", "items/col_b.json"),
                        ["items/col_a.json"] = Collection("items/a.json"),
                        ["items/col_b.json"] = Collection("items/a.json"),
                        ["items/a.json"] = Weapon("wpn_a")
                    },
                    root =>
                    {
                        var report = Preflight.Validate(root);
                        False(report.IsValid, "двойная ссылка должна быть ошибкой");
                        True(HasIssue(report, "items/a.json"), "нет ошибки двойной ссылки");
                    });
            });

            Check("preflight: недостижимый файл — предупреждение", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/col.json", "combat.json", "equipment.json", "topology.json", "talents.json", "progression.json"),
                        ["items/col.json"] = Collection("items/a.json"),
                        ["items/a.json"] = Weapon("wpn_a"),
                        ["items/orphan.json"] = Weapon("wpn_orphan"),
                        ["combat.json"] = CombatJson(),
                        ["equipment.json"] = EquipmentJson(),
                        ["topology.json"] = TopologyJson(),
                        ["talents.json"] = TalentsJson(),
                        ["progression.json"] = ProgressionJson()
                    },
                    root =>
                    {
                        var report = Preflight.Validate(root);
                        True(report.IsValid, "недостижимость не блокирует старт: " + Describe(report));
                        var found = false;
                        foreach (var warning in report.Warnings)
                        {
                            if (warning.Path.IndexOf("items/orphan.json", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                found = true;
                            }
                        }

                        True(found, "нет предупреждения о недостижимом файле");
                    });
            });

            Check("preflight: нет топологии — ошибка", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json", "combat.json", "equipment.json"),
                        ["items/a.json"] = Weapon("wpn_a"),
                        ["combat.json"] = CombatJson(),
                        ["equipment.json"] = EquipmentJson()
                    },
                    root =>
                    {
                        var report = Preflight.Validate(root);
                        False(report.IsValid, "без топологии мир не стартует");
                        True(HasIssue(report, "topology", "Файл не найден"), "нет ошибки топологии");
                    });
            });

            Check("preflight: сообщения локализованы", () =>
            {
                WithFixture(
                    new Dictionary<string, string> { ["items/a.json"] = Weapon("wpn_a") },
                    root =>
                    {
                        var missingCatalog = Preflight.Validate(root);
                        Contains(missingCatalog.Summary, "не пройдена", "сводка");
                    });
            });
        }
    }
}
