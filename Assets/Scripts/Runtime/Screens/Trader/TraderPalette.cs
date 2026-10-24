#nullable enable
using UnityEngine;

namespace AnomalyPath.Runtime.Screens.Trader
{
    /// <summary>Цвета экрана торговца.</summary>
    public static class TraderPalette
    {
        /// <summary>Фон строки товара.</summary>
        public static readonly Color Row = new Color(0.10f, 0.13f, 0.12f, 0.9f);

        /// <summary>Фон рамки портрета.</summary>
        public static readonly Color PortraitFrame = new Color(0.08f, 0.10f, 0.10f, 0.95f);

        /// <summary>Фон раздела.</summary>
        public static readonly Color Section = new Color(0.06f, 0.08f, 0.07f, 0.85f);

        /// <summary>Цвет сообщения об ошибке.</summary>
        public static readonly Color Error = new Color(0.95f, 0.55f, 0.45f, 1f);
    }
}
