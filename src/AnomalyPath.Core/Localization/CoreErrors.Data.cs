#nullable enable
namespace AnomalyPath.Core.Localization
{
    /// <summary>Часть класса <see cref="CoreErrors"/>: ошибки данных.</summary>
    public static partial class CoreErrors
    {
        /// <summary>Пустой путь к файлу.</summary>
        public const string EmptyPath = "error.empty_path";

        /// <summary>Обязательное строковое поле не задано.</summary>
        public const string RequiredField = "error.required_field";

        /// <summary>Обязательный список пуст.</summary>
        public const string EmptyCollection = "error.empty_collection";

        /// <summary>Числовое поле меньше единицы.</summary>
        public const string MinimumOne = "error.minimum_one";

        /// <summary>Числовое поле отрицательное.</summary>
        public const string NonNegative = "error.non_negative";

        /// <summary>Значение должно быть в диапазоне 0..1.</summary>
        public const string ChanceRange = "error.chance_range";

        /// <summary>Индекс вне диапазона.</summary>
        public const string IndexOutOfRange = "error.index_out_of_range";

        /// <summary>Ник длиннее допустимого.</summary>
        public const string NicknameLength = "error.nickname_length";

        /// <summary>Верхняя граница меньше нижней.</summary>
        public const string RangeOrder = "error.range_order";

        /// <summary>Шаг должен быть положительным.</summary>
        public const string StepPositive = "error.step_positive";

        /// <summary>Значение должно быть больше нуля.</summary>
        public const string PositiveValue = "error.positive_value";

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

        /// <summary>Вместимость магазина оружия меньше одного патрона.</summary>
        public const string WeaponMagazineInvalid = "error.weapon_magazine_invalid";

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

        /// <summary>Предмет не помещается в контейнер.</summary>
        public const string ItemDoesNotFit = "error.item_does_not_fit";

        /// <summary>У артефакта не указан тип.</summary>
        public const string ArtifactWithoutType = "error.artifact_without_type";

        /// <summary>Неизвестный материал брони или шлема.</summary>
        public const string UnknownMaterial = "error.unknown_material";

        /// <summary>Неизвестная категория предмета.</summary>
        public const string UnknownItemCategory = "error.unknown_item_category";

        /// <summary>Неизвестный режим цены.</summary>
        public const string UnknownPriceMode = "error.unknown_price_mode";

        /// <summary>Неизвестный тип урона в блоке защиты.</summary>
        public const string UnknownDamageType = "error.unknown_damage_type";

        /// <summary>Проверка данных пройдена.</summary>
        public const string ValidationOk = "validation.ok";

        /// <summary>Проверка данных не пройдена.</summary>
        public const string ValidationFailed = "validation.failed";

        /// <summary>В наборе переводов не хватает ключа.</summary>
        public const string TranslationMissing = "validation.translation_missing";

        /// <summary>Каталог текстов игры отсутствует.</summary>
        public const string GameTextsMissing = "validation.game_texts_missing";

        /// <summary>Заголовок экрана ошибки данных.</summary>
        public const string DataErrorTitle = "validation.data_error_title";

        /// <summary>Данные не загружены.</summary>
        public const string DataErrorMissing = "validation.data_missing";

        /// <summary>Кнопка выхода на экране ошибки данных.</summary>
        public const string DataErrorQuit = "validation.data_quit";

        /// <summary>Строка отчёта: путь и сообщение.</summary>
        public const string ReportLine = "log.report_line";

        /// <summary>Не удалось сохранить exit_save.</summary>
        public const string ExitSaveFailed = "log.exit_save_failed";

        /// <summary>Данные не прошли проверку.</summary>
        public const string DataCheckFailed = "log.data_check_failed";

        /// <summary>Файл не используется ни каталогом, ни коллекциями.</summary>
        public const string UnreachableFile = "validation.unreachable";

        /// <summary>Дублирующийся id с указанием первого файла.</summary>
        public const string DuplicateIdFiles = "error.duplicate_id_files";

        /// <summary>Отсутствует обязательный объект.</summary>
        public const string MissingObjectField = "error.missing_object_field";
    }
}
