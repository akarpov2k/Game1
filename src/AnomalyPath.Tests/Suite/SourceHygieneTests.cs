#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AnomalyPath.Tests
{
    public sealed class SourceHygieneTests : TestBase
    {
        private readonly string _dataRoot;
        public SourceHygieneTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        public override void Run()
        {
            Check("исходники: UTF-8 без битых символов", () =>
            {
                var broken = new List<string>();
                foreach (var file in SourceFiles(_dataRoot))
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
                foreach (var file in SourceFiles(_dataRoot))
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

            Check("исходники: в Core нет литеральных текстов исключений", () =>
            {
                var pattern = new Regex(
                    "throw\\s+new\\s+(?:ArgumentException|ArgumentOutOfRangeException|" +
                    "InvalidOperationException|NotSupportedException)\\s*\\(\\s*\"" +
                    "|ArgumentOutOfRangeException\\s*\\(\\s*nameof\\([^)]+\\)\\s*,\\s*\"");
                var offenders = new List<string>();
                foreach (var file in SourceFiles(_dataRoot))
                {
                    if (file.Replace('\\', '/').IndexOf("/src/AnomalyPath.Core/", StringComparison.Ordinal) < 0)
                    {
                        continue;
                    }

                    if (pattern.IsMatch(System.IO.File.ReadAllText(file)))
                    {
                        offenders.Add(file);
                    }
                }

                True(offenders.Count == 0, "литеральные тексты: " + string.Join(", ", offenders));
            });

            Check("интерфейс: видимый текст только из каталога строк", () =>
            {
                var runtimeRoot = Path.GetFullPath(Path.Combine(_dataRoot, "..", "..", "Assets", "Scripts", "Runtime"));
                if (!Directory.Exists(runtimeRoot))
                {
                    return;
                }

                var pattern = new Regex("\"[^\"\\r\\n]*[А-Яа-яЁё][^\"\\r\\n]*\"");
                var offenders = new List<string>();
                foreach (var file in Directory.EnumerateFiles(runtimeRoot, "*.cs", SearchOption.AllDirectories))
                {
                    var lines = System.IO.File.ReadAllLines(file);
                    for (var index = 0; index < lines.Length; index++)
                    {
                        var line = lines[index];
                        if (line.TrimStart().StartsWith("[Tooltip", StringComparison.Ordinal))
                        {
                            continue;
                        }

                        if (pattern.IsMatch(line))
                        {
                            offenders.Add(file + ":" + (index + 1));
                        }
                    }
                }

                True(offenders.Count == 0, "литералы в интерфейсе: " + string.Join(", ", offenders));
            });
        }
    }
}
