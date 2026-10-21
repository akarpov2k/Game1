#nullable enable
using System.Collections.Generic;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Каталог текстов по ключу: сообщения об ошибках и другие служебные строки.</summary>
    public interface IStringCatalog
    {
        /// <summary>Возвращает текст по ключу.</summary>
        /// <param name="key">Ключ вида <c>error.file_not_found</c>.</param>
        /// <returns>Локализованный текст; если ключа нет — сам ключ.</returns>
        string Get(string key);

        /// <summary>Проверяет наличие ключа в каталоге.</summary>
        /// <param name="key">Ключ.</param>
        /// <returns><c>true</c>, если ключ есть.</returns>
        bool Contains(string key);

        /// <summary>Все ключи каталога.</summary>
        IReadOnlyCollection<string> Keys { get; }
    }
}
