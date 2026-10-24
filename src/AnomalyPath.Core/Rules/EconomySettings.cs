#nullable enable
using System;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Rules
{
    /// <summary>Экономика из данных: округление и нижняя граница цены.</summary>
    public sealed class EconomySettings : IEconomySettings
    {
        /// <summary>Настройки по умолчанию: округление до рубля, без нижней границы.</summary>
        public static readonly EconomySettings Default = new EconomySettings(1, 0);

        /// <summary>Создаёт настройки экономики.</summary>
        /// <param name="rounding">Шаг округления, не меньше единицы.</param>
        /// <param name="minPrice">Минимальная цена, не меньше нуля.</param>
        /// <exception cref="ArgumentOutOfRangeException">Значения вне допустимых границ.</exception>
        public EconomySettings(int rounding, int minPrice)
        {
            if (rounding < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rounding),
                    CoreText.Format(CoreErrors.EconomyRounding, rounding));
            }

            if (minPrice < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minPrice),
                    CoreText.Format(CoreErrors.EconomyMinPrice, minPrice));
            }

            Rounding = rounding;
            MinPrice = minPrice;
        }

        /// <inheritdoc />
        public int Rounding { get; }

        /// <inheritdoc />
        public int MinPrice { get; }

        /// <summary>Читает настройки экономики из записи данных.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Настройки экономики.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static EconomySettings FromJson(JsonValue value, string sourcePath)
        {
            var rounding = DataMap.RequireInt(value, "rounding", sourcePath);
            if (rounding < 1)
            {
                throw new DataException(sourcePath, CoreErrors.EconomyRounding, rounding);
            }

            var minPrice = DataMap.OptionalInt(value, "minPrice", 0);
            if (minPrice < 0)
            {
                throw new DataException(sourcePath, CoreErrors.EconomyMinPrice, minPrice);
            }

            return new EconomySettings(rounding, minPrice);
        }
    }
}
