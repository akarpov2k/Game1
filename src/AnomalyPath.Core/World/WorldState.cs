#nullable enable
using System;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.World
{
    /// <summary>Изменяемое состояние прохождения: где игрок, сколько у него сил и времени.</summary>
    public sealed class WorldState
    {
        /// <summary>Создаёт состояние.</summary>
        /// <param name="map">Карта мира.</param>
        /// <param name="currentLocationId">Текущая локация.</param>
        /// <param name="stamina">Текущая выносливость.</param>
        /// <param name="staminaMax">Максимум выносливости.</param>
        /// <param name="clock">Часы мира.</param>
        /// <param name="weather">Текущая погода.</param>
        /// <exception cref="DataException">Локация не найдена или выносливость вне границ.</exception>
        public WorldState(
            WorldMap map,
            string currentLocationId,
            int stamina,
            int staminaMax,
            WorldClock clock,
            WeatherKind weather = WeatherKind.Clear)
        {
            Map = map ?? throw new ArgumentNullException(nameof(map));
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
            if (staminaMax < 0 || stamina < 0 || stamina > staminaMax)
            {
                throw new DataException("player.stamina", CoreErrors.WorldRange, "stamina");
            }

            if (!map.TryGetLocation(currentLocationId, out _))
            {
                throw new DataException(currentLocationId, CoreErrors.WorldInstanceNotFound, currentLocationId);
            }

            CurrentLocationId = currentLocationId;
            Stamina = stamina;
            StaminaMax = staminaMax;
            Weather = weather;
            Map.GetLocation(currentLocationId).MarkVisited();
        }

        /// <summary>Карта мира.</summary>
        public WorldMap Map { get; }

        /// <summary>Часы мира.</summary>
        public WorldClock Clock { get; }

        /// <summary>Где находится игрок.</summary>
        public string CurrentLocationId { get; private set; }

        /// <summary>Текущая выносливость.</summary>
        public int Stamina { get; private set; }

        /// <summary>Максимум выносливости.</summary>
        public int StaminaMax { get; }

        /// <summary>Текущая погода.</summary>
        public WeatherKind Weather { get; private set; }

        /// <summary>Текущая локация.</summary>
        public LocationInstance CurrentLocation => Map.GetLocation(CurrentLocationId);

        /// <summary>Создаёт состояние в начале новой игры.</summary>
        /// <param name="map">Карта мира.</param>
        /// <returns>Состояние в стартовой локации с полной выносливостью.</returns>
        public static WorldState Start(WorldMap map)
        {
            var clock = new WorldClock(1, map.DayStartHour, map.DayStartHour, map.NightStartHour);
            return new WorldState(map, map.StartLocationId, map.StartingStamina, map.StartingStamina, clock);
        }

        /// <summary>Тратит выносливость, если её хватает.</summary>
        /// <param name="amount">Сколько списать.</param>
        /// <returns><c>true</c>, если выносливости хватило.</returns>
        public bool TrySpendStamina(int amount)
        {
            if (amount < 0 || Stamina < amount)
            {
                return false;
            }

            Stamina -= amount;
            return true;
        }

        /// <summary>Восстанавливает выносливость в пределах максимума.</summary>
        /// <param name="amount">Сколько добавить.</param>
        public void RestoreStamina(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            Stamina = Math.Min(StaminaMax, Stamina + amount);
        }

        /// <summary>Перемещает игрока в другую локацию.</summary>
        /// <param name="locationId">Id экземпляра локации.</param>
        /// <exception cref="DataException">Локация не найдена.</exception>
        public void MoveTo(string locationId)
        {
            if (!Map.TryGetLocation(locationId, out var location) || location == null)
            {
                throw new DataException(locationId, CoreErrors.WorldInstanceNotFound, locationId);
            }

            CurrentLocationId = locationId;
            location.MarkVisited();
        }

        /// <summary>Меняет погоду.</summary>
        /// <param name="weather">Новая погода.</param>
        public void SetWeather(WeatherKind weather) => Weather = weather;
    }
}
