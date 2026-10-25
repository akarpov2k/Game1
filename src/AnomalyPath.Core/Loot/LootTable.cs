#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Loot
{
    /// <summary>
    /// Пул лута врага: собственные записи и ссылки на общие таблицы.
    /// Один файл — одна таблица, вид <c>lootTables</c>.
    /// </summary>
    public sealed class LootTable
    {
        /// <summary>Создаёт таблицу лута.</summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="sharedTableIds">Общие таблицы, чьи записи входят в пул.</param>
        /// <param name="entries">Собственные записи пула.</param>
        /// <exception cref="ArgumentNullException">Списки не заданы.</exception>
        public LootTable(
            string id,
            IReadOnlyList<string> sharedTableIds,
            IReadOnlyList<LootEntry> entries)
        {
            Id = id;
            SharedTableIds = sharedTableIds ?? throw new ArgumentNullException(nameof(sharedTableIds));
            Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        }

        /// <summary>Идентификатор таблицы.</summary>
        public string Id { get; }

        /// <summary>Общие таблицы, разворачиваемые в пул.</summary>
        public IReadOnlyList<string> SharedTableIds { get; }

        /// <summary>Собственные записи пула.</summary>
        public IReadOnlyList<LootEntry> Entries { get; }

        /// <summary>Читает таблицу лута из записи данных.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Таблица лута.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static LootTable FromJson(JsonValue value, string sourcePath)
        {
            var id = DataMap.RequireString(value, "id", sourcePath);
            var shared = DataMap.OptionalStringArray(value, "sharedTableIds");

            var entries = new List<LootEntry>();
            if (value.TryGet("entries", out var array))
            {
                if (!array.IsArray)
                {
                    throw new DataException(sourcePath, CoreErrors.MissingArrayField, "entries");
                }

                foreach (var item in array.Items)
                {
                    if (!item.IsObject)
                    {
                        throw new DataException(sourcePath, CoreErrors.LootEntryObject, id);
                    }

                    entries.Add(LootEntry.FromJson(item, sourcePath));
                }
            }

            return new LootTable(id, shared, entries);
        }
    }
}
