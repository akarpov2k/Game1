#nullable enable
namespace AnomalyPath.Core.World
{
    /// <summary>Итог попытки перехода.</summary>
    public enum TravelStatus
    {
        /// <summary>Переход выполнен.</summary>
        Success,

        /// <summary>Ребро не найдено.</summary>
        EdgeNotFound,

        /// <summary>Ребро начинается не из текущей локации.</summary>
        WrongLocation,

        /// <summary>Не хватает выносливости.</summary>
        NotEnoughStamina,

        /// <summary>Переход запрещён во время боя.</summary>
        InCombat
    }
}
