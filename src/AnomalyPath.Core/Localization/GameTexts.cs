#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;

namespace AnomalyPath.Core.Localization
{
    /// <summary>
    /// Тексты игры поверх двух каталогов: текущий язык и английский как откат.
    /// Если ключа нет нигде, возвращается сам ключ — игра не падает из-за перевода.
    /// </summary>
    public sealed class GameTexts : IGameTexts
    {
        private readonly IStringCatalog _primary;
        private readonly IStringCatalog? _fallback;

        /// <summary>Создаёт каталог текстов игры.</summary>
        /// <param name="primary">Каталог текущего языка.</param>
        /// <param name="fallback">Каталог второго языка или <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">Основной каталог не задан.</exception>
        public GameTexts(IStringCatalog primary, IStringCatalog? fallback = null)
        {
            _primary = primary ?? throw new ArgumentNullException(nameof(primary));
            _fallback = fallback;
        }

        /// <summary>Пустой каталог: любой ключ возвращается как есть.</summary>
        public static IGameTexts Empty { get; } = new GameTexts(new JsonStringCatalog(new Dictionary<string, string>()));

        /// <summary>Читает каталоги из источника данных.</summary>
        /// <param name="source">Источник файлов.</param>
        /// <param name="primary">Путь к каталогу текущего языка.</param>
        /// <param name="fallback">Путь к каталогу второго языка.</param>
        /// <returns>Каталог текстов; недостающий второй язык просто не используется.</returns>
        /// <exception cref="ArgumentNullException">Источник или путь не заданы.</exception>
        public static GameTexts FromSource(IDataSource source, string primary, string fallback)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var main = JsonStringCatalog.FromSource(source, primary);
            JsonStringCatalog? second = source.Exists(fallback)
                ? JsonStringCatalog.FromSource(source, fallback)
                : null;
            return new GameTexts(main, second);
        }

        /// <inheritdoc />
        public string Get(string key)
        {
            if (_primary.Contains(key))
            {
                return _primary.Get(key);
            }

            if (_fallback != null && _fallback.Contains(key))
            {
                return _fallback.Get(key);
            }

            return key;
        }

        /// <inheritdoc />
        public bool Contains(string key) =>
            _primary.Contains(key) || (_fallback != null && _fallback.Contains(key));

        /// <inheritdoc />
        public IReadOnlyCollection<string> Keys => _primary.Keys;
    }
}
