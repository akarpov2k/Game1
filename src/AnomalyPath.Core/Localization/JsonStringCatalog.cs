using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;

namespace AnomalyPath.Core.Localization
{
    /// <summary>Каталог текстов из плоского JSON-объекта <c>{ "ключ": "текст" }</c>.</summary>
    public sealed class JsonStringCatalog : IStringCatalog
    {
        private readonly Dictionary<string, string> _texts;

        /// <summary>Создаёт каталог из готового словаря.</summary>
        /// <param name="texts">Пары «ключ — текст».</param>
        public JsonStringCatalog(IReadOnlyDictionary<string, string> texts)
        {
            _texts = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var pair in texts)
            {
                _texts[pair.Key] = pair.Value;
            }
        }

        /// <summary>Читает каталог из файла источника данных.</summary>
        /// <param name="source">Источник файлов.</param>
        /// <param name="relativePath">Путь к JSON-файлу текстов.</param>
        /// <returns>Каталог текстов.</returns>
        /// <exception cref="DataException">Файл отсутствует или содержит не объект строк.</exception>
        public static JsonStringCatalog FromSource(IDataSource source, string relativePath)
        {
            if (!source.Exists(relativePath))
            {
                throw new DataException(relativePath, CoreErrors.FileNotFound);
            }

            return FromValue(JsonParser.Parse(source.ReadAllText(relativePath), relativePath), relativePath);
        }

        /// <summary>Строит каталог из уже разобранного JSON-объекта.</summary>
        /// <param name="value">Разобранный объект «ключ — текст».</param>
        /// <param name="path">Путь файла для сообщения об ошибке.</param>
        /// <returns>Каталог текстов.</returns>
        /// <exception cref="DataException">Значение не объект.</exception>
        public static JsonStringCatalog FromValue(JsonValue value, string path)
        {
            if (!value.IsObject)
            {
                throw new DataException(path, CoreErrors.JsonNotObject);
            }

            var texts = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var pair in value.Members)
            {
                if (pair.Value.Kind == JsonKind.String)
                {
                    texts[pair.Key] = pair.Value.Text;
                }
            }

            return new JsonStringCatalog(texts);
        }

        /// <summary>Ищет текст по ключу.</summary>
        /// <param name="key">Ключ.</param>
        /// <returns>Текст или сам ключ, если перевода нет.</returns>
        public string Get(string key) =>
            _texts.TryGetValue(key, out var text) ? text : key;

        /// <summary>Проверяет наличие ключа.</summary>
        /// <param name="key">Ключ.</param>
        /// <returns><c>true</c>, если ключ есть.</returns>
        public bool Contains(string key) => _texts.ContainsKey(key);

        /// <summary>Все ключи каталога.</summary>
        public IReadOnlyCollection<string> Keys => _texts.Keys;
    }
}
