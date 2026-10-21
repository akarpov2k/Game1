#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Templates;

namespace AnomalyPath.Core.Repositories
{
    /// <summary>Репозиторий типов урона в памяти: строится один раз при загрузке данных.</summary>
    public sealed class DamageTypeRepository : IDamageTypeRepository
    {
        private readonly Dictionary<string, DamageTypeTemplate> _byId;
        private readonly List<DamageTypeTemplate> _all;

        /// <summary>Создаёт репозиторий из готовых типов урона.</summary>
        /// <param name="templates">Типы урона.</param>
        /// <exception cref="DataException">Дублирующийся id.</exception>
        public DamageTypeRepository(IEnumerable<DamageTypeTemplate> templates)
        {
            _byId = new Dictionary<string, DamageTypeTemplate>(StringComparer.Ordinal);
            _all = new List<DamageTypeTemplate>();

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
        public IReadOnlyList<DamageTypeTemplate> All => _all;

        /// <inheritdoc />
        public bool TryGet(string id, out DamageTypeTemplate template) => _byId.TryGetValue(id, out template!);

        /// <inheritdoc />
        public DamageTypeTemplate Get(string id) =>
            _byId.TryGetValue(id, out var template)
                ? template
                : throw new DataException(id, CoreErrors.ItemNotFound);

        /// <inheritdoc />
        public bool Contains(string id) => _byId.ContainsKey(id);

        /// <summary>Строит репозиторий из загруженных сущностей вида <c>damageTypes</c>.</summary>
        /// <param name="data">Результат загрузки.</param>
        /// <returns>Заполненный репозиторий.</returns>
        /// <exception cref="DataException">Сущность не соответствует схеме типа урона.</exception>
        public static DamageTypeRepository FromLoadedData(LoadedData data)
        {
            var templates = new List<DamageTypeTemplate>();
            foreach (var entity in data.All("damageTypes"))
            {
                templates.Add(DamageTypeTemplate.FromJson(entity, entity.TryGet("id", out var id) ? id.Text : "damageTypes"));
            }

            return new DamageTypeRepository(templates);
        }
    }
}
