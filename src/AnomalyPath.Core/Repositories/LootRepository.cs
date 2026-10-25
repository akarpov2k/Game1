#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Loot;

namespace AnomalyPath.Core.Repositories
{
    /// <summary>Таблицы лута и настройка полос тира из загруженных данных.</summary>
    public sealed class LootRepository : ILootRepository
    {
        private readonly Dictionary<string, LootTable> _byId;

        /// <summary>Создаёт репозиторий лута.</summary>
        /// <param name="tables">Таблицы лута.</param>
        /// <param name="settings">Настройка полос тира.</param>
        /// <exception cref="ArgumentNullException">Аргументы не заданы.</exception>
        /// <exception cref="DataException">Дублирующийся id таблицы.</exception>
        public LootRepository(IEnumerable<LootTable> tables, LootSettings settings)
        {
            if (tables == null)
            {
                throw new ArgumentNullException(nameof(tables));
            }

            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _byId = new Dictionary<string, LootTable>(StringComparer.Ordinal);
            foreach (var table in tables)
            {
                if (_byId.ContainsKey(table.Id))
                {
                    throw new DataException("lootTables", CoreErrors.DuplicateEntity, table.Id);
                }

                _byId.Add(table.Id, table);
            }
        }

        /// <inheritdoc />
        public LootSettings Settings { get; }

        /// <inheritdoc />
        public IReadOnlyList<LootTable> All => new List<LootTable>(_byId.Values);

        /// <inheritdoc />
        public bool TryGet(string id, out LootTable table) => _byId.TryGetValue(id, out table!);

        /// <inheritdoc />
        public LootTable Get(string id) =>
            _byId.TryGetValue(id, out var table)
                ? table
                : throw new DataException(id, CoreErrors.LootTableNotFound, id);

        /// <summary>Строит репозиторий лута из загруженных данных.</summary>
        /// <param name="data">Результат загрузки.</param>
        /// <returns>Заполненный репозиторий.</returns>
        /// <exception cref="DataException">Настройка лута отсутствует, задана дважды или разобрана с ошибкой.</exception>
        public static LootRepository FromLoadedData(LoadedData data)
        {
            var tables = new List<LootTable>();
            foreach (var record in data.All("lootTables"))
            {
                tables.Add(LootTable.FromJson(record, "lootTables"));
            }

            var settingsRecords = data.All(LootSettings.RecordId);
            if (settingsRecords.Count > 1)
            {
                throw new DataException(LootSettings.RecordId, CoreErrors.LootSettingsMultiple, settingsRecords.Count);
            }

            var settings = settingsRecords.Count == 0
                ? LootSettings.Empty
                : LootSettings.FromJson(settingsRecords[0], LootSettings.RecordId);
            return new LootRepository(tables, settings);
        }
    }
}
