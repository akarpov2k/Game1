#nullable enable
using System;

namespace AnomalyPath.Core.Combat
{
    /// <summary>Сценарий боя: вид боя и сторона, которая ходит первой.</summary>
    public sealed class CombatScenario
    {
        private static readonly CombatScenario NormalScenario =
            new CombatScenario(CombatScenarioKind.Normal, CombatStartSide.Player);

        private static readonly CombatScenario AmbushScenario =
            new CombatScenario(CombatScenarioKind.Ambush, CombatStartSide.Enemy);

        private CombatScenario(CombatScenarioKind kind, CombatStartSide first)
        {
            Kind = kind;
            First = first;
        }

        /// <summary>Обычный бой: игрок всегда ходит первым. Сценарий по умолчанию.</summary>
        public static CombatScenario Normal => NormalScenario;

        /// <summary>Засада: противник всегда ходит первым.</summary>
        public static CombatScenario Ambush => AmbushScenario;

        /// <summary>Вид сценария.</summary>
        public CombatScenarioKind Kind { get; }

        /// <summary>Сторона, которая ходит первой.</summary>
        public CombatStartSide First { get; }

        /// <summary>Сюжетный бой: первым ходит сторона, заданная сюжетной сценой.</summary>
        /// <param name="first">Сторона первого хода.</param>
        /// <returns>Сценарий сюжетного боя.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Сторона неизвестна.</exception>
        public static CombatScenario Story(CombatStartSide first)
        {
            if (first != CombatStartSide.Player && first != CombatStartSide.Enemy)
            {
                throw new ArgumentOutOfRangeException(nameof(first));
            }

            return new CombatScenario(CombatScenarioKind.Story, first);
        }
    }
}
