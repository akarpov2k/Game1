#nullable enable
using System.Collections.Generic;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>
    /// Тексты игры по ключу: заголовки локаций, названия предметов, подписи интерфейса.
    /// Отличается от каталога сообщений об ошибках: разрешение идёт «текущий язык → en → сам ключ».
    /// </summary>
    public interface IGameTexts
    {
        /// <summary>Возвращает текст по ключу.</summary>
        /// <param name="key">Ключ вида <c>item.wpn_ak74.name</c>.</param>
        /// <returns>Текст текущего языка, затем английский, затем сам ключ.</returns>
        string Get(string key);

        /// <summary>Проверяет наличие ключа хотя бы в одном языке.</summary>
        /// <param name="key">Ключ.</param>
        /// <returns><c>true</c>, если ключ есть.</returns>
        bool Contains(string key);

        /// <summary>Ключи основного языка.</summary>
        IReadOnlyCollection<string> Keys { get; }
    }
}
