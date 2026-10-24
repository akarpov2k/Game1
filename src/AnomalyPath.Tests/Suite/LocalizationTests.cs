#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Json;
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

            Check("строки: у каждого ключа данных есть текст в ru и en", () =>
            {
                var ru = JsonStringCatalog.FromSource(_source, Bootstrap.DefaultGameTextCatalog);
                var en = JsonStringCatalog.FromSource(_source, Bootstrap.FallbackGameTextCatalog);
                var keys = CollectTextKeys(ResolveDataRootForTests());

                True(keys.Count > 0, "в данных должны быть ключи текстов");
                foreach (var key in keys)
                {
                    True(ru.Contains(key), $"ru: нет текста для ключа {key}");
                    True(en.Contains(key), $"en: нет текста для ключа {key}");
                }
            });

            Check("строки: откат с текущего языка на en, затем на сам ключ", () =>
            {
                var primary = new JsonStringCatalog(new Dictionary<string, string>
                {
                    ["a"] = "А"
                });
                var fallback = new JsonStringCatalog(new Dictionary<string, string>
                {
                    ["b"] = "B",
                    ["c"] = "C"
                });

                var texts = new GameTexts(primary, fallback);

                Equal("А", texts.Get("a"), "текущий язык");
                Equal("B", texts.Get("b"), "откат на en");
                Equal("d", texts.Get("d"), "неизвестный ключ");
                True(texts.Contains("c"), "Contains учитывает второй язык");
                False(texts.Contains("d"), "Contains не находит отсутствующий ключ");
            });

            Check("строки: ключа нет в ru — ошибка проверки данных", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json", "combat.json", "equipment.json", "topology.json"),
                        ["items/a.json"] = Weapon("wpn_a"),
                        ["Localization/strings.ru.json"] = "{}",
                        ["Localization/strings.en.json"] = "{\"n\":\"Name\"}"
                    },
                    root =>
                    {
                        var report = Preflight.Validate(root);
                        False(report.IsValid, "отсутствие текста — ошибка");
                        True(HasIssue(report, "items/a.json", "'n'"), "нет ошибки про ключ 'n'");
                    });
            });

            Check("строки: ключа нет в en — предупреждение, старт возможен", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json", "combat.json", "equipment.json", "topology.json", "talents.json"),
                        ["items/a.json"] = Weapon("wpn_a"),
                        ["combat.json"] = CombatJson(),
                        ["equipment.json"] = EquipmentJson(),
                        ["topology.json"] = TopologyJson(),
                        ["talents.json"] = TalentsJson(),
                        ["Localization/strings.ru.json"] = "{\"n\":\"Название\"}",
                        ["Localization/strings.en.json"] = "{}"
                    },
                    root =>
                    {
                        var report = Preflight.Validate(root);
                        True(report.IsValid, "неполный en не блокирует старт: " + Describe(report));
                        True(HasWarning(report, "items/a.json", "'n'"), "нет предупреждения про ключ 'n'");
                    });
            });

            Check("строки: каталог текстов отсутствует — предупреждение, старт возможен", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json", "combat.json", "equipment.json", "topology.json", "talents.json"),
                        ["items/a.json"] = Weapon("wpn_a"),
                        ["combat.json"] = CombatJson(),
                        ["equipment.json"] = EquipmentJson(),
                        ["topology.json"] = TopologyJson(),
                        ["talents.json"] = TalentsJson()
                    },
                    root =>
                    {
                        var report = Preflight.Validate(root);
                        True(report.IsValid, "без каталога текстов игра стартует: " + Describe(report));
                        True(HasWarning(report, "strings.ru.json", "не найден"), "нет предупреждения о каталоге");
                    });
            });
        }

        private static List<string> CollectTextKeys(string dataRoot)
        {
            var keys = new List<string>();
            foreach (var file in Directory.EnumerateFiles(dataRoot, "*.json", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(dataRoot, file).Replace('\\', '/');
                if (relative.StartsWith("Localization/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var value = JsonParser.Parse(File.ReadAllText(file), relative);
                Walk(value, keys);
            }

            return keys;
        }

        private static void Walk(JsonValue value, List<string> keys)
        {
            if (value.Kind == JsonKind.Object)
            {
                foreach (var pair in value.Entries)
                {
                    if (pair.Key.EndsWith("Key", StringComparison.Ordinal) && pair.Value.Kind == JsonKind.String)
                    {
                        keys.Add(pair.Value.Text);
                        continue;
                    }

                    Walk(pair.Value, keys);
                }
            }
            else if (value.Kind == JsonKind.Array)
            {
                foreach (var item in value.Items)
                {
                    Walk(item, keys);
                }
            }
        }
    }
}
