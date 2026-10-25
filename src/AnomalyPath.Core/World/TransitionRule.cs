#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.World
{
    /// <summary>
    /// Разрешённый маршрутом переход между подтипами через набор способов: подтип источника
    /// и назначения, необязательное уточнение роли и вес выбора.
    /// </summary>
    public sealed class TransitionRule
    {
        /// <summary>Создаёт правило перехода.</summary>
        /// <param name="from">Подтип исходной локации.</param>
        /// <param name="to">Подтип целевой локации.</param>
        /// <param name="via">Разрешённые способы перехода.</param>
        /// <param name="fromRole">Роль источника; <c>null</c> — любая.</param>
        /// <param name="toRole">Роль назначения; <c>null</c> — любая.</param>
        /// <param name="weight">Вес выбора пары, больше нуля.</param>
        /// <exception cref="ArgumentException">Подтип неизвестен, список способов пуст или вес не положителен.</exception>
        public TransitionRule(
            LocationSubtype from,
            LocationSubtype to,
            IReadOnlyList<string> via,
            string? fromRole = null,
            string? toRole = null,
            double weight = 1)
        {
            if (!Enum.IsDefined(typeof(LocationSubtype), from))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.WorldUnknownSubtype, from),
                    nameof(from));
            }

            if (!Enum.IsDefined(typeof(LocationSubtype), to))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.WorldUnknownSubtype, to),
                    nameof(to));
            }

            if (via == null || via.Count == 0)
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.EmptyCollection, nameof(via)),
                    nameof(via));
            }

            if (weight <= 0 || double.IsNaN(weight) || double.IsInfinity(weight))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.WorldRange, nameof(weight)),
                    nameof(weight));
            }

            From = from;
            To = to;
            FromRole = string.IsNullOrWhiteSpace(fromRole) ? null : fromRole;
            ToRole = string.IsNullOrWhiteSpace(toRole) ? null : toRole;
            Via = via;
            Weight = weight;
        }

        /// <summary>Подтип исходной локации.</summary>
        public LocationSubtype From { get; }

        /// <summary>Подтип целевой локации.</summary>
        public LocationSubtype To { get; }

        /// <summary>Роль источника; <c>null</c> — любая.</summary>
        public string? FromRole { get; }

        /// <summary>Роль назначения; <c>null</c> — любая.</summary>
        public string? ToRole { get; }

        /// <summary>Разрешённые способы перехода.</summary>
        public IReadOnlyList<string> Via { get; }

        /// <summary>Вес выбора пары.</summary>
        public double Weight { get; }

        /// <summary>Проверяет, входит ли способ перехода в правило.</summary>
        /// <param name="via">Код способа.</param>
        /// <returns><c>true</c>, если способ разрешён.</returns>
        public bool Allows(string via)
        {
            foreach (var item in Via)
            {
                if (string.Equals(item, via, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Подходит ли правило паре шаблонов: подтипы совпадают, роли не противоречат.</summary>
        /// <param name="from">Шаблон источника.</param>
        /// <param name="to">Шаблон назначения.</param>
        /// <returns><c>true</c>, если правило описывает этот переход.</returns>
        public bool Matches(LocationTemplate from, LocationTemplate to)
        {
            if (from.Subtype != From || to.Subtype != To)
            {
                return false;
            }

            if (FromRole != null && !string.Equals(FromRole, from.Role, StringComparison.Ordinal))
            {
                return false;
            }

            return ToRole == null || string.Equals(ToRole, to.Role, StringComparison.Ordinal);
        }

        /// <summary>Читает правило из записи данных.</summary>
        /// <param name="value">Запись правила.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Правило.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static TransitionRule FromJson(JsonValue value, string sourcePath)
        {
            var fromCode = DataMap.RequireString(value, "from", sourcePath);
            if (!SchemaEnum.TryParse(fromCode, out LocationSubtype from))
            {
                throw new DataException(sourcePath, CoreErrors.WorldUnknownSubtype, fromCode);
            }

            var toCode = DataMap.RequireString(value, "to", sourcePath);
            if (!SchemaEnum.TryParse(toCode, out LocationSubtype to))
            {
                throw new DataException(sourcePath, CoreErrors.WorldUnknownSubtype, toCode);
            }

            var via = DataMap.OptionalStringArray(value, "via");
            if (via.Count == 0)
            {
                throw new DataException(sourcePath, CoreErrors.MissingArrayField, "via");
            }

            foreach (var item in via)
            {
                if (!ViaRules.IsKnown(item))
                {
                    throw new DataException(sourcePath, CoreErrors.WorldUnknownVia, item);
                }
            }

            var weight = DataMap.OptionalDouble(value, "weight") ?? 1;
            try
            {
                return new TransitionRule(
                    from,
                    to,
                    via,
                    DataMap.OptionalString(value, "fromRole"),
                    DataMap.OptionalString(value, "toRole"),
                    weight);
            }
            catch (ArgumentException error)
            {
                throw new DataException(sourcePath, CoreErrors.WorldRange, error.Message);
            }
        }
    }
}
