#nullable enable
namespace AnomalyPath.Core.Templates
{
    /// <summary>Блок цены: фиксированная или от репутации.</summary>
    public sealed class PriceBlock
    {
        /// <summary>Создаёт блок цены.</summary>
        /// <param name="mode">Режим цены.</param>
        /// <param name="buy">Цена покупки игроком.</param>
        /// <param name="sell">Цена продажи игроком.</param>
        /// <param name="basePrice">Базовая цена для режима репутации.</param>
        /// <param name="factionId">Фракция, чья репутация влияет.</param>
        public PriceBlock(PriceMode mode, double? buy, double? sell, double? basePrice, string? factionId)
        {
            Mode = mode;
            Buy = buy;
            Sell = sell;
            Base = basePrice;
            FactionId = factionId;
        }

        /// <summary>Режим цены.</summary>
        public PriceMode Mode { get; }

        /// <summary>Цена покупки игроком.</summary>
        public double? Buy { get; }

        /// <summary>Цена продажи игроком.</summary>
        public double? Sell { get; }

        /// <summary>Базовая цена.</summary>
        public double? Base { get; }

        /// <summary>Фракция для репутационной цены.</summary>
        public string? FactionId { get; }
    }
}
