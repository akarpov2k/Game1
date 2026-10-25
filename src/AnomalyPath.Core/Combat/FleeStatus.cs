#nullable enable
namespace AnomalyPath.Core.Combat
{
    /// <summary>Исход попытки побега из боя.</summary>
    public enum FleeStatus
    {
        /// <summary>Игрок выжил после хода противника и покинул бой.</summary>
        Escaped = 0,

        /// <summary>Противник убил игрока: побег не состоялся, применяется обычная смерть.</summary>
        Died = 1,

        /// <summary>Бой не идёт.</summary>
        NotInCombat = 2,

        /// <summary>Сейчас ход противника, а не игрока.</summary>
        NotPlayerTurn = 3,

        /// <summary>Бой уже завершён.</summary>
        Finished = 4
    }
}
