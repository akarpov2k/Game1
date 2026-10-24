#nullable enable
namespace AnomalyPath.Core.Talents
{
    /// <summary>Итог попытки изучить талант.</summary>
    public enum TalentResultStatus
    {
        /// <summary>Талант изучен.</summary>
        Ok,

        /// <summary>Таланта нет в дереве.</summary>
        UnknownTalent,

        /// <summary>Достигнут предельный ранг.</summary>
        MaxRank,

        /// <summary>Не хватает вложенных очков для открытия.</summary>
        Locked,

        /// <summary>Нет свободных очков талантов.</summary>
        NoPoints
    }
}
