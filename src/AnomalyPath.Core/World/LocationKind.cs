#nullable enable
namespace AnomalyPath.Core.World
{
    /// <summary>Тип локации из схемы: код в данных — имя члена в нижнем регистре.</summary>
    public enum LocationKind
    {
        /// <summary>Обустроенная зона: база, лагерь, поселение.</summary>
        Hub,

        /// <summary>Дикая местность.</summary>
        Wild,

        /// <summary>Особая локация: аномальная зона, квестовое место.</summary>
        Special
    }
}
