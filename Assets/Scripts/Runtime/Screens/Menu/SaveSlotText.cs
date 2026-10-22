#nullable enable
using System.Globalization;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Save;
using AnomalyPath.Runtime.Ui.Core;

namespace AnomalyPath.Runtime.Screens.Menu
{
    /// <summary>Подписи сохранений для списков загрузки и сохранения: название, уровень, деньги и день мира.</summary>
    internal static class SaveSlotText
    {
        /// <summary>Деньги в виде «1000 руб».</summary>
        /// <param name="texts">Каталог текстов.</param>
        /// <param name="money">Сумма.</param>
        /// <returns>Подпись.</returns>
        public static string Money(IGameTexts texts, int money) =>
            UiText.Format(texts, UiTextKey.Common_Money, money);

        /// <summary>Строка прогресса сохранения.</summary>
        /// <param name="texts">Каталог текстов.</param>
        /// <param name="slot">Описание слота.</param>
        /// <returns>Название, уровень, деньги и день мира.</returns>
        public static string Label(IGameTexts texts, SaveSlotInfo slot)
        {
            if (!slot.IsReadable)
            {
                return UiText.Format(texts, UiTextKey.SaveSlot_Corrupted_Line, slot.Slot);
            }

            var title = string.IsNullOrWhiteSpace(slot.Title) ? slot.Slot : slot.Title;
            var progress = slot.Level > 0
                ? UiText.Format(texts, UiTextKey.Common_Level, slot.Level) + " · " +
                  Money(texts, slot.Money) + " · "
                : string.Empty;
            return UiText.Format(
                texts,
                UiTextKey.SaveSlot_Progress,
                title,
                progress,
                slot.Day,
                slot.Hours.ToString("0.0", CultureInfo.InvariantCulture));
        }
    }
}
