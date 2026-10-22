#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Save
{
    /// <summary>Снимок мира в сохранении: карта, текущее положение, время и погода.</summary>
    public sealed class WorldSaveData
    {
        /// <summary>Создаёт снимок мира.</summary>
        /// <param name="startLocationId">Стартовая локация.</param>
        /// <param name="currentLocationId">Текущая локация.</param>
        /// <param name="stamina">Текущая выносливость.</param>
        /// <param name="staminaMax">Максимум выносливости.</param>
        /// <param name="startingStamina">Стартовая выносливость новой игры.</param>
        /// <param name="day">День.</param>
        /// <param name="hours">Часы.</param>
        /// <param name="dayStartHour">Час начала дня.</param>
        /// <param name="nightStartHour">Час начала ночи.</param>
        /// <param name="weather">Погода.</param>
        /// <param name="locations">Экземпляры локаций.</param>
        /// <param name="edges">Рёбра переходов.</param>
        public WorldSaveData(
            string startLocationId,
            string currentLocationId,
            int stamina,
            int staminaMax,
            int startingStamina,
            int day,
            double hours,
            double dayStartHour,
            double nightStartHour,
            string weather,
            IReadOnlyList<LocationSaveData> locations,
            IReadOnlyList<EdgeSaveData> edges)
        {
            StartLocationId = startLocationId;
            CurrentLocationId = currentLocationId;
            Stamina = stamina;
            StaminaMax = staminaMax;
            StartingStamina = startingStamina;
            Day = day;
            Hours = hours;
            DayStartHour = dayStartHour;
            NightStartHour = nightStartHour;
            Weather = weather;
            Locations = locations;
            Edges = edges;
        }

        /// <summary>Стартовая локация.</summary>
        public string StartLocationId { get; }

        /// <summary>Текущая локация.</summary>
        public string CurrentLocationId { get; }

        /// <summary>Текущая выносливость.</summary>
        public int Stamina { get; }

        /// <summary>Максимум выносливости.</summary>
        public int StaminaMax { get; }

        /// <summary>Стартовая выносливость новой игры.</summary>
        public int StartingStamina { get; }

        /// <summary>День.</summary>
        public int Day { get; }

        /// <summary>Часы.</summary>
        public double Hours { get; }

        /// <summary>Час начала дня.</summary>
        public double DayStartHour { get; }

        /// <summary>Час начала ночи.</summary>
        public double NightStartHour { get; }

        /// <summary>Код погоды.</summary>
        public string Weather { get; }

        /// <summary>Экземпляры локаций.</summary>
        public IReadOnlyList<LocationSaveData> Locations { get; }

        /// <summary>Рёбра переходов.</summary>
        public IReadOnlyList<EdgeSaveData> Edges { get; }

        /// <summary>Сериализует снимок в JSON-узел.</summary>
        /// <returns>Объект сохранения мира.</returns>
        public JsonValue ToJson()
        {
            var value = JsonValue.NewObject();
            value.Members["startLocationId"] = JsonValue.FromString(StartLocationId);
            value.Members["currentLocationId"] = JsonValue.FromString(CurrentLocationId);
            value.Members["stamina"] = JsonValue.FromNumber(Stamina);
            value.Members["staminaMax"] = JsonValue.FromNumber(StaminaMax);
            value.Members["startingStamina"] = JsonValue.FromNumber(StartingStamina);
            value.Members["day"] = JsonValue.FromNumber(Day);
            value.Members["hours"] = JsonValue.FromNumber(Hours);
            value.Members["dayStartHour"] = JsonValue.FromNumber(DayStartHour);
            value.Members["nightStartHour"] = JsonValue.FromNumber(NightStartHour);
            value.Members["weather"] = JsonValue.FromString(Weather);
            var locations = JsonValue.NewArray();
            foreach (var location in Locations)
            {
                locations.Items.Add(location.ToJson());
            }

            value.Members["locations"] = locations;
            var edges = JsonValue.NewArray();
            foreach (var edge in Edges)
            {
                edges.Items.Add(edge.ToJson());
            }

            value.Members["edges"] = edges;
            return value;
        }

        /// <summary>Читает снимок мира.</summary>
        /// <param name="value">Объект сохранения.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Снимок мира.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static WorldSaveData FromJson(JsonValue value, string sourcePath)
        {
            var startLocationId = DataMap.RequireString(value, "startLocationId", sourcePath);
            var currentLocationId = DataMap.RequireString(value, "currentLocationId", sourcePath);
            var stamina = DataMap.RequireInt(value, "stamina", sourcePath);
            var staminaMax = DataMap.RequireInt(value, "staminaMax", sourcePath);
            var startingStamina = DataMap.OptionalInt(value, "startingStamina", staminaMax);
            var day = DataMap.RequireInt(value, "day", sourcePath);
            var hours = DataMap.RequireDouble(value, "hours", sourcePath);
            var dayStartHour = DataMap.OptionalDouble(value, "dayStartHour") ?? 6;
            var nightStartHour = DataMap.OptionalDouble(value, "nightStartHour") ?? 22;
            var weather = DataMap.RequireString(value, "weather", sourcePath);
            if (stamina < 0 || staminaMax < 0 || stamina > staminaMax || startingStamina < 0 || day < 1 ||
                !WorldClock.IsValidHours(hours) || dayStartHour < 0 || nightStartHour > 24 ||
                dayStartHour >= nightStartHour)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "world");
            }

            var locations = ReadLocations(value, sourcePath);
            var edges = ReadEdges(value, sourcePath);
            return new WorldSaveData(
                startLocationId,
                currentLocationId,
                stamina,
                staminaMax,
                startingStamina,
                day,
                hours,
                dayStartHour,
                nightStartHour,
                weather,
                locations,
                edges);
        }

        private static IReadOnlyList<LocationSaveData> ReadLocations(
            JsonValue value,
            string sourcePath)
        {
            var result = new List<LocationSaveData>();
            if (!value.TryGet("locations", out var locations) || !locations.IsArray)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "locations");
            }

            foreach (var item in locations.Items)
            {
                if (!item.IsObject)
                {
                    throw new DataException(sourcePath, CoreErrors.JsonNotObject);
                }

                result.Add(LocationSaveData.FromJson(item, sourcePath));
            }

            return result;
        }

        private static IReadOnlyList<EdgeSaveData> ReadEdges(
            JsonValue value,
            string sourcePath)
        {
            var result = new List<EdgeSaveData>();
            if (!value.TryGet("edges", out var edges) || !edges.IsArray)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "edges");
            }

            foreach (var item in edges.Items)
            {
                if (!item.IsObject)
                {
                    throw new DataException(sourcePath, CoreErrors.JsonNotObject);
                }

                result.Add(EdgeSaveData.FromJson(item, sourcePath));
            }

            return result;
        }
    }
}
