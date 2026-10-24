#nullable enable
namespace AnomalyPath.Core.Quests
{
    /// <summary>Тип цели повторяемого квеста.</summary>
    public enum QuestType
    {
        /// <summary>Зачистить конкретную локацию.</summary>
        ClearLocation,

        /// <summary>Убить N мутантов из пула сложности.</summary>
        KillMutants,

        /// <summary>Собрать N артефактов из пула сложности.</summary>
        GatherArtifacts,

        /// <summary>Найти оружие или броню и убить NPC.</summary>
        FindItems
    }
}
