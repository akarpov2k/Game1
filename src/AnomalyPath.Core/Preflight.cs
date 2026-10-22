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
            var gameTexts = LoadGameTexts(parsed, syntaxIssues);
            var services = report.IsValid && data != null ? Bootstrap.BuildServices(data, gameTexts) : null;
            return new PreflightResult(report, services);
        }

        private static Abstractions.IGameTexts LoadGameTexts(
            System.Collections.Generic.Dictionary<string, Json.JsonValue> parsed,
            System.Collections.Generic.List<ValidationIssue> issues)
        {
            if (!parsed.TryGetValue(Bootstrap.DefaultGameTextCatalog, out var primary))
            {
                return Localization.GameTexts.Empty;
            }

            Localization.JsonStringCatalog main;
            try
            {
                main = Localization.JsonStringCatalog.FromValue(primary, Bootstrap.DefaultGameTextCatalog);
            }
            catch (Data.DataException error)
            {
                issues.Add(new ValidationIssue(error.Path, error.Code, error.Reason));
                return Localization.GameTexts.Empty;
            }

            Localization.JsonStringCatalog? fallback = null;
            if (parsed.TryGetValue(Bootstrap.FallbackGameTextCatalog, out var secondary))
            {
                try
                {
                    fallback = Localization.JsonStringCatalog.FromValue(secondary, Bootstrap.FallbackGameTextCatalog);
                }
                catch (Data.DataException error)
                {
                    issues.Add(new ValidationIssue(error.Path, error.Code, error.Reason));
                }
            }

            return new Localization.GameTexts(main, fallback);
        }

        /// <summary>Проверяет папку данных и возвращает только отчёт.</summary>
        /// <param name="dataRoot">Корень данных.</param>
        /// <returns>Отчёт проверки.</returns>
        public static ValidationReport Validate(string dataRoot) => Run(dataRoot).Report;
    }
}
