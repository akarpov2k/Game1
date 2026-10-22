#nullable enable
namespace AnomalyPath.Runtime.Screens.Game
{
    /// <summary>Размеры основного игрового экрана в единицах канваса 1920×1080.</summary>
    public static class PlayMetrics
    {
        /// <summary>Боковой отступ верхней полосы.</summary>
        public const float SidePadding = 48f;

        /// <summary>Высота панели действий.</summary>
        public const float ActionPanelHeight = 340f;

        /// <summary>Высота заголовка панели действий.</summary>
        public const float ActionHeaderHeight = 40f;

        /// <summary>Высота текста заголовка панели действий.</summary>
        public const float ActionHeaderTextHeight = 30f;

        /// <summary>Отступ заголовка панели действий слева.</summary>
        public const float ActionHeaderX = 32f;

        /// <summary>Отступ заголовка панели действий сверху.</summary>
        public const float ActionHeaderY = 8f;

        /// <summary>Ширина заголовка панели действий.</summary>
        public const float ActionHeaderWidth = 500f;

        /// <summary>Боковой отступ области прокрутки действий.</summary>
        public const float ActionPanelSidePadding = 32f;

        /// <summary>Нижний отступ области прокрутки действий.</summary>
        public const float ActionPanelBottomPadding = 16f;

        /// <summary>Высота видимой части списка действий: ровно четыре кнопки.</summary>
        public const float ActionViewportHeight =
            (VisibleActions * ActionButtonHeight) + ((VisibleActions - 1) * ActionGap) + (2f * ActionPadding);

        /// <summary>Сколько кнопок действий видно без прокрутки.</summary>
        public const int VisibleActions = 4;

        /// <summary>Высота кнопки действия.</summary>
        public const float ActionButtonHeight = 54f;

        /// <summary>Зазор между кнопками действий.</summary>
        public const float ActionGap = 8f;

        /// <summary>Внутренний отступ списка действий.</summary>
        public const float ActionPadding = 12f;

        /// <summary>Ширина полосы прокрутки действий.</summary>
        public const float ScrollbarWidth = 10f;

        /// <summary>Высота пустой подписи списка действий.</summary>
        public const float ActionEmptyHeight = 40f;

        /// <summary>Отступ заголовка экрана сверху.</summary>
        public const float TitleTop = 16f;

        /// <summary>Ширина заголовка экрана.</summary>
        public const float TitleWidth = 900f;

        /// <summary>Высота заголовка экрана.</summary>
        public const float TitleHeight = 70f;

        /// <summary>Отступ сведений о локации сверху.</summary>
        public const float LocationSummaryTop = 88f;

        /// <summary>Ширина сведений о локации.</summary>
        public const float LocationSummaryWidth = 900f;

        /// <summary>Высота сведений о локации.</summary>
        public const float LocationSummaryHeight = 30f;

        /// <summary>Размер шрифта сведений о локации.</summary>
        public const int LocationSummaryFontSize = 20;

        /// <summary>Отступ часов сверху.</summary>
        public const float ClockTop = 128f;

        /// <summary>Сторона круглого циферблата.</summary>
        public const float ClockSize = 132f;

        /// <summary>Зазор между циферблатом и численным временем.</summary>
        public const float ClockTimeGap = 16f;

        /// <summary>Ширина подписи времени.</summary>
        public const float ClockTimeWidth = 280f;

        /// <summary>Высота подписи времени.</summary>
        public const float ClockTimeHeight = 70f;

        /// <summary>Размер шрифта подписи времени.</summary>
        public const int ClockTimeFontSize = 24;

        /// <summary>Отступ денег сверху.</summary>
        public const float MoneyTop = 20f;

        /// <summary>Ширина подписи денег.</summary>
        public const float MoneyWidth = 400f;

        /// <summary>Высота подписи денег.</summary>
        public const float MoneyHeight = 40f;

        /// <summary>Отступ всплывающего сообщения снизу.</summary>
        public const float ToastBottom = 20f;

        /// <summary>Ширина всплывающего сообщения.</summary>
        public const float ToastWidth = 900f;

        /// <summary>Высота всплывающего сообщения.</summary>
        public const float ToastHeight = 56f;

        /// <summary>Боковой отступ текста в области объекта.</summary>
        public const float ObjectTextPadding = 24f;

        /// <summary>Отступ заголовка объекта сверху.</summary>
        public const float ObjectTitleTop = 24f;

        /// <summary>Отступ подсказки объекта снизу.</summary>
        public const float ObjectHintBottom = 24f;

        /// <summary>Отступ подсказки объекта сверху.</summary>
        public const float ObjectHintTop = 8f;

        /// <summary>Размер шрифта денег.</summary>
        public const int MoneyFontSize = 22;

        /// <summary>Размер шрифта всплывающего сообщения.</summary>
        public const int ToastFontSize = 20;

        /// <summary>Размер шрифта заголовка объекта.</summary>
        public const int ObjectTitleFontSize = 34;

        /// <summary>Размер шрифта подсказки объекта.</summary>
        public const int ObjectHintFontSize = 18;

        /// <summary>Размер шрифта заголовка панели действий.</summary>
        public const int ActionHeaderFontSize = 26;

        /// <summary>Размер шрифта пустого списка действий.</summary>
        public const int ActionEmptyFontSize = 20;

        /// <summary>Высота нижней полосы с меню и рюкзаком.</summary>
        public const float BottomBarHeight = 100f;

        /// <summary>Ширина области объекта.</summary>
        public const float ObjectWidth = 620f;

        /// <summary>Высота области объекта.</summary>
        public const float ObjectHeight = 340f;

        /// <summary>Смещение объекта от центра экрана вверх.</summary>
        public const float ObjectOffsetY = 80f;

        /// <summary>Сторона кнопки-иконки в нижней полосе.</summary>
        public const float IconButtonSize = 56f;

        /// <summary>Отступ иконок от края нижней полосы.</summary>
        public const float IconPadding = 24f;
    }
}
