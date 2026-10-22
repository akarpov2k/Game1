#nullable enable
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Repositories
{
    /// <summary>Репозиторий топологии: единственная запись или запасная пустая.</summary>
    public sealed class TopologyRepository : ITopologyRepository
    {
        /// <summary>Создаёт репозиторий поверх готовой топологии.</summary>
        /// <param name="topology">Топология мира.</param>
        public TopologyRepository(TopologyTemplate topology)
        {
            Topology = topology;
        }

        /// <inheritdoc />
        public TopologyTemplate Topology { get; }

        /// <summary>Строит репозиторий из загруженных сущностей вида <c>topology</c>.</summary>
        /// <param name="data">Результат загрузки.</param>
        /// <returns>Заполненный репозиторий.</returns>
        /// <exception cref="DataException">Записей больше одной или запись битая.</exception>
        public static TopologyRepository FromLoadedData(LoadedData data)
        {
            var records = data.All("topology");
            if (records.Count == 0)
            {
                throw new DataException("topology", CoreErrors.FileNotFound);
            }

            if (records.Count > 1)
            {
                throw new DataException("topology", CoreErrors.DuplicateIdFiles, "topology", "topology");
            }

            return new TopologyRepository(TopologyTemplate.FromJson(records[0], "topology"));
        }
    }
}
