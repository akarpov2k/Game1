#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.World
{
    /// <summary>
    /// Правила маршрута: какие подтипы соединяются, окно генерации, возврат в хаб и рост числа веток.
    /// </summary>
    public sealed class RouteSettings
    {
        /// <summary>Создаёт правила маршрута.</summary>
        /// <param name="transitions">Разрешённые переходы.</param>
        /// <param name="lookaheadEdges">Сколько звеньев вперёд строит генератор.</param>
        /// <param name="hubReturn">Базовая цена возврата в хаб за один шаг.</param>
        /// <param name="branchOddRange">Сколько веток даёт нечётное звено; минимум ≥ 1.</param>
        /// <param name="branchEvenGrowth">Насколько чётное звено даёт больше веток.</param>
        /// <param name="branchMaxChains">Сколько цепочек локаций может быть в одной ветке.</param>
        /// <exception cref="ArgumentException">Значения выходят за границы.</exception>
        public RouteSettings(
            IReadOnlyList<TransitionRule> transitions,
            int lookaheadEdges,
            TravelCost hubReturn,
            IntRange branchOddRange,
            int branchEvenGrowth,
            int branchMaxChains)
        {
            if (transitions == null || transitions.Count == 0)
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.EmptyCollection, nameof(transitions)),
                    nameof(transitions));
            }

            if (lookaheadEdges < 1)
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.WorldRange, nameof(lookaheadEdges)),
                    nameof(lookaheadEdges));
            }

            if (branchOddRange.Min < 1)
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.WorldBranchRange, nameof(branchOddRange)),
                    nameof(branchOddRange));
            }

            if (branchEvenGrowth < 0)
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.WorldBranchRange, nameof(branchEvenGrowth)),
                    nameof(branchEvenGrowth));
            }

            if (branchMaxChains < 1)
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.WorldBranchRange, nameof(branchMaxChains)),
                    nameof(branchMaxChains));
            }

            Transitions = transitions;
            LookaheadEdges = lookaheadEdges;
            HubReturn = hubReturn ?? throw new ArgumentNullException(nameof(hubReturn));
            BranchOddRange = branchOddRange;
            BranchEvenGrowth = branchEvenGrowth;
            BranchMaxChains = branchMaxChains;
        }

        /// <summary>Разрешённые переходы между подтипами.</summary>
        public IReadOnlyList<TransitionRule> Transitions { get; }

        /// <summary>Сколько звеньев вперёд строит генератор.</summary>
        public int LookaheadEdges { get; }

        /// <summary>Базовая цена возврата в хаб за один шаг.</summary>
        public TravelCost HubReturn { get; }

        /// <summary>Сколько веток даёт нечётное звено; минимум ≥ 1 — у звена всегда есть ветка.</summary>
        public IntRange BranchOddRange { get; }

        /// <summary>Насколько чётное звено даёт больше веток, чем предыдущее.</summary>
        public int BranchEvenGrowth { get; }

        /// <summary>
        /// Сколько цепочек локаций может быть в одной ветке. Одна цепочка — `chainLength` локаций
        /// одного шаблона; продолжение идёт по таблице переходов (например здание → лаборатория).
        /// </summary>
        public int BranchMaxChains { get; }

        /// <summary>Правила перехода из шаблона в шаблон, подходящие по подтипам и ролям.</summary>
        /// <param name="from">Шаблон источника.</param>
        /// <param name="to">Шаблон назначения.</param>
        /// <returns>Список подходящих правил.</returns>
        public IReadOnlyList<TransitionRule> Rules(LocationTemplate from, LocationTemplate to)
        {
            var result = new List<TransitionRule>();
            foreach (var rule in Transitions)
            {
                if (rule.Matches(from, to))
                {
                    result.Add(rule);
                }
            }

            return result;
        }

        /// <summary>Читает правила маршрута из записи топологии.</summary>
        /// <param name="value">Запись топологии.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Правила маршрута.</returns>
        /// <exception cref="DataException">Блок отсутствует или задан неверно.</exception>
        public static RouteSettings FromJson(JsonValue value, string sourcePath)
        {
            if (!value.TryGet("route", out var route) || !route.IsObject)
            {
                throw new DataException(sourcePath, CoreErrors.MissingArrayField, "route");
            }

            var transitions = new List<TransitionRule>();
            if (!route.TryGet("transitions", out var transitionsValue) || !transitionsValue.IsArray)
            {
                throw new DataException(sourcePath, CoreErrors.MissingArrayField, "route.transitions");
            }

            foreach (var item in transitionsValue.Items)
            {
                if (!item.IsObject)
                {
                    throw new DataException(sourcePath, CoreErrors.JsonNotObject);
                }

                transitions.Add(TransitionRule.FromJson(item, sourcePath));
            }

            var lookahead = DataMap.RequireInt(route, "lookaheadEdges", sourcePath);
            if (!route.TryGet("hubReturn", out var hubReturn) || !hubReturn.IsObject)
            {
                throw new DataException(sourcePath, CoreErrors.MissingNumberField, "route.hubReturn");
            }

            var hubReturnCost = TravelCost.FromJson(hubReturn, "route.hubReturn", sourcePath);

            if (!route.TryGet("branches", out var branches) || !branches.IsObject)
            {
                throw new DataException(sourcePath, CoreErrors.MissingArrayField, "route.branches");
            }

            var oddRange = IntRange.FromJson(branches, "oddRange", sourcePath);
            if (oddRange.Min < 1)
            {
                throw new DataException(sourcePath, CoreErrors.WorldBranchRange, "route.branches.oddRange");
            }

            var evenGrowth = DataMap.RequireInt(branches, "evenGrowth", sourcePath);
            if (evenGrowth < 0)
            {
                throw new DataException(sourcePath, CoreErrors.WorldBranchRange, "route.branches.evenGrowth");
            }

            var maxChains = DataMap.RequireInt(branches, "maxChains", sourcePath);
            if (maxChains < 1)
            {
                throw new DataException(sourcePath, CoreErrors.WorldBranchRange, "route.branches.maxChains");
            }

            try
            {
                return new RouteSettings(transitions, lookahead, hubReturnCost, oddRange, evenGrowth, maxChains);
            }
            catch (ArgumentException error)
            {
                throw new DataException(sourcePath, CoreErrors.WorldRange, error.Message);
            }
        }
    }
}
