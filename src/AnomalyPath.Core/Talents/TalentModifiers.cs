#nullable enable
namespace AnomalyPath.Core.Talents
{
    /// <summary>Собранные бонусы дерева талантов для текущего состояния игрока.</summary>
    public sealed class TalentModifiers
    {
        /// <summary>Пустые бонусы.</summary>
        public static TalentModifiers None { get; } = new TalentModifiers();

        /// <summary>Создаёт набор бонусов.</summary>
        /// <param name="apRestore">Прибавка к восстановлению ОД за ход.</param>
        /// <param name="apMax">Прибавка к максимуму ОД.</param>
        /// <param name="healthMax">Прибавка к максимуму здоровья.</param>
        /// <param name="accuracy">Прибавка к меткости.</param>
        /// <param name="staminaMax">Прибавка к максимуму выносливости.</param>
        /// <param name="luck">Прибавка к удаче.</param>
        /// <param name="carryRows">Прибавка к числу строк инвентаря.</param>
        public TalentModifiers(
            int apRestore = 0,
            int apMax = 0,
            int healthMax = 0,
            double accuracy = 0,
            int staminaMax = 0,
            int luck = 0,
            int carryRows = 0)
        {
            ApRestore = apRestore;
            ApMax = apMax;
            HealthMax = healthMax;
            Accuracy = accuracy;
            StaminaMax = staminaMax;
            Luck = luck;
            CarryRows = carryRows;
        }

        /// <summary>Прибавка к восстановлению ОД за ход.</summary>
        public int ApRestore { get; }

        /// <summary>Прибавка к максимуму ОД.</summary>
        public int ApMax { get; }

        /// <summary>Прибавка к максимуму здоровья.</summary>
        public int HealthMax { get; }

        /// <summary>Прибавка к меткости.</summary>
        public double Accuracy { get; }

        /// <summary>Прибавка к максимуму выносливости.</summary>
        public int StaminaMax { get; }

        /// <summary>Прибавка к удаче.</summary>
        public int Luck { get; }

        /// <summary>Прибавка к числу строк инвентаря.</summary>
        public int CarryRows { get; }
    }
}
