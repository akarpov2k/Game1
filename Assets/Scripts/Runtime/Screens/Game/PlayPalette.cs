#nullable enable
using UnityEngine;

namespace AnomalyPath.Runtime.Screens.Game
{
    /// <summary>Цвета основного игрового экрана.</summary>
    public static class PlayPalette
    {
        /// <summary>Фон панели действий.</summary>
        public static readonly Color ActionPanel = new Color(0.06f, 0.08f, 0.07f, 0.92f);

        /// <summary>Фон нижней полосы.</summary>
        public static readonly Color BottomBar = new Color(0.03f, 0.045f, 0.04f, 0.96f);

        /// <summary>Фон области объекта.</summary>
        public static readonly Color Object = new Color(0.13f, 0.17f, 0.15f, 0.72f);

        /// <summary>Цвет сообщения об ошибке действия.</summary>
        public static readonly Color Error = new Color(0.95f, 0.55f, 0.45f, 1f);

        /// <summary>Светлая половина циферблата: день.</summary>
        public static readonly Color ClockDay = new Color(0.64f, 0.5f, 0.24f, 1f);

        /// <summary>Тёмная половина циферблата: ночь.</summary>
        public static readonly Color ClockNight = new Color(0.1f, 0.16f, 0.28f, 1f);

        /// <summary>Обводка циферблата.</summary>
        public static readonly Color ClockBorder = new Color(0.9f, 0.84f, 0.64f, 1f);

        /// <summary>Стрелка часов.</summary>
        public static readonly Color ClockHand = new Color(1f, 0.94f, 0.72f, 1f);

        /// <summary>Значок солнца на дневной половине.</summary>
        public static readonly Color ClockSun = new Color(1f, 0.86f, 0.38f, 1f);

        /// <summary>Значок луны на ночной половине.</summary>
        public static readonly Color ClockMoon = new Color(0.82f, 0.88f, 1f, 1f);
    }
}
