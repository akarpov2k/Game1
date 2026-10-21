#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;

namespace AnomalyPath.Core.Localization
{
    /// <summary>Ключи служебных текстов. Сами тексты живут в файлах ресурсов, не в коде.</summary>
    public static class CoreErrors
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

        /// <summary>Пустой путь к файлу.</summary>
        public const string EmptyPath = "error.empty_path";

        /// <summary>Путь выходит за корень данных.</summary>
        public const string PathOutsideRoot = "error.path_outside_root";

        /// <summary>Файл не найден.</summary>
        public const string FileNotFound = "error.file_not_found";

        /// <summary>Отсутствует обязательное строковое поле.</summary>
        public const string MissingStringField = "error.missing_string_field";

        /// <summary>Отсутствует обязательное числовое поле.</summary>
        public const string MissingNumberField = "error.missing_number_field";

        /// <summary>Отсутствует обязательный массив.</summary>
        public const string MissingArrayField = "error.missing_array_field";

        /// <summary>Элемент массива должен быть строкой.</summary>
        public const string ArrayItemNotString = "error.array_item_not_string";

        /// <summary>Ожидалась пара чисел.</summary>
        public const string NumberPairExpected = "error.number_pair_expected";

        /// <summary>Файл сущности подключён дважды.</summary>
        public const string DuplicateEntity = "error.duplicate_entity";

        /// <summary>Коллекция подключена дважды.</summary>
        public const string DuplicateCollection = "error.duplicate_collection";

        /// <summary>Дублирующийся id внутри вида.</summary>
        public const string DuplicateId = "error.duplicate_id";

        /// <summary>Цикл наследования.</summary>
        public const string InheritanceCycle = "error.inheritance_cycle";

        /// <summary>Слишком глубокая цепочка наследования.</summary>
        public const string InheritanceDepth = "error.inheritance_depth";

        /// <summary>Наследование между разными видами.</summary>
        public const string KindMismatch = "error.kind_mismatch";

        /// <summary>Категория weapon без блока weapon.</summary>
        public const string WeaponWithoutBlock = "error.weapon_without_block";

        /// <summary>Предмет не найден в репозитории.</summary>
        public const string ItemNotFound = "error.item_not_found";

        /// <summary>Дублирующийся id в репозитории.</summary>
        public const string DuplicateRepositoryId = "error.duplicate_repository_id";

        /// <summary>Сервис не зарегистрирован в контейнере.</summary>
        public const string ServiceNotRegistered = "error.service_not_registered";

        /// <summary>Сервис уже зарегистрирован.</summary>
        public const string ServiceAlreadyRegistered = "error.service_already_registered";

        /// <summary>Циклическая зависимость сервисов.</summary>
        public const string CyclicDependency = "error.cyclic_dependency";

        /// <summary>Проверка данных пройдена.</summary>
        public const string ValidationOk = "validation.ok";

        /// <summary>Проверка данных не пройдена.</summary>
        public const string ValidationFailed = "validation.failed";

        /// <summary>В наборе переводов не хватает ключа.</summary>
        public const string TranslationMissing = "validation.translation_missing";

        /// <summary>Файл не используется ни каталогом, ни коллекциями.</summary>
        public const string UnreachableFile = "validation.unreachable";

        /// <summary>Дублирующийся id с указанием первого файла.</summary>
        public const string DuplicateIdFiles = "error.duplicate_id_files";
    }

    /// <summary>
    /// Доступ к служебным текстам. Каталог подставляется при сборке приложения,
    /// поэтому тексты ошибок локализуются вместе с остальной игрой.
    /// </summary>
    public static class CoreText
    {
        private static IStringCatalog _catalog = new KeyOnlyCatalog();

        /// <summary>Назначает каталог текстов. Вызывается один раз при старте.</summary>
        /// <param name="catalog">Каталог с сообщениями об ошибках.</param>
        public static void Configure(IStringCatalog catalog) => _catalog = catalog;

        /// <summary>Возвращает текст по ключу или сам ключ, если перевода нет.</summary>
        /// <param name="key">Ключ из <see cref="CoreErrors"/>.</param>
        /// <returns>Локализованный текст.</returns>
        public static string Get(string key) => _catalog.Get(key);

        /// <summary>Форматирует текст по ключу с подстановкой аргументов.</summary>
        /// <param name="key">Ключ из <see cref="CoreErrors"/>.</param>
        /// <param name="args">Аргументы для <see cref="string.Format(string, object[])"/>.</param>
        /// <returns>Готовое сообщение.</returns>
        public static string Format(string key, params object[] args)
        {
            var template = _catalog.Get(key);
            return args == null || args.Length == 0 ? template : string.Format(template, args);
        }

        private sealed class KeyOnlyCatalog : IStringCatalog
        {
            public string Get(string key) => key;

            public bool Contains(string key) => false;

            public IReadOnlyCollection<string> Keys => new string[0];
        }
    }
}
