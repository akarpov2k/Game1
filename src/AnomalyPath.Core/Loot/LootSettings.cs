#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Loot
{
    /// <summary>
    /// Настройка лута: полосы тира предмета относительно тира локации.
    /// Единственная запись вида <c>loot</c>, файл <c>loot.json</c>.
    /// </summary>
    public sealed class LootSettings
    {
        /// <summary>Обязательный id единственной записи: <c>loot</c>.</summary>
        public const string RecordId = "loot";

        private const double BandSumTolerance = 1e-6;

        private readonly IReadOnlyList<TierBand> _bands;

        private LootSettings()
        {
            _bands = Array.Empty<TierBand>();
        }

        /// <summary>Пустая настройка: полос нет, предметы с тиром не выпадают.</summary>
        public static LootSettings Empty { get; } = new LootSettings();

        /// <summary>Создаёт настройку лута из полос тира.</summary>
        /// <param name="bands">Полосы тира; сумма вкладов 1.0.</param>
        /// <exception cref="ArgumentNullException">Полосы не заданы.</exception>
        /// <exception cref="ArgumentException">Полос нет или сумма вкладов не равна 1.</exception>
        public LootSettings(IReadOnlyList<TierBand> bands)
        {
            if (bands == null)
            {
                throw new ArgumentNullException(nameof(bands));
            }

            if (bands.Count == 0)
            {
                throw new ArgumentException(CoreText.Get(CoreErrors.LootBandEmpty), nameof(bands));
            }

            var sum = 0.0;
            foreach (var band in bands)
            {
                sum += band.Chance;
            }

            if (Math.Abs(sum - 1.0) > BandSumTolerance)
            {
                throw new ArgumentException(CoreText.Format(CoreErrors.LootBandSum, sum), nameof(bands));
            }

            _bands = bands;
        }

        /// <summary>Полосы тира: сдвиг относительно тира локации и вклад в шанс.</summary>
        public IReadOnlyList<TierBand> Bands => _bands;

        /// <summary>Читает настройку лута из записи данных.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Настройка лута.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static LootSettings FromJson(JsonValue value, string sourcePath)
        {
            var id = DataMap.RequireString(value, "id", sourcePath);
            if (!string.Equals(id, RecordId, StringComparison.Ordinal))
            {
                throw new DataException(sourcePath, CoreErrors.LootSettingsId, id);
            }

            if (!value.TryGet("tierBands", out var bandArray) || !bandArray.IsArray || bandArray.Items.Count == 0)
            {
                throw new DataException(sourcePath, CoreErrors.MissingNumberField, "tierBands");
            }

            var bands = new List<TierBand>(bandArray.Items.Count);
            var sum = 0.0;
            foreach (var item in bandArray.Items)
            {
                if (!item.IsObject)
                {
                    throw new DataException(sourcePath, CoreErrors.LootBand, RecordId);
                }

                var offset = DataMap.RequireInt(item, "offset", sourcePath);
                var chance = DataMap.RequireDouble(item, "chance", sourcePath);
                if (chance < 0 || chance > 1)
                {
                    throw new DataException(sourcePath, CoreErrors.LootBand, offset);
                }

                sum += chance;
                bands.Add(new TierBand(offset, chance));
            }

            if (Math.Abs(sum - 1.0) > BandSumTolerance)
            {
                throw new DataException(sourcePath, CoreErrors.LootBandSum, sum);
            }

            return new LootSettings(bands);
        }

        /// <summary>
        /// Шанс полосы для предмета указанного тира: полосы за краем шкалы
        /// складываются с крайним тиром, поэтому у локации 1-го тира получается 80/20.
        /// </summary>
        /// <param name="itemTier">Тир предмета.</param>
        /// <param name="locationTier">Тир локации.</param>
        /// <param name="minItemTier">Минимальный тир предметов в каталоге.</param>
        /// <param name="maxItemTier">Максимальный тир предметов в каталоге.</param>
        /// <returns>Шанс 0..1: вероятность пройти полосу тира.</returns>
        public double ChanceForTier(int itemTier, int locationTier, int minItemTier, int maxItemTier)
        {
            if (minItemTier > maxItemTier)
            {
                return 0;
            }

            var chance = 0.0;
            foreach (var band in _bands)
            {
                var target = locationTier + band.Offset;
                if (target < minItemTier)
                {
                    target = minItemTier;
                }
                else if (target > maxItemTier)
                {
                    target = maxItemTier;
                }

                if (target == itemTier)
                {
                    chance += band.Chance;
                }
            }

            return chance > 1 ? 1 : chance;
        }
    }
}
