#nullable enable
namespace AnomalyPath.Runtime.Screens.Menu
{
    /// <summary>
    /// Размеры экрана старта игры для сборщика префаба: стартовые значения, которые затем
    /// правятся в инспекторе. В рантайме геометрия берётся из префаба, а не из этих чисел.
    /// </summary>
    public static class MenuMetrics
    {
        /// <summary>Боковой отступ от края экрана.</summary>
        public const float SidePadding = 24f;

        /// <summary>Отступ сверху.</summary>
        public const float TopPadding = 16f;

        /// <summary>Отступ снизу.</summary>
        public const float BottomPadding = 16f;

        /// <summary>Зазор между рядами экрана.</summary>
        public const float RowSpacing = 16f;

        /// <summary>Высота строки с ником.</summary>
        public const float NicknameHeight = 44f;

        /// <summary>Ширина поля ника: под имя, а не на всю строку.</summary>
        public const float NicknameWidth = 320f;

        /// <summary>Сторона кнопки-кубика.</summary>
        public const float DiceSize = 44f;

        /// <summary>Сторона точки на грани кубика.</summary>
        public const float DicePipSize = 8f;

        /// <summary>Шаг между точками грани кубика.</summary>
        public const float DicePipStep = 12f;

        /// <summary>Отступ первой точки грани кубика.</summary>
        public const float DicePipInset = 9f;

        /// <summary>Внутренний отступ блока наборов.</summary>
        public const float LoadoutPadding = 12f;

        /// <summary>Зазор между кнопками наборов по вертикали.</summary>
        public const float LoadoutGap = 10f;

        /// <summary>Зазор между колонками наборов.</summary>
        public const float LoadoutColumnsGap = 24f;

        /// <summary>Высота кнопки набора.</summary>
        public const float LoadoutButtonHeight = 50f;

        /// <summary>Минимальная высота блока наборов.</summary>
        public const float LoadoutMinHeight = 74f;

        /// <summary>Зазор между колонкой настроек и набором экипировки.</summary>
        public const float ColumnGap = 24f;

        /// <summary>Минимальная ширина колонки настроек.</summary>
        public const float SettingsMinWidth = 360f;

        /// <summary>Минимальная ширина панели набора экипировки.</summary>
        public const float PreviewMinWidth = 320f;

        /// <summary>Минимальная высота верхнего ряда с колонками.</summary>
        public const float UpperMinHeight = 480f;

        /// <summary>Высота строки с кнопками старта и возврата.</summary>
        public const float ButtonsHeight = 56f;

        /// <summary>Высота строки состояния.</summary>
        public const float StatusHeight = 22f;

        /// <summary>Ширина полосы прокрутки набора экипировки.</summary>
        public const float PreviewScrollbarWidth = 10f;

        /// <summary>Внутренний отступ панели набора экипировки.</summary>
        public const float PreviewPadding = 16f;

        /// <summary>Верхняя граница области сетки: выше лежит плашка валюты.</summary>
        public const float PreviewPanelTop = 0.93f;

        /// <summary>Зазор между клетками сетки набора.</summary>
        public const float PreviewGridGap = 4f;

        /// <summary>Толщина линий сетки набора.</summary>
        public const float PreviewGridLine = 2f;

        /// <summary>Минимальный размер клетки набора.</summary>
        public const float PreviewMinCell = 18f;

        /// <summary>Высота строки настройки вместе с подписью и полем.</summary>
        public const float SettingRowHeight = 96f;

        /// <summary>Высота строки настройки с чекбоксом.</summary>
        public const float CheckboxRowHeight = 96f;

        /// <summary>Сторона квадратного чекбокса.</summary>
        public const float CheckboxSize = 44f;

        /// <summary>Толщина штриха галочки.</summary>
        public const float CheckboxTickWidth = 5f;

        /// <summary>Длина короткого штриха галочки.</summary>
        public const float CheckboxShortTick = 14f;

        /// <summary>Длина длинного штриха галочки.</summary>
        public const float CheckboxLongTick = 24f;

        /// <summary>Толщина контура квадратного чекбокса.</summary>
        public const float CheckboxOutline = 2f;

        /// <summary>Высота подписи настройки.</summary>
        public const float SettingLabelHeight = 30f;

        /// <summary>Внутренний отступ панели настроек.</summary>
        public const float SettingsPadding = 16f;

        /// <summary>Зазор между строками и колонками настроек.</summary>
        public const float SettingsGap = 16f;

        /// <summary>Ширина поля коэффициента: под две цифры, без растягивания.</summary>
        public const float NumberFieldWidth = 130f;

        /// <summary>Высота поля коэффициента.</summary>
        public const float NumberFieldHeight = 44f;

        /// <summary>Сторона кнопок шага у поля коэффициента.</summary>
        public const float StepButtonSize = 40f;
    }
}
