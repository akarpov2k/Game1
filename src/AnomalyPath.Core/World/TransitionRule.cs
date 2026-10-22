#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.World
{
    /// <summary>Разрешённый топологией переход между ролями через набор способов.</summary>
    public sealed class TransitionRule
    {
        /// <summary>Создаёт правило.</summary>
        /// <param name="from">Роль исходной локации.</param>
        /// <param name="to">Роль целевой локации.</param>
        /// <param name="via">Разрешённые способы перехода.</param>
        /// <exception cref="ArgumentException">Роль неизвестна или список пуст.</exception>
        public TransitionRule(LocationNodeType from, LocationNodeType to, IReadOnlyList<string> via)
        {
            if (!Enum.IsDefined(typeof(LocationNodeType), from))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.WorldUnknownNodeType, from),
                    nameof(from));
            }

            if (!Enum.IsDefined(typeof(LocationNodeType), to))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.WorldUnknownNodeType, to),
                    nameof(to));
            }

            if (via == null || via.Count == 0)
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.EmptyCollection, nameof(via)),
                    nameof(via));
            }

            From = from;
            To = to;
            Via = via;
        }

        /// <summary>Роль исходной локации.</summary>
        public LocationNodeType From { get; }

        /// <summary>Роль целевой локации.</summary>
        public LocationNodeType To { get; }

        /// <summary>Разрешённые способы перехода.</summary>
        public IReadOnlyList<string> Via { get; }

        /// <summary>Проверяет, входит ли способ перехода в правило.</summary>
        /// <param name="via">Способ перехода.</param>
        /// <returns><c>true</c>, если способ разрешён.</returns>
        public bool Allows(string via)
        {
            foreach (var item in Via)
            {
                if (item == via)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Читает правило из записи данных.</summary>
        /// <param name="value">Запись правила.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Правило.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static TransitionRule FromJson(JsonValue value, string sourcePath)
        {
            var fromCode = DataMap.RequireString(value, "from", sourcePath);
            if (!SchemaEnum.TryParse(fromCode, out LocationNodeType from))
            {
                throw new DataException(sourcePath, CoreErrors.WorldUnknownNodeType, fromCode);
            }

            var toCode = DataMap.RequireString(value, "to", sourcePath);
            if (!SchemaEnum.TryParse(toCode, out LocationNodeType to))
            {
                throw new DataException(sourcePath, CoreErrors.WorldUnknownNodeType, toCode);
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

            return new TransitionRule(from, to, via);
        }
    }
}
