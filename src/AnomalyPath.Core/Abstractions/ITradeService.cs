#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.Trade;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Торговля с NPC: доступность, ассортимент, покупка и продажа.</summary>
    public interface ITradeService
    {
        /// <summary>Проверяет, доступен ли торговец игроку в текущей локации.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="actorId">Идентификатор торговца.</param>
        /// <returns><c>true</c>, если торговец есть в хабе и его условия выполнены.</returns>
        bool IsAvailable(GameSession session, string actorId);

        /// <summary>Возвращает товары, доступные к покупке; скрытые условиями и распроданные не входят.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="actorId">Идентификатор торговца.</param>
        /// <returns>Список предложений в порядке ассортимента.</returns>
        IReadOnlyList<TraderOffer> GetOffers(GameSession session, string actorId);

        /// <summary>Возвращает предметы игрока, которые торговец готов купить.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="actorId">Идентификатор торговца.</param>
        /// <returns>Список предложений продажи.</returns>
        IReadOnlyList<TraderSellOffer> GetSellOffers(GameSession session, string actorId);

        /// <summary>Покупает товар у торговца.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="actorId">Идентификатор торговца.</param>
        /// <param name="itemId">Идентификатор предмета.</param>
        /// <param name="count">Количество, не меньше единицы.</param>
        /// <returns>Результат сделки.</returns>
        TradeResult Buy(GameSession session, string actorId, string itemId, int count = 1);

        /// <summary>Продаёт торговцу всю стопку предмета из инвентаря.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="actorId">Идентификатор торговца.</param>
        /// <param name="instanceId">Идентификатор экземпляра.</param>
        /// <returns>Результат сделки.</returns>
        TradeResult Sell(GameSession session, string actorId, string instanceId);
    }
}
