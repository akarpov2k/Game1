#nullable enable
using AnomalyPath.Core.World;
using AnomalyPath.Runtime.Ui.Core;

namespace AnomalyPath.Runtime.Screens.Game
{
    /// <summary>Ключи текстов основного экрана для подтипов локаций и статусов перехода.</summary>
    public static class PlayText
    {
        /// <summary>Ключ названия подтипа локации.</summary>
        /// <param name="subtype">Подтип маршрута.</param>
        /// <returns>Ключ видимого текста.</returns>
        public static UiTextKey Subtype(LocationSubtype subtype) => subtype switch
        {
            LocationSubtype.Hub => UiTextKey.Game_Node_Hub,
            LocationSubtype.Path => UiTextKey.Game_Subtype_Path,
            LocationSubtype.Building => UiTextKey.Game_Subtype_Building,
            LocationSubtype.Terrain => UiTextKey.Game_Subtype_Terrain
        };

        /// <summary>Ключ сообщения о результате перехода.</summary>
        /// <param name="status">Статус перехода.</param>
        /// <returns>Ключ видимого текста.</returns>
        public static UiTextKey Travel(TravelStatus status) => status switch
        {
            TravelStatus.Success => UiTextKey.Game_Travel_Success,
            TravelStatus.EdgeNotFound => UiTextKey.Game_Travel_EdgeNotFound,
            TravelStatus.WrongLocation => UiTextKey.Game_Travel_WrongLocation,
            TravelStatus.HubUnavailable => UiTextKey.Game_Travel_HubUnavailable,
            TravelStatus.NotEnoughStamina => UiTextKey.Game_Travel_NotEnoughStamina,
            _ => UiTextKey.Game_Travel_InCombat
        };
    }
}
