#nullable enable
using AnomalyPath.Core.Loadouts;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Пробная раскладка набора снаряжения без создания партии: что игрок получит на старте.</summary>
    public interface ILoadoutPreviewService
    {
        /// <summary>Раскладывает набор в сетку инвентаря и по слотам экипировки.</summary>
        /// <param name="loadoutId">Id набора снаряжения из данных.</param>
        /// <returns>Инвентарь, экипировка и отчёт о том, что не поместилось или не надето.</returns>
        /// <exception cref="Data.DataException">Набор с таким id не найден.</exception>
        LoadoutPreview Build(string loadoutId);
    }
}
