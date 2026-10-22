#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Переходы по миру: проверка стоимости и перемещение игрока.</summary>
    public interface ITravelService
    {
        /// <summary>Рёбра, видимые из текущей локации.</summary>
        /// <param name="state">Состояние мира.</param>
        /// <returns>Список видимых рёбер.</returns>
        IReadOnlyList<WorldEdge> GetAvailableEdges(WorldState state);

        /// <summary>Проверяет, можно ли пройти по ребру.</summary>
        /// <param name="state">Состояние мира.</param>
        /// <param name="edgeId">Id ребра.</param>
        /// <returns><c>true</c>, если переход возможен.</returns>
        bool CanTravel(WorldState state, string edgeId);

        /// <summary>Выполняет переход, если он возможен.</summary>
        /// <param name="state">Состояние мира.</param>
        /// <param name="edgeId">Id ребра.</param>
        /// <returns>Результат перехода.</returns>
        TravelResult Travel(WorldState state, string edgeId);
    }
}
