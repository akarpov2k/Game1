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
        /// <param name="tradeDifficulty">Коэффициент усложнения торговли по умолчанию.</param>
        /// <param name="tradeDifficultyMax">Максимум усложнения торговли.</param>
        /// <param name="tradeDifficultyStep">Шаг изменения усложнения торговли.</param>
        /// <param name="lootLossStep">Шаг изменения потери лута.</param>
        /// <param name="nicknameMaxLength">Максимальная длина ника.</param>
        /// <param name="previewMaxRows">Предел рядов в окне предпросмотра набора.</param>
        /// <param name="savePageSize">Число ячеек сохранения на странице.</param>
        /// <exception cref="ArgumentException">Строковые поля пусты.</exception>
        /// <exception cref="ArgumentNullException">Список вариантов потери лута не задан.</exception>
        public GameRulesTemplate(
            string id,
            DeathMode deathMode,
            IReadOnlyList<LootLossOption> lootLossOptions,
            string defaultLootLoss,
            double insuranceChance,
            InsuranceScope insuranceAppliesTo,
            int baseRows,
            int tierCount,
            string playerActorId,
            int gridWidth,
            int needsMax,
            int startingMoney,
            double tradeDifficulty,
            double tradeDifficultyMax,
            double tradeDifficultyStep,
            double lootLossStep,
            int nicknameMaxLength,
            int previewMaxRows,
            int savePageSize)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(id)),
                    nameof(id));
            }

            if (string.IsNullOrWhiteSpace(defaultLootLoss))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(defaultLootLoss)),
                    nameof(defaultLootLoss));
            }

            if (string.IsNullOrWhiteSpace(playerActorId))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(playerActorId)),
                    nameof(playerActorId));
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
            TradeDifficulty = tradeDifficulty;
            TradeDifficultyMax = tradeDifficultyMax;
            TradeDifficultyStep = tradeDifficultyStep;
            LootLossStep = lootLossStep;
            NicknameMaxLength = nicknameMaxLength;
            PreviewMaxRows = previewMaxRows;
            SavePageSize = savePageSize;
        }

        /// <summary>Идентификатор записи, всегда <c>rules</c>.</summary>
        public string Id { get; }

        /// <summary>Режим смерти.</summary>
        public DeathMode DeathMode { get; }

        /// <summary>Варианты потери лута.</summary>
        public IReadOnlyList<LootLossOption> LootLossOptions { get; }

        /// <summary>Id варианта потери лута по умолчанию.</summary>
        public string DefaultLootLoss { get; }

        /// <summary>Шанс страховки вещи, от 0 до 1.</summary>
        public double InsuranceChance { get; }

        /// <summary>Область действия страховки.</summary>
        public InsuranceScope InsuranceAppliesTo { get; }

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

        /// <summary>Коэффициент усложнения торговли по умолчанию.</summary>
        public double TradeDifficulty { get; }

        /// <summary>Максимум усложнения торговли.</summary>
        public double TradeDifficultyMax { get; }

        /// <summary>Шаг изменения усложнения торговли.</summary>
        public double TradeDifficultyStep { get; }

        /// <summary>Шаг изменения потери лута.</summary>
        public double LootLossStep { get; }

        /// <summary>Максимальная длина ника.</summary>
        public int NicknameMaxLength { get; }

        /// <summary>Предел рядов в окне предпросмотра набора.</summary>
        public int PreviewMaxRows { get; }

        /// <summary>Число ячеек сохранения на странице.</summary>
        public int SavePageSize { get; }

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

            var deathModeCode = DataMap.RequireString(value, "deathMode", path);
            if (!DeathModeCodec.TryParse(deathModeCode, out DeathMode deathMode))
            {
                throw new DataException(path, CoreErrors.RulesUnknownDeathMode, deathModeCode);
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

            var insuranceCode = DataMap.RequireString(value, "insuranceAppliesTo", path);
            if (!SchemaEnum.TryParse(insuranceCode, out InsuranceScope insuranceAppliesTo))
            {
                throw new DataException(path, CoreErrors.RulesUnknownInsuranceScope, insuranceCode);
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

            var startingMoney = DataMap.RequireInt(value, "startingMoney", path);
            if (startingMoney < 0)
            {
                throw new DataException(path, CoreErrors.RulesRange, "startingMoney");
            }

            var tradeDifficulty = DataMap.RequireDouble(value, "tradeDifficulty", path);
            if (tradeDifficulty < 0)
            {
                throw new DataException(path, CoreErrors.RulesChance, "tradeDifficulty");
            }

            var tradeDifficultyMax = DataMap.RequireDouble(value, "tradeDifficultyMax", path);
            if (tradeDifficultyMax < tradeDifficulty)
            {
                throw new DataException(path, CoreErrors.RangeOrder, "tradeDifficultyMax");
            }

            var tradeDifficultyStep = DataMap.RequireDouble(value, "tradeDifficultyStep", path);
            if (tradeDifficultyStep <= 0)
            {
                throw new DataException(path, CoreErrors.PositiveValue, "tradeDifficultyStep");
            }

            var lootLossStep = DataMap.RequireDouble(value, "lootLossStep", path);
            if (lootLossStep <= 0)
            {
                throw new DataException(path, CoreErrors.PositiveValue, "lootLossStep");
            }

            var nicknameMaxLength = DataMap.RequireInt(value, "nicknameMaxLength", path);
            if (nicknameMaxLength < 1)
            {
                throw new DataException(path, CoreErrors.RulesRange, "nicknameMaxLength");
            }

            var previewMaxRows = DataMap.RequireInt(value, "previewMaxRows", path);
            if (previewMaxRows < 1)
            {
                throw new DataException(path, CoreErrors.RulesRange, "previewMaxRows");
            }

            var savePageSize = DataMap.RequireInt(value, "savePageSize", path);
            if (savePageSize < 1)
            {
                throw new DataException(path, CoreErrors.RulesRange, "savePageSize");
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
                startingMoney,
                tradeDifficulty,
                tradeDifficultyMax,
                tradeDifficultyStep,
                lootLossStep,
                nicknameMaxLength,
                previewMaxRows,
                savePageSize);
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
