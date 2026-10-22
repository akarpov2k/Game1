#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Nicks
{
    /// <summary>Список ников для случайного выбора при старте новой игры.</summary>
    public sealed class NicknameCatalogTemplate
    {
        /// <summary>Создаёт каталог ников.</summary>
        /// <param name="id">Идентификатор записи.</param>
        /// <param name="names">Ники.</param>
        /// <exception cref="ArgumentException">id пуст.</exception>
        public NicknameCatalogTemplate(string id, IReadOnlyList<string> names)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("id каталога ников не задан", nameof(id));
            }

            Id = id;
            Names = names ?? throw new ArgumentNullException(nameof(names));
        }

        /// <summary>Идентификатор записи.</summary>
        public string Id { get; }

        /// <summary>Ники.</summary>
        public IReadOnlyList<string> Names { get; }

        /// <summary>Читает каталог ников из данных.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Каталог ников.</returns>
        /// <exception cref="DataException">Поле <c>names</c> отсутствует, пустое или содержит пустые строки.</exception>
        public static NicknameCatalogTemplate FromJson(JsonValue value, string sourcePath)
        {
            var id = DataMap.RequireString(value, "id", sourcePath);
            if (!value.TryGet("names", out var names) || !names.IsArray || names.Items.Count == 0)
            {
                throw new DataException(sourcePath, CoreErrors.MissingArrayField, "names");
            }

            var result = new List<string>();
            foreach (var item in names.Items)
            {
                if (item.Kind != JsonKind.String)
                {
                    throw new DataException(sourcePath, CoreErrors.ArrayItemNotString, "names");
                }

                var name = item.Text.Trim();
                if (name.Length == 0)
                {
                    throw new DataException(sourcePath, CoreErrors.ArrayItemNotString, "names");
                }

                result.Add(name);
            }

            return new NicknameCatalogTemplate(id, result);
        }
    }
}
