#nullable enable
namespace AnomalyPath.Runtime.Ui.Dialogs
{
    /// <summary>Разметка диалога лута: размеры в пикселях при опорном разрешении 1920×1080.</summary>
    public static class LootMetrics
    {
        /// <summary>Ширина окна диалога.</summary>
        public const float Width = 900f;

        /// <summary>Сторона клетки предмета.</summary>
        public const float Cell = 120f;

        /// <summary>Зазор между клетками и кнопками.</summary>
        public const float Gap = 8f;

        /// <summary>Сколько клеток в строке сетки.</summary>
        public const int Columns = 6;

        /// <summary>Высота строки кнопок.</summary>
        public const float ButtonHeight = 54f;

        /// <summary>Высота строки подсказки.</summary>
        public const float HintHeight = 34f;

        /// <summary>Размер шрифта подсказки.</summary>
        public const int HintFontSize = 18;

        /// <summary>Размер шрифта подписи предмета в клетке.</summary>
        public const int CaptionFontSize = 12;

        /// <summary>Цвет фона окна диалога.</summary>
        public static readonly UnityEngine.Color BoxColor = new UnityEngine.Color(0.12f, 0.16f, 0.15f, 1f);

        /// <summary>Цвет кнопки подтверждения.</summary>
        public static readonly UnityEngine.Color ConfirmColor = new UnityEngine.Color(0.24f, 0.48f, 0.30f, 1f);
    }
}
