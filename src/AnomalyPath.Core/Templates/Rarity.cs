#nullable enable
namespace AnomalyPath.Core.Templates
{
    /// <summary>
    /// Ступень редкости предмета и противника. Числовые значения — множитель шанса лута:
    /// обычная 1, редкая 2, особая 3, эпическая 4, легендарная 5, уникальная 6.
    /// Код в данных — имя члена в нижнем регистре.
    /// </summary>
    public enum Rarity
    {
        /// <summary>Обычная ступень; множитель шанса 1.</summary>
        Common = 1,

        /// <summary>Редкая ступень; множитель шанса 2.</summary>
        Rare = 2,

        /// <summary>Особая ступень; множитель шанса 3.</summary>
        Special = 3,

        /// <summary>Эпическая ступень; множитель шанса 4.</summary>
        Epic = 4,

        /// <summary>Легендарная ступень; множитель шанса 5.</summary>
        Legendary = 5,

        /// <summary>Уникальная ступень; множитель шанса 6.</summary>
        Unique = 6
    }
}
