#nullable enable
namespace AnomalyPath.Core.Quests
{
    /// <summary>Конкретное предложение квеста у торговца: шаблон и зафиксированная цель.</summary>
    public sealed class QuestOffer
    {
        /// <summary>Создаёт предложение.</summary>
        /// <param name="template">Шаблон квеста.</param>
        /// <param name="state">Состояние предложения с конкретной целью.</param>
        public QuestOffer(QuestTemplate template, QuestState state)
        {
            Template = template;
            State = state;
        }

        /// <summary>Шаблон квеста.</summary>
        public QuestTemplate Template { get; }

        /// <summary>Состояние предложения с конкретной целью.</summary>
        public QuestState State { get; }

        /// <summary>Идентификатор квеста.</summary>
        public string QuestId => Template.Id;
    }
}
