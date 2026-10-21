#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Di;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Services;
using AnomalyPath.Core.Templates;
using AnomalyPath.Core.Validation;

namespace AnomalyPath.Tests
{
    /// <summary>
    /// Системные тесты Core: загрузка данных, наследование, коллекции, репозитории,
    /// сервисы, DI и preflight. Без ссылок на UnityEngine — один и тот же набор
    /// запускается консолью и в Unity EditMode.
    /// </summary>
    public static class TestSuite
    {
        private static int _failed;
        private static int _passed;
        private static List<string> _failures = new List<string>();
        private static Action<string> _log = Console.WriteLine;

        /// <summary>Прогоняет набор тестов и пишет результат в консоль.</summary>
        /// <param name="dataRoot">Корень данных.</param>
        /// <returns>Счётчики и список упавших проверок.</returns>
        public static TestRunResult Run(string dataRoot) => Run(dataRoot, Console.WriteLine);

        /// <summary>Прогоняет набор тестов, вывод — в переданный лог.</summary>
        /// <param name="dataRoot">Корень данных.</param>
        /// <param name="log">Куда писать строки результата.</param>
        /// <returns>Счётчики и список упавших проверок.</returns>
        public static TestRunResult Run(string dataRoot, Action<string> log)
        {
            _failed = 0;
            _passed = 0;
            _failures = new List<string>();
            _log = log;
            var source = new FileSystemDataSource(dataRoot);
            CoreText.Configure(JsonStringCatalog.FromSource(source, Bootstrap.DefaultTextCatalog));

            LoadingTests(dataRoot);
            InheritanceTests(dataRoot);
            RepositoryTests(dataRoot);
            DiTests(dataRoot);
            LocalizationTests(source);
            JsonFormatTests();
            FileFormatTests();
            ErrorTests();
            PreflightTests(dataRoot);
            SourceHygieneTests(dataRoot);

            _log($"Итого: {_passed} прошло, {_failed} упало");
            return new TestRunResult(_passed, _failed, _failures);
        }

        /// <summary>
        /// Проверка исходников пакета: каждый файл — UTF-8 без битых символов
        /// и начинается с <c>#nullable enable</c>. Ловит порчу кодировки до Unity.
        /// </summary>
        private static void SourceHygieneTests(string dataRoot)
        {
            Check("исходники: UTF-8 без битых символов", () =>
            {
                var broken = new List<string>();
                foreach (var file in SourceFiles(dataRoot))
                {
                    var text = System.IO.File.ReadAllText(file);
                    if (text.IndexOf('\uFFFD') >= 0)
                    {
                        broken.Add(file);
                    }
                }

                True(broken.Count == 0, "битые символы: " + string.Join(", ", broken));
            });

            Check("исходники: каждый файл начинается с #nullable enable", () =>
            {
                var missing = new List<string>();
                foreach (var file in SourceFiles(dataRoot))
                {
                    var lines = System.IO.File.ReadAllLines(file);
                    var firstLine = lines.Length > 0 ? lines[0] : string.Empty;
                    if (!firstLine.TrimStart('\uFEFF').StartsWith("#nullable", StringComparison.Ordinal))
                    {
                        missing.Add(file);
                    }
                }

                True(missing.Count == 0, "без директивы: " + string.Join(", ", missing));
            });
        }

        private static IEnumerable<string> SourceFiles(string dataRoot)
        {
            var srcRoot = Path.GetFullPath(Path.Combine(dataRoot, "..", "..", "src"));
            if (!Directory.Exists(srcRoot))
            {
                yield break;
            }

            foreach (var file in Directory.EnumerateFiles(srcRoot, "*.cs", SearchOption.AllDirectories))
            {
                yield return file;
            }
        }

        private static void LocalizationTests(IDataSource source)
        {
            Check("локализация: тексты ошибок берутся из файла", () =>
            {
                var ru = JsonStringCatalog.FromSource(source, "Localization/errors.ru.json");
                var en = JsonStringCatalog.FromSource(source, "Localization/errors.en.json");

                var ruText = ru.Get(CoreErrors.FileNotFound);
                var enText = en.Get(CoreErrors.FileNotFound);

                Equal("Файл не найден", ruText, "ru");
                Equal("File not found", enText, "en");
                False(string.Equals(ruText, enText, StringComparison.Ordinal), "тексты должны различаться");
            });

            Check("локализация: наборы ключей ru и en совпадают", () =>
            {
                var ru = JsonStringCatalog.FromSource(source, "Localization/errors.ru.json");
                var en = JsonStringCatalog.FromSource(source, "Localization/errors.en.json");

                foreach (var key in ErrorKeys())
                {
                    True(ru.Contains(key), $"ru: нет ключа {key}");
                    True(en.Contains(key), $"en: нет ключа {key}");
                }
            });

            Check("локализация: подстановка аргументов", () =>
            {
                CoreText.Configure(JsonStringCatalog.FromSource(source, "Localization/errors.ru.json"));
                var text = CoreText.Format(CoreErrors.MissingStringField, "id");
                Contains(text, "'id'", "аргумент поля");
            });

            Check("локализация: неизвестный ключ возвращается как есть", () =>
            {
                var catalog = new JsonStringCatalog(new Dictionary<string, string>());
                Equal("unknown.key", catalog.Get("unknown.key"), "fallback");
            });
        }

        private static IEnumerable<string> ErrorKeys()
        {
            foreach (var field in typeof(CoreErrors).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            {
                if (field.IsLiteral && field.FieldType == typeof(string))
                {
                    yield return (string)field.GetRawConstantValue()!;
                }
            }
        }

        private static void LoadingTests(string dataRoot)
        {
            var data = Load(dataRoot);
            Check("загрузка: две сущности в коллекции", () =>
            {
                Equal(2, data.All("items").Count, "items");
            });

            Check("загрузка: абстрактные шаблоны не зарегистрированы", () =>
            {
                var items = data.All("items");
                foreach (var item in items)
                {
                    var isAbstract = item.TryGet("abstract", out var value) && value.Kind == JsonKind.Bool && value.Bool;
                    False(isAbstract, "абстрактный файл попал в реестр");
                }
            });

            Check("загрузка: id и вид на месте", () =>
            {
                True(data.TryGet("items", "wpn_ak74", out _), "wpn_ak74 не найден");
                True(data.TryGet("items", "wpn_akm", out _), "wpn_akm не найден");
                False(data.TryGet("items", "wpn_unknown", out _), "лишний id");
            });
        }

        private static void InheritanceTests(string dataRoot)
        {
            var container = Bootstrap.Build(dataRoot);
            var items = container.Resolve<IItemRepository>();

            Check("наследование: поля базового шаблона", () =>
            {
                var ak74 = items.Get("wpn_ak74");
                Equal(2.0, ak74.Weapon!.ReloadTime, "reloadTime из _base");
                Equal("dmg_bullet", ak74.Weapon!.DamageTypeId, "damageTypeId из _base");
                Equal(100, ak74.Weapon!.DurabilityMax, "durabilityMax из _base");
                Equal(1, ak74.MaxStack, "maxStack из _base");
                True(ak74.Rotatable, "rotatable из _base");
            });

            Check("наследование: свои числа перекрывают базу", () =>
            {
                var ak74 = items.Get("wpn_ak74");
                var akm = items.Get("wpn_akm");
                Equal(12.0, ak74.Weapon!.Damage, "ak74 damage");
                Equal(14.0, akm.Weapon!.Damage, "akm damage");
                Equal(0.55, ak74.Weapon!.Accuracy, "ak74 accuracy");
                Equal(0.5, akm.Weapon!.Accuracy, "akm accuracy");
                Equal(2, ak74.Tier, "ak74 tier");
                Equal(3, akm.Tier, "akm tier");
            });

            Check("include: миксин цены и свои перекрытия", () =>
            {
                var ak74 = items.Get("wpn_ak74");
                Equal("reputation", ak74.Price!.Mode, "ak74 price.mode");
                Equal(1200.0, ak74.Price!.Base, "ak74 price.base");
                Equal("fct_caravan", ak74.Price!.FactionId, "ak74 price.factionId");

                var akm = items.Get("wpn_akm");
                Equal("fixed", akm.Price!.Mode, "akm price.mode");
                Equal(900.0, akm.Price!.Buy, "akm price.buy");
            });

            Check("include: sellable и droppable из миксина", () =>
            {
                var data = Load(dataRoot);
                JsonValue akm = default!;
                True(data.TryGet("items", "wpn_akm", out akm), "akm не найден");
                True(akm.Get("sellable").Bool, "sellable");
                True(akm.Get("droppable").Bool, "droppable");
            });
        }

        private static void RepositoryTests(string dataRoot)
        {
            var container = Bootstrap.Build(dataRoot);
            var repository = container.Resolve<IItemRepository>();
            var catalog = container.Resolve<IItemCatalogService>();

            Check("репозиторий: все предметы", () =>
            {
                Equal(2, repository.All.Count, "All");
            });

            Check("репозиторий: поиск по id", () =>
            {
                True(repository.TryGet("wpn_ak74", out var ak74), "TryGet ak74");
                Equal("wpn_ak74", ak74.Id, "id");
                Throws<DataException>(() => repository.Get("wpn_unknown"), "Get unknown");
            });

            Check("сервис: выборка по категории", () =>
            {
                Equal(2, catalog.ByCategory("weapon").Count, "weapon");
                Equal(0, catalog.ByCategory("armor").Count, "armor");
            });

            Check("сервис: выборка по тиру", () =>
            {
                var tier2 = catalog.ByTier(2);
                Equal(1, tier2.Count, "tier 2");
                Equal("wpn_ak74", tier2[0].Id, "tier 2 id");
            });

            Check("распределение: id уникальны и разрешаются", () =>
            {
                var ids = new HashSet<string>();
                foreach (var item in repository.All)
                {
                    True(ids.Add(item.Id), $"дубль {item.Id}");
                    Equal(item.Id, repository.Get(item.Id).Id, "Get by id");
                }
            });
        }

        private static void DiTests(string dataRoot)
        {
            Check("DI: репозиторий и сервис разрешаются", () =>
            {
                var container = Bootstrap.Build(dataRoot);
                True(container.IsRegistered<IItemRepository>(), "репозиторий зарегистрирован");
                True(container.IsRegistered<IItemCatalogService>(), "сервис зарегистрирован");
                Equal(2, container.Resolve<IItemCatalogService>().All().Count, "All через сервис");
            });

            Check("DI: повторная регистрация — ошибка", () =>
            {
                var container = new ServiceContainer();
                var repository = new ItemRepository(Array.Empty<ItemTemplate>());
                container.RegisterSingleton<IItemRepository>(repository);
                Throws<InvalidOperationException>(
                    () => container.RegisterSingleton<IItemRepository>(repository),
                    "дубль регистрации");
            });

            Check("DI: незарегистрированный сервис — ошибка", () =>
            {
                Throws<InvalidOperationException>(
                    () => new ServiceContainer().Resolve<IItemCatalogService>(),
                    "resolve missing");
            });
        }

        private static void JsonFormatTests()
        {
            var deep = new string('[', 20) + "1" + new string(']', 20);
            var valid = new (string Name, string Json, Action<JsonValue> Check)[]
            {
                ("null", "null", v => Equal(JsonKind.Null, v.Kind, "kind")),
                ("true", "true", v => True(v.Bool, "bool")),
                ("false", "false", v => False(v.Bool, "bool")),
                ("целое число", "42", v => Equal(42.0, v.Number, "number")),
                ("дробное отрицательное", "-0.5", v => Equal(-0.5, v.Number, "number")),
                ("экспонента", "1.5e-2", v => Equal(0.015, v.Number, "number")),
                ("пустая строка", "\"\"", v => Equal(string.Empty, v.Text, "text")),
                ("кириллица", "\"Привет\"", v => Equal("Привет", v.Text, "text")),
                (
                    "экранирование",
                    "\"a\\\"b\\\\c\\/d\\be\\ff\\ng\\rh\\ti\"",
                    v => Equal("a\"b\\c/d\be\ff\ng\rh\ti", v.Text, "text")),
                ("unicode escape", "\"\\u0416\\u0443\\u043a\"", v => Equal("Жук", v.Text, "text")),
                ("пустой объект", "{}", v => Equal(0, v.Members.Count, "members")),
                (
                    "объект",
                    "{\"a\":1,\"b\":\"x\",\"c\":true,\"d\":null}",
                    v => Equal(4, v.Members.Count, "members")),
                ("вложенный объект", "{\"a\":{\"b\":{\"c\":true}}}", v => True(v.Get("a").Get("b").Get("c").Bool, "nested")),
                ("пустой массив", "[]", v => Equal(0, v.Items.Count, "items")),
                (
                    "массив смешанных значений",
                    "[1,\"x\",true,null]",
                    v => Equal(4, v.Items.Count, "items")),
                ("массив объектов", "[{\"a\":1},{\"b\":2}]", v => Equal(2, v.Items.Count, "items")),
                (
                    "пробелы вокруг значений",
                    "  { \"a\" : [ 1 , 2 ] }  ",
                    v => Equal(2, v.Get("a").Items.Count, "array")),
                ("переводы строк CRLF", "{\r\n \"a\": 1\r\n}", v => Equal(1.0, v.Get("a").Number, "number")),
                ("табуляция", "{\t\"a\":\t1\t}", v => Equal(1.0, v.Get("a").Number, "number")),
                ("BOM в начале", "\uFEFF{\"a\":1}", v => Equal(1.0, v.Get("a").Number, "number")),
                ("дубли ключей: побеждает последний", "{\"a\":1,\"a\":2}", v => Equal(2.0, v.Get("a").Number, "number")),
                ("глубокая вложенность", deep, v => True(v.Items[0].IsArray, "nested array")),
                ("числа в массиве", "[0,-1,2.5,1e3]", v => Equal(4, v.Items.Count, "items"))
            };

            foreach (var testCase in valid)
            {
                Check($"json: {testCase.Name}", () =>
                {
                    var value = JsonParser.Parse(testCase.Json);
                    testCase.Check(value);
                });
            }

            var invalid = new (string Name, string Json)[]
            {
                ("пустой документ", string.Empty),
                ("только пробелы", "   "),
                ("обрыв строки", "\"abc"),
                ("запятая перед закрытием объекта", "{\"a\":1,}"),
                ("запятая перед закрытием массива", "[1,]"),
                ("одинарные кавычки", "{'a':1}"),
                ("ключ без кавычек", "{a:1}"),
                ("нет двоеточия", "{\"a\" 1}"),
                ("нет значения", "{\"a\":}"),
                ("нет закрывающей скобки", "{\"a\":1"),
                ("нет закрывающей скобки массива", "[1,2"),
                ("лишний текст после значения", "{} x"),
                ("плохой литерал", "tru"),
                ("неизвестный escape", "\"\\q\""),
                ("обрезанный unicode", "\"\\u12\""),
                ("NaN", "NaN"),
                ("комментарий", "{/*x*/\"a\":1}"),
                ("нет ключа", "{:1}"),
                ("два значения подряд", "1 2")
            };

            foreach (var testCase in invalid)
            {
                Check($"json-ошибка: {testCase.Name}", () =>
                {
                    var error = Throws<DataException>(() => JsonParser.Parse(testCase.Json, "fixture.json"), testCase.Name);
                    Equal("fixture.json", error.Path, "path");
                });
            }
        }

        private static void FileFormatTests()
        {
            Check("формат: сущность прямо из каталога", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/wpn_x.json"),
                        ["items/wpn_x.json"] = Weapon("wpn_x")
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("items").Count, "items");
                    });
            });

            Check("формат: коллекция с вложенной коллекцией", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/col_a.json"),
                        ["items/col_a.json"] = CollectionWithInclude("items/a.json", "items/col_b.json"),
                        ["items/col_b.json"] = Collection("items/b.json"),
                        ["items/a.json"] = Weapon("wpn_a"),
                        ["items/b.json"] = Weapon("wpn_b")
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(2, data.All("items").Count, "items");
                    });
            });

            Check("формат: абстрактный шаблон не регистрируется, но наследуется", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/wpn_x.json"),
                        ["_base/base.json"] = Abstract("{\"sellable\":true}"),
                        ["items/wpn_x.json"] = Weapon("wpn_x", extends: "_base/base.json")
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("items").Count, "items");
                        True(data.OfKind("items")["wpn_x"].Get("sellable").Bool, "sellable из базы");
                        False(data.TryGet("items", "base", out _), "абстрактный файл не должен регистрироваться");
                    });
            });

            Check("формат: null удаляет унаследованное поле", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/wpn_x.json"),
                        ["_base/base.json"] = Abstract("{\"sellable\":true}"),
                        ["items/wpn_x.json"] = Weapon("wpn_x", extends: "_base/base.json", extra: ",\"sellable\":null")
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        False(data.OfKind("items")["wpn_x"].TryGet("sellable", out _), "sellable должен исчезнуть");
                    });
            });

            Check("формат: массивы заменяются, а не сливаются", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/wpn_x.json"),
                        ["_base/base.json"] = Abstract("{\"tags\":[\"a\",\"b\"]}"),
                        ["items/wpn_x.json"] = Weapon("wpn_x", extends: "_base/base.json", extra: ",\"tags\":[\"c\"]")
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        var tags = data.OfKind("items")["wpn_x"].Get("tags");
                        Equal(1, tags.Items.Count, "tags");
                        Equal("c", tags.Items[0].Text, "tag");
                    });
            });

            Check("формат: повторная коллекция — ошибка", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/col_a.json"),
                        ["items/col_a.json"] = CollectionWithInclude("items/a.json", "items/col_a.json"),
                        ["items/a.json"] = Weapon("wpn_a")
                    },
                    root => Throws<DataException>(() => new DataLoader(new FileSystemDataSource(root)).Load(), "цикл"));
            });
        }

        private static string CollectionWithInclude(string entry, string include)
        {
            return $"{{\"schemaVersion\":1,\"kind\":\"items\",\"collection\":true,\"id\":\"col\",\"entries\":[\"{entry}\"],\"include\":[\"{include}\"]}}";
        }

        private static string CollectionWithIncludeMany(string entriesCsv, string includesCsv)
        {
            var entries = string.Join(",", Array.ConvertAll(entriesCsv.Split(','), e => $"\"{e.Trim()}\""));
            var includes = string.Join(",", Array.ConvertAll(includesCsv.Split(','), i => $"\"{i.Trim()}\""));
            return $"{{\"schemaVersion\":1,\"kind\":\"items\",\"collection\":true,\"id\":\"col\",\"entries\":[{entries}],\"include\":[{includes}]}}";
        }

        private static string Abstract(string body)
        {
            return $"{{\"schemaVersion\":1,\"kind\":\"items\",\"abstract\":true,{body.TrimStart('{').TrimEnd('}')}}}";
        }

        private static void PreflightTests(string dataRoot)
        {
            Check("preflight: данные проекта проходят проверку", () =>
            {
                var result = Preflight.Run(dataRoot);
                True(result.Report.IsValid, "ошибки: " + Describe(result.Report));
                Equal(0, result.Report.Warnings.Count, "предупреждения");
                Contains(result.Report.Summary, "пройдена", "сводка");
                True(result.CanStart, "игра может стартовать");
                Equal(2, result.Services!.Resolve<IItemRepository>().All.Count, "предметы из сервисов");
            });

            Check("preflight: данные читаются один раз", () =>
            {
                var source = new CountingSource(new FileSystemDataSource(dataRoot));
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
                        ["catalog.json"] = Catalog("items/a.json"),
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
                    BuildTranslationFixture(dataRoot),
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
                        ["catalog.json"] = Catalog("items/col.json"),
                        ["items/col.json"] = Collection("items/a.json", "items/b.json", "items/c.json", "items/missing.json"),
                        ["items/a.json"] = Weapon("wpn_dup"),
                        ["items/b.json"] = Weapon("wpn_dup"),
                        ["items/c.json"] = "{\"kind\":\"items\",\"id\":\"wpn_c\",\"category\":\"weapon\",\"footprint\":[1,1],\"maxStack\":1,\"tier\":1,\"icon\":\"i\",\"weapon\":{\"classId\":\"wclass_rifle\",\"damage\":1,\"accuracy\":0.5,\"magazine\":10,\"ammoCaliber\":\"5.45x39\",\"reloadTime\":2,\"durabilityMax\":10,\"durabilityPerShot\":0.1}}"
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
                        ["catalog.json"] = Catalog("items/col.json"),
                        ["items/col.json"] = Collection("items/a.json"),
                        ["items/a.json"] = Weapon("wpn_a"),
                        ["items/orphan.json"] = Weapon("wpn_orphan")
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

            ErrorBehaviorTests(dataRoot);
        }

        /// <summary>
        /// Сценарий «всё сломано»: одна фикстура со всеми видами ошибок.
        /// Проверяет, что отчёт содержит каждую ошибку, игра не стартует и сервисы не создаются.
        /// </summary>
        private static void ErrorBehaviorTests(string dataRoot)
        {
            WithFixture(BuildBrokenFixture(dataRoot), root =>
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

        private static Dictionary<string, string> BuildBrokenFixture(string dataRoot)
        {
            var ru = File.ReadAllText(Path.Combine(dataRoot, "Localization", "errors.ru.json"));
            var en = File.ReadAllText(Path.Combine(dataRoot, "Localization", "errors.en.json"));
            var enWithoutOne = en.Replace("\"error.item_not_found\": \"Item not found in repository\",", string.Empty);

            return new Dictionary<string, string>
            {
                ["catalog.json"] = Catalog("items/col.json", "items/col_dup.json", "topologies/missing.json"),
                ["items/col.json"] = CollectionWithIncludeMany(
                    "items/a.json,items/b.json,items/bad.json,items/absent.json",
                    "items/col_cycle.json"),
                ["items/col_dup.json"] = Collection("items/a.json"),
                ["items/col_cycle.json"] = CollectionWithIncludeMany("items/a.json", "items/col_cycle.json"),
                ["items/a.json"] = Weapon("wpn_a"),
                ["items/b.json"] = Weapon("wpn_a"),
                ["items/bad.json"] = "{\"kind\":\"items\",\"id\":\"wpn_bad\",\"category\":\"weapon\",\"footprint\":[1,1],\"maxStack\":1,\"tier\":1,\"icon\":\"i\"}",
                ["items/broken_syntax.json"] = "{\"kind\":",
                ["items/ext.json"] = Weapon("wpn_ext", extends: "_base/missing.json"),
                ["items/mismatch.json"] = Weapon("wpn_mismatch", extends: "_base/loc.json"),
                ["items/orphan.json"] = Weapon("wpn_orphan"),
                ["_base/loc.json"] = "{\"schemaVersion\":1,\"kind\":\"locations\",\"abstract\":true}",
                ["Localization/errors.ru.json"] = ru,
                ["Localization/errors.en.json"] = enWithoutOne
            };
        }

        private static bool HasIssue(ValidationReport report, string path, string fragment)
        {
            foreach (var issue in report.Errors)
            {
                if (Matches(issue, path, fragment))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasWarning(ValidationReport report, string path, string fragment)
        {
            foreach (var issue in report.Warnings)
            {
                if (Matches(issue, path, fragment))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Matches(ValidationIssue issue, string path, string fragment)
        {
            var pathMatch = path == "items/translation"
                ? issue.Path.StartsWith("Localization/", StringComparison.OrdinalIgnoreCase)
                : issue.Path.Replace('\\', '/').IndexOf(path, StringComparison.OrdinalIgnoreCase) >= 0;
            return pathMatch && issue.Message.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private sealed class CountingSource : IDataSource
        {
            private readonly IDataSource _inner;

            public CountingSource(IDataSource inner)
            {
                _inner = inner;
            }

            public int Reads { get; private set; }

            public bool Exists(string relativePath) => _inner.Exists(relativePath);

            public string ReadAllText(string relativePath)
            {
                Reads++;
                return _inner.ReadAllText(relativePath);
            }

            public IReadOnlyList<string> EnumerateFiles() => _inner.EnumerateFiles();
        }

        private static Dictionary<string, string> BuildTranslationFixture(string dataRoot)
        {
            var ru = File.ReadAllText(Path.Combine(dataRoot, "Localization", "errors.ru.json"));
            var en = File.ReadAllText(Path.Combine(dataRoot, "Localization", "errors.en.json"));
            var enWithExtra = en.TrimEnd().TrimEnd('}') + ",\"extra.key\":\"x\"}";

            return new Dictionary<string, string>
            {
                ["catalog.json"] = Catalog("items/a.json"),
                ["items/a.json"] = Weapon("wpn_a"),
                ["Localization/errors.ru.json"] = ru,
                ["Localization/errors.en.json"] = enWithExtra
            };
        }

        private static bool HasIssue(ValidationReport report, string path)
        {
            foreach (var issue in report.Errors)
            {
                if (issue.Path.Replace('\\', '/').IndexOf(path, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static string Describe(ValidationReport report)
        {
            var parts = new List<string>();
            foreach (var issue in report.Errors)
            {
                parts.Add($"{issue.Path}: {issue.Message}");
            }

            return string.Join("; ", parts);
        }

        private static void ErrorTests()
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

        private static string Catalog(params string[] files)
        {
            var list = string.Join(",", Array.ConvertAll(files, f => $"\"{f}\""));
            return $"{{\"schemaVersion\":1,\"id\":\"core\",\"files\":[{list}]}}";
        }

        private static string Collection(params string[] entries)
        {
            var list = string.Join(",", Array.ConvertAll(entries, e => $"\"{e}\""));
            return $"{{\"schemaVersion\":1,\"kind\":\"items\",\"collection\":true,\"id\":\"col\",\"entries\":[{list}]}}";
        }

        private static string Weapon(string id, string? extends = null, string extra = "")
        {
            var extendsPart = extends == null ? string.Empty : $"\"extends\":\"{extends}\",";
            var extraPart = string.IsNullOrWhiteSpace(extra)
                ? string.Empty
                : extra.Trim().TrimStart(',').TrimEnd(',') + ",";
            return $"{{\"schemaVersion\":1,\"kind\":\"items\",\"id\":\"{id}\",{extendsPart}\"nameKey\":\"n\",\"category\":\"weapon\",\"footprint\":[2,3],\"maxStack\":1,\"tier\":1,\"icon\":\"i\",{extraPart}\"weapon\":{{\"classId\":\"wclass_rifle\",\"damage\":1,\"accuracy\":0.5,\"magazine\":10,\"ammoCaliber\":\"5.45x39\",\"reloadTime\":2,\"durabilityMax\":10,\"durabilityPerShot\":0.1}}}}";
        }

        private static void WithFixture(Dictionary<string, string> files, Action<string> body)
        {
            var root = Path.Combine(Path.GetTempPath(), "anomalypath-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                foreach (var pair in files)
                {
                    var full = Path.Combine(root, pair.Key.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(full)!);
                    File.WriteAllText(full, pair.Value);
                }

                body(root);
            }
            finally
            {
                try
                {
                    Directory.Delete(root, recursive: true);
                }
                catch (IOException)
                {
                    // временная папка не критична для результата теста
                }
            }
        }

        private static LoadedData Load(string dataRoot) =>
            new DataLoader(new FileSystemDataSource(dataRoot)).Load();

        private static void Check(string name, Action body)
        {
            try
            {
                body();
                _passed++;
                _log($"OK   {name}");
            }
            catch (Exception error)
            {
                _failed++;
                _log($"FAIL {name}: {error.Message}");
                _failures.Add($"{name}: {error.Message}");
            }
        }

        private static void True(bool value, string what)
        {
            if (!value)
            {
                throw new InvalidOperationException($"Ожидалось true: {what}");
            }
        }

        private static void False(bool value, string what)
        {
            if (value)
            {
                throw new InvalidOperationException($"Ожидалось false: {what}");
            }
        }

        private static void Equal<T>(T expected, T actual, string what)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new InvalidOperationException($"{what}: ожидалось {expected}, получено {actual}");
            }
        }

        private static void Contains(string text, string fragment, string what)
        {
            if (text.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) < 0)
            {
                throw new InvalidOperationException($"{what}: '{text}' не содержит '{fragment}'");
            }
        }

        private static T Throws<T>(Action body, string what)
            where T : Exception
        {
            try
            {
                body();
            }
            catch (T error)
            {
                return error;
            }
            catch (Exception other)
            {
                throw new InvalidOperationException($"{what}: ожидалось {typeof(T).Name}, получено {other.GetType().Name}: {other.Message}");
            }

            throw new InvalidOperationException($"{what}: исключение не брошено");
        }
    }
}
