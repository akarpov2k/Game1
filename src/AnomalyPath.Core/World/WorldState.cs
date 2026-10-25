#nullable enable
using System;
using System.Collections.Generic;
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
        /// <param name="lastHubId">Последний посещённый хаб; <c>null</c> — определить по карте.</param>
        /// <param name="lootSerial">Номер последнего броска лута; в старых сейвах 0.</param>
        /// <param name="stashChecks">Проверки локаций без тайника с последней находки; в старых сейвах 0.</param>
        /// <param name="stashOpenings">Вскрытия тайников без джекпота с последнего срабатывания; в старых сейвах 0.</param>
        /// <param name="objectSeed">Постоянная часть seed выбора объектов; <c>null</c> — новый GUID.</param>
        /// <exception cref="DataException">Локация не найдена или выносливость вне границ.</exception>
        public WorldState(
            WorldMap map,
            string currentLocationId,
            int stamina,
            int staminaMax,
            WorldClock clock,
            WeatherKind weather = WeatherKind.Clear,
            string? lastHubId = null,
            int lootSerial = 0,
            int stashChecks = 0,
            int stashOpenings = 0,
            string? objectSeed = null)
        {
            Map = map ?? throw new ArgumentNullException(nameof(map));
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
            if (staminaMax < 0 || stamina < 0 || stamina > staminaMax)
            {
                throw new DataException("player.stamina", CoreErrors.WorldRange, "stamina");
            }

            if (lootSerial < 0)
            {
                throw new DataException("world.lootSerial", CoreErrors.WorldRange, "lootSerial");
            }

            if (stashChecks < 0 || stashOpenings < 0)
            {
                throw new DataException("world.stashChecks", CoreErrors.WorldRange, "stashChecks");
            }

            if (!map.TryGetLocation(currentLocationId, out _))
            {
                throw new DataException(currentLocationId, CoreErrors.WorldInstanceNotFound, currentLocationId);
            }

            CurrentLocationId = currentLocationId;
            Stamina = stamina;
            StaminaMax = staminaMax;
            Weather = weather;
            LootSerial = lootSerial;
            StashChecks = stashChecks;
            StashOpenings = stashOpenings;
            ObjectSeed = string.IsNullOrWhiteSpace(objectSeed)
                ? Guid.NewGuid().ToString("N")
                : objectSeed!;
            LastHubId = ResolveLastHub(map, currentLocationId, lastHubId);
            Map.GetLocation(currentLocationId).MarkVisited();
        }

        /// <summary>Карта мира.</summary>
        public WorldMap Map { get; }

        /// <summary>Часы мира.</summary>
        public WorldClock Clock { get; }

        /// <summary>Где находится игрок.</summary>
        public string CurrentLocationId { get; private set; }

        /// <summary>Последний посещённый хаб: точка возврата после смерти.</summary>
        public string LastHubId { get; private set; }

        /// <summary>Текущая выносливость.</summary>
        public int Stamina { get; private set; }

        /// <summary>Максимум выносливости.</summary>
        public int StaminaMax { get; }

        /// <summary>Текущая погода.</summary>
        public WeatherKind Weather { get; private set; }

        /// <summary>
        /// Номер последнего броска лута. Входит в seed броска, поэтому каждый следующий бросок
        /// в одном состоянии даёт новый результат, а сохранённый номер делает его проверяемым.
        /// </summary>
        public int LootSerial { get; private set; }

        /// <summary>
        /// Проверки локаций без находки тайника. Входит в шанс следующей проверки,
        /// поэтому шанс растёт с каждой вылазкой и сбрасывается после находки.
        /// </summary>
        public int StashChecks { get; private set; }

        /// <summary>
        /// Вскрытия тайников без джекпота. Входит в шанс следующего вскрытия
        /// и сбрасывается после срабатывания джекпота.
        /// </summary>
        public int StashOpenings { get; private set; }

        /// <summary>
        /// Постоянная часть seed выбора объектов локаций: GUID, созданный при новой игре
        /// и сохранённый в сейве. Временная часть добавляется при входе на локацию.
        /// </summary>
        public string ObjectSeed { get; }

        /// <summary>Текущая локация.</summary>
        public LocationInstance CurrentLocation => Map.GetLocation(CurrentLocationId);

        /// <summary>Сколько звеньев маршрута уже сгенерировано.</summary>
        public int RouteEdges => Map.RouteEdges;

        /// <summary>Id последней локации конца маршрута; <c>null</c> — маршрут ещё не строился.</summary>
        public string? RouteTailId => Map.RouteTailId;

        /// <summary>Сколько веток дало последнее сгенерированное звено.</summary>
        public int RouteLastBranches => Map.RouteLastBranches;

        /// <summary>Выходы последнего звена — концы его веток, ведущие в следующее звено.</summary>
        public IReadOnlyList<string> RouteFrontIds => Map.RouteFrontIds;

        /// <summary>Запоминает, докуда достроен маршрут.</summary>
        /// <param name="edges">Сколько звеньев сгенерировано.</param>
        /// <param name="tailId">Id конца маршрута; <c>null</c> — маршрут не строился.</param>
        /// <param name="lastBranches">Веток на последнем звене.</param>
        /// <param name="frontIds">Выходы звена — концы его веток.</param>
        /// <exception cref="DataException">Отрицательные значения или неизвестный конец маршрута.</exception>
        public void MarkRoute(int edges, string? tailId, int lastBranches, IReadOnlyList<string>? frontIds = null) =>
            Map.MarkRoute(edges, tailId, lastBranches, frontIds);

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
            if (location.Subtype == LocationSubtype.Hub)
            {
                LastHubId = locationId;
            }
        }

        /// <summary>Меняет погоду.</summary>
        /// <param name="weather">Новая погода.</param>
        public void SetWeather(WeatherKind weather) => Weather = weather;

        /// <summary>Выдаёт номер следующего броска лута и увеличивает счётчик.</summary>
        /// <returns>Номер броска, начиная с 1.</returns>
        public int NextLootSerial() => ++LootSerial;

        /// <summary>Отмечает проверку локации без находки тайника.</summary>
        public void RegisterStashCheck() => StashChecks++;

        /// <summary>Сбрасывает счётчик проверок после находки тайника.</summary>
        public void ResetStashChecks() => StashChecks = 0;

        /// <summary>Отмечает вскрытие тайника без джекпота.</summary>
        public void RegisterStashOpening() => StashOpenings++;

        /// <summary>Сбрасывает счётчик вскрытий после срабатывания джекпота.</summary>
        public void ResetStashOpenings() => StashOpenings = 0;

        private static string ResolveLastHub(WorldMap map, string currentLocationId, string? lastHubId)
        {
            if (lastHubId != null)
            {
                if (!map.TryGetLocation(lastHubId, out var savedHub) ||
                    savedHub == null ||
                    savedHub.Subtype != LocationSubtype.Hub)
                {
                    throw new DataException(lastHubId, CoreErrors.WorldInstanceNotFound, lastHubId);
                }

                return lastHubId;
            }

            if (map.TryGetLocation(currentLocationId, out var current) &&
                current != null &&
                current.Subtype == LocationSubtype.Hub)
            {
                return currentLocationId;
            }

            var hubs = new List<LocationInstance>();
            foreach (var location in map.Locations)
            {
                if (location.Subtype == LocationSubtype.Hub)
                {
                    hubs.Add(location);
                }
            }

            if (hubs.Count == 0)
            {
                throw new DataException("world", CoreErrors.WorldNoHub);
            }

            hubs.Sort((left, right) => string.CompareOrdinal(left.InstanceId, right.InstanceId));
            return hubs[0].InstanceId;
        }
    }
}
