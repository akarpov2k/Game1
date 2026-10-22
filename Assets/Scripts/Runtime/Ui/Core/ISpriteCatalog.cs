#nullable enable
using UnityEngine;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>Каталог спрайтов по строковому id из данных: иконки предметов, фоны локаций.</summary>
    public interface ISpriteCatalog
    {
        /// <summary>Ищет спрайт по id.</summary>
        /// <param name="id">Id из данных: <c>icons/wpn_ak74</c>, <c>bg_hub_settlement</c>.</param>
        /// <param name="sprite">Найденный спрайт или <c>null</c>.</param>
        /// <returns><c>true</c>, если спрайт есть в каталоге.</returns>
        bool TryGet(string id, out Sprite? sprite);
    }
}
