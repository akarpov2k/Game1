#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>
    /// Фон экрана: картинка, заданная в инспекторе или взятая по id из каталога спрайтов,
    /// а если картинки нет — градиент из цветов темы. Фон не перехватывает клики и не мешает раскладке.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class ScreenBackdrop : MonoBehaviour
    {
        private static readonly Dictionary<string, Sprite> Gradients = new Dictionary<string, Sprite>();

        [Tooltip("Компонент изображения задника; пусто — берётся с этого объекта")]
        [SerializeField]
        private Image? image = null;

        [Tooltip("Фоновое изображение из инспектора; если задано, используется вместо темы")]
        [SerializeField]
        private Sprite? sprite = null;

        [Tooltip("Id изображения в каталоге спрайтов; работает, если спрайт не задан")]
        [SerializeField]
        private string spriteId = string.Empty;

        private IUiHost? _host;

        /// <summary>Готовит задник к работе: запоминает хост и настраивает изображение.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="target">Компонент изображения; <c>null</c> — взять с этого объекта.</param>
        public void Initialize(IUiHost host, Image? target = null)
        {
            _host = host;
            if (target != null)
            {
                image = target;
            }

            if (image == null)
            {
                image = GetComponent<Image>();
            }

            if (image == null)
            {
                Debug.LogError(CoreText.Format(CoreErrors.BackdropImageMissing, name));
                return;
            }

            image.raycastTarget = false;
            image.preserveAspect = false;
            image.type = Image.Type.Simple;
            image.color = Color.white;
        }

        /// <summary>Применяет тему: спрайт из инспектора, затем id каталога, затем картинка темы, иначе градиент.</summary>
        /// <param name="theme">Тема фона.</param>
        public void Apply(ScreenTheme theme)
        {
            if (image == null)
            {
                return;
            }

            var tint = theme.Tint;
            tint.a *= theme.Opacity;

            var selected = sprite;
            if (selected == null && !string.IsNullOrEmpty(spriteId) && TryGetFromCatalog(spriteId, out var fromInspector))
            {
                selected = fromInspector;
            }

            if (selected == null &&
                !string.IsNullOrEmpty(theme.SpriteId) &&
                TryGetFromCatalog(theme.SpriteId!, out var fromTheme))
            {
                selected = fromTheme;
            }

            if (selected != null)
            {
                image.sprite = selected;
                image.color = tint;
                return;
            }

            image.sprite = Gradient(theme.TopColor, theme.BottomColor);
            image.color = tint;
        }

        /// <summary>Создаёт задник поверх панели: растянутое изображение с компонентом.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="parent">Панель экрана.</param>
        /// <returns>Готовый задник.</returns>
        public static ScreenBackdrop Create(IUiHost host, Transform parent)
        {
            var image = host.Ui.CreateImage("ScreenBackdrop", parent, Color.white);
            var rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var backdrop = image.gameObject.AddComponent<ScreenBackdrop>();
            backdrop.Initialize(host, image);
            return backdrop;
        }

        private bool TryGetFromCatalog(string id, out Sprite? found)
        {
            found = null;
            return _host != null && _host.Sprites.TryGet(id, out found) && found != null;
        }

        private void Reset()
        {
            image = GetComponent<Image>();
        }

        private void OnValidate()
        {
            if (image == null)
            {
                image = GetComponent<Image>();
            }
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
