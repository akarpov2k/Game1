#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Loadouts;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Доступ к подготовленным наборам снаряжения.</summary>
    public interface ILoadoutRepository
    {
        /// <summary>Все наборы.</summary>
        IReadOnlyList<LoadoutTemplate> All { get; }

        /// <summary>Возвращает набор по id.</summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Набор снаряжения.</returns>
        /// <exception cref="DataException">Набор не найден.</exception>
        LoadoutTemplate Get(string id);

        /// <summary>Проверяет, существует ли набор.</summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns><c>true</c>, если набор существует.</returns>
        bool Contains(string id);
    }
}
