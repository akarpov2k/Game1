#nullable enable
using System;
using System.Collections.Generic;

namespace AnomalyPath.Core.Quests
{
    /// <summary>Награда за квест: деньги, предметы и открытия.</summary>
    public sealed class QuestRewardTemplate
    {
        /// <summary>Пустая награда.</summary>
        public static readonly QuestRewardTemplate Empty = new QuestRewardTemplate(
            0,
            Array.Empty<QuestItemTarget>(),
            Array.Empty<string>());

        /// <summary>Создаёт награду.</summary>
        /// <param name="money">Деньги.</param>
        /// <param name="items">Предметы.</param>
        /// <param name="unlocks">Открытия.</param>
        public QuestRewardTemplate(
            int money,
            IReadOnlyList<QuestItemTarget> items,
            IReadOnlyList<string> unlocks)
        {
            Money = money;
            Items = items ?? Array.Empty<QuestItemTarget>();
            Unlocks = unlocks ?? Array.Empty<string>();
        }

        /// <summary>Деньги.</summary>
        public int Money { get; }

        /// <summary>Предметы.</summary>
        public IReadOnlyList<QuestItemTarget> Items { get; }

        /// <summary>Открытия.</summary>
        public IReadOnlyList<string> Unlocks { get; }
    }
}
