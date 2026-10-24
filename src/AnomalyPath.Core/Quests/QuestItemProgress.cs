#nullable enable
namespace AnomalyPath.Core.Quests
{
    /// <summary>Прогресс по одному целевому предмету.</summary>
    public sealed class QuestItemProgress
    {
        /// <summary>Создаёт прогресс предмета.</summary>
        /// <param name="itemId">Идентификатор предмета.</param>
        /// <param name="current">Сколько уже есть.</param>
        /// <param name="required">Сколько нужно.</param>
        public QuestItemProgress(string itemId, int current, int required)
        {
            ItemId = itemId;
            Current = current;
            Required = required;
        }

        /// <summary>Идентификатор предмета.</summary>
        public string ItemId { get; }

        /// <summary>Сколько уже есть.</summary>
        public int Current { get; }

        /// <summary>Сколько нужно.</summary>
        public int Required { get; }

        /// <summary>Выполнено ли требование по предмету.</summary>
        public bool IsMet => Current >= Required;
    }
}
