#nullable enable
using System;
using AnomalyPath.Core.Validation;

namespace AnomalyPath.Core
{
    /// <summary>
    /// Обязательная проверка данных перед инициализацией игры. Данные читаются и разбираются
    /// один раз: при успехе отдаёт готовый контейнер сервисов, при ошибках — отчёт.
    /// </summary>
    public static class Preflight
    {
        /// <summary>Проверяет папку данных и собирает сервисы из того же прочитанного набора.</summary>
        /// <param name="dataRoot">Корень данных (в редакторе — <c>Assets/Data</c>).</param>
        /// <returns>Отчёт и контейнер сервисов (последний — только при успехе).</returns>
        public static PreflightResult Run(string dataRoot) =>
            Run(new Data.FileSystemDataSource(dataRoot));

        /// <summary>Проверяет произвольный источник данных: используется тестами и загрузчиком модов.</summary>
        /// <param name="source">Источник файлов.</param>
        /// <returns>Отчёт и контейнер сервисов (последний — только при успехе).</returns>
        public static PreflightResult Run(Abstractions.IDataSource source)
        {
            var parsed = new System.Collections.Generic.Dictionary<string, Json.JsonValue>(
                System.StringComparer.Ordinal);
            var syntaxIssues = new System.Collections.Generic.List<ValidationIssue>();

            foreach (var file in source.EnumerateFiles())
            {
                if (!file.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    parsed[file] = Json.JsonParser.Parse(source.ReadAllText(file), file);
                }
                catch (Data.DataException error)
                {
                    syntaxIssues.Add(new ValidationIssue(error.Path, error.Code, error.Reason));
                }
            }

            if (parsed.TryGetValue(Bootstrap.DefaultTextCatalog, out var texts))
            {
                try
                {
                    Localization.CoreText.Configure(
                        Localization.JsonStringCatalog.FromValue(texts, Bootstrap.DefaultTextCatalog));
                }
                catch (Data.DataException error)
                {
                    syntaxIssues.Add(new ValidationIssue(error.Path, error.Code, error.Reason));
                }
            }

            var report = new DataValidator(source, parsed, syntaxIssues).Validate(out var data);
            var services = report.IsValid && data != null ? Bootstrap.BuildServices(data) : null;
            return new PreflightResult(report, services);
        }

        /// <summary>Проверяет папку данных и возвращает только отчёт.</summary>
        /// <param name="dataRoot">Корень данных.</param>
        /// <returns>Отчёт проверки.</returns>
        public static ValidationReport Validate(string dataRoot) => Run(dataRoot).Report;
    }
}
