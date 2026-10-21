#nullable enable
using System;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Equipment
{
    /// <summary>Настройки экипировки: число слотов быстрого доступа из данных.</summary>
    public sealed class EquipmentSettings : IEquipmentSettings
    {
        /// <summary>Значение по умолчанию, если файл настроек отсутствует.</summary>
        public const int DefaultQuickSlots = 4;

        /// <summary>Создаёт настройки.</summary>
        /// <param name="quickSlots">Число быстрых слотов.</param>
        public EquipmentSettings(int quickSlots)
        {
            if (quickSlots < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quickSlots), "Число быстрых слотов не может быть отрицательным");
            }

            QuickSlots = quickSlots;
        }

        /// <inheritdoc />
        public int QuickSlots { get; }

        /// <summary>Настройки по умолчанию.</summary>
        public static EquipmentSettings Default => new EquipmentSettings(DefaultQuickSlots);

        /// <summary>Читает настройки из записи вида <c>equipment</c>.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Настройки экипировки.</returns>
        /// <exception cref="DataException">Поле имеет неверный тип.</exception>
        public static EquipmentSettings FromJson(JsonValue value, string sourcePath)
        {
            var quickSlots = DataMap.OptionalInt(value, "quickSlots", DefaultQuickSlots);
            if (quickSlots < 0)
            {
                throw new DataException(sourcePath, CoreErrors.MissingNumberField, "quickSlots");
            }

            return new EquipmentSettings(quickSlots);
        }
    }
}
