#nullable enable
using System;
using System.Collections.Generic;
using System.IO;

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
        }
    }
}
