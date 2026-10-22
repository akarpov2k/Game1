#nullable enable
using System;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.World
{
    /// <summary>Стоимость перехода: выносливость и часы мира.</summary>
    public sealed class TravelCost
    {
        /// <summary>Создаёт стоимость.</summary>
        /// <param name="stamina">Сколько выносливости тратит переход.</param>
        /// <param name="hours">Сколько часов мира занимает переход.</param>
        /// <exception cref="ArgumentOutOfRangeException">Значение отрицательное или не число.</exception>
        public TravelCost(int stamina, double hours)
        {
            if (stamina < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stamina));
            }

            if (hours < 0 || double.IsNaN(hours) || double.IsInfinity(hours))
            {
                throw new ArgumentOutOfRangeException(nameof(hours));
            }

            Stamina = stamina;
            Hours = hours;
        }

        /// <summary>Сколько выносливости тратит переход.</summary>
        public int Stamina { get; }

        /// <summary>Сколько часов мира занимает переход.</summary>
        public double Hours { get; }

        /// <summary>Нулевая стоимость: двери и коридоры внутри локации.</summary>
        public static TravelCost Zero { get; } = new TravelCost(0, 0);

        /// <summary>Умножает стоимость на длину ребра.</summary>
        /// <param name="length">Длина ребра в шагах; должна быть больше нуля.</param>
        /// <returns>Стоимость перехода по ребру.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Длина не положительна.</exception>
        public TravelCost Scale(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return new TravelCost(Stamina * length, Hours * length);
        }

        /// <summary>Читает стоимость из объекта.</summary>
        /// <param name="value">Объект стоимости.</param>
        /// <param name="field">Имя поля для сообщения.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Стоимость.</returns>
        /// <exception cref="DataException">Поля отсутствуют или отрицательны.</exception>
        public static TravelCost FromJson(JsonValue value, string field, string sourcePath)
        {
            if (!value.IsObject)
            {
                throw new DataException(sourcePath, CoreErrors.MissingNumberField, field);
            }

            var stamina = DataMap.RequireInt(value, "stamina", sourcePath);
            var hours = DataMap.RequireDouble(value, "hours", sourcePath);
            if (stamina < 0 || hours < 0 || double.IsNaN(hours) || double.IsInfinity(hours))
            {
                throw new DataException(sourcePath, CoreErrors.WorldRange, field);
            }

            return new TravelCost(stamina, hours);
        }
    }
}
