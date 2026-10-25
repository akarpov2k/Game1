#nullable enable
namespace AnomalyPath.Core.Localization
{
    /// <summary>Часть класса <see cref="CoreErrors"/>: ошибки правил, снаряжения и торговцев.</summary>
    public static partial class CoreErrors
    {
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
    }
}
