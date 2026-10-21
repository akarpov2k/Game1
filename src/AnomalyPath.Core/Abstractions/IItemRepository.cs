using System.Collections.Generic;
using AnomalyPath.Core.Templates;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Доступ к предметам: единственный путь к данным предметов из игрового кода.</summary>
    public interface IItemRepository
    {
        /// <summary>Все предметы, загруженные при старте, в порядке объявления.</summary>
        IReadOnlyList<ItemTemplate> All { get; }

        /// <summary>Ищет предмет по id.</summary>
        /// <param name="id">Идентификатор предмета.</param>
        /// <param name="template">Найденное описание или <c>null</c>.</param>
        /// <returns><c>true</c>, если предмет найден.</returns>
        bool TryGet(string id, out ItemTemplate template);

        /// <summary>Возвращает предмет по id.</summary>
        /// <param name="id">Идентификатор предмета.</param>
        /// <returns>Описание предмета.</returns>
        /// <exception cref="Data.DataException">Предмет не найден.</exception>
        ItemTemplate Get(string id);
    }
}
