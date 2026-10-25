#nullable enable
using AnomalyPath.Core.Data;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Генератор мира: один seed даёт один детерминированный маршрут.</summary>
    public interface IWorldGenerator
    {
        /// <summary>Строит карту мира: хаб и окно звеньев вперёд.</summary>
        /// <param name="seed">Seed генерации.</param>
        /// <returns>Карта с локациями и рёбрами.</returns>
        /// <exception cref="DataException">В данных нет хаба, пути или подходящего маршрута.</exception>
        WorldMap Generate(int seed);

        /// <summary>
        /// Начинает новый маршрут из хаба: прежние звенья снимаются с карты, номер выхода
        /// растёт, и маршрут строится заново — так каждый выход из хаба даёт другой маршрут.
        /// </summary>
        /// <param name="world">Состояние мира; игрок должен стоять в хабе.</param>
        /// <exception cref="DataException">Игрок не в хабе или в данных нет хаба или пути.</exception>
        void BeginRoute(WorldState world);

        /// <summary>
        /// Достраивает маршрут вперёд от текущей локации до окна <c>route.lookaheadEdges</c>.
        /// Каждое звено строится один раз, поэтому повторный вызов ничего не меняет.
        /// </summary>
        /// <param name="world">Состояние мира: где игрок, докуда достроен маршрут.</param>
        /// <exception cref="DataException">Для текущего звена нет подходящего перехода.</exception>
        void Extend(WorldState world);
    }
}
