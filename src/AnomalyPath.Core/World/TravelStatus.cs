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

        /// <summary>Переход запрещён во время боя.</summary>
        InCombat,

        /// <summary>В хаб можно вернуться только с пути.</summary>
        HubUnavailable,

        /// <summary>Не хватает выносливости на возврат в хаб.</summary>
        NotEnoughStamina
    }
}
