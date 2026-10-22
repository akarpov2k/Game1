#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;

namespace AnomalyPath.Core.Localization
{
    /// <summary>
    /// Доступ к служебным текстам. Каталог подставляется при сборке приложения,
    /// поэтому тексты ошибок локализуются вместе с остальной игрой.
    /// </summary>
    public static class CoreText
    {
        private static IStringCatalog _catalog = new KeyOnlyCatalog();

        /// <summary>Назначает каталог текстов. Вызывается один раз при старте.</summary>
        /// <param name="catalog">Каталог с сообщениями об ошибках.</param>
        public static void Configure(IStringCatalog catalog) => _catalog = catalog;

        /// <summary>Возвращает текст по ключу или сам ключ, если перевода нет.</summary>
        /// <param name="key">Ключ из <see cref="CoreErrors"/>.</param>
        /// <returns>Локализованный текст.</returns>
        public static string Get(string key) => _catalog.Get(key);

        /// <summary>Форматирует текст по ключу с подстановкой аргументов.</summary>
        /// <param name="key">Ключ из <see cref="CoreErrors"/>.</param>
        /// <param name="args">Аргументы для <see cref="string.Format(string, object[])"/>.</param>
        /// <returns>Готовое сообщение.</returns>
        public static string Format(string key, params object[] args)
        {
            var template = _catalog.Get(key);
            return args == null || args.Length == 0 ? template : string.Format(template, args);
        }

        private sealed class KeyOnlyCatalog : IStringCatalog
        {
            public string Get(string key) => key;

            public bool Contains(string key) => false;

            public IReadOnlyCollection<string> Keys => new string[0];
        }
    }
}
