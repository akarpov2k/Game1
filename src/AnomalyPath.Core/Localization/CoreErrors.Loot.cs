#nullable enable
namespace AnomalyPath.Core.Localization
{
    /// <summary>Часть класса <see cref="CoreErrors"/>: ошибки добычи.</summary>
    public static partial class CoreErrors
    {
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
    }
}
