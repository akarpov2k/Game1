#nullable enable
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Json;

namespace AnomalyPath.Core.Data
{
    /// <summary>Строгие читалки полей: ошибка сообщает код, путь и имя поля.</summary>
    public static class DataMap
    {
        /// <summary>Читает обязательное строковое поле.</summary>
        /// <param name="value">Объект сущности.</param>
        /// <param name="field">Имя поля.</param>
        /// <param name="path">Путь для сообщения.</param>
        /// <returns>Значение поля.</returns>
        /// <exception cref="DataException">Поле отсутствует или не строка.</exception>
        public static string RequireString(JsonValue value, string field, string path)
        {
            if (!value.TryGet(field, out var fieldValue) || fieldValue.Kind != JsonKind.String ||
                string.IsNullOrWhiteSpace(fieldValue.Text))
            {
                throw new DataException(path, CoreErrors.MissingStringField, field);
            }

            return fieldValue.Text;
        }

        /// <summary>Читает необязательное строковое поле.</summary>
        /// <param name="value">Объект сущности.</param>
        /// <param name="field">Имя поля.</param>
        /// <returns>Значение или <c>null</c>.</returns>
        public static string? OptionalString(JsonValue value, string field) =>
            value.TryGet(field, out var fieldValue) && fieldValue.Kind == JsonKind.String
                ? fieldValue.Text
                : null;

        /// <summary>Читает обязательное целое поле.</summary>
        /// <param name="value">Объект сущности.</param>
        /// <param name="field">Имя поля.</param>
        /// <param name="path">Путь для сообщения.</param>
        /// <returns>Значение поля.</returns>
        /// <exception cref="DataException">Поле отсутствует или не число.</exception>
        public static int RequireInt(JsonValue value, string field, string path)
        {
            if (!value.TryGet(field, out var fieldValue) || fieldValue.Kind != JsonKind.Number)
            {
                throw new DataException(path, CoreErrors.MissingNumberField, field);
            }

            return (int)System.Math.Round(fieldValue.Number);
        }

        /// <summary>Читает обязательное числовое поле.</summary>
        /// <param name="value">Объект сущности.</param>
        /// <param name="field">Имя поля.</param>
        /// <param name="path">Путь для сообщения.</param>
        /// <returns>Значение поля.</returns>
        /// <exception cref="DataException">Поле отсутствует или не число.</exception>
        public static double RequireDouble(JsonValue value, string field, string path)
        {
            if (!value.TryGet(field, out var fieldValue) || fieldValue.Kind != JsonKind.Number)
            {
                throw new DataException(path, CoreErrors.MissingNumberField, field);
            }

            return fieldValue.Number;
        }

        /// <summary>Читает необязательное числовое поле.</summary>
        /// <param name="value">Объект сущности.</param>
        /// <param name="field">Имя поля.</param>
        /// <returns>Значение или <c>null</c>.</returns>
        public static double? OptionalDouble(JsonValue value, string field) =>
            value.TryGet(field, out var fieldValue) && fieldValue.Kind == JsonKind.Number
                ? fieldValue.Number
                : null;

        /// <summary>Читает необязательное целое поле с значением по умолчанию.</summary>
        /// <param name="value">Объект сущности.</param>
        /// <param name="field">Имя поля.</param>
        /// <param name="fallback">Значение, если поля нет.</param>
        /// <returns>Значение поля или <paramref name="fallback"/>.</returns>
        public static int OptionalInt(JsonValue value, string field, int fallback) =>
            value.TryGet(field, out var fieldValue) && fieldValue.Kind == JsonKind.Number
                ? (int)System.Math.Round(fieldValue.Number)
                : fallback;

        /// <summary>Читает необязательный флаг с значением по умолчанию.</summary>
        /// <param name="value">Объект сущности.</param>
        /// <param name="field">Имя поля.</param>
        /// <param name="fallback">Значение, если поле отсутствует.</param>
        /// <returns>Значение поля или <paramref name="fallback"/>.</returns>
        public static bool OptionalBool(JsonValue value, string field, bool fallback) =>
            value.TryGet(field, out var fieldValue) && fieldValue.Kind == JsonKind.Bool
                ? fieldValue.Bool
                : fallback;

        /// <summary>Читает пару целых чисел.</summary>
        /// <param name="value">Объект сущности.</param>
        /// <param name="field">Имя поля.</param>
        /// <param name="path">Путь для сообщения.</param>
        /// <returns>Два числа.</returns>
        /// <exception cref="DataException">Поле не массив из двух чисел.</exception>
        public static System.Tuple<int, int> RequireIntPair(JsonValue value, string field, string path)
        {
            if (!value.TryGet(field, out var fieldValue) || !fieldValue.IsArray || fieldValue.Items.Count != 2)
            {
                throw new DataException(path, CoreErrors.NumberPairExpected, field);
            }

            var first = fieldValue.Items[0];
            var second = fieldValue.Items[1];
            if (first.Kind != JsonKind.Number || second.Kind != JsonKind.Number)
            {
                throw new DataException(path, CoreErrors.NumberPairExpected, field);
            }

            return System.Tuple.Create((int)System.Math.Round(first.Number), (int)System.Math.Round(second.Number));
        }

        /// <summary>Читает необязательный массив строк.</summary>
        /// <param name="value">Объект сущности.</param>
        /// <param name="field">Имя поля.</param>
        /// <returns>Список строк; пустой, если поля нет.</returns>
        public static System.Collections.Generic.IReadOnlyList<string> OptionalStringArray(JsonValue value, string field)
        {
            var result = new System.Collections.Generic.List<string>();
            if (!value.TryGet(field, out var fieldValue) || !fieldValue.IsArray)
            {
                return result;
            }

            foreach (var item in fieldValue.Items)
            {
                if (item.Kind == JsonKind.String)
                {
                    result.Add(item.Text);
                }
            }

            return result;
        }
    }
}
