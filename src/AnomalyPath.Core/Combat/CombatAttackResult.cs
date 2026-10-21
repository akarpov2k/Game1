#nullable enable
namespace AnomalyPath.Core.Combat
{
    /// <summary>Результат одной атаки.</summary>
    public readonly struct CombatAttackResult
    {
        /// <summary>Создаёт результат атаки.</summary>
        /// <param name="hit">Попал ли удар.</param>
        /// <param name="damage">Нанесённый урон.</param>
        /// <param name="killed">Убита ли цель.</param>
        /// <param name="roll">Бросок кубика [0, 1).</param>
        /// <param name="chance">Шанс попадания, с которым сравнивался бросок.</param>
        public CombatAttackResult(bool hit, int damage, bool killed, double roll, double chance)
        {
            Hit = hit;
            Damage = damage;
            Killed = killed;
            Roll = roll;
            Chance = chance;
        }

        /// <summary><c>true</c>, если удар попал.</summary>
        public bool Hit { get; }

        /// <summary>Нанесённый урон; 0 при промахе.</summary>
        public int Damage { get; }

        /// <summary><c>true</c>, если цель убита.</summary>
        public bool Killed { get; }

        /// <summary>Бросок кубика [0, 1).</summary>
        public double Roll { get; }

        /// <summary>Шанс попадания, с которым сравнивался бросок.</summary>
        public double Chance { get; }
    }
}
