#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Data
{
    /// <summary>Читает файлы данных относительно корня, не выпуская путь наружу.</summary>
    public sealed class FileSystemDataSource : IDataSource
    {
        private readonly string _root;

        /// <summary>Создаёт источник с корнем данных.</summary>
        /// <param name="root">Абсолютный или относительный путь к папке данных.</param>
        /// <exception cref="ArgumentException">Корень не задан.</exception>
        public FileSystemDataSource(string root)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                throw new ArgumentException(CoreText.Get(CoreErrors.EmptyPath), nameof(root));
            }

            _root = Path.GetFullPath(root);
        }

        /// <summary>Проверяет наличие файла.</summary>
        /// <param name="relativePath">Путь относительно корня.</param>
        /// <returns><c>true</c>, если файл существует.</returns>
        public bool Exists(string relativePath) => File.Exists(Resolve(relativePath));

        /// <summary>Читает текст файла.</summary>
        /// <param name="relativePath">Путь относительно корня.</param>
        /// <returns>Содержимое файла.</returns>
        /// <exception cref="DataException">Файл не найден.</exception>
        public string ReadAllText(string relativePath) => File.ReadAllText(Resolve(relativePath));

        /// <summary>Перечисляет все файлы под корнем.</summary>
        /// <returns>Пути относительно корня с прямыми слэшами.</returns>
        public IReadOnlyList<string> EnumerateFiles()
        {
            var result = new List<string>();
            foreach (var file in Directory.EnumerateFiles(_root, "*", SearchOption.AllDirectories))
            {
                result.Add(file.Substring(_root.Length).TrimStart('\\', '/').Replace('\\', '/'));
            }

            return result;
        }

        private string Resolve(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new DataException("<empty>", CoreErrors.EmptyPath);
            }

            var normalized = relativePath.Replace('\\', '/').TrimStart('/');
            var full = Path.GetFullPath(Path.Combine(_root, normalized));

            if (!full.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
            {
                throw new DataException(relativePath, CoreErrors.PathOutsideRoot);
            }

            return full;
        }
    }
}
