#nullable enable
using System;
using System.Collections.Generic;

namespace AnomalyPath.Core.Quests
{
    /// <summary>Результат взятия или сдачи квеста.</summary>
    public sealed class QuestResult
    {
        private QuestResult(
            QuestResultStatus status,
            string? questId,
            int money,
            IReadOnlyList<QuestItemTarget>? items)
        {
            Status = status;
            QuestId = questId;
            Money = money;
            Items = items ?? Array.Empty<QuestItemTarget>();
        }

        /// <summary>Статус.</summary>
        public QuestResultStatus Status { get; }

        /// <summary>Идентификатор квеста, если известен.</summary>
        public string? QuestId { get; }

        /// <summary>Полученные деньги.</summary>
        public int Money { get; }

        /// <summary>Полученные предметы.</summary>
        public IReadOnlyList<QuestItemTarget> Items { get; }

        /// <summary><c>true</c>, если действие выполнено.</summary>
        public bool Succeeded => Status == QuestResultStatus.Ok;

        /// <summary>Успешный результат.</summary>
        /// <param name="questId">Идентификатор квеста.</param>
        /// <param name="money">Полученные деньги.</param>
        /// <param name="items">Полученные предметы.</param>
        /// <returns>Результат.</returns>
        public static QuestResult Ok(string questId, int money = 0, IReadOnlyList<QuestItemTarget>? items = null) =>
            new QuestResult(QuestResultStatus.Ok, questId, money, items);

        /// <summary>Ошибка.</summary>
        /// <param name="status">Статус ошибки.</param>
        /// <param name="questId">Идентификатор квеста, если известен.</param>
        /// <returns>Результат.</returns>
        public static QuestResult Error(QuestResultStatus status, string? questId = null) =>
            new QuestResult(status, questId, 0, null);
    }
}
