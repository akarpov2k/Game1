#nullable enable
namespace AnomalyPath.Core.Trade
{
    /// <summary>Результат торговой сделки.</summary>
    public enum TradeStatus
    {
        /// <summary>Сделка выполнена.</summary>
        Ok,

        /// <summary>Торговец с таким id не найден.</summary>
        TraderNotFound,

        /// <summary>Актёр не является торговцем.</summary>
        NotTrader,

        /// <summary>Торговец недоступен по условиям.</summary>
        Unavailable,

        /// <summary>Торговец находится в другой локации.</summary>
        WrongLocation,

        /// <summary>Товара нет в ассортименте.</summary>
        OfferNotFound,

        /// <summary>Не хватает денег.</summary>
        NotEnoughMoney,

        /// <summary>Товара не хватает на складе.</summary>
        NoStock,

        /// <summary>В инвентаре нет места.</summary>
        NoRoom,

        /// <summary>Предмет для продажи не найден.</summary>
        ItemNotFound,

        /// <summary>У предмета нет цены продажи.</summary>
        ItemNotSellable,

        /// <summary>Некорректное количество.</summary>
        InvalidCount
    }
}
