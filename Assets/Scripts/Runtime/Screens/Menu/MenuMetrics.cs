#nullable enable
namespace AnomalyPath.Runtime.Screens.Menu
{
    /// <summary>
    /// Размеры экрана новой игры в единицах канваса: раскладка адаптивная, числа — границы,
    /// между которыми фреймы растягиваются под окно.
    /// </summary>
    public static class MenuMetrics
    {
        /// <summary>Боковой отступ от края экрана.</summary>
        public const float SidePadding = 60f;

        /// <summary>Отступ сверху под заголовок экрана.</summary>
        public const float TopPadding = 110f;

        /// <summary>Отступ снизу.</summary>
        public const float BottomPadding = 40f;

        /// <summary>Максимальная ширина колонки с настройками.</summary>
        public const float SettingsWidth = 760f;

        /// <summary>Минимальная ширина колонки с настройками.</summary>
        public const float SettingsMinWidth = 560f;

        /// <summary>Доля ширины области, которую занимает колонка настроек.</summary>
        public const float SettingsShare = 0.42f;

        /// <summary>Зазор между колонкой настроек и окном инвентаря.</summary>
        public const float ColumnGap = 48f;

        /// <summary>Минимальная ширина окна инвентаря.</summary>
        public const float PreviewMinWidth = 420f;

        /// <summary>Минимальная высота окна инвентаря.</summary>
        public const float PreviewMinHeight = 560f;

        /// <summary>Ширина полосы прокрутки справа.</summary>
        public const float ScrollbarWidth = 12f;

        /// <summary>Внутренний отступ окна инвентаря.</summary>
        public const float PreviewPadding = 24f;
    }
}
