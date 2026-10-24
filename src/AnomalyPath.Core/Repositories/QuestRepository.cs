#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Quests;

namespace AnomalyPath.Core.Repositories
{
    /// <summary>Репозиторий шаблонов квестов в памяти.</summary>
    public sealed class QuestRepository : IQuestRepository
    {
        private readonly Dictionary<string, QuestTemplate> _byId;
        private readonly List<QuestTemplate> _all;

        /// <summary>Создаёт репозиторий из готовых шаблонов.</summary>
        /// <param name="templates">Шаблоны квестов.</param>
        /// <exception cref="DataException">Дублирующийся id.</exception>
        public QuestRepository(IEnumerable<QuestTemplate> templates)
        {
            _byId = new Dictionary<string, QuestTemplate>(StringComparer.Ordinal);
            _all = new List<QuestTemplate>();
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
        public IReadOnlyList<QuestTemplate> All => _all;

        /// <inheritdoc />
        public bool TryGet(string id, out QuestTemplate template) => _byId.TryGetValue(id, out template!);

        /// <inheritdoc />
        public bool Contains(string id) => _byId.ContainsKey(id);

        /// <inheritdoc />
        public QuestTemplate Get(string id) =>
            _byId.TryGetValue(id, out var template)
                ? template
                : throw new DataException(id, CoreErrors.QuestNotFound);

        /// <summary>Строит репозиторий из загруженных сущностей вида <c>quests</c>.</summary>
        /// <param name="data">Результат загрузки.</param>
        /// <returns>Заполненный репозиторий.</returns>
        public static QuestRepository FromLoadedData(LoadedData data)
        {
            var templates = new List<QuestTemplate>();
            foreach (var entity in data.All("quests"))
            {
                templates.Add(QuestTemplate.FromJson(entity, entity.TryGet("id", out var id) ? id.Text : "quests"));
            }

            return new QuestRepository(templates);
        }
    }
}
