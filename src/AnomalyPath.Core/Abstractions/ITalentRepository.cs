#nullable enable
using AnomalyPath.Core.Talents;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Дерево талантов из данных.</summary>
    public interface ITalentRepository
    {
        /// <summary>Единственное дерево талантов.</summary>
        TalentTreeTemplate Tree { get; }
    }
}
