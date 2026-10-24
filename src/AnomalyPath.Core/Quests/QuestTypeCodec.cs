#nullable enable
using System;

namespace AnomalyPath.Core.Quests
{
    /// <summary>Код типа цели квеста в JSON: читаемый camelCase, а не имя члена перечисления.</summary>
    public static class QuestTypeCodec
    {
        /// <summary>Возвращает код типа для JSON.</summary>
        /// <param name="type">Тип цели.</param>
        /// <returns>Код.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Тип неизвестен.</exception>
        public static string ToCode(QuestType type) => type switch
        {
            QuestType.ClearLocation => "clearLocation",
            QuestType.KillMutants => "killMutants",
            QuestType.GatherArtifacts => "gatherArtifacts",
            QuestType.FindItems => "findItems",
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        /// <summary>Разбирает код типа.</summary>
        /// <param name="code">Код из JSON.</param>
        /// <param name="type">Разобранный тип.</param>
        /// <returns><c>true</c>, если код известен.</returns>
        public static bool TryParse(string? code, out QuestType type)
        {
            switch (code)
            {
                case "clearLocation":
                    type = QuestType.ClearLocation;
                    return true;
                case "killMutants":
                    type = QuestType.KillMutants;
                    return true;
                case "gatherArtifacts":
                    type = QuestType.GatherArtifacts;
                    return true;
                case "findItems":
                    type = QuestType.FindItems;
                    return true;
                default:
                    type = default;
                    return false;
            }
        }
    }
}
