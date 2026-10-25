#nullable enable
using AnomalyPath.Core.Inventory;

namespace AnomalyPath.Core.Combat
{
    /// <summary>Состояние боя: участники, дистанция, чей ход и номер раунда.</summary>
    public sealed class CombatState
    {
        internal CombatState(
            Combatant player,
            Combatant enemy,
            DistanceZone distance,
            CombatScenario scenario,
            ItemInstance? enemyWeapon = null)
        {
            Player = player;
            Enemy = enemy;
            Distance = distance;
            Scenario = scenario;
            EnemyWeapon = enemyWeapon;
            Current = scenario.First == CombatStartSide.Player ? player : enemy;
        }

        /// <summary>Игрок.</summary>
        public Combatant Player { get; }

        /// <summary>Противник.</summary>
        public Combatant Enemy { get; }

        /// <summary>Оружие противника; <c>null</c> у естественной атаки.</summary>
        public ItemInstance? EnemyWeapon { get; }

        /// <summary>Сценарий боя: обычный, засада или сюжетный.</summary>
        public CombatScenario Scenario { get; }

        /// <summary>Текущая дистанция.</summary>
        public DistanceZone Distance { get; internal set; }

        /// <summary>Кто ходит сейчас.</summary>
        public Combatant Current { get; internal set; }

        /// <summary>Номер раунда, начиная с 1.</summary>
        public int Round { get; internal set; } = 1;

        /// <summary>Сколько зон уже пройдено бесплатно в этом ходу.</summary>
        public int ShiftUsed { get; internal set; }

        /// <summary>Противник текущего участника.</summary>
        public Combatant Opponent => ReferenceEquals(Current, Player) ? Enemy : Player;

        /// <summary><c>true</c>, если бой завершён смертью одного из участников.</summary>
        public bool IsFinished => Player.IsDead || Enemy.IsDead;
    }
}
