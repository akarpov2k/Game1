#nullable enable
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Репозиторий топологии мира: одна запись на игру.</summary>
    public interface ITopologyRepository
    {
        /// <summary>Топология мира.</summary>
        TopologyTemplate Topology { get; }
    }
}
