#nullable enable
using UnityEngine;

namespace AnomalyPath.Runtime.Screens.Game
{
    /// <summary>Простые значки игрового экрана, которые рисуются в памяти без отдельных ассетов.</summary>
    public static class PlayArt
    {
        private const int Size = 128;
        private const int ClockPixels = 256;

        private static Sprite? _circle;
        private static Sprite? _gear;
        private static Sprite? _clockFace;
        private static Sprite? _clockHand;

        /// <summary>Белый круг для области объекта; растягивается в эллипс.</summary>
        /// <returns>Спрайт круга.</returns>
        public static Sprite Circle()
        {
            if (_circle == null)
            {
                _circle = BuildCircle();
            }

            return _circle;
        }

        /// <summary>Белая шестерёнка для кнопки меню.</summary>
        /// <returns>Спрайт шестерёнки.</returns>
        public static Sprite Gear()
        {
            if (_gear == null)
            {
                _gear = BuildGear();
            }

            return _gear;
        }

        /// <summary>Круглый циферблат: правая половина — день, левая — ночь.</summary>
        /// <returns>Спрайт циферблата.</returns>
        public static Sprite ClockFace()
        {
            if (_clockFace == null)
            {
                _clockFace = BuildClockFace();
            }

            return _clockFace;
        }

        /// <summary>Стрелка часов, направленная вверх в исходном положении.</summary>
        /// <returns>Спрайт стрелки.</returns>
        public static Sprite ClockHand()
        {
            if (_clockHand == null)
            {
                _clockHand = BuildClockHand();
            }

            return _clockHand;
        }

        private static Sprite BuildCircle()
        {
            var center = (Size - 1) * 0.5f;
            var radius = (Size * 0.5f) - 2f;
            var pixels = new Color32[Size * Size];
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var dx = x - center;
                    var dy = y - center;
                    var distance = Mathf.Sqrt((dx * dx) + (dy * dy));
                    var alpha = Mathf.Clamp01(radius - distance + 1f);
                    pixels[(y * Size) + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            return CreateSprite(pixels, "PlayCircle");
        }

        private static Sprite BuildGear()
        {
            var center = (Size - 1) * 0.5f;
            const float outer = 58f;
            const float inner = 46f;
            const float hole = 14f;
            var pixels = new Color32[Size * Size];
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var dx = x - center;
                    var dy = y - center;
                    var distance = Mathf.Sqrt((dx * dx) + (dy * dy));
                    var angle = Mathf.Atan2(dy, dx);
                    var teeth = (Mathf.Cos(angle * 8f) + 1f) * 0.5f;
                    var rim = inner + ((outer - inner) * Mathf.SmoothStep(0.35f, 1f, teeth));
                    var solid = distance <= inner || (distance <= rim && teeth > 0.5f);
                    var alpha = solid && distance >= hole ? 1f : 0f;
                    if (distance > hole && distance < hole + 1.5f)
                    {
                        alpha = Mathf.Clamp01((distance - hole) / 1.5f);
                    }

                    pixels[(y * Size) + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            return CreateSprite(pixels, "PlayGear");
        }

        private static Sprite BuildClockFace()
        {
            var center = (ClockPixels - 1) * 0.5f;
            var radius = center - 2f;
            var pixels = new Color32[ClockPixels * ClockPixels];
            for (var y = 0; y < ClockPixels; y++)
            {
                for (var x = 0; x < ClockPixels; x++)
                {
                    var dx = (x - center) / radius;
                    var dy = (y - center) / radius;
                    var distance = Mathf.Sqrt((dx * dx) + (dy * dy));
                    if (distance > 1f)
                    {
                        continue;
                    }

                    var color = dx >= 0f ? PlayPalette.ClockDay : PlayPalette.ClockNight;
                    var angle = Mathf.Atan2(dy, dx);
                    var tick = Mathf.Abs(angle - (Mathf.Round(angle / (Mathf.PI / 6f)) * (Mathf.PI / 6f)));
                    if (distance >= 0.82f && distance <= 0.92f && tick < 0.02f)
                    {
                        color = PlayPalette.ClockBorder;
                    }

                    if (distance >= 0.94f)
                    {
                        color = PlayPalette.ClockBorder;
                    }

                    var sunDistance = Mathf.Sqrt(((dx - 0.45f) * (dx - 0.45f)) + (dy * dy));
                    if (sunDistance <= 0.1f || (sunDistance >= 0.16f && sunDistance <= 0.19f))
                    {
                        color = PlayPalette.ClockSun;
                    }

                    var moonDistance = Mathf.Sqrt(((dx + 0.45f) * (dx + 0.45f)) + (dy * dy));
                    if (moonDistance <= 0.12f)
                    {
                        color = PlayPalette.ClockMoon;
                    }

                    var crescent = Mathf.Sqrt(((dx + 0.395f) * (dx + 0.395f)) + ((dy - 0.03f) * (dy - 0.03f)));
                    if (crescent <= 0.1f)
                    {
                        color = PlayPalette.ClockNight;
                    }

                    pixels[(y * ClockPixels) + x] = color;
                }
            }

            return CreateSprite(pixels, "PlayClockFace", ClockPixels);
        }

        private static Sprite BuildClockHand()
        {
            var center = (ClockPixels - 1) * 0.5f;
            var radius = center - 2f;
            var pixels = new Color32[ClockPixels * ClockPixels];
            for (var y = 0; y < ClockPixels; y++)
            {
                for (var x = 0; x < ClockPixels; x++)
                {
                    var dx = (x - center) / radius;
                    var dy = (y - center) / radius;
                    var hub = Mathf.Sqrt((dx * dx) + (dy * dy));
                    var visible = false;
                    if (dy >= 0f && dy <= 0.79f)
                    {
                        var width = Mathf.Lerp(0.026f, 0.006f, dy / 0.79f);
                        visible = Mathf.Abs(dx) <= width;
                    }
                    else if (dy < 0f && dy >= -0.12f)
                    {
                        visible = Mathf.Abs(dx) <= 0.012f;
                    }

                    if (hub <= 0.045f)
                    {
                        visible = true;
                    }

                    if (visible)
                    {
                        pixels[(y * ClockPixels) + x] = hub <= 0.045f ? PlayPalette.ClockBorder : PlayPalette.ClockHand;
                    }
                }
            }

            return CreateSprite(pixels, "PlayClockHand", ClockPixels);
        }

        private static Sprite CreateSprite(Color32[] pixels, string name, int size = Size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = name,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave
            };
            texture.SetPixels32(pixels);
            texture.Apply();
            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect);
            sprite.name = name;
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }
    }
}
