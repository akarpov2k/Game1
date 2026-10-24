#nullable enable
using UnityEngine;

namespace AnomalyPath.Runtime.Screens.Menu
{
    /// <summary>Цвета экрана старта игры: панели, выбранный набор, кубик и клетки сетки.</summary>
    public static class MenuPalette
    {
        /// <summary>Фон панели набора экипировки.</summary>
        public static readonly Color Frame = new Color(0.09f, 0.11f, 0.12f, 1f);

        /// <summary>Фон панели настроек и блока наборов.</summary>
        public static readonly Color Panel = new Color(0.12f, 0.15f, 0.16f, 1f);

        /// <summary>Фон клетки сетки набора.</summary>
        public static readonly Color Cell = new Color(0.10f, 0.12f, 0.13f, 1f);

        /// <summary>Линии сетки набора поверх текстуры инвентаря.</summary>
        public static readonly Color GridLine = new Color(0.40f, 0.46f, 0.45f, 1f);

        /// <summary>Фон выбранного набора.</summary>
        public static readonly Color ButtonSelected = new Color(0.22f, 0.40f, 0.34f, 1f);

        /// <summary>Грань кубика.</summary>
        public static readonly Color DiceFace = new Color(0.90f, 0.90f, 0.86f, 1f);

        /// <summary>Точки на грани кубика.</summary>
        public static readonly Color DicePip = new Color(0.08f, 0.09f, 0.09f, 1f);

        /// <summary>Фон поля ввода: светлый, чтобы чёрный текст читался.</summary>
        public static readonly Color Field = new Color(0.94f, 0.95f, 0.91f, 1f);

        /// <summary>Насыщенный чёрный текст в полях ввода.</summary>
        public static readonly Color FieldText = new Color(0.02f, 0.02f, 0.02f, 1f);

        /// <summary>Подсказка в пустом поле ввода.</summary>
        public static readonly Color FieldHint = new Color(0.34f, 0.36f, 0.35f, 1f);
    }
}
