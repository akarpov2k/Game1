#nullable enable
using UnityEngine;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>Тема фона экрана: картинка из каталога спрайтов или градиент с оттенком.</summary>
    public sealed class ScreenTheme
    {
        private static readonly Color DefaultTop = new Color(0.035f, 0.055f, 0.06f, 1f);
        private static readonly Color DefaultBottom = new Color(0.10f, 0.135f, 0.12f, 1f);

        /// <summary>Создаёт тему фона.</summary>
        /// <param name="spriteId">Id картинки в каталоге спрайтов; <c>null</c> — рисовать градиент.</param>
        /// <param name="tint">Оттенок картинки.</param>
        /// <param name="topColor">Верхний цвет градиента.</param>
        /// <param name="bottomColor">Нижний цвет градиента.</param>
        /// <param name="opacity">Непрозрачность фона, 0..1.</param>
        public ScreenTheme(string? spriteId, Color tint, Color topColor, Color bottomColor, float opacity = 1f)
        {
            SpriteId = spriteId;
            Tint = tint;
            TopColor = topColor;
            BottomColor = bottomColor;
            Opacity = Mathf.Clamp01(opacity);
        }

        /// <summary>Id картинки в каталоге спрайтов; <c>null</c> — только градиент.</summary>
        public string? SpriteId { get; }

        /// <summary>Оттенок картинки.</summary>
        public Color Tint { get; }

        /// <summary>Верхний цвет градиента.</summary>
        public Color TopColor { get; }

        /// <summary>Нижний цвет градиента.</summary>
        public Color BottomColor { get; }

        /// <summary>Непрозрачность фона, 0..1.</summary>
        public float Opacity { get; }

        /// <summary>Тема по умолчанию: общий тематический задник окон и тёмный градиент.</summary>
        public static ScreenTheme Default { get; } =
            new ScreenTheme("backdrops/screen", Color.white, DefaultTop, DefaultBottom);

        /// <summary>Тема с фоном локации: картинка локации приглушается, чтобы не спорить с интерфейсом.</summary>
        /// <param name="backgroundId">Id фона локации; <c>null</c> — тема по умолчанию.</param>
        /// <returns>Тема с картинкой локации.</returns>
        public static ScreenTheme Location(string? backgroundId) =>
            backgroundId == null
                ? Default
                : new ScreenTheme(backgroundId, new Color(1f, 1f, 1f, 0.55f), DefaultTop, DefaultBottom, 0.55f);

        /// <summary>Тема с фоном локации без приглушения: основной игровой экран.</summary>
        /// <param name="backgroundId">Id фона локации; пусто — тема по умолчанию.</param>
        /// <returns>Тема с картинкой локации во всю яркость.</returns>
        public static ScreenTheme LocationFull(string? backgroundId) =>
            string.IsNullOrEmpty(backgroundId)
                ? Default
                : new ScreenTheme(backgroundId, Color.white, DefaultTop, DefaultBottom, 1f);
    }
}
