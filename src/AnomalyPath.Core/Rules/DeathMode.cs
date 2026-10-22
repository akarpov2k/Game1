#nullable enable
namespace AnomalyPath.Core.Rules
{
    /// <summary>Режим смерти в правилах прохождения.</summary>
    public enum DeathMode
    {
        /// <summary>Смерть возвращает игрока в хаб с потерей лута.</summary>
        HubReturn,

        /// <summary>Смерть удаляет прохождение полностью.</summary>
        Permadeath
    }
}
