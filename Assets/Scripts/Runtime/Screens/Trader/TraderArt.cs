#nullable enable
using AnomalyPath.Core.Combat;
using UnityEngine;

namespace AnomalyPath.Runtime.Screens.Trader
{
    /// <summary>
    /// Процедурный портрет-заглушка торговца: силуэт в полный рост или по пояс.
    /// Используется, пока в каталоге спрайтов нет картинки по id из данных.
    /// </summary>
    public static class TraderArt
    {
        private static Sprite? _full;
        private static Sprite? _waist;

        /// <summary>Возвращает портрет-заглушку нужного кадрирования.</summary>
        /// <param name="mode">Кадрирование.</param>
        /// <returns>Спрайт-заглушка.</returns>
        public static Sprite Get(PortraitMode mode)
        {
            if (mode == PortraitMode.Waist)
            {
                return _waist ??= Build(mode);
            }

            return _full ??= Build(mode);
        }

        private static Sprite Build(PortraitMode mode)
        {
            var width = 256;
            var height = mode == PortraitMode.Waist ? 256 : 384;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            var baseColor = mode == PortraitMode.Waist
                ? new Color(0.34f, 0.42f, 0.38f, 1f)
                : new Color(0.40f, 0.36f, 0.30f, 1f);
            var dark = Color.Lerp(baseColor, Color.black, 0.55f);
            var headRadius = height * 0.075f;
            var headY = mode == PortraitMode.Waist ? height * 0.72f : height * 0.82f;
            var shoulderY = mode == PortraitMode.Waist ? height * 0.52f : height * 0.68f;
            var torsoBottom = mode == PortraitMode.Waist ? height * 0.10f : height * 0.30f;
            var torsoHalf = width * 0.17f;
            var centerX = width / 2f;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var dx = x - centerX;
                    var inHead = (dx * dx) + ((y - headY) * (y - headY)) <= headRadius * headRadius;
                    var inTorso = Mathf.Abs(dx) <= torsoHalf && y >= torsoBottom && y <= shoulderY;
                    var inLegs = mode == PortraitMode.Full &&
                                 y >= height * 0.04f && y < torsoBottom &&
                                 ((dx >= -torsoHalf * 0.9f && dx <= -torsoHalf * 0.1f) ||
                                  (dx >= torsoHalf * 0.1f && dx <= torsoHalf * 0.9f));
                    if (!inHead && !inTorso && !inLegs)
                    {
                        continue;
                    }

                    var t = (float)y / height;
                    pixels[(y * width) + x] = Color.Lerp(dark, baseColor, t * 0.8f);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, width, height),
                new Vector2(0.5f, 0.5f),
                100f);
        }
    }
}
