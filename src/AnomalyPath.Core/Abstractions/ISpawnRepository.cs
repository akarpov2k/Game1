#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Spawns;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Записи спавна объектов локаций.</summary>
    public interface ISpawnRepository
    {
        /// <summary>Все записи спавна.</summary>
        IReadOnlyList<SpawnEntry> All { get; }

        /// <summary>Ищет запись по id.</summary>
        /// <param name="id">Идентификатор записи.</param>
        /// <param name="entry">Найденная запись или <c>null</c>.</param>
        /// <returns><c>true</c>, если запись найдена.</returns>
        bool TryGet(string id, out SpawnEntry? entry);

        /// <summary>Возвращает запись по id.</summary>
        /// <param name="id">Идентификатор записи.</param>
        /// <returns>Запись спавна.</returns>
        SpawnEntry Get(string id);
    }
}
