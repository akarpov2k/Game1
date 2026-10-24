#nullable enable
namespace AnomalyPath.Core.Trade
{
    /// <summary>Итог торговой сделки: статус, предмет, количество и цена.</summary>
    public sealed class TradeResult
    {
        /// <summary>Создаёт результат сделки.</summary>
        /// <param name="status">Статус.</param>
        /// <param name="itemId">Идентификатор предмета.</param>
        /// <param name="count">Количество.</param>
        /// <param name="price">Итоговая цена.</param>
        public TradeResult(TradeStatus status, string itemId, int count, int price)
        {
            Status = status;
            ItemId = itemId;
            Count = count;
            Price = price;
        }

        /// <summary>Статус сделки.</summary>
        public TradeStatus Status { get; }

        /// <summary>Идентификатор предмета.</summary>
        public string ItemId { get; }

        /// <summary>Количество предметов.</summary>
        public int Count { get; }

        /// <summary>Итоговая цена сделки.</summary>
        public int Price { get; }

        /// <summary><c>true</c>, если сделка выполнена.</summary>
        public bool Succeeded => Status == TradeStatus.Ok;

        /// <summary>Создаёт результат ошибки без предмета.</summary>
        /// <param name="status">Статус ошибки.</param>
        /// <returns>Результат.</returns>
        public static TradeResult Error(TradeStatus status) => new TradeResult(status, string.Empty, 0, 0);
    }
}
