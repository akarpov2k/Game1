#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Tests
{
    /// <summary>Запуск всех категорий тестов на папке данных.</summary>
    public static class TestSuite
    {
        /// <summary>Прогоняет набор и пишет результат в консоль.</summary>
        /// <param name="dataRoot">Корень данных.</param>
        /// <returns>Счётчики и список упавших проверок.</returns>
        public static TestRunResult Run(string dataRoot) => Run(dataRoot, Console.WriteLine);

        /// <summary>Прогоняет набор и пишет результат в переданный лог.</summary>
        /// <param name="dataRoot">Корень данных.</param>
        /// <param name="log">Куда писать строки результата.</param>
        /// <returns>Счётчики и список упавших проверок.</returns>
        public static TestRunResult Run(string dataRoot, Action<string> log)
        {
            var source = new FileSystemDataSource(dataRoot);
            CoreText.Configure(JsonStringCatalog.FromSource(source, Bootstrap.DefaultTextCatalog));

            var suites = new TestBase[]
            {
                new DataLoadingTests(dataRoot),
                new RepositoryTests(dataRoot),
                new InventoryTests(),
                new EquipmentTests(dataRoot),
                new FormulaTests(dataRoot),
                new LocalizationTests(source),
                new DamageTypeTests(dataRoot),
                new CombatTests(dataRoot),
                new WorldTests(dataRoot),
                new SaveTests(dataRoot),
                new JsonFormatTests(),
                new FileFormatTests(),
                new ValidationErrorTests(dataRoot),
                new PreflightTests(dataRoot),
                new SourceHygieneTests(dataRoot),
            };

            var passed = 0;
            var failed = 0;
            var failures = new List<string>();

            foreach (var suite in suites)
            {
                suite.Log = log;
                suite.Run();
                passed += suite.Passed;
                failed += suite.Failed;
                failures.AddRange(suite.Failures);
            }

            log($"Итого: {passed} прошло, {failed} упало");
            return new TestRunResult(passed, failed, failures);
        }
    }
}
