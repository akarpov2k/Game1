#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Loadouts;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Repositories
{
    /// <summary>Репозиторий наборов снаряжения в памяти.</summary>
    public sealed class LoadoutRepository : ILoadoutRepository
    {
        private readonly Dictionary<string, LoadoutTemplate> _byId;

        /// <summary>Создаёт репозиторий из готовых наборов.</summary>
        /// <param name="loadouts">Наборы снаряжения.</param>
        /// <exception cref="DataException">Дублирующийся id.</exception>
        public LoadoutRepository(IEnumerable<LoadoutTemplate> loadouts)
        {
            _byId = new Dictionary<string, LoadoutTemplate>(StringComparer.Ordinal);
            foreach (var loadout in loadouts)
            {
                if (_byId.ContainsKey(loadout.Id))
                {
                    throw new DataException(loadout.Id, CoreErrors.DuplicateRepositoryId);
                }

                _byId[loadout.Id] = loadout;
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<LoadoutTemplate> All => new List<LoadoutTemplate>(_byId.Values);

        /// <inheritdoc />
        public LoadoutTemplate Get(string id) =>
            _byId.TryGetValue(id, out var loadout)
                ? loadout
                : throw new DataException(id, CoreErrors.LoadoutNotFound, id);

        /// <inheritdoc />
        public bool Contains(string id) => _byId.ContainsKey(id);

        /// <summary>Строит репозиторий из загруженных сущностей вида <c>loadouts</c>.</summary>
        /// <param name="data">Результат загрузки.</param>
        /// <returns>Заполненный репозиторий.</returns>
        public static LoadoutRepository FromLoadedData(LoadedData data)
        {
            var templates = new List<LoadoutTemplate>();
            foreach (var entity in data.All("loadouts"))
            {
                templates.Add(LoadoutTemplate.FromJson(
                    entity,
                    entity.TryGet("id", out var id) ? id.Text : "loadouts"));
            }

            return new LoadoutRepository(templates);
        }
    }
}
