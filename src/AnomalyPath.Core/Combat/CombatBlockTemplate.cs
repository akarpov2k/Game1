#nullable enable
using System.Collections.Generic;

namespace AnomalyPath.Core.Combat
{
    /// <summary>Боевые характеристики актёра из данных.</summary>
    public sealed class CombatBlockTemplate
    {
        /// <summary>Создаёт боевой блок.</summary>
        /// <param name="tier">Тир.</param>
        /// <param name="level">Уровень актёра.</param>
        /// <param name="health">Здоровье.</param>
        /// <param name="ap">Очки действия.</param>
        /// <param name="protection">Защита по типам урона.</param>
        /// <param name="ai">Модель поведения.</param>
        /// <param name="accuracy">Базовая меткость 0..1.</param>
        /// <param name="accuracyBonus">Бонус меткости от талантов.</param>
        /// <param name="xp">Опыт за победу над актёром; у врага обязателен и больше нуля.</param>
        public CombatBlockTemplate(int tier, int level, int health, int ap, IReadOnlyDictionary<string, int> protection, string ai, double accuracy, double accuracyBonus, int xp = 0)
        {
            Tier = tier;
            Level = level;
            Health = health;
            Ap = ap;
            Protection = protection;
            Ai = ai;
            Accuracy = accuracy;
            AccuracyBonus = accuracyBonus;
            Xp = xp;
        }

        /// <summary>Опыт за победу над актёром.</summary>
        public int Xp { get; }

        /// <summary>Базовая меткость актёра, 0..1.</summary>
        public double Accuracy { get; }

        /// <summary>Бонус меткости от талантов, прибавляется к базовой.</summary>
        public double AccuracyBonus { get; }

        /// <summary>Тир.</summary>
        public int Tier { get; }

        /// <summary>Уровень актёра.</summary>
        public int Level { get; }

        /// <summary>Здоровье.</summary>
        public int Health { get; }

        /// <summary>Очки действия.</summary>
        public int Ap { get; }

        /// <summary>Защита по типам урона.</summary>
        public IReadOnlyDictionary<string, int> Protection { get; }

        /// <summary>Модель поведения.</summary>
        public string Ai { get; }
    }
}
