#nullable enable
namespace AnomalyPath.Core.Trade
{
    /// <summary>Товар, доступный к покупке у торговца прямо сейчас.</summary>
    public sealed class TraderOffer
    {
        /// <summary>Создаёт предложение торговца.</summary>
        /// <param name="itemId">Идентификатор предмета.</param>
        /// <param name="unitPrice">Цена за одну штуку.</param>
        /// <param name="stock">Сколько штук на складе.</param>
        public TraderOffer(string itemId, int unitPrice, int stock)
        {
            ItemId = itemId;
            UnitPrice = unitPrice;
            Stock = stock;
        }

        /// <summary>Идентификатор предмета.</summary>
        public string ItemId { get; }

        /// <summary>Цена за одну штуку.</summary>
        public int UnitPrice { get; }

        /// <summary>Остаток на складе.</summary>
        public int Stock { get; }
    }
}
