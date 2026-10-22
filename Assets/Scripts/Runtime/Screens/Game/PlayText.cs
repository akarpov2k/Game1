#nullable enable
using AnomalyPath.Core.World;
using AnomalyPath.Runtime.Ui.Core;

namespace AnomalyPath.Runtime.Screens.Game
{
    /// <summary>Ключи текстов основного экрана для ролей локаций и статусов перехода.</summary>
    public static class PlayText
    {
        /// <summary>Ключ названия роли локации.</summary>
        /// <param name="type">Роль в цепочке.</param>
        /// <returns>Ключ видимого текста.</returns>
        public static UiTextKey Node(LocationNodeType type) => type switch
        {
            LocationNodeType.Hub => UiTextKey.Game_Node_Hub,
            LocationNodeType.Route => UiTextKey.Game_Node_Route,
            LocationNodeType.Site => UiTextKey.Game_Node_Site,
            _ => UiTextKey.Game_Node_Interior
        };

        /// <summary>Ключ сообщения о результате перехода.</summary>
        /// <param name="status">Статус перехода.</param>
        /// <returns>Ключ видимого текста.</returns>
        public static UiTextKey Travel(TravelStatus status) => status switch
        {
            TravelStatus.Success => UiTextKey.Game_Travel_Success,
            TravelStatus.EdgeNotFound => UiTextKey.Game_Travel_EdgeNotFound,
            TravelStatus.WrongLocation => UiTextKey.Game_Travel_WrongLocation,
            _ => UiTextKey.Game_Travel_InCombat
        };
    }
}
