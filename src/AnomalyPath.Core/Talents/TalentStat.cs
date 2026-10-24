#nullable enable
namespace AnomalyPath.Core.Talents
{
    /// <summary>Характеристика, на которую влияет талант.</summary>
    public enum TalentStat
    {
        /// <summary>Сколько очков действия восстанавливается за ход.</summary>
        ApRestore,

        /// <summary>Максимум очков действия.</summary>
        ApMax,

        /// <summary>Максимум здоровья.</summary>
        HealthMax,

        /// <summary>Меткость.</summary>
        Accuracy,

        /// <summary>Максимум выносливости.</summary>
        StaminaMax,

        /// <summary>Удача.</summary>
        Luck,

        /// <summary>Число строк инвентаря.</summary>
        CarryRows
    }
}
