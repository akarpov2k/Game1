#nullable enable
using AnomalyPath.Core.Data;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Генератор мира: один seed даёт один детерминированный маршрут.</summary>
    public interface IWorldGenerator
    {
        /// <summary>Строит карту мира.</summary>
        /// <param name="seed">Seed генерации.</param>
        /// <returns>Карта с локациями и рёбрами.</returns>
        /// <exception cref="DataException">В данных нет хаба, точки интереса или подходящего маршрута.</exception>
        WorldMap Generate(int seed);
    }
}
