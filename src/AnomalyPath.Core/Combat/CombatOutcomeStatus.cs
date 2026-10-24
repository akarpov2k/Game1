#nullable enable
namespace AnomalyPath.Core.Combat
{
    /// <summary>Итог завершённого боя.</summary>
    public enum CombatOutcomeStatus
    {
        /// <summary>Боя нет: он не начинался или уже завершён.</summary>
        NoCombat,

        /// <summary>Бой идёт: ни одна из сторон ещё не мертва.</summary>
        NotFinished,

        /// <summary>Победа игрока.</summary>
        Victory,

        /// <summary>Поражение игрока.</summary>
        Defeat
    }
}
