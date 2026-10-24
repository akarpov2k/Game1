#nullable enable
using System;

namespace AnomalyPath.Core.Talents
{
    /// <summary>Результат изучения таланта: статус и новое состояние очков.</summary>
    public sealed class TalentResult
    {
        private TalentResult(
            TalentResultStatus status,
            string? talentId,
            int rank,
            int spentPoints,
            int availablePoints,
            int requiredPoints)
        {
            Status = status;
            TalentId = talentId;
            Rank = rank;
            SpentPoints = spentPoints;
            AvailablePoints = availablePoints;
            RequiredPoints = requiredPoints;
        }

        /// <summary>Статус.</summary>
        public TalentResultStatus Status { get; }

        /// <summary>Идентификатор таланта, если известен.</summary>
        public string? TalentId { get; }

        /// <summary>Ранг таланта после попытки.</summary>
        public int Rank { get; }

        /// <summary>Сколько очков вложено в дерево.</summary>
        public int SpentPoints { get; }

        /// <summary>Сколько свободных очков осталось.</summary>
        public int AvailablePoints { get; }

        /// <summary>Сколько очков требует талант.</summary>
        public int RequiredPoints { get; }

        /// <summary><c>true</c>, если талант изучен.</summary>
        public bool Succeeded => Status == TalentResultStatus.Ok;

        /// <summary>Успешное изучение.</summary>
        /// <param name="talentId">Идентификатор таланта.</param>
        /// <param name="rank">Новый ранг.</param>
        /// <param name="spentPoints">Вложено очков.</param>
        /// <param name="availablePoints">Осталось очков.</param>
        /// <param name="requiredPoints">Требование таланта.</param>
        /// <returns>Результат.</returns>
        public static TalentResult Ok(
            string talentId,
            int rank,
            int spentPoints,
            int availablePoints,
            int requiredPoints) =>
            new TalentResult(TalentResultStatus.Ok, talentId, rank, spentPoints, availablePoints, requiredPoints);

        /// <summary>Отказ.</summary>
        /// <param name="status">Причина.</param>
        /// <param name="talentId">Идентификатор таланта, если известен.</param>
        /// <param name="rank">Текущий ранг.</param>
        /// <param name="spentPoints">Вложено очков.</param>
        /// <param name="availablePoints">Осталось очков.</param>
        /// <param name="requiredPoints">Требование таланта.</param>
        /// <returns>Результат.</returns>
        public static TalentResult Error(
            TalentResultStatus status,
            string? talentId = null,
            int rank = 0,
            int spentPoints = 0,
            int availablePoints = 0,
            int requiredPoints = 0) =>
            new TalentResult(status, talentId, rank, spentPoints, availablePoints, requiredPoints);
    }
}
