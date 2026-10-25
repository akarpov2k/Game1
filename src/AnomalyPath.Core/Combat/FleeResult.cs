#nullable enable
namespace AnomalyPath.Core.Combat
{
    /// <summary>Результат попытки побега: исход, отклик противника и здоровье игрока.</summary>
    public readonly struct FleeResult
    {
        private FleeResult(FleeStatus status, int health, CombatAttackResult? attack)
        {
            Status = status;
            Health = health;
            Attack = attack;
        }

        /// <summary>Исход попытки.</summary>
        public FleeStatus Status { get; }

        /// <summary>Здоровье игрока после хода противника; у неуспешной попытки — 0.</summary>
        public int Health { get; }

        /// <summary>Стандартная атака противника; <c>null</c>, если он не смог атаковать.</summary>
        public CombatAttackResult? Attack { get; }

        /// <summary><c>true</c>, если игрок выжил и покинул бой.</summary>
        public bool Escaped => Status == FleeStatus.Escaped;

        /// <summary><c>true</c>, если противник убил игрока.</summary>
        public bool Died => Status == FleeStatus.Died;

        /// <summary>Побег состоялся: бой можно закрывать без победы.</summary>
        /// <param name="health">Здоровье игрока после атаки противника.</param>
        /// <param name="attack">Атака противника или <c>null</c>.</param>
        /// <returns>Результат побега.</returns>
        public static FleeResult EscapedWith(int health, CombatAttackResult? attack) =>
            new FleeResult(FleeStatus.Escaped, health, attack);

        /// <summary>Противник убил игрока.</summary>
        /// <param name="attack">Атака противника или <c>null</c>.</param>
        /// <returns>Результат попытки.</returns>
        public static FleeResult DiedTo(CombatAttackResult? attack) =>
            new FleeResult(FleeStatus.Died, 0, attack);

        /// <summary>Боя нет.</summary>
        /// <returns>Результат попытки.</returns>
        public static FleeResult NotInCombat() => new FleeResult(FleeStatus.NotInCombat, 0, null);

        /// <summary>Сейчас не ход игрока.</summary>
        /// <returns>Результат попытки.</returns>
        public static FleeResult NotPlayerTurn() => new FleeResult(FleeStatus.NotPlayerTurn, 0, null);

        /// <summary>Бой уже завершён.</summary>
        /// <returns>Результат попытки.</returns>
        public static FleeResult Finished() => new FleeResult(FleeStatus.Finished, 0, null);
    }
}
