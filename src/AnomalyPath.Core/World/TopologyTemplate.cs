#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.World
{
    /// <summary>Правила связности мира: какие роли и пути допустимы, сколько стоит переход.</summary>
    public sealed class TopologyTemplate
    {
        /// <summary>Создаёт топологию.</summary>
        /// <param name="id">Идентификатор записи.</param>
        /// <param name="transitions">Разрешённые переходы ролей.</param>
        /// <param name="routeLength">Сколько отрезков пути до точки интереса.</param>
        /// <param name="edgeLength">Вилка длины ребра.</param>
        /// <param name="costs">Стоимость по способу перехода.</param>
        /// <param name="hubFastTravel">Можно ли ходить между хабами напрямую.</param>
        /// <param name="hubFastTravelCost">Цена быстрого перехода между хабами.</param>
        /// <param name="maxBranchDepth">Предел глубины ветки от хаба.</param>
        /// <param name="initialStamina">Стартовая и максимальная выносливость.</param>
        /// <param name="dayStartHour">Час начала дня.</param>
        /// <param name="nightStartHour">Час начала ночи.</param>
        public TopologyTemplate(
            string id,
            IReadOnlyList<TransitionRule> transitions,
            IntRange routeLength,
            IntRange edgeLength,
            IReadOnlyDictionary<string, TravelCost> costs,
            bool hubFastTravel,
            TravelCost? hubFastTravelCost,
            int maxBranchDepth,
            int initialStamina,
            double dayStartHour,
            double nightStartHour)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(CoreText.Format(CoreErrors.RequiredField, nameof(id)), nameof(id));
            }

            if (maxBranchDepth < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxBranchDepth));
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
            Transitions = transitions;
            RouteLength = routeLength;
            EdgeLength = edgeLength;
            Costs = costs;
            HubFastTravel = hubFastTravel;
            HubFastTravelCost = hubFastTravelCost;
            MaxBranchDepth = maxBranchDepth;
            InitialStamina = initialStamina;
            DayStartHour = dayStartHour;
            NightStartHour = nightStartHour;
        }

        /// <summary>Идентификатор записи.</summary>
        public string Id { get; }

        /// <summary>Разрешённые переходы ролей.</summary>
        public IReadOnlyList<TransitionRule> Transitions { get; }

        /// <summary>Сколько отрезков пути до точки интереса.</summary>
        public IntRange RouteLength { get; }

        /// <summary>Вилка длины ребра.</summary>
        public IntRange EdgeLength { get; }

        /// <summary>Стоимость по способу перехода.</summary>
        public IReadOnlyDictionary<string, TravelCost> Costs { get; }

        /// <summary>Можно ли ходить между хабами напрямую.</summary>
        public bool HubFastTravel { get; }

        /// <summary>Цена быстрого перехода между хабами.</summary>
        public TravelCost? HubFastTravelCost { get; }

        /// <summary>Предел глубины ветки от хаба.</summary>
        public int MaxBranchDepth { get; }

        /// <summary>Стартовая и максимальная выносливость.</summary>
        public int InitialStamina { get; }

        /// <summary>Час начала дня.</summary>
        public double DayStartHour { get; }

        /// <summary>Час начала ночи.</summary>
        public double NightStartHour { get; }

        /// <summary>Проверяет, разрешён ли переход между ролями через способ.</summary>
        /// <param name="from">Роль исходной локации.</param>
        /// <param name="to">Роль целевой локации.</param>
        /// <param name="via">Способ перехода.</param>
        /// <returns><c>true</c>, если переход разрешён.</returns>
        public bool Allows(LocationNodeType from, LocationNodeType to, string via)
        {
            foreach (var rule in Transitions)
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
        /// <returns>Базовая стоимость ребра.</returns>
        /// <exception cref="DataException">Стоимость не задана.</exception>
        public TravelCost GetCost(string via) =>
            Costs.TryGetValue(via, out var cost)
                ? cost
                : throw new DataException(Id, CoreErrors.WorldCost, via);

        /// <summary>Читает топологию из записи данных.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Топология.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static TopologyTemplate FromJson(JsonValue value, string sourcePath)
        {
            var id = DataMap.RequireString(value, "id", sourcePath);
            var transitions = ReadTransitions(value, sourcePath);
            var routeLength = IntRange.FromJson(value, "routeLength", sourcePath);
            if (routeLength.Min < 1)
            {
                throw new DataException(sourcePath, CoreErrors.WorldRange, "routeLength");
            }

            var edgeLength = IntRange.FromJson(value, "edgeLength", sourcePath);
            if (edgeLength.Min < 1)
            {
                throw new DataException(sourcePath, CoreErrors.WorldRange, "edgeLength");
            }

            var costs = ReadCosts(value, sourcePath);
            var maxDepth = DataMap.RequireInt(value, "maxBranchDepth", sourcePath);
            if (maxDepth < 1)
            {
                throw new DataException(sourcePath, CoreErrors.WorldRange, "maxBranchDepth");
            }

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

            TravelCost? fastTravelCost = null;
            if (value.TryGet("hubFastTravelCost", out var fastTravel) && fastTravel.IsObject)
            {
                fastTravelCost = TravelCost.FromJson(fastTravel, "hubFastTravelCost", sourcePath);
            }

            foreach (var rule in transitions)
            {
                foreach (var via in rule.Via)
                {
                    if (!costs.ContainsKey(via))
                    {
                        throw new DataException(sourcePath, CoreErrors.WorldCost, via);
                    }
                }
            }

            return new TopologyTemplate(
                id,
                transitions,
                routeLength,
                edgeLength,
                costs,
                DataMap.OptionalBool(value, "hubFastTravel", false),
                fastTravelCost,
                maxDepth,
                initialStamina,
                dayStart,
                nightStart);
        }

        private static IReadOnlyList<TransitionRule> ReadTransitions(JsonValue value, string sourcePath)
        {
            var result = new List<TransitionRule>();
            if (!value.TryGet("transitions", out var transitions) || !transitions.IsArray)
            {
                throw new DataException(sourcePath, CoreErrors.MissingArrayField, "transitions");
            }

            foreach (var item in transitions.Items)
            {
                if (!item.IsObject)
                {
                    throw new DataException(sourcePath, CoreErrors.JsonNotObject);
                }

                result.Add(TransitionRule.FromJson(item, sourcePath));
            }

            return result;
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
