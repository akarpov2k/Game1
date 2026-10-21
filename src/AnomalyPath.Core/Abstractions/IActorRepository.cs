#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Data;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Репозиторий актёров: только чтение.</summary>
    public interface IActorRepository
    {
        /// <summary>Все актёры.</summary>
        IReadOnlyList<ActorTemplate> All { get; }

        /// <summary>Возвращает актёра по id.</summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Актёр.</returns>
        /// <exception cref="DataException">Актёр не найден.</exception>
        ActorTemplate Get(string id);

        /// <summary>Проверяет, существует ли актёр.</summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns><c>true</c>, если актёр существует.</returns>
        bool Contains(string id);
    }
}
