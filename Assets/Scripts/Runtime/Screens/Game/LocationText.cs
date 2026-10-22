#nullable enable
using AnomalyPath.Core.World;
using AnomalyPath.Runtime.Ui.Core;

namespace AnomalyPath.Runtime.Screens.Game
{
    /// <summary>Читает видимое название экземпляра локации через ключ шаблона.</summary>
    public static class LocationText
    {
        /// <summary>Название локации для текущего языка.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="location">Экземпляр локации.</param>
        /// <returns>Текст названия.</returns>
        public static string Title(IUiHost host, LocationInstance location) =>
            host.Texts.Get(host.Locations.Get(location.TemplateId).TitleKey);
    }
}
