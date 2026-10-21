#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Tests
{
    public sealed class LocalizationTests : TestBase
    {
        private readonly IDataSource _source;
        public LocalizationTests(IDataSource source)
        {
            _source = source;
        }

        public override void Run()
        {
            Check("локализация: тексты ошибок берутся из файла", () =>
            {
                var ru = JsonStringCatalog.FromSource(_source, "Localization/errors.ru.json");
                var en = JsonStringCatalog.FromSource(_source, "Localization/errors.en.json");

                var ruText = ru.Get(CoreErrors.FileNotFound);
                var enText = en.Get(CoreErrors.FileNotFound);

                Equal("Файл не найден", ruText, "ru");
                Equal("File not found", enText, "en");
                False(string.Equals(ruText, enText, StringComparison.Ordinal), "тексты должны различаться");
            });

            Check("локализация: наборы ключей ru и en совпадают", () =>
            {
                var ru = JsonStringCatalog.FromSource(_source, "Localization/errors.ru.json");
                var en = JsonStringCatalog.FromSource(_source, "Localization/errors.en.json");

                foreach (var key in ErrorKeys())
                {
                    True(ru.Contains(key), $"ru: нет ключа {key}");
                    True(en.Contains(key), $"en: нет ключа {key}");
                }
            });

            Check("локализация: подстановка аргументов", () =>
            {
                CoreText.Configure(JsonStringCatalog.FromSource(_source, "Localization/errors.ru.json"));
                var text = CoreText.Format(CoreErrors.MissingStringField, "id");
                Contains(text, "'id'", "аргумент поля");
            });

            Check("локализация: неизвестный ключ возвращается как есть", () =>
            {
                var catalog = new JsonStringCatalog(new Dictionary<string, string>());
                Equal("unknown.key", catalog.Get("unknown.key"), "fallback");
            });
        }
    }
}
