#nullable enable
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

        /// <summary>Предмет не помещается в контейнер.</summary>
        public const string ItemDoesNotFit = "error.item_does_not_fit";

        /// <summary>У артефакта не указан тип.</summary>
        public const string ArtifactWithoutType = "error.artifact_without_type";

        /// <summary>Неизвестный материал брони или шлема.</summary>
        public const string UnknownMaterial = "error.unknown_material";

        /// <summary>Неизвестный тип урона в блоке защиты.</summary>
        public const string UnknownDamageType = "error.unknown_damage_type";

        /// <summary>Пустое выражение формулы.</summary>
        public const string FormulaEmpty = "error.formula_empty";

        /// <summary>Синтаксическая ошибка в формуле.</summary>
        public const string FormulaSyntaxError = "error.formula_syntax";

        /// <summary>Формула использует неизвестное имя.</summary>
        public const string FormulaUnknownVariable = "error.formula_unknown_variable";

        /// <summary>Формула использует неизвестную функцию.</summary>
        public const string FormulaUnknownFunction = "error.formula_unknown_function";

        /// <summary>Формула должна задавать ровно одно представление.</summary>
        public const string FormulaRepresentation = "error.formula_representation";

        /// <summary>Ссылка на формулу не найдена.</summary>
        public const string FormulaNotFound = "error.formula_not_found";

        /// <summary>У актёра нет боевого блока, а он обязателен.</summary>
        public const string ActorWithoutCombat = "error.actor_without_combat";

        /// <summary>Неизвестный тип актёра.</summary>
        public const string UnknownActorType = "error.unknown_actor_type";

        /// <summary>Боевое действие невозможно: не тот ход, нет ОД или боя нет.</summary>
        public const string CombatActionUnavailable = "error.combat_unavailable";

        /// <summary>Атака невозможна: цель мертва или оружие сломано.</summary>
        public const string CombatAttackUnavailable = "error.combat_attack_unavailable";

        /// <summary>Неизвестный вид урона в поле kind.</summary>
        public const string UnknownDamageTypeKind = "error.unknown_damage_type_kind";

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

        /// <summary>Неизвестная роль локации в цепочке.</summary>
        public const string WorldUnknownNodeType = "error.world_unknown_node_type";

        /// <summary>Неизвестный тип локации.</summary>
        public const string WorldUnknownLocationKind = "error.world_unknown_location_kind";

        /// <summary>Неизвестный способ перехода.</summary>
        public const string WorldUnknownVia = "error.world_unknown_via";

        /// <summary>Переход ведёт на несуществующую локацию.</summary>
        public const string WorldConnectionTarget = "error.world_connection_target";

        /// <summary>Переход запрещён топологией.</summary>
        public const string WorldTransitionForbidden = "error.world_transition_forbidden";

        /// <summary>Теги локаций не подходят для выбранного способа перехода.</summary>
        public const string WorldViaTags = "error.world_via_tags";

        /// <summary>Некорректный диапазон.</summary>
        public const string WorldRange = "error.world_range";

        /// <summary>Значение должно быть в диапазоне 0..1.</summary>
        public const string WorldChance = "error.world_chance";

        /// <summary>Нет стоимости для способа перехода.</summary>
        public const string WorldCost = "error.world_cost";

        /// <summary>В данных нет ни одного хаба.</summary>
        public const string WorldNoHub = "error.world_no_hub";

        /// <summary>В данных нет ни одной точки интереса.</summary>
        public const string WorldNoSite = "error.world_no_site";

        /// <summary>Для локации нельзя построить маршрут.</summary>
        public const string WorldRouteUnavailable = "error.world_route_unavailable";

        /// <summary>Переход не найден.</summary>
        public const string WorldEdgeNotFound = "error.world_edge_not_found";

        /// <summary>Не хватает выносливости на переход.</summary>
        public const string WorldNotEnoughStamina = "error.world_not_enough_stamina";

        /// <summary>Переход начинается не из текущей локации.</summary>
        public const string WorldWrongLocation = "error.world_wrong_location";

        /// <summary>Часы мира вне диапазона 0..24.</summary>
        public const string WorldClockHours = "error.world_clock_hours";

        /// <summary>Локация не найдена в сгенерированном мире.</summary>
        public const string WorldInstanceNotFound = "error.world_instance_not_found";

        /// <summary>Маршрут длиннее предела ветвления.</summary>
        public const string WorldDepth = "error.world_depth";

        /// <summary>Уникальная локация должна иметь редкость unique.</summary>
        public const string WorldUniqueRarity = "error.world_unique_rarity";
    }
}
