#nullable enable
using System;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Quests
{
    /// <summary>Требуемый предмет и его количество в цели квеста.</summary>
    public sealed class QuestItemTarget
    {
        /// <summary>Создаёт требование предмета.</summary>
        /// <param name="itemId">Идентификатор предмета.</param>
        /// <param name="count">Сколько нужно.</param>
        /// <exception cref="ArgumentException">Идентификатор пуст.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Количество меньше одного.</exception>
        public QuestItemTarget(string itemId, int count)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(itemId)),
                    nameof(itemId));
            }

            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            ItemId = itemId;
            Count = count;
        }

        /// <summary>Идентификатор предмета.</summary>
        public string ItemId { get; }

        /// <summary>Сколько нужно.</summary>
        public int Count { get; }
    }
}
