#nullable enable
namespace AnomalyPath.Core.Rules
{
    /// <summary>Вид потери лута: код в данных — имя члена в нижнем регистре.</summary>
    public enum LootLossKind
    {
        /// <summary>Лут не теряется.</summary>
        None,

        /// <summary>Теряется доля лута.</summary>
        Partial,

        /// <summary>Теряется весь лут.</summary>
        All
    }
}
