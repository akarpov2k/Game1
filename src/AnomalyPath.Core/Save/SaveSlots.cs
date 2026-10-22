#nullable enable
using System;

namespace AnomalyPath.Core.Save
{
    /// <summary>Служебные имена слотов сохранения и ячейки экрана сохранения.</summary>
    public static class SaveSlots
    {
        /// <summary>Слот ручного сохранения из игрового меню (старый формат).</summary>
        public const string Manual = "manual_save";

        /// <summary>Слот принудительного сохранения при выходе.</summary>
        public const string Exit = "exit_save";

        /// <summary>Число ячеек на одной странице экрана сохранения.</summary>
        public const int SlotCount = 6;

        /// <summary>Имя ячейки сохранения по номеру; число ячеек не ограничено.</summary>
        /// <param name="number">Номер ячейки от 1.</param>
        /// <returns>Имя слота вида <c>save_1</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Номер меньше 1.</exception>
        public static string Slot(int number)
        {
            if (number < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(number));
            }

            return "save_" + number;
        }
    }
}
