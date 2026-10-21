#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Templates;
using AnomalyPath.Core.Validation;

namespace AnomalyPath.Tests
{
    public abstract class TestBase
    {
        private int _passed;
        private int _failed;
        private readonly List<string> _failures = new List<string>();

        /// <summary>Куда писать строки результата.</summary>
        public Action<string> Log { get; set; } = Console.WriteLine;

        /// <summary>Сколько проверок прошло.</summary>
        public int Passed => _passed;

        /// <summary>Сколько проверок упало.</summary>
        public int Failed => _failed;

        /// <summary>Описания упавших проверок.</summary>
        public IReadOnlyList<string> Failures => _failures;

        /// <summary>Прогоняет проверки категории.</summary>
        public abstract void Run();
        protected IEnumerable<string> SourceFiles(string dataRoot)
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

        protected ItemInstance Instance(string itemId) => new ItemInstance("inst_" + itemId, itemId, 1);

        protected int CountPlaced(GridContainer grid)
        {
            var count = 0;
            foreach (var item in grid.Items)
            {
                if (!item.Overflow)
                {
                    count++;
                }
            }

            return count;
        }

        protected int CountArtifacts(Equipment equipment)
        {
            var count = 0;
            foreach (var artifact in equipment.Artifacts)
            {
                if (artifact != null)
                {
                    count++;
                }
            }

            return count;
        }

        protected (IInventoryService Service, GridContainer Grid) NewInventory(int width, int rows)
        {
            var templates = new[]
            {
                Template(TestData.RifleJson),
                Template(TestData.AmmoJson),
                Template(TestData.CrateJson),
                Template(TestData.PartJson)
            };

            var repository = new ItemRepository(templates);
            var service = new InventoryService(repository, new SequentialInstanceIdGenerator());
            return (service, new GridContainer("test", width, rows));
        }

        protected ItemTemplate Template(string json)
        {
            return ItemTemplate.FromJson(JsonParser.Parse(json, "test"), "test");
        }

        protected int PlacedCount(GridContainer grid)
        {
            var count = 0;
            foreach (var item in grid.Items)
            {
                if (!item.Overflow)
                {
                    count++;
                }
            }

            return count;
        }

        protected string ResolveDataRootForTests()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "Assets", "Data");
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            throw new InvalidOperationException("Не найдена папка Assets/Data");
        }

        protected IEnumerable<string> ErrorKeys()
        {
            foreach (var field in typeof(CoreErrors).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            {
                if (field.IsLiteral && field.FieldType == typeof(string))
                {
                    yield return (string)field.GetRawConstantValue()!;
                }
            }
        }

        protected string CollectionWithInclude(string entry, string include)
        {
            return $"{{\"schemaVersion\":1,\"kind\":\"items\",\"collection\":true,\"id\":\"col\",\"entries\":[\"{entry}\"],\"include\":[\"{include}\"]}}";
        }

        protected string CollectionWithIncludeMany(string entriesCsv, string includesCsv)
        {
            var entries = string.Join(",", Array.ConvertAll(entriesCsv.Split(','), e => $"\"{e.Trim()}\""));
            var includes = string.Join(",", Array.ConvertAll(includesCsv.Split(','), i => $"\"{i.Trim()}\""));
            return $"{{\"schemaVersion\":1,\"kind\":\"items\",\"collection\":true,\"id\":\"col\",\"entries\":[{entries}],\"include\":[{includes}]}}";
        }

        protected string Abstract(string body)
        {
            return $"{{\"schemaVersion\":1,\"kind\":\"items\",\"abstract\":true,{body.TrimStart('{').TrimEnd('}')}}}";
        }

        protected Dictionary<string, string> BuildBrokenFixture(string dataRoot)
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

        protected bool HasIssue(ValidationReport report, string path, string fragment)
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

        protected bool HasWarning(ValidationReport report, string path, string fragment)
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

        protected bool Matches(ValidationIssue issue, string path, string fragment)
        {
            var pathMatch = path == "items/translation"
                ? issue.Path.StartsWith("Localization/", StringComparison.OrdinalIgnoreCase)
                : issue.Path.Replace('\\', '/').IndexOf(path, StringComparison.OrdinalIgnoreCase) >= 0;
            return pathMatch && issue.Message.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        protected Dictionary<string, string> BuildTranslationFixture(string dataRoot)
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

        protected bool HasIssue(ValidationReport report, string path)
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

        protected string Describe(ValidationReport report)
        {
            var parts = new List<string>();
            foreach (var issue in report.Errors)
            {
                parts.Add($"{issue.Path}: {issue.Message}");
            }

            return string.Join("; ", parts);
        }

        protected string Catalog(params string[] files)
        {
            var list = string.Join(",", Array.ConvertAll(files, f => $"\"{f}\""));
            return $"{{\"schemaVersion\":1,\"id\":\"core\",\"files\":[{list}]}}";
        }

        protected string Collection(params string[] entries)
        {
            var list = string.Join(",", Array.ConvertAll(entries, e => $"\"{e}\""));
            return $"{{\"schemaVersion\":1,\"kind\":\"items\",\"collection\":true,\"id\":\"col\",\"entries\":[{list}]}}";
        }

        protected string Weapon(string id, string? extends = null, string extra = "")
        {
            var extendsPart = extends == null ? string.Empty : $"\"extends\":\"{extends}\",";
            var extraPart = string.IsNullOrWhiteSpace(extra)
                ? string.Empty
                : extra.Trim().TrimStart(',').TrimEnd(',') + ",";
            return $"{{\"schemaVersion\":1,\"kind\":\"items\",\"id\":\"{id}\",{extendsPart}\"nameKey\":\"n\",\"category\":\"weapon\",\"footprint\":[2,3],\"maxStack\":1,\"tier\":1,\"icon\":\"i\",{extraPart}\"weapon\":{{\"classId\":\"wclass_rifle\",\"damage\":1,\"accuracy\":0.5,\"magazine\":10,\"ammoCaliber\":\"5.45x39\",\"reloadTime\":2,\"durabilityMax\":10,\"durabilityPerShot\":0.1}}}}";
        }

        protected void WithFixture(Dictionary<string, string> files, Action<string> body)
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

        protected LoadedData Load(string dataRoot) =>
            new DataLoader(new FileSystemDataSource(dataRoot)).Load();

        protected void Check(string name, Action body)
        {
            try
            {
                body();
                _passed++;
                Log($"OK   {name}");
            }
            catch (Exception error)
            {
                _failed++;
                Log($"FAIL {name}: {error.Message}");
                _failures.Add($"{name}: {error.Message}");
            }
        }

        protected void True(bool value, string what)
        {
            if (!value)
            {
                throw new InvalidOperationException($"Ожидалось true: {what}");
            }
        }

        protected void False(bool value, string what)
        {
            if (value)
            {
                throw new InvalidOperationException($"Ожидалось false: {what}");
            }
        }

        protected void Equal<T>(T expected, T actual, string what)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new InvalidOperationException($"{what}: ожидалось {expected}, получено {actual}");
            }
        }

        protected void Contains(string text, string fragment, string what)
        {
            if (text.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) < 0)
            {
                throw new InvalidOperationException($"{what}: '{text}' не содержит '{fragment}'");
            }
        }

        protected T Throws<T>(Action body, string what)
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
