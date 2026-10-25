#nullable enable
namespace AnomalyPath.Core.Localization
{
    /// <summary>Ключи служебных текстов. Сами тексты живут в файлах ресурсов, не в коде.</summary>
    public static partial class CoreErrors
    {
        /// <summary>Документ JSON пуст.</summary>
        public const string JsonEmpty = "error.json_empty";

        /// <summary>После значения остались лишние символы.</summary>
        public const string JsonTrailing = "error.json_trailing";

        /// <summary>Неожиданный конец документа.</summary>
        public const string JsonUnexpectedEnd = "error.json_unexpected_end";

        /// <summary>Ожидалось имя поля в кавычках.</summary>
        public const string JsonExpectedFieldName = "error.json_expected_field_name";

        /// <summary>Ожидалось двоеточие после имени поля.</summary>
        public const string JsonExpectedColon = "error.json_expected_colon";

        /// <summary>Ожидалась запятая или закрывающая скобка объекта.</summary>
        public const string JsonExpectedCommaOrBrace = "error.json_expected_comma_or_brace";

        /// <summary>Ожидалась запятая или закрывающая скобка массива.</summary>
        public const string JsonExpectedCommaOrBracket = "error.json_expected_comma_or_bracket";

        /// <summary>Незавершённая строка.</summary>
        public const string JsonUnterminatedString = "error.json_unterminated_string";

        /// <summary>Незавершённая escape-последовательность.</summary>
        public const string JsonUnterminatedEscape = "error.json_unterminated_escape";

        /// <summary>Неизвестная escape-последовательность.</summary>
        public const string JsonBadEscape = "error.json_bad_escape";

        /// <summary>Обрезанная escape-последовательность Unicode.</summary>
        public const string JsonBadUnicode = "error.json_bad_unicode";

        /// <summary>Некорректное число.</summary>
        public const string JsonBadNumber = "error.json_bad_number";

        /// <summary>Ожидался литерал (true/false/null).</summary>
        public const string JsonExpectedLiteral = "error.json_expected_literal";

        /// <summary>Сообщение с позицией в документе.</summary>
        public const string JsonAtPosition = "error.json_at_position";

        /// <summary>Ожидался JSON-объект.</summary>
        public const string JsonNotObject = "error.json_not_object";
    }
}
