#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Spawns;

namespace AnomalyPath.Core.Repositories
{
    /// <summary>Записи спавна объектов локаций из загруженных данных.</summary>
    public sealed class SpawnRepository : ISpawnRepository
    {
        private readonly Dictionary<string, SpawnEntry> _byId;

        /// <summary>Создаёт репозиторий спавна.</summary>
        /// <param name="entries">Записи спавна.</param>
        /// <exception cref="ArgumentNullException">Список не задан.</exception>
        /// <exception cref="DataException">Дублирующийся id записи.</exception>
        public SpawnRepository(IEnumerable<SpawnEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            _byId = new Dictionary<string, SpawnEntry>(StringComparer.Ordinal);
            foreach (var entry in entries)
            {
                if (_byId.ContainsKey(entry.Id))
                {
                    throw new DataException("spawns", CoreErrors.DuplicateEntity, entry.Id);
                }

                _byId.Add(entry.Id, entry);
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<SpawnEntry> All => new List<SpawnEntry>(_byId.Values);

        /// <inheritdoc />
        public bool TryGet(string id, out SpawnEntry? entry) => _byId.TryGetValue(id, out entry);

        /// <inheritdoc />
        public SpawnEntry Get(string id) =>
            _byId.TryGetValue(id, out var entry)
                ? entry
                : throw new DataException(id, CoreErrors.SpawnNotFound, id);

        /// <summary>Строит репозиторий из загруженных данных.</summary>
        /// <param name="data">Результат загрузки.</param>
        /// <returns>Заполненный репозиторий.</returns>
        /// <exception cref="DataException">Запись вида <c>spawns</c> разобрана с ошибкой.</exception>
        public static SpawnRepository FromLoadedData(LoadedData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var entries = new List<SpawnEntry>();
            foreach (var record in data.All("spawns"))
            {
                entries.Add(SpawnEntry.FromJson(record, "spawns"));
            }

            return new SpawnRepository(entries);
        }
    }
}
