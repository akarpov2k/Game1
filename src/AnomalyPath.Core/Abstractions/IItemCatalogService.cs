#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Templates;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Запросы к каталогу предметов: выборки без изменения состояния.</summary>
    public interface IItemCatalogService
    {
        /// <summary>Предметы указанной категории.</summary>
        /// <param name="category">Категория: <c>weapon</c>, <c>armor</c> и т.д.</param>
        /// <returns>Список предметов; пустой, если таких нет.</returns>
        IReadOnlyList<ItemTemplate> ByCategory(string category);

        /// <summary>Предметы указанного тира.</summary>
        /// <param name="tier">Тир предмета.</param>
        /// <returns>Список предметов; пустой, если таких нет.</returns>
        IReadOnlyList<ItemTemplate> ByTier(int tier);

        /// <summary>Все предметы каталога.</summary>
        /// <returns>Список предметов.</returns>
        IReadOnlyList<ItemTemplate> All();
    }
}
