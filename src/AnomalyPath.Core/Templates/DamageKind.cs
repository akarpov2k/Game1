#nullable enable
namespace AnomalyPath.Core.Templates
{
    /// <summary>Вид урона: код в данных — имя члена в нижнем регистре.</summary>
    public enum DamageKind
    {
        /// <summary>Пулевой урон.</summary>
        Bullet,

        /// <summary>Аномальный урон.</summary>
        Anomaly,

        /// <summary>Физический урон.</summary>
        Physical,

        /// <summary>Стихийный урон.</summary>
        Elemental
    }
}
