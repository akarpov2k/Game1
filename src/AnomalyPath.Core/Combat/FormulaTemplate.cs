#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Combat
{
    /// <summary>Формула из данных: выражение, число или таблица с линейной интерполяцией.</summary>
    public sealed class FormulaTemplate
    {
        private readonly FormulaExpression? _expression;
        private readonly double _value;
        private readonly IReadOnlyList<TablePoint> _table;

        private FormulaTemplate(
            string id,
            IReadOnlyList<string> variables,
            FormulaExpression? expression,
            double value,
            IReadOnlyList<TablePoint> table,
            string result,
            double? clampMin,
            double? clampMax)
        {
            Id = id;
            Variables = variables;
            _expression = expression;
            _value = value;
            _table = table;
            Result = result;
            ClampMin = clampMin;
            ClampMax = clampMax;
            UsedNames = expression?.UsedNames ?? Array.Empty<string>();
        }

        /// <summary>Идентификатор формулы.</summary>
        public string Id { get; }

        /// <summary>Имена переменных, которые формула вправе использовать.</summary>
        public IReadOnlyList<string> Variables { get; }

        /// <summary>Имена, которые встречаются в выражении.</summary>
        public IReadOnlyCollection<string> UsedNames { get; }

        /// <summary>Тип результата: <c>int</c> или <c>float</c>.</summary>
        public string Result { get; }

        /// <summary>Нижняя граница результата, если задана.</summary>
        public double? ClampMin { get; }

        /// <summary>Верхняя граница результата, если задана.</summary>
        public double? ClampMax { get; }

        /// <summary>Читает формулу из записи данных.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Формула.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static FormulaTemplate FromJson(JsonValue value, string sourcePath)
        {
            var id = DataMap.RequireString(value, "id", sourcePath);
            var variables = ReadVariables(value, id);
            var result = DataMap.OptionalString(value, "result") ?? "int";
            var clampMin = DataMap.OptionalDouble(value, "clampMin");
            var clampMax = DataMap.OptionalDouble(value, "clampMax");

            var hasExpression = value.TryGet("expression", out var expression) && expression.Kind == JsonKind.String;
            var hasValue = value.TryGet("value", out var number) && number.Kind == JsonKind.Number;
            var hasTable = value.TryGet("table", out var table) && table.IsArray;
            var count = (hasExpression ? 1 : 0) + (hasValue ? 1 : 0) + (hasTable ? 1 : 0);
            if (count != 1)
            {
                throw new DataException(sourcePath, CoreErrors.FormulaRepresentation);
            }

            if (hasExpression)
            {
                var parsed = FormulaExpression.Parse(expression.Text, variables, sourcePath);
                return new FormulaTemplate(id, variables, parsed, 0, Array.Empty<TablePoint>(), result, clampMin, clampMax);
            }

            if (hasValue)
            {
                return new FormulaTemplate(id, variables, null, number.Number, Array.Empty<TablePoint>(), result, clampMin, clampMax);
            }

            var points = new List<TablePoint>();
            foreach (var item in table.Items)
            {
                var x = DataMap.RequireDouble(item, "x", sourcePath);
                var y = DataMap.RequireDouble(item, "y", sourcePath);
                points.Add(new TablePoint(x, y));
            }

            points.Sort((left, right) => left.X.CompareTo(right.X));
            return new FormulaTemplate(id, variables, null, 0, points, result, clampMin, clampMax);
        }

        /// <summary>Вычисляет формулу.</summary>
        /// <param name="variables">Значения переменных.</param>
        /// <param name="constants">Константы баланса.</param>
        /// <param name="defaultMin">Нижняя граница, если своя не задана.</param>
        /// <param name="defaultMax">Верхняя граница, если своя не задана.</param>
        /// <returns>Округлённый результат.</returns>
        public double Evaluate(
            IReadOnlyDictionary<string, double> variables,
            IReadOnlyDictionary<string, double> constants,
            double? defaultMin = null,
            double? defaultMax = null)
        {
            double raw;
            if (_expression != null)
            {
                raw = _expression.Evaluate(variables, constants);
            }
            else if (_table.Count > 0)
            {
                raw = Interpolate(variables, constants);
            }
            else
            {
                raw = _value;
            }

            var min = ClampMin ?? defaultMin;
            var max = ClampMax ?? defaultMax;
            if (min.HasValue && raw < min.Value)
            {
                raw = min.Value;
            }

            if (max.HasValue && raw > max.Value)
            {
                raw = max.Value;
            }

            return Result == "float" ? raw : Math.Round(raw, MidpointRounding.AwayFromZero);
        }

        private double Interpolate(IReadOnlyDictionary<string, double> variables, IReadOnlyDictionary<string, double> constants)
        {
            var xName = Variables.Count > 0 ? Variables[0] : "x";
            var x = variables.TryGetValue(xName, out var variable) ? variable : constants.TryGetValue(xName, out var constant) ? constant : 0;
            if (x <= _table[0].X)
            {
                return _table[0].Y;
            }

            for (var i = 1; i < _table.Count; i++)
            {
                var previous = _table[i - 1];
                var current = _table[i];
                if (x <= current.X)
                {
                    var span = current.X - previous.X;
                    var ratio = span == 0 ? 0 : (x - previous.X) / span;
                    return previous.Y + (current.Y - previous.Y) * ratio;
                }
            }

            return _table[_table.Count - 1].Y;
        }

        private static IReadOnlyList<string> ReadVariables(JsonValue value, string sourcePath)
        {
            var names = new List<string>();
            if (!value.TryGet("variables", out var variables) || !variables.IsArray)
            {
                throw new DataException(sourcePath, CoreErrors.MissingArrayField, "variables");
            }

            foreach (var item in variables.Items)
            {
                if (item.Kind == JsonKind.String)
                {
                    names.Add(item.Text);
                }
                else if (item.IsObject && item.TryGet("name", out var name) && name.Kind == JsonKind.String)
                {
                    names.Add(name.Text);
                }
                else
                {
                    throw new DataException(sourcePath, CoreErrors.MissingStringField, "variables");
                }
            }

            return names;
        }

        /// <summary>Точка таблицы: значение <c>x</c> и результат <c>y</c>.</summary>
        public readonly struct TablePoint
        {
            /// <summary>Создаёт точку.</summary>
            /// <param name="x">Входное значение.</param>
            /// <param name="y">Результат.</param>
            public TablePoint(double x, double y)
            {
                X = x;
                Y = y;
            }

            /// <summary>Входное значение.</summary>
            public double X { get; }

            /// <summary>Результат.</summary>
            public double Y { get; }
        }
    }
}
