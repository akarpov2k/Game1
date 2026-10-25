#nullable enable
using System;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Progression
{
    /// <summary>
    /// Настройки прогрессии из данных: кривая опыта уровня и награда за убийство
    /// с учётом ранга и редкости противника. Уровни сверху не ограничены.
    /// </summary>
    public sealed class ProgressionSettings
    {
        /// <summary>Единственный допустимый id записи и он же её вид.</summary>
        public const string RecordId = "progression";

        /// <summary>Создаёт настройки прогрессии.</summary>
        /// <param name="id">Идентификатор записи, всегда <c>progression</c>.</param>
        /// <param name="levelBaseXp">Опыт на переход с первого уровня, больше нуля.</param>
        /// <param name="levelGrowthPercent">Рост требования к следующему уровню в процентах, не меньше нуля.</param>
        /// <param name="killXpPerRank">Базовый опыт за убийство на единицу ранга, больше нуля.</param>
        /// <param name="rarityBonusPercent">Прибавка к базовому опыту за единицу редкости в процентах, не меньше нуля.</param>
        /// <param name="maxRank">Предел ранга противника, больше нуля.</param>
        /// <param name="maxRarity">Предел редкости противника, больше нуля.</param>
        /// <param name="levelRewardStartLevel">Первый уровень, за достижение которого даётся награда, не меньше единицы.</param>
        /// <param name="levelUpTalentPoints">Сколько очков талантов даёт повышение уровня, не меньше нуля.</param>
        /// <param name="levelUpHealth">Прибавка к максимуму здоровья за повышение уровня, не меньше нуля.</param>
        /// <exception cref="ArgumentException">Идентификатор пуст.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Числовое поле вне допустимых границ.</exception>
        public ProgressionSettings(
            string id,
            int levelBaseXp,
            double levelGrowthPercent,
            int killXpPerRank,
            double rarityBonusPercent,
            int maxRank,
            int maxRarity,
            int levelRewardStartLevel,
            int levelUpTalentPoints,
            int levelUpHealth)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(id)),
                    nameof(id));
            }

            if (levelBaseXp < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(levelBaseXp));
            }

            if (levelGrowthPercent < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(levelGrowthPercent));
            }

            if (killXpPerRank < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(killXpPerRank));
            }

            if (rarityBonusPercent < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rarityBonusPercent));
            }

            if (maxRank < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRank));
            }

            if (maxRarity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRarity));
            }

            if (levelRewardStartLevel < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(levelRewardStartLevel));
            }

            if (levelUpTalentPoints < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(levelUpTalentPoints));
            }

            if (levelUpHealth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(levelUpHealth));
            }

            Id = id;
            LevelBaseXp = levelBaseXp;
            LevelGrowthPercent = levelGrowthPercent;
            KillXpPerRank = killXpPerRank;
            RarityBonusPercent = rarityBonusPercent;
            MaxRank = maxRank;
            MaxRarity = maxRarity;
            LevelRewardStartLevel = levelRewardStartLevel;
            LevelUpTalentPoints = levelUpTalentPoints;
            LevelUpHealth = levelUpHealth;
        }

        /// <summary>Идентификатор записи, всегда <c>progression</c>.</summary>
        public string Id { get; }

        /// <summary>Опыт на переход с первого уровня.</summary>
        public int LevelBaseXp { get; }

        /// <summary>Рост требования к следующему уровню в процентах от предыдущего.</summary>
        public double LevelGrowthPercent { get; }

        /// <summary>Базовый опыт за убийство на единицу ранга.</summary>
        public int KillXpPerRank { get; }

        /// <summary>Прибавка к базовому опыту за единицу редкости в процентах.</summary>
        public double RarityBonusPercent { get; }

        /// <summary>Предел ранга противника.</summary>
        public int MaxRank { get; }

        /// <summary>Предел редкости противника.</summary>
        public int MaxRarity { get; }

        /// <summary>Первый уровень, за достижение которого даётся награда.</summary>
        public int LevelRewardStartLevel { get; }

        /// <summary>Сколько очков талантов даёт повышение уровня.</summary>
        public int LevelUpTalentPoints { get; }

        /// <summary>Прибавка к максимуму здоровья за повышение уровня.</summary>
        public int LevelUpHealth { get; }

        /// <summary>Читает настройки прогрессии из записи данных.</summary>
        /// <param name="value">Запись вида <c>progression</c>.</param>
        /// <param name="path">Путь для сообщения об ошибке.</param>
        /// <returns>Настройки прогрессии.</returns>
        /// <exception cref="DataException">Поля отсутствуют, id не тот или значения вне границ.</exception>
        public static ProgressionSettings FromJson(JsonValue value, string path)
        {
            var id = DataMap.RequireString(value, "id", path);
            if (id != RecordId)
            {
                throw new DataException(path, CoreErrors.ProgressionId, id);
            }

            var levelBaseXp = DataMap.RequireInt(value, "levelBaseXp", path);
            if (levelBaseXp < 1)
            {
                throw new DataException(path, CoreErrors.ProgressionRange, "levelBaseXp");
            }

            var levelGrowthPercent = DataMap.RequireDouble(value, "levelGrowthPercent", path);
            if (levelGrowthPercent < 0)
            {
                throw new DataException(path, CoreErrors.ProgressionRange, "levelGrowthPercent");
            }

            var killXpPerRank = DataMap.RequireInt(value, "killXpPerRank", path);
            if (killXpPerRank < 1)
            {
                throw new DataException(path, CoreErrors.ProgressionRange, "killXpPerRank");
            }

            var rarityBonusPercent = DataMap.RequireDouble(value, "rarityBonusPercent", path);
            if (rarityBonusPercent < 0)
            {
                throw new DataException(path, CoreErrors.ProgressionRange, "rarityBonusPercent");
            }

            var maxRank = DataMap.RequireInt(value, "maxRank", path);
            if (maxRank < 1)
            {
                throw new DataException(path, CoreErrors.ProgressionRange, "maxRank");
            }

            var maxRarity = DataMap.RequireInt(value, "maxRarity", path);
            if (maxRarity < 1)
            {
                throw new DataException(path, CoreErrors.ProgressionRange, "maxRarity");
            }

            var levelRewardStartLevel = DataMap.RequireInt(value, "levelRewardStartLevel", path);
            if (levelRewardStartLevel < 1)
            {
                throw new DataException(path, CoreErrors.ProgressionRange, "levelRewardStartLevel");
            }

            var levelUpTalentPoints = DataMap.RequireInt(value, "levelUpTalentPoints", path);
            if (levelUpTalentPoints < 0)
            {
                throw new DataException(path, CoreErrors.ProgressionRange, "levelUpTalentPoints");
            }

            var levelUpHealth = DataMap.RequireInt(value, "levelUpHealth", path);
            if (levelUpHealth < 0)
            {
                throw new DataException(path, CoreErrors.ProgressionRange, "levelUpHealth");
            }

            return new ProgressionSettings(
                id,
                levelBaseXp,
                levelGrowthPercent,
                killXpPerRank,
                rarityBonusPercent,
                maxRank,
                maxRarity,
                levelRewardStartLevel,
                levelUpTalentPoints,
                levelUpHealth);
        }
    }
}
