using System.Collections.Generic;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Источник файлов данных: в игре — папка, в тестах — временная папка или память.</summary>
    public interface IDataSource
    {
        /// <summary>Проверяет, существует ли файл по относительному пути.</summary>
        /// <param name="relativePath">Путь относительно корня источника.</param>
        /// <returns><c>true</c>, если файл существует.</returns>
        bool Exists(string relativePath);

        /// <summary>Читает текст файла по относительному пути.</summary>
        /// <param name="relativePath">Путь относительно корня источника.</param>
        /// <returns>Содержимое файла целиком.</returns>
        /// <exception cref="Data.DataException">Файл не найден или путь выходит за корень.</exception>
        string ReadAllText(string relativePath);

        /// <summary>Перечисляет все файлы источника относительно его корня.</summary>
        /// <returns>Пути с прямыми слэшами; порядок не гарантирован.</returns>
        IReadOnlyList<string> EnumerateFiles();
    }
}
