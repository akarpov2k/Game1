#nullable enable
namespace AnomalyPath.Core.Quests
{
    /// <summary>Результат действия с квестом.</summary>
    public enum QuestResultStatus
    {
        /// <summary>Успех.</summary>
        Ok,

        /// <summary>Квест не найден.</summary>
        QuestNotFound,

        /// <summary>Квест не повторяемый.</summary>
        NotRepeatable,

        /// <summary>Этот торговец не выдаёт такой квест.</summary>
        NotOffered,

        /// <summary>Торговец не найден.</summary>
        TraderNotFound,

        /// <summary>Актёр не торговец.</summary>
        NotTrader,

        /// <summary>Действие доступно только в хабе.</summary>
        WrongLocation,

        /// <summary>Торговец недоступен.</summary>
        Unavailable,

        /// <summary>Условия выдачи не выполнены.</summary>
        RequirementsNotMet,

        /// <summary>Этот квест уже взят.</summary>
        AlreadyActive,

        /// <summary>Достигнут предел активных квестов.</summary>
        ActiveLimit,

        /// <summary>Квест на откате.</summary>
        Cooldown,

        /// <summary>В данных нет подходящих целей.</summary>
        NoTargets,

        /// <summary>Цель ещё не выполнена.</summary>
        NotReady,

        /// <summary>Квест выдан не этим торговцем.</summary>
        WrongGiver,

        /// <summary>Нет места для награды.</summary>
        NoRoom,

        /// <summary>Действие недоступно.</summary>
        Invalid
    }
}
