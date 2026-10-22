#nullable enable
namespace AnomalyPath.Core.Combat
{
    /// <summary>Режим восстановления очков действия: код в данных — имя члена в нижнем регистре.</summary>
    public enum ApRestoreKind
    {
        /// <summary>Полностью каждый ход.</summary>
        Full,

        /// <summary>Частично, по значению <c>apPerTurn</c>.</summary>
        Partial,

        /// <summary>Только запас на бой.</summary>
        None
    }
}
