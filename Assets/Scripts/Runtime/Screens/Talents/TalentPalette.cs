#nullable enable
using UnityEngine;

namespace AnomalyPath.Runtime.Screens.Talents
{
    /// <summary>Цвета экрана талантов: карточки, иконки, связи и подсказка.</summary>
    public static class TalentPalette
    {
        /// <summary>Карточка доступного таланта.</summary>
        public static readonly Color Node = new Color(0.12f, 0.16f, 0.15f, 1f);

        /// <summary>Карточка закрытого таланта.</summary>
        public static readonly Color LockedNode = new Color(0.1f, 0.11f, 0.11f, 1f);

        /// <summary>Карточка полностью изученного таланта.</summary>
        public static readonly Color MaxNode = new Color(0.16f, 0.22f, 0.18f, 1f);

        /// <summary>Цвет иконки доступного таланта.</summary>
        public static readonly Color NodeIcon = new Color(0.95f, 0.96f, 0.93f, 1f);

        /// <summary>Цвет иконки полностью изученного таланта.</summary>
        public static readonly Color MaxIcon = new Color(0.95f, 0.88f, 0.68f, 1f);

        /// <summary>Цвет иконки закрытого таланта.</summary>
        public static readonly Color LockedIcon = new Color(0.42f, 0.45f, 0.43f, 1f);

        /// <summary>Цвет иконки, для которой нет спрайта.</summary>
        public static readonly Color MissingIcon = new Color(0.18f, 0.21f, 0.2f, 1f);

        /// <summary>Плашка ранга доступного таланта.</summary>
        public static readonly Color RankBadge = new Color(0.08f, 0.1f, 0.1f, 0.9f);

        /// <summary>Плашка ранга изученного таланта.</summary>
        public static readonly Color MaxBadge = new Color(0.32f, 0.42f, 0.3f, 0.95f);

        /// <summary>Связь между талантами.</summary>
        public static readonly Color Link = new Color(0.35f, 0.45f, 0.4f, 1f);

        /// <summary>Фон подсказки.</summary>
        public static readonly Color Tooltip = new Color(0.07f, 0.09f, 0.09f, 0.98f);

        /// <summary>Рамка подсказки.</summary>
        public static readonly Color TooltipBorder = new Color(0.45f, 0.54f, 0.48f, 1f);

        /// <summary>Ошибка.</summary>
        public static readonly Color Error = new Color(0.95f, 0.45f, 0.4f, 1f);
    }
}
