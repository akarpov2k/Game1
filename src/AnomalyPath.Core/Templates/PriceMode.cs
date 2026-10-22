#nullable enable
namespace AnomalyPath.Core.Templates
{
    /// <summary>Режим цены предмета: код в данных — имя члена в нижнем регистре.</summary>
    public enum PriceMode
    {
        /// <summary>Фиксированная цена.</summary>
        Fixed,

        /// <summary>Цена зависит от репутации.</summary>
        Reputation
    }
}
