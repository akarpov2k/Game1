#nullable enable
using System;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Save;

namespace AnomalyPath.Core.Session
{
    /// <summary>Игровая сессия: состояние прохождения и текущий бой, если он идёт.</summary>
    public sealed class GameSession
    {
        /// <summary>Создаёт сессию поверх состояния прохождения.</summary>
        /// <param name="state">Состояние мира и игрока.</param>
        /// <param name="combat">Текущий бой или <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">Состояние не задано.</exception>
        public GameSession(GameState state, CombatState? combat = null)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
            Combat = combat;
        }

        /// <summary>Состояние мира и игрока.</summary>
        public GameState State { get; }

        /// <summary>Текущий бой или <c>null</c>.</summary>
        public CombatState? Combat { get; private set; }

        /// <summary><c>true</c>, если сейчас идёт бой.</summary>
        public bool InCombat => Combat != null;

        /// <summary>Начинает или завершает бой.</summary>
        /// <param name="combat">Новый бой или <c>null</c>, чтобы завершить текущий.</param>
        internal void SetCombat(CombatState? combat) => Combat = combat;
    }
}
