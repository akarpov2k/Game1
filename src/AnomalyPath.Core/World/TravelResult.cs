#nullable enable
namespace AnomalyPath.Core.World
{
    /// <summary>Результат перехода: статус и ребро, если переход удался.</summary>
    public sealed class TravelResult
    {
        /// <summary>Создаёт результат.</summary>
        /// <param name="status">Статус перехода.</param>
        /// <param name="edge">Ребро или <c>null</c>.</param>
        public TravelResult(TravelStatus status, WorldEdge? edge = null)
        {
            Status = status;
            Edge = edge;
        }

        /// <summary>Статус перехода.</summary>
        public TravelStatus Status { get; }

        /// <summary>Ребро, если переход удался.</summary>
        public WorldEdge? Edge { get; }

        /// <summary>Переход выполнен.</summary>
        public bool Succeeded => Status == TravelStatus.Success;
    }
}
