#nullable enable
namespace AnomalyPath.Core.Localization
{
    /// <summary>Часть класса <see cref="CoreErrors"/>: ошибки сохранений.</summary>
    public static partial class CoreErrors
    {
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

        /// <summary>Некорректные настройки новой игры в сохранении.</summary>
        public const string SaveSettings = "error.save_settings";
    }
}
