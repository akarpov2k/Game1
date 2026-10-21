using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Templates;

namespace AnomalyPath.Core.Repositories
{
    /// <summary>Репозиторий предметов в памяти: строится один раз при загрузке данных.</summary>
    public sealed class ItemRepository : IItemRepository
    {
        private readonly Dictionary<string, ItemTemplate> _byId;
        private readonly List<ItemTemplate> _all;

        /// <summary>Создаёт репозиторий из готовых описаний.</summary>
        /// <param name="templates">Описания предметов.</param>
        /// <exception cref="DataException">Дублирующийся id.</exception>
        public ItemRepository(IEnumerable<ItemTemplate> templates)
        {
            _byId = new Dictionary<string, ItemTemplate>(StringComparer.Ordinal);
            _all = new List<ItemTemplate>();

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
        public IReadOnlyList<ItemTemplate> All => _all;

        /// <inheritdoc />
        public bool TryGet(string id, out ItemTemplate template) => _byId.TryGetValue(id, out template!);

        /// <inheritdoc />
        public ItemTemplate Get(string id) =>
            _byId.TryGetValue(id, out var template)
                ? template
                : throw new DataException(id, CoreErrors.ItemNotFound);

        /// <summary>Строит репозиторий из загруженных сущностей вида <c>items</c>.</summary>
        /// <param name="data">Результат загрузки.</param>
        /// <returns>Заполненный репозиторий.</returns>
        /// <exception cref="DataException">Сущность не соответствует схеме предмета.</exception>
        public static ItemRepository FromLoadedData(LoadedData data)
        {
            var templates = new List<ItemTemplate>();
            foreach (var entity in data.All("items"))
            {
                templates.Add(ItemTemplate.FromJson(entity, entity.TryGet("id", out var id) ? id.Text : "items"));
            }

            return new ItemRepository(templates);
        }
    }
}
