#nullable enable
using System;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>
    /// Вертикальная прокрутка экрана: видимая область, контент и полоса прокрутки справа.
    /// Полоса появляется только когда контент выше области, и не наезжает на содержимое.
    /// </summary>
    public sealed class ScrollArea : MonoBehaviour
    {
        private static readonly Color TrackColor = new Color(0.12f, 0.14f, 0.14f, 0.85f);
        private static readonly Color HandleColor = new Color(0.42f, 0.50f, 0.46f, 0.95f);

        private ScrollRect _scroll = null!;
        private RectTransform _viewport = null!;
        private RectTransform _content = null!;
        private Scrollbar _scrollbar = null!;

        [Tooltip("Компонент прокрутки из префаба; пусто — область строится кодом через Configure")]
        [SerializeField]
        private ScrollRect? prefabScroll = null;

        [Tooltip("Видимая область прокрутки из префаба")]
        [SerializeField]
        private RectTransform? prefabViewport = null;

        [Tooltip("Контент прокрутки из префаба")]
        [SerializeField]
        private RectTransform? prefabContent = null;

        [Tooltip("Полоса прокрутки из префаба")]
        [SerializeField]
        private Scrollbar? prefabScrollbar = null;

        /// <summary>Размеры области прокрутки изменились — пора пересчитать раскладку.</summary>
        public event Action? Resized;

        /// <summary>Контент прокрутки: сюда кладутся фреймы экрана.</summary>
        public RectTransform Content => _content;

        /// <summary>Ширина видимой области в единицах канваса.</summary>
        public float ViewportWidth => _viewport != null ? _viewport.rect.width : 0f;

        /// <summary>Высота видимой области в единицах канваса.</summary>
        public float ViewportHeight => _viewport != null ? _viewport.rect.height : 0f;

        /// <summary>Полоса прокрутки; <c>null</c> до настройки.</summary>
        public Scrollbar Bar => _scrollbar;

        private void Awake()
        {
            if (prefabScroll == null || prefabViewport == null || prefabContent == null || prefabScrollbar == null)
            {
                return;
            }

            _scroll = prefabScroll;
            _viewport = prefabViewport;
            _content = prefabContent;
            _scrollbar = prefabScrollbar;
            _scroll.content = _content;
            _scroll.viewport = _viewport;
            _scroll.verticalScrollbar = _scrollbar;
            LayoutRebuilder.MarkLayoutForRebuild(_content);
        }

        /// <summary>Строит область прокрутки с полосой справа.</summary>
        /// <param name="barWidth">Ширина полосы прокрутки.</param>
        public void Configure(float barWidth)
        {
            _scroll = gameObject.AddComponent<ScrollRect>();

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
            viewportGo.transform.SetParent(transform, false);
            _viewport = viewportGo.GetComponent<RectTransform>();
            _viewport.anchorMin = Vector2.zero;
            _viewport.anchorMax = Vector2.one;
            _viewport.offsetMin = Vector2.zero;
            _viewport.offsetMax = new Vector2(-(barWidth + 8f), 0f);

            // Прозрачная плашка ловит колесо мыши: без Graphic области прокрутки нечего перехватывать.
            var catcher = viewportGo.GetComponent<Image>();
            catcher.color = new Color(0f, 0f, 0f, 0f);
            catcher.raycastTarget = true;

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(_viewport, false);
            _content = contentGo.GetComponent<RectTransform>();
            _content.anchorMin = new Vector2(0f, 1f);
            _content.anchorMax = new Vector2(1f, 1f);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.offsetMin = new Vector2(0f, 0f);
            _content.offsetMax = new Vector2(0f, 0f);

            _scrollbar = CreateScrollbar(barWidth);

            _scroll.content = _content;
            _scroll.viewport = _viewport;
            _scroll.horizontal = false;
            _scroll.vertical = true;
            _scroll.movementType = ScrollRect.MovementType.Clamped;
            _scroll.scrollSensitivity = 60f;
            _scroll.inertia = true;
            _scroll.decelerationRate = 0.135f;
            _scroll.verticalScrollbar = _scrollbar;
            _scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

            // Сохраняем ссылки в сериализуемые поля: префаб, собранный этим методом, восстановит их в Awake.
            prefabScroll = _scroll;
            prefabViewport = _viewport;
            prefabContent = _content;
            prefabScrollbar = _scrollbar;
        }

        /// <summary>Ставит высоту контента и возвращает прокрутку к началу.</summary>
        /// <param name="height">Высота контента в единицах канваса.</param>
        public void SetContentHeight(float height)
        {
            if (_content == null)
            {
                return;
            }

            _content.sizeDelta = new Vector2(0f, Mathf.Max(0f, height));
            _scroll.verticalNormalizedPosition = 1f;
        }

        private Scrollbar CreateScrollbar(float barWidth)
        {
            var go = new GameObject("Scrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            go.transform.SetParent(transform, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.offsetMin = new Vector2(-barWidth, 0f);
            rect.offsetMax = new Vector2(0f, 0f);

            var track = go.GetComponent<Image>();
            track.color = TrackColor;

            var sliding = new GameObject("Sliding Area", typeof(RectTransform));
            sliding.transform.SetParent(go.transform, false);
            var slidingRect = sliding.GetComponent<RectTransform>();
            slidingRect.anchorMin = Vector2.zero;
            slidingRect.anchorMax = Vector2.one;
            slidingRect.offsetMin = new Vector2(2f, 2f);
            slidingRect.offsetMax = new Vector2(-2f, -2f);

            var handleGo = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handleGo.transform.SetParent(sliding.transform, false);
            var handleRect = handleGo.GetComponent<RectTransform>();
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.offsetMin = Vector2.zero;
            handleRect.offsetMax = Vector2.zero;
            var handle = handleGo.GetComponent<Image>();
            handle.color = HandleColor;

            var bar = go.GetComponent<Scrollbar>();
            bar.targetGraphic = handle;
            bar.handleRect = handleRect;
            bar.direction = Scrollbar.Direction.BottomToTop;
            bar.value = 1f;
            bar.size = 0.5f;
            return bar;
        }

        private void OnRectTransformDimensionsChange()
        {
            if (_content != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(_content);
                Resized?.Invoke();
            }
        }
    }
}
