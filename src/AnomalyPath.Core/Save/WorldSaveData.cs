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
        /// <param name="settings">Настройки новой игры или <c>null</c> для старых сейвов.</param>
        /// <param name="traders">Состояние торговцев или <c>null</c> для старых сейвов.</param>
        /// <param name="quests">Состояние квестов или <c>null</c> для старых сейвов.</param>
        /// <param name="questSerial">Счётчик выданных квестов.</param>
        /// <param name="lastHubId">Последний посещённый хаб; <c>null</c> в старых сейвах.</param>
        /// <param name="lootSerial">Счётчик бросков лута; в старых сейвах 0.</param>
        /// <param name="stashChecks">Проверки локаций без тайника; в старых сейвах 0.</param>
        /// <param name="stashOpenings">Вскрытия тайников без джекпота; в старых сейвах 0.</param>
        /// <param name="objectSeed">Постоянная часть seed выбора объектов; <c>null</c> — новый GUID.</param>
        /// <param name="routeEdges">Сколько звеньев маршрута сгенерировано.</param>
        /// <param name="routeTailId">Id конца маршрута; <c>null</c> — маршрут не строился.</param>
        /// <param name="routeLastBranches">Веток на последнем звене.</param>
        /// <param name="routeFront">Выходы последнего звена — концы его веток.</param>
        /// <param name="routeSerial">Номер выхода из хаба: входит в seed маршрута; в сейвах до v17 0.</param>
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
            IReadOnlyList<EdgeSaveData> edges,
            NewGameSettingsSaveData? settings = null,
            IReadOnlyList<TraderSaveData>? traders = null,
            IReadOnlyList<QuestSaveData>? quests = null,
            int questSerial = 0,
            string? lastHubId = null,
            int lootSerial = 0,
            int stashChecks = 0,
            int stashOpenings = 0,
            string? objectSeed = null,
            int routeEdges = 0,
            string? routeTailId = null,
            int routeLastBranches = 0,
            IReadOnlyList<string>? routeFront = null,
            int routeSerial = 0)
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
            Settings = settings;
            Traders = traders ?? new List<TraderSaveData>();
            Quests = quests ?? new List<QuestSaveData>();
            QuestSerial = questSerial;
            LastHubId = lastHubId;
            LootSerial = lootSerial;
            StashChecks = stashChecks;
            StashOpenings = stashOpenings;
            ObjectSeed = string.IsNullOrWhiteSpace(objectSeed)
                ? System.Guid.NewGuid().ToString("N")
                : objectSeed!;
            RouteEdges = routeEdges;
            RouteTailId = routeTailId;
            RouteLastBranches = routeLastBranches;
            RouteFront = routeFront ?? new List<string>();
            RouteSerial = routeSerial;
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

        /// <summary>Настройки новой игры или <c>null</c>.</summary>
        public NewGameSettingsSaveData? Settings { get; }

        /// <summary>Состояние торговцев.</summary>
        public IReadOnlyList<TraderSaveData> Traders { get; }

        /// <summary>Состояние квестов.</summary>
        public IReadOnlyList<QuestSaveData> Quests { get; }

        /// <summary>Счётчик выданных квестов.</summary>
        public int QuestSerial { get; }

        /// <summary>Последний посещённый хаб; <c>null</c> в старых сейвах.</summary>
        public string? LastHubId { get; }

        /// <summary>Счётчик бросков лута; в старых сейвах 0.</summary>
        public int LootSerial { get; }

        /// <summary>Проверки локаций без тайника; в старых сейвах 0.</summary>
        public int StashChecks { get; }

        /// <summary>Вскрытия тайников без джекпота; в старых сейвах 0.</summary>
        public int StashOpenings { get; }

        /// <summary>Постоянная часть seed выбора объектов: GUID, созданный при новой игре.</summary>
        public string ObjectSeed { get; }

        /// <summary>Сколько звеньев маршрута сгенерировано.</summary>
        public int RouteEdges { get; }

        /// <summary>Id конца маршрута; <c>null</c> — маршрут не строился.</summary>
        public string? RouteTailId { get; }

        /// <summary>Веток на последнем звене.</summary>
        public int RouteLastBranches { get; }

        /// <summary>Выходы последнего звена — концы его веток; в сейвах до v16 пусто.</summary>
        public IReadOnlyList<string> RouteFront { get; }

        /// <summary>Номер выхода из хаба: входит в seed маршрута; в сейвах до v17 0.</summary>
        public int RouteSerial { get; }

        /// <summary>Сериализует снимок в JSON-узел.</summary>
        /// <returns>Объект сохранения мира.</returns>
        public JsonValue ToJson()
        {
            var value = JsonValue.NewObject();
            value.Members["startLocationId"] = JsonValue.FromString(StartLocationId);
            value.Members["currentLocationId"] = JsonValue.FromString(CurrentLocationId);
            if (LastHubId != null)
            {
                value.Members["lastHubId"] = JsonValue.FromString(LastHubId);
            }
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
            if (Settings != null)
            {
                value.Members["settings"] = Settings.ToJson();
            }

            var traders = JsonValue.NewArray();
            foreach (var trader in Traders)
            {
                traders.Items.Add(trader.ToJson());
            }

            value.Members["traders"] = traders;
            value.Members["questSerial"] = JsonValue.FromNumber(QuestSerial);
            value.Members["lootSerial"] = JsonValue.FromNumber(LootSerial);
            value.Members["stashChecks"] = JsonValue.FromNumber(StashChecks);
            value.Members["stashOpenings"] = JsonValue.FromNumber(StashOpenings);
            value.Members["objectSeed"] = JsonValue.FromString(ObjectSeed);
            value.Members["routeEdges"] = JsonValue.FromNumber(RouteEdges);
            if (RouteTailId != null)
            {
                value.Members["routeTailId"] = JsonValue.FromString(RouteTailId);
            }

            value.Members["routeLastBranches"] = JsonValue.FromNumber(RouteLastBranches);
            var routeFront = JsonValue.NewArray();
            foreach (var id in RouteFront)
            {
                routeFront.Items.Add(JsonValue.FromString(id));
            }

            value.Members["routeFront"] = routeFront;
            value.Members["routeSerial"] = JsonValue.FromNumber(RouteSerial);
            var quests = JsonValue.NewArray();
            foreach (var quest in Quests)
            {
                quests.Items.Add(quest.ToJson());
            }

            value.Members["quests"] = quests;
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
            NewGameSettingsSaveData? settings = null;
            if (value.TryGet("settings", out var settingsValue) && settingsValue.IsObject)
            {
                settings = NewGameSettingsSaveData.FromJson(settingsValue, sourcePath);
            }

            var traders = ReadTraders(value, sourcePath);
            var questSerial = DataMap.OptionalInt(value, "questSerial", 0);
            if (questSerial < 0)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "questSerial");
            }

            var quests = ReadQuests(value, sourcePath);
            var lastHubId = DataMap.OptionalString(value, "lastHubId");
            var lootSerial = DataMap.OptionalInt(value, "lootSerial", 0);
            if (lootSerial < 0)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "lootSerial");
            }

            var stashChecks = DataMap.OptionalInt(value, "stashChecks", 0);
            var stashOpenings = DataMap.OptionalInt(value, "stashOpenings", 0);
            if (stashChecks < 0 || stashOpenings < 0)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "stashChecks");
            }

            var objectSeed = DataMap.OptionalString(value, "objectSeed");
            var routeEdges = DataMap.OptionalInt(value, "routeEdges", 0);
            var routeLastBranches = DataMap.OptionalInt(value, "routeLastBranches", 0);
            if (routeEdges < 0 || routeLastBranches < 0)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "routeEdges");
            }

            var routeTailId = DataMap.OptionalString(value, "routeTailId");
            var routeSerial = DataMap.OptionalInt(value, "routeSerial", 0);
            if (routeSerial < 0)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "routeSerial");
            }
            var routeFront = new List<string>();
            if (value.TryGet("routeFront", out var routeFrontValue) && routeFrontValue.IsArray)
            {
                foreach (var item in routeFrontValue.Items)
                {
                    if (item.Kind != JsonKind.String)
                    {
                        throw new DataException(sourcePath, CoreErrors.SaveMissingField, "routeFront");
                    }

                    routeFront.Add(item.Text);
                }
            }

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
                edges,
                settings,
                traders,
                quests,
                questSerial,
                lastHubId,
                lootSerial,
                stashChecks,
                stashOpenings,
                objectSeed,
                routeEdges,
                routeTailId,
                routeLastBranches,
                routeFront,
                routeSerial);
        }

        private static IReadOnlyList<QuestSaveData> ReadQuests(JsonValue value, string sourcePath)
        {
            var result = new List<QuestSaveData>();
            if (!value.TryGet("quests", out var quests) || !quests.IsArray)
            {
                return result;
            }

            foreach (var item in quests.Items)
            {
                if (!item.IsObject)
                {
                    throw new DataException(sourcePath, CoreErrors.JsonNotObject);
                }

                result.Add(QuestSaveData.FromJson(item, sourcePath));
            }

            return result;
        }

        private static IReadOnlyList<TraderSaveData> ReadTraders(JsonValue value, string sourcePath)
        {
            var result = new List<TraderSaveData>();
            if (!value.TryGet("traders", out var traders) || !traders.IsArray)
            {
                return result;
            }

            foreach (var item in traders.Items)
            {
                if (!item.IsObject)
                {
                    throw new DataException(sourcePath, CoreErrors.JsonNotObject);
                }

                result.Add(TraderSaveData.FromJson(item, sourcePath));
            }

            return result;
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
