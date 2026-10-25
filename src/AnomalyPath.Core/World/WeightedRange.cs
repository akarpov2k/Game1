#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.World
{
    /// <summary>
    /// Целочисленный диапазон с весами значений: «длина» локации, где минимум выпадает реже
    /// середины. Веса приходят из данных, в коде чисел нет.
    /// </summary>
    public sealed class WeightedRange
    {
        private readonly IReadOnlyDictionary<int, double> _weights;

        /// <summary>Создаёт взвешенный диапазон.</summary>
        /// <param name="range">Диапазон значений.</param>
        /// <param name="weights">Веса значений; <c>null</c> — равномерный выбор.</param>
        /// <exception cref="ArgumentException">Вес отрицателен, не покрывает значение диапазона или сумма равна нулю.</exception>
        public WeightedRange(IntRange range, IReadOnlyDictionary<int, double>? weights = null)
        {
            Range = range;
            if (weights == null)
            {
                _weights = new Dictionary<int, double>();
                return;
            }

            var checkedWeights = new Dictionary<int, double>();
            foreach (var pair in weights)
            {
                if (pair.Value < 0)
                {
                    throw new ArgumentException(
                        CoreText.Format(CoreErrors.WorldWeights, pair.Key),
                        nameof(weights));
                }

                checkedWeights[pair.Key] = pair.Value;
            }

            for (var value = range.Min; value <= range.Max; value++)
            {
                if (!checkedWeights.ContainsKey(value))
                {
                    throw new ArgumentException(
                        CoreText.Format(CoreErrors.WorldWeights, value),
                        nameof(weights));
                }
            }

            var total = 0.0;
            foreach (var pair in checkedWeights)
            {
                total += pair.Value;
            }

            if (total <= 0)
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.WorldWeights, range.Min),
                    nameof(weights));
            }

            _weights = checkedWeights;
        }

        /// <summary>Диапазон значений.</summary>
        public IntRange Range { get; }

        /// <summary>Веса значений; пусто — выбор равномерный.</summary>
        public IReadOnlyDictionary<int, double> Weights => _weights;

        /// <summary>Выбирает значение: по весам, если они есть, иначе равномерно.</summary>
        /// <param name="random">Источник случайных чисел.</param>
        /// <returns>Значение внутри диапазона.</returns>
        public int Sample(IWorldRandom random)
        {
            if (_weights.Count == 0)
            {
                return Range.Sample(random);
            }

            var total = 0.0;
            for (var value = Range.Min; value <= Range.Max; value++)
            {
                total += _weights[value];
            }

            var roll = random.NextDouble() * total;
            for (var value = Range.Min; value <= Range.Max; value++)
            {
                roll -= _weights[value];
                if (roll < 0)
                {
                    return value;
                }
            }

            return Range.Max;
        }

        /// <summary>Читает диапазон и веса из записи данных.</summary>
        /// <param name="value">Запись сущности.</param>
        /// <param name="rangeField">Имя поля диапазона.</param>
        /// <param name="weightsField">Имя поля весов.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <param name="fallback">Диапазон, если поле не задано.</param>
        /// <returns>Взвешенный диапазон.</returns>
        /// <exception cref="DataException">Поле отсутствует, границы или веса некорректны.</exception>
        public static WeightedRange Optional(
            JsonValue value,
            string rangeField,
            string weightsField,
            string sourcePath,
            IntRange fallback)
        {
            var range = IntRange.Optional(value, rangeField, sourcePath, fallback);
            if (!value.TryGet(weightsField, out var weightsValue))
            {
                return new WeightedRange(range);
            }

            if (!weightsValue.IsObject)
            {
                throw new DataException(sourcePath, CoreErrors.JsonNotObject);
            }

            var weights = new Dictionary<int, double>();
            foreach (var pair in weightsValue.Entries)
            {
                if (!int.TryParse(pair.Key, System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture, out var key))
                {
                    throw new DataException(sourcePath, CoreErrors.WorldWeights, pair.Key);
                }

                if (pair.Value.Kind != JsonKind.Number)
                {
                    throw new DataException(sourcePath, CoreErrors.MissingNumberField, weightsField);
                }

                if (!range.Contains(key))
                {
                    throw new DataException(sourcePath, CoreErrors.WorldWeights, pair.Key);
                }

                weights[key] = pair.Value.Number;
            }

            try
            {
                return new WeightedRange(range, weights);
            }
            catch (ArgumentException)
            {
                throw new DataException(sourcePath, CoreErrors.WorldWeights, rangeField);
            }
        }
    }
}
