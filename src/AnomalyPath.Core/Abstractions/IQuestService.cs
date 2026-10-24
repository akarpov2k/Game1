#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Quests;
using AnomalyPath.Core.Session;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>
    /// Повторяемые квесты: общий пул у торговцев, выдача, прогресс, сдача и откат.
    /// </summary>
    public interface IQuestService
    {
        /// <summary>Возвращает предложения торговца на текущий период обновления.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="traderActorId">Идентификатор торговца-квестодателя.</param>
        /// <returns>Список предложений; пустой, если торговец недоступен.</returns>
        IReadOnlyList<QuestOffer> GetOffers(GameSession session, string traderActorId);

        /// <summary>Берёт предложенный квест у торговца.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="traderActorId">Идентификатор торговца.</param>
        /// <param name="questId">Идентификатор квеста.</param>
        /// <returns>Результат взятия.</returns>
        QuestResult Accept(GameSession session, string traderActorId, string questId);

        /// <summary>Отменяет предложение торговца, не беря его.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="traderActorId">Идентификатор торговца.</param>
        /// <param name="questId">Идентификатор квеста.</param>
        /// <returns>Результат отмены.</returns>
        QuestResult CancelOffer(GameSession session, string traderActorId, string questId);

        /// <summary>Отменяет взятый квест.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="questId">Идентификатор квеста.</param>
        /// <returns>Результат отмены.</returns>
        QuestResult CancelQuest(GameSession session, string questId);

        /// <summary>Сдаёт выполненный квест тому же торговцу.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="traderActorId">Идентификатор торговца.</param>
        /// <param name="questId">Идентификатор квеста.</param>
        /// <returns>Результат сдачи с наградой.</returns>
        QuestResult TurnIn(GameSession session, string traderActorId, string questId);

        /// <summary>Возвращает активные квесты игрока.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <returns>Список активных квестов.</returns>
        IReadOnlyList<QuestState> GetActive(GameSession session);

        /// <summary>Ищет шаблон квеста для отображения.</summary>
        /// <param name="questId">Идентификатор квеста.</param>
        /// <param name="template">Найденный шаблон или <c>null</c>.</param>
        /// <returns><c>true</c>, если шаблон найден.</returns>
        bool TryGetTemplate(string questId, out QuestTemplate template);

        /// <summary>Считает текущий прогресс квеста по состоянию игрока.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="quest">Состояние квеста.</param>
        /// <returns>Прогресс; для неизвестного квеста — пустой.</returns>
        QuestProgress GetProgress(GameSession session, QuestState quest);

        /// <summary>Учитывает убийство актёра в активных квестах.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="actorId">Кого убили.</param>
        void NotifyActorKilled(GameSession session, string actorId);

        /// <summary>Учитывает зачистку локации в активных квестах.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="locationInstanceId">Экземпляр зачищенной локации.</param>
        void NotifyLocationCleared(GameSession session, string locationInstanceId);
    }
}
