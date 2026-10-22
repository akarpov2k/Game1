#nullable enable
using System.Collections.Generic;

namespace AnomalyPath.Core.Json
{
    /// <summary>
    /// Минимальный узел JSON: без зависимостей и рефлексии, чтобы одинаково работать
    /// в Unity и в отдельном консольном тесте.
    /// </summary>
    public sealed class JsonValue
    {
        private JsonValue(JsonKind kind)
        {
            Kind = kind;
        }

        /// <summary>Вид значения.</summary>
        public JsonKind Kind { get; }

        /// <summary>Логическое значение, если вид — <see cref="JsonKind.Bool"/>.</summary>
        public bool Bool { get; private set; }

        /// <summary>Число, если вид — <see cref="JsonKind.Number"/>.</summary>
        public double Number { get; private set; }

        /// <summary>Строка, если вид — <see cref="JsonKind.String"/>.</summary>
        public string Text { get; private set; } = string.Empty;

        /// <summary>Элементы массива, если вид — <see cref="JsonKind.Array"/>.</summary>
        public List<JsonValue> Items { get; } = new List<JsonValue>();

        /// <summary>Поля объекта, если вид — <see cref="JsonKind.Object"/>.</summary>
        public Dictionary<string, JsonValue> Members { get; } = new Dictionary<string, JsonValue>();

        /// <summary>Создаёт пустое значение.</summary>
        /// <returns>Значение <see cref="JsonKind.Null"/>.</returns>
        public static JsonValue Null() => new JsonValue(JsonKind.Null);

        /// <summary>Создаёт логическое значение.</summary>
        /// <param name="value">Значение.</param>
        /// <returns>Узел с видом <see cref="JsonKind.Bool"/>.</returns>
        public static JsonValue FromBool(bool value) => new JsonValue(JsonKind.Bool) { Bool = value };

        /// <summary>Создаёт числовое значение.</summary>
        /// <param name="value">Значение.</param>
        /// <returns>Узел с видом <see cref="JsonKind.Number"/>.</returns>
        public static JsonValue FromNumber(double value) => new JsonValue(JsonKind.Number) { Number = value };

        /// <summary>Создаёт строковое значение.</summary>
        /// <param name="value">Значение.</param>
        /// <returns>Узел с видом <see cref="JsonKind.String"/>.</returns>
        public static JsonValue FromString(string value) => new JsonValue(JsonKind.String) { Text = value };

        /// <summary>Создаёт пустой массив.</summary>
        /// <returns>Узел с видом <see cref="JsonKind.Array"/>.</returns>
        public static JsonValue NewArray() => new JsonValue(JsonKind.Array);

        /// <summary>Создаёт пустой объект.</summary>
        /// <returns>Узел с видом <see cref="JsonKind.Object"/>.</returns>
        public static JsonValue NewObject() => new JsonValue(JsonKind.Object);

        /// <summary>Признак объекта.</summary>
        public bool IsObject => Kind == JsonKind.Object;

        /// <summary>Признак массива.</summary>
        public bool IsArray => Kind == JsonKind.Array;

        /// <summary>Пробует получить поле объекта.</summary>
        /// <param name="key">Имя поля.</param>
        /// <param name="value">Значение поля или пустое значение.</param>
        /// <returns><c>true</c>, если поле есть.</returns>
        public bool TryGet(string key, out JsonValue value)
        {
            if (Kind == JsonKind.Object && Members.TryGetValue(key, out var found))
            {
                value = found;
                return true;
            }

            value = Null();
            return false;
        }

        /// <summary>Возвращает поле объекта или пустое значение.</summary>
        /// <param name="key">Имя поля.</param>
        /// <returns>Значение или <see cref="Null"/>.</returns>
        public JsonValue Get(string key) => TryGet(key, out var value) ? value : Null();

        /// <summary>Поля объекта.</summary>
        public IEnumerable<KeyValuePair<string, JsonValue>> Entries => Members;
    }
}
