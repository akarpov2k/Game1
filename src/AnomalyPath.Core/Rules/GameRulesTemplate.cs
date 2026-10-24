#nullable enable
using System;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Rules
{
    /// <summary>
    /// Правила прохождения из данных: страховка надетых вещей, базовая сетка,
    /// число тиров и настройки новой игры.
    /// </summary>
    public sealed class GameRulesTemplate
    {
        /// <summary>Единственный допустимый id записи правил.</summary>
        public const string IdValue = "rules";

        /// <summary>Создаёт правила прохождения.</summary>
        /// <param name="id">Идентификатор записи, всегда <c>rules</c>.</param>
        /// <param name="insuranceChance">Коэффициент страховки надетой вещи по умолчанию, от 0 до 1.</param>
        /// <param name="insuranceStep">Шаг изменения коэффициента страховки, больше 0.</param>
        /// <param name="baseRows">Строк сетки без рюкзака.</param>
        /// <param name="tierCount">Число тиров.</param>
        /// <param name="playerActorId">Актёр игрока для стартовых характеристик.</param>
        /// <param name="gridWidth">Ширина сетки инвентаря.</param>
        /// <param name="needsMax">Максимум шкал потребностей.</param>
        /// <param name="startingMoney">Деньги на персонаже на старте.</param>
        /// <param name="tradeDifficulty">Коэффициент усложнения торговли по умолчанию.</param>
        /// <param name="tradeDifficultyMax">Максимум усложнения торговли.</param>
        /// <param name="tradeDifficultyStep">Шаг изменения усложнения торговли.</param>
        /// <param name="nicknameMaxLength">Максимальная длина ника.</param>
        /// <param name="previewMaxRows">Предел рядов в окне предпросмотра набора.</param>
        /// <param name="savePageSize">Число ячеек сохранения на странице.</param>
        /// <param name="respawnPercent">Доля здоровья и потолка еды/воды после смерти, 0..1.</param>
        /// <param name="maxActiveQuests">Предел одновременно взятых квестов.</param>
        /// <param name="questOffersPerTrader">Сколько квестов показывает один торговец.</param>
        /// <param name="questOfferCooldownHours">Задержка перед появлением нового квеста в освободившемся слоте.</param>
        /// <param name="questCooldownHours">Откат повторяемого квеста по умолчанию.</param>
        /// <exception cref="ArgumentException">Строковые поля пусты.</exception>
        public GameRulesTemplate(
            string id,
            double insuranceChance,
            double insuranceStep,
            int baseRows,
            int tierCount,
            string playerActorId,
            int gridWidth,
            int needsMax,
            int startingMoney,
            double tradeDifficulty,
            double tradeDifficultyMax,
            double tradeDifficultyStep,
            int nicknameMaxLength,
            int previewMaxRows,
            int savePageSize,
            double respawnPercent,
            int maxActiveQuests = 3,
            int questOffersPerTrader = 4,
            int questOfferCooldownHours = 6,
            int questCooldownHours = 48)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(id)),
                    nameof(id));
            }

            if (string.IsNullOrWhiteSpace(playerActorId))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(playerActorId)),
                    nameof(playerActorId));
            }

            Id = id;
            InsuranceChance = insuranceChance;
            InsuranceStep = insuranceStep;
            BaseRows = baseRows;
            TierCount = tierCount;
            PlayerActorId = playerActorId;
            GridWidth = gridWidth;
            NeedsMax = needsMax;
            StartingMoney = startingMoney;
            TradeDifficulty = tradeDifficulty;
            TradeDifficultyMax = tradeDifficultyMax;
            TradeDifficultyStep = tradeDifficultyStep;
            NicknameMaxLength = nicknameMaxLength;
            PreviewMaxRows = previewMaxRows;
            SavePageSize = savePageSize;
            RespawnPercent = respawnPercent;
            MaxActiveQuests = maxActiveQuests;
            QuestOffersPerTrader = questOffersPerTrader;
            QuestOfferCooldownHours = questOfferCooldownHours;
            QuestCooldownHours = questCooldownHours;
        }

        /// <summary>Идентификатор записи, всегда <c>rules</c>.</summary>
        public string Id { get; }

        /// <summary>
        /// Коэффициент страховки надетой вещи по умолчанию, от 0 до 1.
        /// Применяется к каждому надетому предмету отдельным броском; 0 — не страхуется ничего,
        /// 1 — страхуется всё.
        /// </summary>
        public double InsuranceChance { get; }

        /// <summary>Шаг изменения коэффициента страховки, больше 0.</summary>
        public double InsuranceStep { get; }

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

        /// <summary>Максимальная длина ника.</summary>
        public int NicknameMaxLength { get; }

        /// <summary>Предел рядов в окне предпросмотра набора.</summary>
        public int PreviewMaxRows { get; }

        /// <summary>Число ячеек сохранения на странице.</summary>
        public int SavePageSize { get; }

        /// <summary>Доля здоровья и потолка еды/воды после смерти, 0..1.</summary>
        public double RespawnPercent { get; }

        /// <summary>Предел одновременно взятых квестов.</summary>
        public int MaxActiveQuests { get; }

        /// <summary>Сколько квестов показывает один торговец.</summary>
        public int QuestOffersPerTrader { get; }

        /// <summary>Задержка перед появлением нового квеста в освободившемся слоте в часах мира.</summary>
        public int QuestOfferCooldownHours { get; }

        /// <summary>Откат повторяемого квеста по умолчанию в часах мира.</summary>
        public int QuestCooldownHours { get; }

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

            var insuranceChance = DataMap.RequireDouble(value, "insuranceChance", path);
            if (insuranceChance < 0 || insuranceChance > 1)
            {
                throw new DataException(path, CoreErrors.RulesChance, "insuranceChance");
            }

            var insuranceStep = DataMap.RequireDouble(value, "insuranceStep", path);
            if (insuranceStep <= 0)
            {
                throw new DataException(path, CoreErrors.PositiveValue, "insuranceStep");
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

            var respawnPercent = DataMap.RequireDouble(value, "respawnPercent", path);
            if (respawnPercent <= 0 || respawnPercent > 1)
            {
                throw new DataException(path, CoreErrors.RulesChance, "respawnPercent");
            }

            var maxActiveQuests = DataMap.OptionalInt(value, "maxActiveQuests", 3);
            if (maxActiveQuests < 1)
            {
                throw new DataException(path, CoreErrors.RulesRange, "maxActiveQuests");
            }

            var questOffersPerTrader = DataMap.OptionalInt(value, "questOffersPerTrader", 4);
            if (questOffersPerTrader < 1)
            {
                throw new DataException(path, CoreErrors.RulesRange, "questOffersPerTrader");
            }

            var questOfferCooldownHours = DataMap.OptionalInt(value, "questOfferCooldownHours", 6);
            if (questOfferCooldownHours < 0)
            {
                throw new DataException(path, CoreErrors.RulesRange, "questOfferCooldownHours");
            }

            var questCooldownHours = DataMap.OptionalInt(value, "questCooldownHours", 48);
            if (questCooldownHours < 0)
            {
                throw new DataException(path, CoreErrors.RulesRange, "questCooldownHours");
            }

            return new GameRulesTemplate(
                id,
                insuranceChance,
                insuranceStep,
                baseRows,
                tierCount,
                playerActorId,
                gridWidth,
                needsMax,
                startingMoney,
                tradeDifficulty,
                tradeDifficultyMax,
                tradeDifficultyStep,
                nicknameMaxLength,
                previewMaxRows,
                savePageSize,
                respawnPercent,
                maxActiveQuests,
                questOffersPerTrader,
                questOfferCooldownHours,
                questCooldownHours);
        }
    }
}
