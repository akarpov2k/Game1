#nullable enable
using System;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.World
{
    /// <summary>Часы и дни мира: переходы и отдых двигают время, фаза суток считается от порогов.</summary>
    public sealed class WorldClock
    {
        /// <summary>Создаёт часы.</summary>
        /// <param name="day">Номер дня, начиная с 1.</param>
        /// <param name="hours">Часы от 0 до 24 (не включая 24).</param>
        /// <param name="dayStartHour">Час начала дня.</param>
        /// <param name="nightStartHour">Час начала ночи.</param>
        /// <exception cref="ArgumentOutOfRangeException">Значения вне допустимых границ.</exception>
        public WorldClock(int day, double hours, double dayStartHour, double nightStartHour)
        {
            if (day < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(day));
            }

            if (hours < 0 || hours >= 24 || double.IsNaN(hours) || double.IsInfinity(hours))
            {
                throw new ArgumentOutOfRangeException(nameof(hours));
            }

            if (dayStartHour < 0 || nightStartHour > 24 || dayStartHour >= nightStartHour)
            {
                throw new ArgumentOutOfRangeException(nameof(dayStartHour));
            }

            Day = day;
            Hours = hours;
            DayStartHour = dayStartHour;
            NightStartHour = nightStartHour;
        }

        /// <summary>Номер дня, начиная с 1.</summary>
        public int Day { get; private set; }

        /// <summary>Часы от 0 до 24.</summary>
        public double Hours { get; private set; }

        /// <summary>Час начала дня.</summary>
        public double DayStartHour { get; }

        /// <summary>Час начала ночи.</summary>
        public double NightStartHour { get; }

        /// <summary>Текущая фаза суток.</summary>
        public TimePhase Phase => Hours >= DayStartHour && Hours < NightStartHour ? TimePhase.Day : TimePhase.Night;

        /// <summary>Двигает время вперёд, перенося лишние часы в следующие дни.</summary>
        /// <param name="hours">Сколько часов добавить; не может быть отрицательным.</param>
        /// <exception cref="ArgumentOutOfRangeException">Отрицательное или не число.</exception>
        public void Advance(double hours)
        {
            if (hours < 0 || double.IsNaN(hours) || double.IsInfinity(hours))
            {
                throw new ArgumentOutOfRangeException(nameof(hours));
            }

            Hours += hours;
            while (Hours >= 24)
            {
                Hours -= 24;
                Day++;
            }
        }

        /// <summary>Проверяет, что часы в допустимом диапазоне.</summary>
        /// <param name="hours">Проверяемое значение.</param>
        /// <returns><c>true</c>, если значение в диапазоне 0..24.</returns>
        public static bool IsValidHours(double hours) =>
            hours >= 0 && hours < 24 && !double.IsNaN(hours) && !double.IsInfinity(hours);

        /// <summary>Создаёт часы из состояния сохранения.</summary>
        /// <param name="day">День.</param>
        /// <param name="hours">Часы.</param>
        /// <param name="dayStartHour">Час начала дня.</param>
        /// <param name="nightStartHour">Час начала ночи.</param>
        /// <returns>Часы.</returns>
        /// <exception cref="DataException">Значения вне диапазона.</exception>
        public static WorldClock FromState(int day, double hours, double dayStartHour, double nightStartHour)
        {
            if (day < 1 || !IsValidHours(hours))
            {
                throw new DataException("world.time", CoreErrors.WorldClockHours, hours);
            }

            return new WorldClock(day, hours, dayStartHour, nightStartHour);
        }
    }
}
