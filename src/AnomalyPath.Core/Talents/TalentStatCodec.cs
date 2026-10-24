#nullable enable
using System;

namespace AnomalyPath.Core.Talents
{
    /// <summary>Код характеристики таланта в JSON: читаемый camelCase, а не имя члена перечисления.</summary>
    public static class TalentStatCodec
    {
        /// <summary>Возвращает код характеристики для JSON.</summary>
        /// <param name="stat">Характеристика.</param>
        /// <returns>Код.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Характеристика неизвестна.</exception>
        public static string ToCode(TalentStat stat) => stat switch
        {
            TalentStat.ApRestore => "apRestore",
            TalentStat.ApMax => "apMax",
            TalentStat.HealthMax => "healthMax",
            TalentStat.Accuracy => "accuracy",
            TalentStat.StaminaMax => "staminaMax",
            TalentStat.Luck => "luck",
            TalentStat.CarryRows => "carryRows",
            _ => throw new ArgumentOutOfRangeException(nameof(stat))
        };

        /// <summary>Разбирает код характеристики.</summary>
        /// <param name="code">Код из JSON.</param>
        /// <param name="stat">Разобранная характеристика.</param>
        /// <returns><c>true</c>, если код известен.</returns>
        public static bool TryParse(string? code, out TalentStat stat)
        {
            switch (code)
            {
                case "apRestore":
                    stat = TalentStat.ApRestore;
                    return true;
                case "apMax":
                    stat = TalentStat.ApMax;
                    return true;
                case "healthMax":
                    stat = TalentStat.HealthMax;
                    return true;
                case "accuracy":
                    stat = TalentStat.Accuracy;
                    return true;
                case "staminaMax":
                    stat = TalentStat.StaminaMax;
                    return true;
                case "luck":
                    stat = TalentStat.Luck;
                    return true;
                case "carryRows":
                    stat = TalentStat.CarryRows;
                    return true;
                default:
                    stat = default;
                    return false;
            }
        }

        /// <summary>Проверяет, что характеристика измеряется целыми единицами.</summary>
        /// <param name="stat">Характеристика.</param>
        /// <returns><c>true</c>, если дробное значение недопустимо.</returns>
        public static bool IsInteger(TalentStat stat) => stat != TalentStat.Accuracy;
    }
}
