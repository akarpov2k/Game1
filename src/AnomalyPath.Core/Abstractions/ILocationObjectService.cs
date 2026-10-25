#nullable enable
using AnomalyPath.Core.Spawns;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>
    /// Выбор единственного объекта локации при входе: сначала тайник по его шансам,
    /// иначе взвешенный выбор из подходящих записей спавна.
    /// </summary>
    public interface ILocationObjectService
    {
        /// <summary>Выбирает объект локации при входе.</summary>
        /// <param name="world">Состояние мира для броска тайника; <c>null</c> — без тайника.</param>
        /// <param name="location">Экземпляр локации.</param>
        /// <param name="seed">Seed выбора: постоянная часть мира и временная часть входа.</param>
        /// <returns>Объект локации или <c>null</c>, если объекта нет.</returns>
        LocationObject? Select(WorldState? world, LocationInstance location, int seed);
    }
}
