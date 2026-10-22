#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Rules
{
    /// <summary>Правила прохождения из данных: режим смерти, потеря лута, базовая сетка и число тиров.</summary>
    public sealed class GameRulesTemplate
    {
        /// <summary>Единственный допустимый id записи правил.</summary>
        public const string IdValue = "rules";

        /// <summary>Смерть возвращает игрока в хаб с потерей лута.</summary>
        public const string DeathModeHubReturn = "hubReturn";

        /// <summary>Смерть удаляет прохождение полностью.</summary>
        public const string DeathModePermadeath = "permadeath";

        /// <summary>Страховка распространяется только на добычу вылазки.</summary>
        public const string InsuranceLoot = "loot";

        /// <summary>Страховка распространяется и на снаряжение.</summary>
        public const string InsuranceAll = "all";

        /// <summary>Стартовые деньги, если поле не задано в данных.</summary>
        public const int DefaultStartingMoney = 1000;

        /// <summary>Создаёт правила прохождения.</summary>
        /// <param name="id">Идентификатор записи, всегда <c>rules</c>.</param>
        /// <param name="deathMode">Режим смерти.</param>
        /// <param name="lootLossOptions">Варианты потери лута.</param>
        /// <param name="defaultLootLoss">Id варианта по умолчанию.</param>
        /// <param name="insuranceChance">Шанс страховки вещи, от 0 до 1.</param>
        /// <param name="insuranceAppliesTo">Область действия страховки.</param>
        /// <param name="baseRows">Строк сетки без рюкзака.</param>
        /// <param name="tierCount">Число тиров.</param>
        /// <param name="playerActorId">Актёр игрока для стартовых характеристик.</param>
        /// <param name="gridWidth">Ширина сетки инвентаря.</param>
        /// <param name="needsMax">Максимум шкал потребностей.</param>
        /// <param name="startingMoney">Деньги на персонаже на старте.</param>
        /// <exception cref="ArgumentException">Строковые поля пусты.</exception>
        /// <exception cref="ArgumentNullException">Список вариантов потери лута не задан.</exception>
        public GameRulesTemplate(
            string id,
            string deathMode,
            IReadOnlyList<LootLossOption> lootLossOptions,
            string defaultLootLoss,
            double insuranceChance,
            string insuranceAppliesTo,
            int baseRows,
            int tierCount,
            string playerActorId,
            int gridWidth,
            int needsMax,
            int startingMoney = DefaultStartingMoney)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("id правил не задан", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(deathMode))
            {
                throw new ArgumentException("deathMode не задан", nameof(deathMode));
            }

            if (string.IsNullOrWhiteSpace(defaultLootLoss))
            {
                throw new ArgumentException("defaultLootLoss не задан", nameof(defaultLootLoss));
            }

            if (string.IsNullOrWhiteSpace(insuranceAppliesTo))
            {
                throw new ArgumentException("insuranceAppliesTo не задан", nameof(insuranceAppliesTo));
            }

            if (string.IsNullOrWhiteSpace(playerActorId))
            {
                throw new ArgumentException("playerActorId не задан", nameof(playerActorId));
            }

            Id = id;
            DeathMode = deathMode;
            LootLossOptions = lootLossOptions ?? throw new ArgumentNullException(nameof(lootLossOptions));
            DefaultLootLoss = defaultLootLoss;
            InsuranceChance = insuranceChance;
            InsuranceAppliesTo = insuranceAppliesTo;
            BaseRows = baseRows;
            TierCount = tierCount;
            PlayerActorId = playerActorId;
            GridWidth = gridWidth;
            NeedsMax = needsMax;
            StartingMoney = startingMoney;
        }

        /// <summary>Идентификатор записи, всегда <c>rules</c>.</summary>
        public string Id { get; }

        /// <summary>Режим смерти.</summary>
        public string DeathMode { get; }

        /// <summary>Варианты потери лута.</summary>
        public IReadOnlyList<LootLossOption> LootLossOptions { get; }

        /// <summary>Id варианта потери лута по умолчанию.</summary>
        public string DefaultLootLoss { get; }

        /// <summary>Шанс страховки вещи, от 0 до 1.</summary>
        public double InsuranceChance { get; }

        /// <summary>Область действия страховки.</summary>
        public string InsuranceAppliesTo { get; }

        /// <summary>Строк сетки без рюкзака.</summary>
        public int BaseRows { get; }

        /// <summary>Число тиров.</summary>
        public int TierCount { get; }

        /// <summary>Актёр игрока для стартовых характеристик.</summary>
        public string PlayerActorId { get; }

        /// <summary>Ширина сетки инвентаря.</summary>
        public int GridWidth { get; }

        /// <summary>Максимум шкал потребностей.</summary>
        public int NeedsMax { get; }

        /// <summary>Деньги на персонаже на старте игры.</summary>
        public int StartingMoney { get; }

        /// <summary>Проверяет, известен ли режим смерти.</summary>
        /// <param name="value">Значение поля <c>deathMode</c>.</param>
        /// <returns><c>true</c>, если режим поддерживается.</returns>
        public static bool IsKnownDeathMode(string value) =>
            value is DeathModeHubReturn or DeathModePermadeath;

        /// <summary>Проверяет, известна ли область действия страховки.</summary>
        /// <param name="value">Значение поля <c>insuranceAppliesTo</c>.</param>
        /// <returns><c>true</c>, если область поддерживается.</returns>
        public static bool IsKnownInsuranceScope(string value) =>
            value is InsuranceLoot or InsuranceAll;

        /// <summary>Читает правила из записи данных.</summary>
        /// <param name="value">Запись вида <c>rules</c>.</param>
        /// <param name="path">Путь для сообщения об ошибке.</param>
        /// <returns>Правила прохождения.</returns>
        /// <exception cref="DataException">Поля отсутствуют, значения неизвестны или вне границ.</exception>
        public static GameRulesTemplate FromJson(JsonValue value, string path)
        {
            var id = DataMap.RequireString(value, "id", path);
            if (id != IdValue)
            {
                throw new DataException(path, CoreErrors.RulesId, id);
            }

            var deathMode = DataMap.RequireString(value, "deathMode", path);
            if (!IsKnownDeathMode(deathMode))
            {
                throw new DataException(path, CoreErrors.RulesUnknownDeathMode, deathMode);
            }

            if (!value.TryGet("lootLossOptions", out var optionsValue) ||
                !optionsValue.IsArray ||
                optionsValue.Items.Count == 0)
            {
                throw new DataException(path, CoreErrors.RulesMissingLootLoss, "lootLossOptions");
            }

            var options = new List<LootLossOption>();
            foreach (var item in optionsValue.Items)
            {
                if (!item.IsObject)
                {
                    throw new DataException(path, CoreErrors.JsonNotObject, "lootLossOptions");
                }

                options.Add(LootLossOption.FromJson(item, path));
            }

            var defaultLootLoss = DataMap.RequireString(value, "defaultLootLoss", path);
            if (!ContainsOption(options, defaultLootLoss))
            {
                throw new DataException(path, CoreErrors.RulesDefaultLootLoss, defaultLootLoss);
            }

            var insuranceChance = DataMap.RequireDouble(value, "insuranceChance", path);
            if (insuranceChance < 0 || insuranceChance > 1)
            {
                throw new DataException(path, CoreErrors.RulesChance, "insuranceChance");
            }

            var insuranceAppliesTo = DataMap.RequireString(value, "insuranceAppliesTo", path);
            if (!IsKnownInsuranceScope(insuranceAppliesTo))
            {
                throw new DataException(path, CoreErrors.RulesUnknownInsuranceScope, insuranceAppliesTo);
            }

            var baseRows = DataMap.RequireInt(value, "baseRows", path);
            if (baseRows < 1)
            {
                throw new DataException(path, CoreErrors.RulesRange, "baseRows");
            }

            var tierCount = DataMap.RequireInt(value, "tierCount", path);
            if (tierCount < 1)
            {
                throw new DataException(path, CoreErrors.RulesRange, "tierCount");
            }

            var playerActorId = DataMap.RequireString(value, "playerActorId", path);
            var gridWidth = DataMap.RequireInt(value, "gridWidth", path);
            if (gridWidth < 1)
            {
                throw new DataException(path, CoreErrors.RulesRange, "gridWidth");
            }

            var needsMax = DataMap.RequireInt(value, "needsMax", path);
            if (needsMax < 1)
            {
                throw new DataException(path, CoreErrors.RulesRange, "needsMax");
            }

            var startingMoney = DataMap.OptionalInt(value, "startingMoney", DefaultStartingMoney);
            if (startingMoney < 0)
            {
                throw new DataException(path, CoreErrors.RulesRange, "startingMoney");
            }

            return new GameRulesTemplate(
                id,
                deathMode,
                options,
                defaultLootLoss,
                insuranceChance,
                insuranceAppliesTo,
                baseRows,
                tierCount,
                playerActorId,
                gridWidth,
                needsMax,
                startingMoney);
        }

        private static bool ContainsOption(IReadOnlyList<LootLossOption> options, string id)
        {
            foreach (var option in options)
            {
                if (string.Equals(option.Id, id, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
