#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AnomalyPath.Core.Json
{
    /// <summary>Детерминированный писатель JSON: ключи объектов сортируются, числа — InvariantCulture.</summary>
    public static class JsonWriter
    {
        /// <summary>Сериализует значение в текст.</summary>
        /// <param name="value">Значение.</param>
        /// <param name="indented">Форматировать с отступами в два пробела.</param>
        /// <returns>JSON-текст.</returns>
        /// <exception cref="ArgumentNullException">Значение не задано.</exception>
        public static string Write(JsonValue value, bool indented = true)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var builder = new StringBuilder();
            WriteValue(builder, value, indented, 0);
            return builder.ToString();
        }

        private static void WriteValue(StringBuilder builder, JsonValue value, bool indented, int depth)
        {
            switch (value.Kind)
            {
                case JsonKind.Null:
                    builder.Append("null");
                    break;
                case JsonKind.Bool:
                    builder.Append(value.Bool ? "true" : "false");
                    break;
                case JsonKind.Number:
                    builder.Append(value.Number.ToString("R", CultureInfo.InvariantCulture));
                    break;
                case JsonKind.String:
                    WriteString(builder, value.Text);
                    break;
                case JsonKind.Array:
                    WriteArray(builder, value, indented, depth);
                    break;
                case JsonKind.Object:
                    WriteObject(builder, value, indented, depth);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        private static void WriteArray(StringBuilder builder, JsonValue value, bool indented, int depth)
        {
            builder.Append('[');
            if (value.Items.Count == 0)
            {
                builder.Append(']');
                return;
            }

            for (var index = 0; index < value.Items.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(',');
                }

                NewLine(builder, indented, depth + 1);
                WriteValue(builder, value.Items[index], indented, depth + 1);
            }

            NewLine(builder, indented, depth);
            builder.Append(']');
        }

        private static void WriteObject(StringBuilder builder, JsonValue value, bool indented, int depth)
        {
            builder.Append('{');
            if (value.Members.Count == 0)
            {
                builder.Append('}');
                return;
            }

            var keys = new List<string>(value.Members.Keys);
            keys.Sort(StringComparer.Ordinal);
            for (var index = 0; index < keys.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(',');
                }

                NewLine(builder, indented, depth + 1);
                WriteString(builder, keys[index]);
                builder.Append(indented ? ": " : ":");
                WriteValue(builder, value.Members[keys[index]], indented, depth + 1);
            }

            NewLine(builder, indented, depth);
            builder.Append('}');
        }

        private static void NewLine(StringBuilder builder, bool indented, int depth)
        {
            if (!indented)
            {
                return;
            }

            builder.Append('\n');
            builder.Append(' ', depth * 2);
        }

        private static void WriteString(StringBuilder builder, string text)
        {
            builder.Append('"');
            foreach (var character in text)
            {
                switch (character)
                {
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        if (character < ' ')
                        {
                            builder.Append("\\u");
                            builder.Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            builder.Append(character);
                        }

                        break;
                }
            }

            builder.Append('"');
        }
    }
}
