using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Templates;

namespace AnomalyPath.Core.Services
{
    /// <summary>Выборки из каталога предметов. Состояния не меняет, поведения не содержит.</summary>
    public sealed class ItemCatalogService : IItemCatalogService
    {
        private readonly IItemRepository _repository;

        /// <summary>Создаёт сервис поверх репозитория предметов.</summary>
        /// <param name="repository">Источник предметов.</param>
        public ItemCatalogService(IItemRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <inheritdoc />
        public IReadOnlyList<ItemTemplate> ByCategory(string category)
        {
            var result = new List<ItemTemplate>();
            foreach (var item in _repository.All)
            {
                if (string.Equals(item.Category, category, StringComparison.Ordinal))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        /// <inheritdoc />
        public IReadOnlyList<ItemTemplate> ByTier(int tier)
        {
            var result = new List<ItemTemplate>();
            foreach (var item in _repository.All)
            {
                if (item.Tier == tier)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        /// <inheritdoc />
        public IReadOnlyList<ItemTemplate> All() => _repository.All;
    }
}
