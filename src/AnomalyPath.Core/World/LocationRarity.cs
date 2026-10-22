#nullable enable
namespace AnomalyPath.Core.World
{
    /// <summary>Редкость локации: код в данных — имя члена в нижнем регистре.</summary>
    public enum LocationRarity
    {
        /// <summary>Обычная локация, шанс появления 1.</summary>
        Common,

        /// <summary>Нечастая локация.</summary>
        Uncommon,

        /// <summary>Редкая локация с уменьшенным шансом появления.</summary>
        Rare,

        /// <summary>Уникальная квестовая локация, одна на прохождение.</summary>
        Unique
    }
}
