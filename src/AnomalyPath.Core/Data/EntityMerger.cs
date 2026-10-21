#nullable enable
using AnomalyPath.Core.Json;

namespace AnomalyPath.Core.Data
{
    /// <summary>
    /// Слияние данных по правилам схем: объекты — глубоко, массивы — заменой,
    /// скаляры — перекрытием, <c>null</c> — удаление поля.
    /// </summary>
    public static class EntityMerger
    {
        /// <summary>Сливает наложение поверх основы.</summary>
        /// <param name="parent">Базовое значение.</param>
        /// <param name="overlay">Наложение.</param>
        /// <returns>Новое значение, исходные не меняются.</returns>
        public static JsonValue Merge(JsonValue parent, JsonValue overlay)
        {
            if (!parent.IsObject || !overlay.IsObject)
            {
                return Clone(overlay);
            }

            var result = Clone(parent);
            foreach (var pair in overlay.Members)
            {
                if (pair.Value.Kind == JsonKind.Null)
                {
                    result.Members.Remove(pair.Key);
                    continue;
                }

                if (result.Members.TryGetValue(pair.Key, out var existing) &&
                    existing.IsObject && pair.Value.IsObject)
                {
                    result.Members[pair.Key] = Merge(existing, pair.Value);
                }
                else
                {
                    result.Members[pair.Key] = Clone(pair.Value);
                }
            }

            return result;
        }

        /// <summary>Глубокая копия значения.</summary>
        /// <param name="value">Исходное значение.</param>
        /// <returns>Независимая копия.</returns>
        public static JsonValue Clone(JsonValue value)
        {
            switch (value.Kind)
            {
                case JsonKind.Null: return JsonValue.Null();
                case JsonKind.Bool: return JsonValue.FromBool(value.Bool);
                case JsonKind.Number: return JsonValue.FromNumber(value.Number);
                case JsonKind.String: return JsonValue.FromString(value.Text);
                case JsonKind.Array:
                    var array = JsonValue.NewArray();
                    foreach (var item in value.Items)
                    {
                        array.Items.Add(Clone(item));
                    }

                    return array;
                default:
                    var obj = JsonValue.NewObject();
                    foreach (var pair in value.Members)
                    {
                        obj.Members[pair.Key] = Clone(pair.Value);
                    }

                    return obj;
            }
        }
    }
}
