#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Repositories
{
    /// <summary>Репозиторий актёров в памяти.</summary>
    public sealed class ActorRepository : IActorRepository
    {
        private readonly Dictionary<string, ActorTemplate> _byId;

        /// <summary>Создаёт репозиторий из готовых актёров.</summary>
        /// <param name="templates">Актёры.</param>
        /// <exception cref="DataException">Дублирующийся id.</exception>
        public ActorRepository(IEnumerable<ActorTemplate> templates)
        {
            _byId = new Dictionary<string, ActorTemplate>(StringComparer.Ordinal);
            foreach (var template in templates)
            {
                if (_byId.ContainsKey(template.Id))
                {
                    throw new DataException(template.Id, CoreErrors.DuplicateRepositoryId);
                }

                _byId[template.Id] = template;
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<ActorTemplate> All => new List<ActorTemplate>(_byId.Values);

        /// <inheritdoc />
        public ActorTemplate Get(string id) =>
            _byId.TryGetValue(id, out var template)
                ? template
                : throw new DataException(id, CoreErrors.ItemNotFound);

        /// <inheritdoc />
        public bool Contains(string id) => _byId.ContainsKey(id);

        /// <summary>Строит репозиторий из загруженных сущностей вида <c>actors</c>.</summary>
        /// <param name="data">Результат загрузки.</param>
        /// <returns>Заполненный репозиторий.</returns>
        public static ActorRepository FromLoadedData(LoadedData data)
        {
            var templates = new List<ActorTemplate>();
            foreach (var entity in data.All("actors"))
            {
                templates.Add(ActorTemplate.FromJson(entity, entity.TryGet("id", out var id) ? id.Text : "actors"));
            }

            return new ActorRepository(templates);
        }
    }
}
