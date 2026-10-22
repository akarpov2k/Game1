#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Репозиторий шаблонов локаций: только чтение.</summary>
    public interface ILocationRepository
    {
        /// <summary>Все шаблоны локаций.</summary>
        IReadOnlyList<LocationTemplate> All { get; }

        /// <summary>Возвращает шаблон по id.</summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Шаблон локации.</returns>
        /// <exception cref="DataException">Локация не найдена.</exception>
        LocationTemplate Get(string id);

        /// <summary>Проверяет, существует ли шаблон.</summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns><c>true</c>, если шаблон существует.</returns>
        bool Contains(string id);
    }
}
