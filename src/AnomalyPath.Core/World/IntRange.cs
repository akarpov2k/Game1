#nullable enable
using System;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.World
{
    /// <summary>Целочисленный диапазон «от и до» из данных.</summary>
    public readonly struct IntRange
    {
        /// <summary>Создаёт диапазон.</summary>
        /// <param name="min">Нижняя граница включительно.</param>
        /// <param name="max">Верхняя граница включительно.</param>
        /// <exception cref="ArgumentOutOfRangeException">Границы отрицательны или перепутаны.</exception>
        public IntRange(int min, int max)
        {
            if (min < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(min));
            }

            if (max < min)
            {
                throw new ArgumentOutOfRangeException(nameof(max));
            }

            Min = min;
            Max = max;
        }

        /// <summary>Нижняя граница включительно.</summary>
        public int Min { get; }

        /// <summary>Верхняя граница включительно.</summary>
        public int Max { get; }

        /// <summary>Проверяет, входит ли значение в диапазон.</summary>
        /// <param name="value">Проверяемое значение.</param>
        /// <returns><c>true</c>, если значение внутри границ.</returns>
        public bool Contains(int value) => value >= Min && value <= Max;

        /// <summary>Выбирает значение из диапазона включительно.</summary>
        /// <param name="random">Источник случайных чисел.</param>
        /// <returns>Значение в границах.</returns>
        public int Sample(IWorldRandom random) => random.NextInt(Min, Max + 1);

        /// <summary>Читает обязательный диапазон из массива двух чисел.</summary>
        /// <param name="value">Запись сущности.</param>
        /// <param name="field">Имя поля.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Диапазон.</returns>
        /// <exception cref="DataException">Поле отсутствует или границы некорректны.</exception>
        public static IntRange FromJson(JsonValue value, string field, string sourcePath)
        {
            var pair = DataMap.RequireIntPair(value, field, sourcePath);
            return Validated(pair.Item1, pair.Item2, field, sourcePath);
        }

        /// <summary>Читает необязательный диапазон.</summary>
        /// <param name="value">Запись сущности.</param>
        /// <param name="field">Имя поля.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <param name="fallback">Значение, если поля нет.</param>
        /// <returns>Диапазон из данных или запасной.</returns>
        /// <exception cref="DataException">Границы некорректны.</exception>
        public static IntRange Optional(JsonValue value, string field, string sourcePath, IntRange fallback)
        {
            if (!value.TryGet(field, out var fieldValue))
            {
                return fallback;
            }

            return FromJson(value, field, sourcePath);
        }

        private static IntRange Validated(int min, int max, string field, string sourcePath)
        {
            if (min < 0 || max < min)
            {
                throw new DataException(sourcePath, CoreErrors.WorldRange, field);
            }

            return new IntRange(min, max);
        }
    }
}
