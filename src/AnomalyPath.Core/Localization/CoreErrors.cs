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

        /// <summary>Неизвестный режим портрета.</summary>
        public const string UnknownPortraitMode = "error.unknown_portrait_mode";

        /// <summary>Торговец описан без блока торговли.</summary>
        public const string TraderWithoutBlock = "error.trader_without_block";

        /// <summary>У торговца нет роли <c>trader</c>.</summary>
        public const string TraderWithoutRole = "error.trader_without_role";

        /// <summary>Блок торговли задан не торговцу.</summary>
        public const string TraderBlockOnNonTrader = "error.trader_block_on_non_trader";

        /// <summary>Роль торговца задана актёру, который не является торговцем.</summary>
        public const string TraderRoleOnNonTrader = "error.trader_role_on_non_trader";

        /// <summary>Ассортимент торговца пуст.</summary>
        public const string TraderAssortmentEmpty = "error.trader_assortment_empty";

        /// <summary>Период пополнения торговца меньше часа.</summary>
        public const string TraderRestockHours = "error.trader_restock_hours";

        /// <summary>Множитель цены торговца вне диапазона.</summary>
        public const string TraderModifierRange = "error.trader_modifier_range";

        /// <summary>Количество товара в ассортименте меньше единицы.</summary>
        public const string TraderOfferStock = "error.trader_offer_stock";

        /// <summary>Товар торговца не найден среди предметов.</summary>
        public const string TraderOfferItemMissing = "error.trader_offer_item_missing";

        /// <summary>У товара торговца нет цены.</summary>
        public const string TraderOfferPriceMissing = "error.trader_offer_price_missing";

        /// <summary>Торговец не привязан ни к одному хабу.</summary>
        public const string TraderNotBound = "error.trader_not_bound";

        /// <summary>Торговец привязан к небезопасной локации.</summary>
        public const string TraderBoundToNonHub = "error.trader_bound_to_non_hub";

        /// <summary>Торговец привязан более чем к одному хабу.</summary>
        public const string TraderDuplicateBinding = "error.trader_duplicate_binding";

        /// <summary>Локация торговца расходится с привязкой к хабу.</summary>
        public const string TraderLocationMismatch = "error.trader_location_mismatch";

        /// <summary>Торговец не найден.</summary>
        public const string TradeTraderNotFound = "error.trade_trader_not_found";

        /// <summary>Актёр не является торговцем.</summary>
        public const string TradeNotTrader = "error.trade_not_trader";

        /// <summary>Торговец недоступен по условиям.</summary>
        public const string TradeUnavailable = "error.trade_unavailable";

        /// <summary>Торговец находится в другой локации.</summary>
        public const string TradeWrongLocation = "error.trade_wrong_location";

        /// <summary>Товар отсутствует в ассортименте торговца.</summary>
        public const string TradeOfferNotFound = "error.trade_offer_not_found";

        /// <summary>Не хватает денег на покупку.</summary>
        public const string TradeNotEnoughMoney = "error.trade_not_enough_money";

        /// <summary>Товара не хватает на складе.</summary>
        public const string TradeNoStock = "error.trade_no_stock";

        /// <summary>В инвентаре нет места для покупки.</summary>
        public const string TradeNoRoom = "error.trade_no_room";

        /// <summary>Предмет для продажи не найден.</summary>
        public const string TradeItemNotFound = "error.trade_item_not_found";

        /// <summary>Предмет нельзя продать: у него нет цены продажи.</summary>
        public const string TradeItemNotSellable = "error.trade_item_not_sellable";

        /// <summary>Некорректное количество товара в сделке.</summary>
        public const string TradeCount = "error.trade_count";

        /// <summary>Отсутствует обязательный объект.</summary>
        public const string MissingObjectField = "error.missing_object_field";

        /// <summary>Неизвестный вид квеста.</summary>
        public const string UnknownQuestKind = "error.unknown_quest_kind";

        /// <summary>Неизвестный тип цели квеста.</summary>
        public const string UnknownQuestType = "error.unknown_quest_type";

        /// <summary>Неизвестное состояние квеста в сохранении.</summary>
        public const string UnknownQuestStatus = "error.unknown_quest_status";

        /// <summary>Квест не найден.</summary>
        public const string QuestNotFound = "error.quest_not_found";

        /// <summary>Диапазон тиров цели квеста задан неверно.</summary>
        public const string QuestTierRange = "error.quest_tier_range";

        /// <summary>Пул целей квеста не дал ни одного кандидата.</summary>
        public const string QuestTargetPool = "error.quest_target_pool";

        /// <summary>Цель зачистки должна быть дикой точкой интереса.</summary>
        public const string QuestLocationNotSite = "error.quest_location_not_site";

        /// <summary>Цель квеста должна быть врагом с нужной ролью.</summary>
        public const string QuestActorRole = "error.quest_actor_role";

        /// <summary>Цель квеста должна быть предметом нужной категории.</summary>
        public const string QuestItemCategory = "error.quest_item_category";

        /// <summary>Предмет награды квеста не найден.</summary>
        public const string QuestRewardItemMissing = "error.quest_reward_item_missing";

        /// <summary>Талант не найден в дереве.</summary>
        public const string TalentNotFound = "error.talent_not_found";

        /// <summary>В дереве талантов нет ни одного таланта.</summary>
        public const string TalentTreeEmpty = "error.talent_tree_empty";

        /// <summary>Дерево талантов должно быть единственным.</summary>
        public const string TalentTreeMultiple = "error.talent_tree_multiple";

        /// <summary>Идентификатор таланта повторяется.</summary>
        public const string TalentNodeDuplicate = "error.talent_node_duplicate";

        /// <summary>Позиция таланта в дереве повторяется.</summary>
        public const string TalentPositionDuplicate = "error.talent_position_duplicate";

        /// <summary>Строка или колонка таланта отрицательна.</summary>
        public const string TalentPosition = "error.talent_position";

        /// <summary>Максимальный ранг таланта вне допустимого диапазона.</summary>
        public const string TalentMaxRank = "error.talent_max_rank";

        /// <summary>У таланта нет ни одного эффекта.</summary>
        public const string TalentNoEffects = "error.talent_no_effects";

        /// <summary>Одна и та же характеристика указана у таланта дважды.</summary>
        public const string TalentEffectDuplicate = "error.talent_effect_duplicate";

        /// <summary>Неизвестная характеристика таланта.</summary>
        public const string UnknownTalentStat = "error.unknown_talent_stat";

        /// <summary>Значение эффекта таланта не положительное или не целое.</summary>
        public const string TalentEffectValue = "error.talent_effect_value";

        /// <summary>Талант требует больше очков, чем можно вложить до него.</summary>
        public const string TalentGate = "error.talent_gate";

        /// <summary>Окно округления цены меньше единицы.</summary>
        public const string EconomyRounding = "error.economy_rounding";

        /// <summary>Минимальная цена отрицательна.</summary>
        public const string EconomyMinPrice = "error.economy_min_price";

        /// <summary>Неизвестная ступень редкости.</summary>
        public const string UnknownRarity = "error.unknown_rarity";

        /// <summary>Таблица лута не найдена.</summary>
        public const string LootTableNotFound = "error.loot_table_not_found";

        /// <summary>Шанс записи лута вне диапазона 0..1.</summary>
        public const string LootChanceRange = "error.loot_chance_range";

        /// <summary>Некорректный диапазон количества в записи лута.</summary>
        public const string LootCountRange = "error.loot_count_range";

        /// <summary>Цикл общих таблиц лута.</summary>
        public const string LootTableCycle = "error.loot_table_cycle";

        /// <summary>Таблица лута включена повторно.</summary>
        public const string LootTableDuplicate = "error.loot_table_duplicate";

        /// <summary>Таблица лута указана не у врага.</summary>
        public const string LootTableOnNonEnemy = "error.loot_table_on_non_enemy";

        /// <summary>Настроек лута должно быть ровно одна запись.</summary>
        public const string LootSettingsMultiple = "error.loot_settings_multiple";

        /// <summary>id настроек лута должен быть <c>loot</c>.</summary>
        public const string LootSettingsId = "error.loot_settings_id";

        /// <summary>Полоса тира задана неверно.</summary>
        public const string LootBand = "error.loot_band";

        /// <summary>Сумма вкладов полос тира не равна 1.</summary>
        public const string LootBandSum = "error.loot_band_sum";

        /// <summary>В настройках лута нет ни одной полосы тира.</summary>
        public const string LootBandEmpty = "error.loot_band_empty";

        /// <summary>Запись пула лута должна быть объектом.</summary>
        public const string LootEntryObject = "error.loot_entry_object";

        /// <summary>Предмет записи лута не найден.</summary>
        public const string LootItemNotFound = "error.loot_item_not_found";

        /// <summary>Блок атаки указан не у врага.</summary>
        public const string AttackOnNonEnemy = "error.attack_on_non_enemy";

        /// <summary>У врага нет блока атаки.</summary>
        public const string ActorWithoutAttack = "error.actor_without_attack";

        /// <summary>В атаке смешаны оружие и естественное оружие.</summary>
        public const string AttackMixedFields = "error.attack_mixed_fields";

        /// <summary>Неизвестная зона дистанции.</summary>
        public const string UnknownDistanceZone = "error.unknown_distance_zone";
    }
}
