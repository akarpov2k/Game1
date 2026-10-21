using System;
using System.Globalization;
using System.Text;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Json
{
    /// <summary>Разбор JSON без внешних пакетов: маленький, предсказуемый, с локализованными ошибками.</summary>
    public static class JsonParser
    {
        /// <summary>Разбирает текст JSON в дерево значений.</summary>
        /// <param name="text">Текст документа.</param>
        /// <param name="path">Путь для сообщений об ошибках.</param>
        /// <returns>Корневое значение.</returns>
        /// <exception cref="DataException">Документ пуст или содержит синтаксическую ошибку.</exception>
        public static JsonValue Parse(string text, string path = "<json>")
        {
            if (text == null)
            {
                throw new DataException(path, CoreErrors.JsonEmpty);
            }

            text = text.TrimStart('\uFEFF');
            var index = 0;
            var value = ParseValue(text, ref index, path);
            SkipWhitespace(text, ref index);

            if (index != text.Length)
            {
                throw Error(text, index, path, CoreErrors.JsonTrailing);
            }

            return value;
        }

        private static JsonValue ParseValue(string text, ref int index, string path)
        {
            SkipWhitespace(text, ref index);
            if (index >= text.Length)
            {
                throw Error(text, index, path, CoreErrors.JsonUnexpectedEnd);
            }

            var ch = text[index];
            switch (ch)
            {
                case '{': return ParseObject(text, ref index, path);
                case '[': return ParseArray(text, ref index, path);
                case '"': return JsonValue.FromString(ParseString(text, ref index, path));
                case 't': Expect(text, ref index, path, "true"); return JsonValue.FromBool(true);
                case 'f': Expect(text, ref index, path, "false"); return JsonValue.FromBool(false);
                case 'n': Expect(text, ref index, path, "null"); return JsonValue.Null();
                default: return JsonValue.FromNumber(ParseNumber(text, ref index, path));
            }
        }

        private static JsonValue ParseObject(string text, ref int index, string path)
        {
            var result = JsonValue.NewObject();
            index++;
            SkipWhitespace(text, ref index);

            if (index < text.Length && text[index] == '}')
            {
                index++;
                return result;
            }

            while (true)
            {
                SkipWhitespace(text, ref index);
                if (index >= text.Length || text[index] != '"')
                {
                    throw Error(text, index, path, CoreErrors.JsonExpectedFieldName);
                }

                var key = ParseString(text, ref index, path);
                SkipWhitespace(text, ref index);
                if (index >= text.Length || text[index] != ':')
                {
                    throw Error(text, index, path, CoreErrors.JsonExpectedColon);
                }

                index++;
                result.Members[key] = ParseValue(text, ref index, path);

                SkipWhitespace(text, ref index);
                if (index < text.Length && text[index] == ',')
                {
                    index++;
                    continue;
                }

                if (index < text.Length && text[index] == '}')
                {
                    index++;
                    return result;
                }

                throw Error(text, index, path, CoreErrors.JsonExpectedCommaOrBrace);
            }
        }

        private static JsonValue ParseArray(string text, ref int index, string path)
        {
            var result = JsonValue.NewArray();
            index++;
            SkipWhitespace(text, ref index);

            if (index < text.Length && text[index] == ']')
            {
                index++;
                return result;
            }

            while (true)
            {
                result.Items.Add(ParseValue(text, ref index, path));
                SkipWhitespace(text, ref index);

                if (index < text.Length && text[index] == ',')
                {
                    index++;
                    continue;
                }

                if (index < text.Length && text[index] == ']')
                {
                    index++;
                    return result;
                }

                throw Error(text, index, path, CoreErrors.JsonExpectedCommaOrBracket);
            }
        }

        private static string ParseString(string text, ref int index, string path)
        {
            index++;
            var builder = new StringBuilder();

            while (index < text.Length)
            {
                var ch = text[index++];
                if (ch == '"')
                {
                    return builder.ToString();
                }

                if (ch != '\\')
                {
                    builder.Append(ch);
                    continue;
                }

                if (index >= text.Length)
                {
                    throw Error(text, index, path, CoreErrors.JsonUnterminatedEscape);
                }

                var escape = text[index++];
                switch (escape)
                {
                    case '"': builder.Append('"'); break;
                    case '\\': builder.Append('\\'); break;
                    case '/': builder.Append('/'); break;
                    case 'b': builder.Append('\b'); break;
                    case 'f': builder.Append('\f'); break;
                    case 'n': builder.Append('\n'); break;
                    case 'r': builder.Append('\r'); break;
                    case 't': builder.Append('\t'); break;
                    case 'u':
                        if (index + 4 > text.Length)
                        {
                            throw Error(text, index, path, CoreErrors.JsonBadUnicode);
                        }

                        var hex = text.Substring(index, 4);
                        builder.Append((char)int.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                        index += 4;
                        break;
                    default:
                        throw Error(text, index, path, CoreErrors.JsonBadEscape, escape);
                }
            }

            throw Error(text, index, path, CoreErrors.JsonUnterminatedString);
        }

        private static double ParseNumber(string text, ref int index, string path)
        {
            var start = index;
            while (index < text.Length && "0123456789+-.eE".IndexOf(text[index]) >= 0)
            {
                index++;
            }

            var token = text.Substring(start, index - start);
            if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                throw Error(text, start, path, CoreErrors.JsonBadNumber, token);
            }

            return value;
        }

        private static void Expect(string text, ref int index, string path, string literal)
        {
            if (index + literal.Length > text.Length ||
                string.CompareOrdinal(text, index, literal, 0, literal.Length) != 0)
            {
                throw Error(text, index, path, CoreErrors.JsonExpectedLiteral, literal);
            }

            index += literal.Length;
        }

        private static void SkipWhitespace(string text, ref int index)
        {
            while (index < text.Length && char.IsWhiteSpace(text[index]))
            {
                index++;
            }
        }

        private static DataException Error(string text, int index, string path, string code, params object[] args)
        {
            var line = 1;
            var column = 1;
            for (var i = 0; i < index && i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    line++;
                    column = 1;
                }
                else
                {
                    column++;
                }
            }

            var inner = CoreText.Format(code, args);
            return new DataException(path, CoreErrors.JsonAtPosition, inner, line, column);
        }
    }
}
