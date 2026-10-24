#nullable enable
using System;
using System.Collections.Generic;

namespace AnomalyPath.Core.Quests
{
    /// <summary>Текущий прогресс взятого квеста.</summary>
    public sealed class QuestProgress
    {
        /// <summary>Создаёт прогресс.</summary>
        /// <param name="type">Тип цели.</param>
        /// <param name="current">Текущее значение основной цели.</param>
        /// <param name="required">Требуемое значение основной цели.</param>
        /// <param name="ready">Можно ли сдавать.</param>
        /// <param name="items">Прогресс по предметам.</param>
        /// <param name="npcCurrent">Убито NPC.</param>
        /// <param name="npcRequired">Нужно убить NPC.</param>
        public QuestProgress(
            QuestType type,
            int current,
            int required,
            bool ready,
            IReadOnlyList<QuestItemProgress>? items = null,
            int npcCurrent = 0,
            int npcRequired = 0)
        {
            Type = type;
            Current = current;
            Required = required;
            Ready = ready;
            Items = items ?? Array.Empty<QuestItemProgress>();
            NpcCurrent = npcCurrent;
            NpcRequired = npcRequired;
        }

        /// <summary>Тип цели.</summary>
        public QuestType Type { get; }

        /// <summary>Текущее значение основной цели.</summary>
        public int Current { get; }

        /// <summary>Требуемое значение основной цели.</summary>
        public int Required { get; }

        /// <summary>Можно ли сдавать.</summary>
        public bool Ready { get; }

        /// <summary>Прогресс по предметам.</summary>
        public IReadOnlyList<QuestItemProgress> Items { get; }

        /// <summary>Убито NPC.</summary>
        public int NpcCurrent { get; }

        /// <summary>Нужно убить NPC.</summary>
        public int NpcRequired { get; }
    }
}
