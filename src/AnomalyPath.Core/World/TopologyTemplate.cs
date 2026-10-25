#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.World
{
    /// <summary>Правила связности мира: маршрут, стоимость шага и начало суток.</summary>
    public sealed class TopologyTemplate
    {
        /// <summary>Создаёт топологию.</summary>
        /// <param name="id">Идентификатор записи.</param>
        /// <param name="route">Правила маршрута: переходы, окно, ветки, возврат в хаб.</param>
        /// <param name="costs">Стоимость шага по способу перехода.</param>
        /// <param name="initialStamina">Стартовая и максимальная выносливость.</param>
        /// <param name="dayStartHour">Час начала дня.</param>
        /// <param name="nightStartHour">Час начала ночи.</param>
        /// <exception cref="ArgumentException">Поля заданы неверно.</exception>
        public TopologyTemplate(
            string id,
            RouteSettings route,
            IReadOnlyDictionary<string, TravelCost> costs,
            int initialStamina,
            double dayStartHour,
            double nightStartHour)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(CoreText.Format(CoreErrors.RequiredField, nameof(id)), nameof(id));
            }

            if (route == null)
            {
                throw new ArgumentNullException(nameof(route));
            }

            if (costs == null || costs.Count == 0)
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.EmptyCollection, nameof(costs)),
                    nameof(costs));
            }

            if (initialStamina < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialStamina));
            }

            if (dayStartHour < 0 || nightStartHour > 24 || dayStartHour >= nightStartHour)
            {
                throw new ArgumentOutOfRangeException(nameof(dayStartHour));
            }

            Id = id;
            Route = route;
            Costs = costs;
            InitialStamina = initialStamina;
            DayStartHour = dayStartHour;
            NightStartHour = nightStartHour;
        }

        /// <summary>Идентификатор записи.</summary>
        public string Id { get; }

        /// <summary>Правила маршрута.</summary>
        public RouteSettings Route { get; }

        /// <summary>Стоимость шага по способу перехода.</summary>
        public IReadOnlyDictionary<string, TravelCost> Costs { get; }

        /// <summary>Стартовая и максимальная выносливость.</summary>
        public int InitialStamina { get; }

        /// <summary>Час начала дня.</summary>
        public double DayStartHour { get; }

        /// <summary>Час начала ночи.</summary>
        public double NightStartHour { get; }

        /// <summary>Проверяет, разрешён ли переход между подтипами через способ.</summary>
        /// <param name="from">Подтип исходной локации.</param>
        /// <param name="to">Подтип целевой локации.</param>
        /// <param name="via">Способ перехода.</param>
        /// <returns><c>true</c>, если переход разрешён.</returns>
        public bool Allows(LocationSubtype from, LocationSubtype to, string via)
        {
            foreach (var rule in Route.Transitions)
            {
                if (rule.From == from && rule.To == to && rule.Allows(via))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Возвращает стоимость способа перехода.</summary>
        /// <param name="via">Способ перехода.</param>
        /// <returns>Базовая стоимость шага.</returns>
        /// <exception cref="DataException">Стоимость не задана.</exception>
        public TravelCost GetCost(string via) =>
            Costs.TryGetValue(via, out var cost)
                ? cost
                : throw new DataException(Id, CoreErrors.WorldCost, via);

        /// <summary>Цена возврата в хаб с шага маршрута.</summary>
        /// <param name="routeStep">Сколько шагов от хаба до локации.</param>
        /// <returns>Стоимость возврата; на шаге 0 — нулевая.</returns>
        public TravelCost HubReturnCost(int routeStep) =>
            routeStep <= 0 ? TravelCost.Zero : Route.HubReturn.Scale(routeStep);

        /// <summary>Читает топологию из записи данных.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Топология.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static TopologyTemplate FromJson(JsonValue value, string sourcePath)
        {
            var id = DataMap.RequireString(value, "id", sourcePath);
            var route = RouteSettings.FromJson(value, sourcePath);
            var costs = ReadCosts(value, sourcePath);

            var initialStamina = DataMap.RequireInt(value, "initialStamina", sourcePath);
            if (initialStamina < 0)
            {
                throw new DataException(sourcePath, CoreErrors.WorldRange, "initialStamina");
            }

            var dayStart = DataMap.RequireDouble(value, "dayStartHour", sourcePath);
            var nightStart = DataMap.RequireDouble(value, "nightStartHour", sourcePath);
            if (dayStart < 0 || nightStart > 24 || dayStart >= nightStart)
            {
                throw new DataException(sourcePath, CoreErrors.WorldRange, "dayStartHour");
            }

            foreach (var rule in route.Transitions)
            {
                foreach (var via in rule.Via)
                {
                    if (!costs.ContainsKey(via))
                    {
                        throw new DataException(sourcePath, CoreErrors.WorldCost, via);
                    }
                }
            }

            try
            {
                return new TopologyTemplate(id, route, costs, initialStamina, dayStart, nightStart);
            }
            catch (ArgumentException error)
            {
                throw new DataException(sourcePath, CoreErrors.WorldRange, error.Message);
            }
        }

        private static IReadOnlyDictionary<string, TravelCost> ReadCosts(JsonValue value, string sourcePath)
        {
            var result = new Dictionary<string, TravelCost>(StringComparer.Ordinal);
            if (!value.TryGet("costs", out var costs) || !costs.IsObject)
            {
                throw new DataException(sourcePath, CoreErrors.MissingArrayField, "costs");
            }

            foreach (var pair in costs.Entries)
            {
                if (!ViaRules.IsKnown(pair.Key))
                {
                    throw new DataException(sourcePath, CoreErrors.WorldUnknownVia, pair.Key);
                }

                result[pair.Key] = TravelCost.FromJson(pair.Value, pair.Key, sourcePath);
            }

            return result;
        }
    }
}
