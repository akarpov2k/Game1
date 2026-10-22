#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>
    /// Фон экрана: растянутая картинка из каталога спрайтов, а если её нет — градиент из цветов темы.
    /// Фон не перехватывает клики и не мешает раскладке.
    /// </summary>
    public sealed class ScreenBackdrop
    {
        private static readonly Dictionary<string, Sprite> Gradients = new Dictionary<string, Sprite>();

        private readonly IUiHost _host;
        private readonly Image _image;

        /// <summary>Создаёт фон на всю панель.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="parent">Панель экрана.</param>
        public ScreenBackdrop(IUiHost host, Transform parent)
        {
            _host = host;
            _image = host.Ui.CreateImage("ScreenBackdrop", parent, Color.white);
            var rect = _image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            _image.raycastTarget = false;
            _image.preserveAspect = false;
            _image.type = Image.Type.Simple;
            _image.color = Color.white;
        }

        /// <summary>Применяет тему: картинка по id, иначе градиент.</summary>
        /// <param name="theme">Тема фона.</param>
        public void Apply(ScreenTheme theme)
        {
            var tint = theme.Tint;
            tint.a *= theme.Opacity;

            if (!string.IsNullOrEmpty(theme.SpriteId) &&
                _host.Sprites.TryGet(theme.SpriteId!, out var sprite) &&
                sprite != null)
            {
                _image.sprite = sprite;
                _image.color = tint;
                return;
            }

            _image.sprite = Gradient(theme.TopColor, theme.BottomColor);
            _image.color = tint;
        }

        private static Sprite Gradient(Color top, Color bottom)
        {
            var key = $"{top.r:F3}:{top.g:F3}:{top.b:F3}:{bottom.r:F3}:{bottom.g:F3}:{bottom.b:F3}";
            if (Gradients.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            const int width = 2;
            const int height = 256;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave
            };
            var pixels = new Color32[width * height];
            for (var y = 0; y < height; y++)
            {
                var color = Color.Lerp(bottom, top, (float)y / (height - 1));
                var bytes = (Color32)color;
                for (var x = 0; x < width; x++)
                {
                    pixels[(y * width) + x] = bytes;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, width, height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect);
            sprite.hideFlags = HideFlags.DontSave;
            Gradients[key] = sprite;
            return sprite;
        }
    }
}
