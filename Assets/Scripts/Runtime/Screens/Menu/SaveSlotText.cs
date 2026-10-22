#nullable enable
using System.Globalization;
using AnomalyPath.Core.Save;

namespace AnomalyPath.Runtime.Screens.Menu
{
    /// <summary>Подписи сохранений для списков загрузки и сохранения: название, уровень, деньги и день мира.</summary>
    internal static class SaveSlotText
    {
        /// <summary>Деньги в виде «1000 руб».</summary>
        /// <param name="money">Сумма.</param>
        /// <returns>Подпись.</returns>
        public static string Money(int money) => money.ToString("0", CultureInfo.InvariantCulture) + " руб";

        /// <summary>Строка прогресса сохранения.</summary>
        /// <param name="slot">Описание слота.</param>
        /// <returns>Название, уровень, деньги и день мира.</returns>
        public static string Label(SaveSlotInfo slot)
        {
            if (!slot.IsReadable)
            {
                return slot.Slot + " — повреждён";
            }

            var title = string.IsNullOrWhiteSpace(slot.Title) ? slot.Slot : slot.Title;
            var progress = slot.Level > 0 ? "ур. " + slot.Level + " · " + Money(slot.Money) + " · " : string.Empty;
            return title + " — " + progress + "день " + slot.Day + ", " +
                   slot.Hours.ToString("0.0", CultureInfo.InvariantCulture) + " ч";
        }
    }
}
