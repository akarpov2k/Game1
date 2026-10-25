#nullable enable
namespace AnomalyPath.Core.Localization
{
    /// <summary>Часть класса <see cref="CoreErrors"/>: ошибки прогрессии и уровней.</summary>
    public static partial class CoreErrors
    {
        /// <summary>Настройки прогрессии не загружены.</summary>
        public const string ProgressionMissing = "error.progression_missing";

        /// <summary>Записей прогрессии должно быть ровно одна.</summary>
        public const string ProgressionSingleRecord = "error.progression_single_record";

        /// <summary>id записи прогрессии должен быть <c>progression</c>.</summary>
        public const string ProgressionId = "error.progression_id";

        /// <summary>Числовое поле прогрессии вне допустимых границ.</summary>
        public const string ProgressionRange = "error.progression_range";

        /// <summary>Уровень меньше единицы; сверху уровни не ограничены.</summary>
        public const string LevelOutOfRange = "error.level_out_of_range";

        /// <summary>Ранг противника вне диапазона, разрешённого данными.</summary>
        public const string RankOutOfRange = "error.rank_out_of_range";

        /// <summary>Редкость противника вне диапазона, разрешённого данными.</summary>
        public const string RarityOutOfRange = "error.rarity_out_of_range";
    }
}
