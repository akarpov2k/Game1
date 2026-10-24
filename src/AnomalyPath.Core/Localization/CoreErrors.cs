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

        /// <summary>Неизвестный тип результата формулы.</summary>
        public const string FormulaResult = "error.formula_result";

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

        /// <summary>Неизвестный режим восстановления ОД.</summary>
        public const string UnknownApRestore = "error.unknown_ap_restore";

        /// <summary>Неизвестное правило инициативы.</summary>
        public const string UnknownInitiative = "error.unknown_initiative";

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

        /// <summary>Неизвестная роль локации в цепочке.</summary>
        public const string WorldUnknownNodeType = "error.world_unknown_node_type";

        /// <summary>Неизвестный тип локации.</summary>
        public const string WorldUnknownLocationKind = "error.world_unknown_location_kind";

        /// <summary>Неизвестный тег локации.</summary>
        public const string WorldUnknownLocationTag = "error.world_unknown_location_tag";

        /// <summary>Неизвестная редкость локации.</summary>
        public const string WorldUnknownRarity = "error.world_unknown_location_rarity";

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

        /// <summary>Некорректное имя слота сохранения.</summary>
        public const string SaveSlotInvalid = "error.save_slot_invalid";

        /// <summary>Папка сохранений не задана.</summary>
        public const string SaveRootMissing = "error.save_root_missing";

        /// <summary>Слот сохранения не найден.</summary>
        public const string SaveNotFound = "error.save_not_found";

        /// <summary>Версия сохранения не поддерживается.</summary>
        public const string SaveUnsupportedVersion = "error.save_unsupported_version";

        /// <summary>Сохранение не удалось записать.</summary>
        public const string SaveWriteFailed = "error.save_write_failed";

        /// <summary>Неизвестная погода в сохранении.</summary>
        public const string SaveUnknownWeather = "error.save_unknown_weather";

        /// <summary>В сохранении отсутствует обязательное поле.</summary>
        public const string SaveMissingField = "error.save_missing_field";

        /// <summary>Правила прохождения не загружены.</summary>
        public const string RulesMissing = "error.rules_missing";

        /// <summary>Правил прохождения должно быть ровно одно.</summary>
        public const string RulesSingleRecord = "error.rules_single_record";

        /// <summary>id правил должен быть <c>rules</c>.</summary>
        public const string RulesId = "error.rules_id";

        /// <summary>Числовое поле правил меньше единицы.</summary>
        public const string RulesRange = "error.rules_range";

        /// <summary>Поле правил должно быть в диапазоне 0..1.</summary>
        public const string RulesChance = "error.rules_chance";

        /// <summary>Актёр не найден.</summary>
        public const string ActorNotFound = "error.actor_not_found";

        /// <summary>Набор снаряжения не найден.</summary>
        public const string LoadoutNotFound = "error.loadout_not_found";

        /// <summary>Предмет набора не найден.</summary>
        public const string LoadoutItemNotFound = "error.loadout_item_not_found";

        /// <summary>Неизвестный слот экипировки в наборе.</summary>
        public const string LoadoutSlot = "error.loadout_slot";

        /// <summary>Поле equipment набора должно быть объектом.</summary>
        public const string LoadoutEquipment = "error.loadout_equipment";

        /// <summary>Предмет нельзя надеть в указанный слот.</summary>
        public const string LoadoutSlotMismatch = "error.loadout_slot_mismatch";

        /// <summary>Предметы набора не помещаются в стартовый инвентарь.</summary>
        public const string LoadoutOverflow = "error.loadout_overflow";

        /// <summary>Некорректное количество предмета в наборе.</summary>
        public const string LoadoutCount = "error.loadout_count";

        /// <summary>Некорректные настройки новой игры в сохранении.</summary>
        public const string SaveSettings = "error.save_settings";

        /// <summary>В сцене не задан префаб экрана.</summary>
        public const string ScreenPrefabMissing = "error.screen_prefab_missing";

        /// <summary>У задника экрана нет компонента изображения.</summary>
        public const string BackdropImageMissing = "error.backdrop_image_missing";

        /// <summary>В каталоге спрайтов нет иконок, на которые ссылаются данные.</summary>
        public const string SpriteMissing = "error.sprite_missing";
    }
}
