#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace AnomalyPath.Editor.Art
{
    /// <summary>
    /// Читает из данных id картинок: <c>icon</c> у предметов и <c>backgrounds</c> у локаций.
    /// Категория и габариты ищутся по цепочке <c>extends</c>, иначе у оружия не было бы категории.
    /// </summary>
    internal static class DataArtIds
    {
        /// <summary>Иконка предмета с категорией и габаритами.</summary>
        internal sealed class IconId
        {
            /// <summary>Создаёт описание иконки предмета.</summary>
            /// <param name="id">Id вида <c>icons/wpn_ak74</c>.</param>
            /// <param name="category">Категория предмета.</param>
            /// <param name="footprintWidth">Ширина в клетках.</param>
            /// <param name="footprintHeight">Высота в клетках.</param>
            internal IconId(string id, string category, int footprintWidth, int footprintHeight)
            {
                Id = id;
                Category = category;
                FootprintWidth = footprintWidth;
                FootprintHeight = footprintHeight;
            }

            /// <summary>Id иконки.</summary>
            internal string Id { get; }

            /// <summary>Категория предмета.</summary>
            internal string Category { get; }

            /// <summary>Ширина предмета в клетках.</summary>
            internal int FootprintWidth { get; }

            /// <summary>Высота предмета в клетках.</summary>
            internal int FootprintHeight { get; }
        }

        private const string DataRoot = "Assets/Data";
        private const int MaxExtendsDepth = 6;

        private static readonly Regex IconPattern = new Regex("\"icon\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.Compiled);
        private static readonly Regex CategoryPattern = new Regex("\"category\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.Compiled);
        private static readonly Regex ExtendsPattern = new Regex("\"extends\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.Compiled);
        private static readonly Regex FootprintPattern =
            new Regex("\"footprint\"\\s*:\\s*\\[\\s*(\\d+)\\s*,\\s*(\\d+)\\s*\\]", RegexOptions.Compiled);
        private static readonly Regex BackgroundsPattern = new Regex("\"backgrounds\"\\s*:\\s*\\[([^\\]]*)\\]", RegexOptions.Compiled);
        private static readonly Regex QuotedPattern = new Regex("\"([^\"]+)\"", RegexOptions.Compiled);

        /// <summary>Возвращает все id иконок из данных без повторов.</summary>
        /// <returns>Список иконок с категориями и габаритами.</returns>
        internal static IReadOnlyList<IconId> Icons()
        {
            var result = new List<IconId>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var file in Files())
            {
                var text = Read(file);
                if (text == null)
                {
                    continue;
                }

                var icon = IconPattern.Match(text);
                if (!icon.Success || !seen.Add(icon.Groups[1].Value))
                {
                    continue;
                }

                var category = ResolveCategory(text);
                var width = 1;
                var height = 1;
                var footprint = ResolveValue(text, FootprintPattern);
                if (footprint != null && footprint.Groups.Count > 2)
                {
                    width = ParseInt(footprint.Groups[1].Value, 1);
                    height = ParseInt(footprint.Groups[2].Value, 1);
                }

                result.Add(new IconId(icon.Groups[1].Value, category, width, height));
            }

            return result;
        }

        /// <summary>Возвращает все id фонов локаций из данных без повторов.</summary>
        /// <returns>Список id фонов.</returns>
        internal static IReadOnlyList<string> Backgrounds()
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var file in Files())
            {
                var text = Read(file);
                if (text == null)
                {
                    continue;
                }

                foreach (Match match in BackgroundsPattern.Matches(text))
                {
                    foreach (Match quoted in QuotedPattern.Matches(match.Groups[1].Value))
                    {
                        if (seen.Add(quoted.Groups[1].Value))
                        {
                            result.Add(quoted.Groups[1].Value);
                        }
                    }
                }
            }

            return result;
        }

        private static string ResolveCategory(string text) =>
            ResolveValue(text, CategoryPattern)?.Groups[1].Value ?? "other";

        private static Match? ResolveValue(string text, Regex pattern)
        {
            var current = text;
            for (var depth = 0; depth < MaxExtendsDepth; depth++)
            {
                var match = pattern.Match(current);
                if (match.Success)
                {
                    return match;
                }

                var extends = ExtendsPattern.Match(current);
                if (!extends.Success)
                {
                    return null;
                }

                var next = Path.Combine(DataRoot, extends.Groups[1].Value.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(next))
                {
                    return null;
                }

                var inherited = Read(next);
                if (inherited == null)
                {
                    return null;
                }

                current = inherited;
            }

            return null;
        }

        private static int ParseInt(string value, int fallback) =>
            int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed > 0
                ? parsed
                : fallback;

        private static string? Read(string file)
        {
            try
            {
                return File.ReadAllText(file);
            }
            catch (IOException)
            {
                return null;
            }
        }

        private static IEnumerable<string> Files() =>
            Directory.Exists(DataRoot)
                ? Directory.EnumerateFiles(DataRoot, "*.json", SearchOption.AllDirectories)
                : Array.Empty<string>();
    }
}
