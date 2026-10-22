#nullable enable
namespace AnomalyPath.Core.Combat
{
    /// <summary>Правило инициативы в бою.</summary>
    public enum InitiativeKind
    {
        /// <summary>Игрок всегда первый.</summary>
        PlayerFirst,

        /// <summary>Порядок считает формула.</summary>
        Formula
    }
}
