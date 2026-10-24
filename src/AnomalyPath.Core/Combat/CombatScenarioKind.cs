#nullable enable
namespace AnomalyPath.Core.Combat
{
    /// <summary>Сценарий боя, задающий первый ход.</summary>
    public enum CombatScenarioKind
    {
        /// <summary>Обычный бой: игрок всегда ходит первым.</summary>
        Normal,

        /// <summary>Засада: противник всегда ходит первым.</summary>
        Ambush,

        /// <summary>Сюжетный бой: первый ход задаёт сцена сюжета.</summary>
        Story
    }
}
