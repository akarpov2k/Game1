#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Repositories
{
    /// <summary>Репозиторий шаблонов локаций в памяти.</summary>
    public sealed class LocationRepository : ILocationRepository
    {
        private readonly Dictionary<string, LocationTemplate> _byId;
        private readonly List<LocationTemplate> _all;

        /// <summary>Создаёт репозиторий из готовых шаблонов.</summary>
        /// <param name="templates">Шаблоны локаций.</param>
        /// <exception cref="DataException">Дублирующийся id.</exception>
        public LocationRepository(IEnumerable<LocationTemplate> templates)
        {
            _byId = new Dictionary<string, LocationTemplate>(StringComparer.Ordinal);
            _all = new List<LocationTemplate>();
            foreach (var template in templates)
            {
                if (_byId.ContainsKey(template.Id))
                {
                    throw new DataException(template.Id, CoreErrors.DuplicateRepositoryId);
                }

                _byId[template.Id] = template;
                _all.Add(template);
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<LocationTemplate> All => _all;

        /// <inheritdoc />
        public LocationTemplate Get(string id) =>
            _byId.TryGetValue(id, out var template)
                ? template
                : throw new DataException(id, CoreErrors.WorldInstanceNotFound, id);

        /// <inheritdoc />
        public bool Contains(string id) => _byId.ContainsKey(id);

        /// <summary>Строит репозиторий из загруженных сущностей вида <c>locations</c>.</summary>
        /// <param name="data">Результат загрузки.</param>
        /// <returns>Заполненный репозиторий.</returns>
        public static LocationRepository FromLoadedData(LoadedData data)
        {
            var templates = new List<LocationTemplate>();
            foreach (var entity in data.All("locations"))
            {
                templates.Add(LocationTemplate.FromJson(entity, entity.TryGet("id", out var id) ? id.Text : "locations"));
            }

            return new LocationRepository(templates);
        }
    }
}
