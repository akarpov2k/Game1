#nullable enable
namespace AnomalyPath.Core.Combat
{
    /// <summary>Типы урона и защиты: пули, аномалии и разрыв.</summary>
    public static class DamageTypes
    {
        /// <summary>Пулевой урон оружия.</summary>
        public const string Bullet = "dmg_bullet";

        /// <summary>Электрический урон аномалий.</summary>
        public const string Electric = "dmg_electric";

        /// <summary>Химический урон аномалий.</summary>
        public const string Chemical = "dmg_chemical";

        /// <summary>Огненный урон аномалий.</summary>
        public const string Fire = "dmg_fire";

        /// <summary>Разрыв: мутанты и гравитационные аномалии.</summary>
        public const string Rupture = "dmg_rupture";

    }
}
