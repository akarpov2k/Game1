#nullable enable
namespace AnomalyPath.Core.Trade
{
    /// <summary>Предмет игрока, который торговец готов купить.</summary>
    public sealed class TraderSellOffer
    {
        /// <summary>Создаёт предложение продажи.</summary>
        /// <param name="instanceId">Идентификатор экземпляра.</param>
        /// <param name="itemId">Идентификатор предмета.</param>
        /// <param name="count">Количество в стопке.</param>
        /// <param name="unitPrice">Цена за одну штуку.</param>
        public TraderSellOffer(string instanceId, string itemId, int count, int unitPrice)
        {
            InstanceId = instanceId;
            ItemId = itemId;
            Count = count;
            UnitPrice = unitPrice;
            TotalPrice = count * unitPrice;
        }

        /// <summary>Идентификатор экземпляра.</summary>
        public string InstanceId { get; }

        /// <summary>Идентификатор предмета.</summary>
        public string ItemId { get; }

        /// <summary>Количество в стопке.</summary>
        public int Count { get; }

        /// <summary>Цена за одну штуку.</summary>
        public int UnitPrice { get; }

        /// <summary>Цена за всю стопку.</summary>
        public int TotalPrice { get; }
    }
}
