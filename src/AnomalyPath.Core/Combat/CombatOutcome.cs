#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Death;
using AnomalyPath.Core.Inventory;

namespace AnomalyPath.Core.Combat
{
    /// <summary>Результат завершённого боя: опыт, лут, последствия смерти и респавн.</summary>
    public sealed class CombatOutcome
    {
        private CombatOutcome(
            CombatOutcomeStatus status,
            int xpGained = 0,
            IReadOnlyList<ItemInstance>? loot = null,
            DeathResult? death = null,
            bool respawned = false,
            string? respawnLocationId = null)
        {
            Status = status;
            XpGained = xpGained;
            Loot = loot ?? Array.Empty<ItemInstance>();
            Death = death;
            Respawned = respawned;
            RespawnLocationId = respawnLocationId;
        }

        /// <summary>Итог боя.</summary>
        public CombatOutcomeStatus Status { get; }

        /// <summary><c>true</c>, если бой завершён победой или поражением.</summary>
        public bool IsFinished => Status == CombatOutcomeStatus.Victory || Status == CombatOutcomeStatus.Defeat;

        /// <summary>Сколько опыта получено за победу; 0 при поражении.</summary>
        public int XpGained { get; }

        /// <summary>Лут с побеждённого противника; пока пуст до описания пула лута.</summary>
        public IReadOnlyList<ItemInstance> Loot { get; }

        /// <summary>Последствия смерти для вещей; <c>null</c> при победе и без боя.</summary>
        public DeathResult? Death { get; }

        /// <summary><c>true</c>, если игрок возродился в хабе.</summary>
        public bool Respawned { get; }

        /// <summary>Id хаба, в который вернулся игрок; <c>null</c>, если возврата не было.</summary>
        public string? RespawnLocationId { get; }

        /// <summary>Боя нет.</summary>
        /// <returns>Результат без последствий.</returns>
        internal static CombatOutcome NoCombat() => new CombatOutcome(CombatOutcomeStatus.NoCombat);

        /// <summary>Бой ещё не завершён.</summary>
        /// <returns>Результат без последствий.</returns>
        internal static CombatOutcome NotFinished() => new CombatOutcome(CombatOutcomeStatus.NotFinished);

        /// <summary>Победа игрока.</summary>
        /// <param name="xpGained">Полученный опыт.</param>
        /// <param name="loot">Лут с противника.</param>
        /// <returns>Результат победы.</returns>
        internal static CombatOutcome Victory(int xpGained, IReadOnlyList<ItemInstance> loot) =>
            new CombatOutcome(CombatOutcomeStatus.Victory, xpGained, loot);

        /// <summary>Поражение игрока.</summary>
        /// <param name="death">Последствия для вещей.</param>
        /// <param name="respawned">Возродился ли игрок в хабе.</param>
        /// <param name="respawnLocationId">Id хаба возврата или <c>null</c>.</param>
        /// <returns>Результат поражения.</returns>
        internal static CombatOutcome Defeat(DeathResult death, bool respawned, string? respawnLocationId) =>
            new CombatOutcome(CombatOutcomeStatus.Defeat, death: death, respawned: respawned, respawnLocationId: respawnLocationId);
    }
}
