#nullable enable
namespace AnomalyPath.Core.Rules
{
    /// <summary>Область действия страховки: код в данных — имя члена в нижнем регистре.</summary>
    public enum InsuranceScope
    {
        /// <summary>Только добыча вылазки.</summary>
        Loot,

        /// <summary>И добыча, и снаряжение.</summary>
        All
    }
}
