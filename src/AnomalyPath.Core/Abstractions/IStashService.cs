#nullable enable
using AnomalyPath.Core.Loot;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>
    /// Тайники: появление при входе в локацию с растущим шансом и вскрытие
    /// с отдельным пулом лута и шансом джекпота.
    /// </summary>
    public interface IStashService
    {
        /// <summary>Настроены ли тайники в данных.</summary>
        bool Enabled { get; }

        /// <summary>
        /// Проверяет появление тайника при входе в локацию. Хаб не проверяется,
        /// одна локация проверяется один раз; при находке счётчик проверок сбрасывается.
        /// </summary>
        /// <param name="world">Состояние мира: счётчики проверок и seed.</param>
        /// <param name="location">Экземпляр локации.</param>
        /// <returns><c>true</c>, если на локации появился тайник.</returns>
        bool RollOnEntry(WorldState world, LocationInstance location);

        /// <summary>Вскрывает тайник локации и бросает джекпот.</summary>
        /// <param name="world">Состояние мира: счётчик вскрытий и seed.</param>
        /// <param name="location">Экземпляр локации с тайником.</param>
        /// <returns>Статус, лут и признак джекпота.</returns>
        StashOpenResult Open(WorldState world, LocationInstance location);
    }
}
