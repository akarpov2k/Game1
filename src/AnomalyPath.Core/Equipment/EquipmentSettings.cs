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
        /// <summary>Создаёт настройки.</summary>
        /// <param name="quickSlots">Число быстрых слотов.</param>
        public EquipmentSettings(int quickSlots)
        {
            if (quickSlots < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(quickSlots),
                    CoreText.Format(CoreErrors.NonNegative, nameof(quickSlots)));
            }

            QuickSlots = quickSlots;
        }

        /// <inheritdoc />
        public int QuickSlots { get; }

        /// <summary>Читает настройки из записи вида <c>equipment</c>.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Настройки экипировки.</returns>
        /// <exception cref="DataException">Поле отсутствует или задано неверно.</exception>
        public static EquipmentSettings FromJson(JsonValue value, string sourcePath)
        {
            var quickSlots = DataMap.RequireInt(value, "quickSlots", sourcePath);
            if (quickSlots < 0)
            {
                throw new DataException(sourcePath, CoreErrors.NonNegative, "quickSlots");
            }

            return new EquipmentSettings(quickSlots);
        }
    }
}
