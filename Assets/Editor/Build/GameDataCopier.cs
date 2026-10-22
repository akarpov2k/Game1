#nullable enable
using System;
using System.IO;

namespace AnomalyPath.Editor
{
    /// <summary>
    /// Кладёт папку данных игры в <c>StreamingAssets</c> готовой сборки.
    /// Нужно потому, что в билде <c>GameBootstrap</c> читает данные из <c>Application.streamingAssetsPath</c>,
    /// а не из <c>Assets/Data</c>.
    /// </summary>
    public static class GameDataCopier
    {
        /// <summary>
        /// Копирует данные рядом с исполняемым файлом сборки.
        /// </summary>
        /// <param name="executablePath">Путь к собранному <c>.exe</c>.</param>
        /// <param name="sourceRoot">Папка данных проекта, обычно <c>Assets/Data</c>.</param>
        /// <returns>Папка данных внутри сборки.</returns>
        /// <exception cref="ArgumentException">Путь к сборке пуст.</exception>
        /// <exception cref="DirectoryNotFoundException">Папка данных проекта не найдена.</exception>
        public static string CopyToBuild(string executablePath, string sourceRoot)
        {
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                throw new ArgumentException("Пустой путь к сборке", nameof(executablePath));
            }

            if (!Directory.Exists(sourceRoot))
            {
                throw new DirectoryNotFoundException($"Папка данных не найдена: {sourceRoot}");
            }

            var target = Path.Combine(BuildStreamingAssets(executablePath), "Data");
            if (Directory.Exists(target))
            {
                Directory.Delete(target, recursive: true);
            }

            CopyDirectory(sourceRoot, target);
            return target;
        }

        /// <summary>
        /// Куда сборка кладёт <c>StreamingAssets</c>: <c>&lt;exe&gt;_Data/StreamingAssets</c>.
        /// </summary>
        /// <param name="executablePath">Путь к собранному <c>.exe</c>.</param>
        /// <returns>Папка <c>StreamingAssets</c> сборки.</returns>
        public static string BuildStreamingAssets(string executablePath)
        {
            var directory = Path.GetDirectoryName(executablePath) ?? ".";
            var name = Path.GetFileNameWithoutExtension(executablePath) + "_Data";
            return Path.Combine(directory, name, "StreamingAssets");
        }

        /// <summary>Проверяет, что в папке данных есть каталог контента.</summary>
        /// <param name="dataRoot">Папка данных.</param>
        /// <returns><c>true</c>, если <c>catalog.json</c> на месте.</returns>
        public static bool HasCatalog(string dataRoot) =>
            !string.IsNullOrWhiteSpace(dataRoot) && File.Exists(Path.Combine(dataRoot, "catalog.json"));

        /// <summary>
        /// Рекурсивно копирует папку. <c>.meta</c>-файлы пропускаются: в сборке Unity их нет.
        /// </summary>
        /// <param name="source">Откуда копировать.</param>
        /// <param name="target">Куда копировать.</param>
        /// <returns>Сколько файлов скопировано.</returns>
        public static int CopyDirectory(string source, string target)
        {
            Directory.CreateDirectory(target);
            var copied = 0;

            foreach (var file in Directory.EnumerateFiles(source))
            {
                if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                File.Copy(file, Path.Combine(target, Path.GetFileName(file)), overwrite: true);
                copied++;
            }

            foreach (var directory in Directory.EnumerateDirectories(source))
            {
                var name = Path.GetFileName(directory);
                if (name.EndsWith("~", StringComparison.Ordinal))
                {
                    continue;
                }

                copied += CopyDirectory(directory, Path.Combine(target, name));
            }

            return copied;
        }
    }
}
