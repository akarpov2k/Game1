#nullable enable
using System;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Game
{
    /// <summary>
    /// Структура объекта локации: эллипс с названием и подсказкой, скрытый, пока в данных нет объекта.
    /// Локация может быть и без объекта, поэтому видимость включается только через <see cref="Show"/>.
    /// </summary>
    public sealed class PlayObject
    {
        private readonly GameObject _root;
        private readonly Text _title;
        private readonly Text _hint;
        private Action? _inspect;

        /// <summary>Создаёт скрытую область объекта.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="parent">Родитель, обычно панель основного экрана.</param>
        public PlayObject(IUiHost host, Transform parent)
        {
            _root = new GameObject("LocationObject", typeof(RectTransform), typeof(Image), typeof(Button));
            _root.transform.SetParent(parent, false);
            var rect = (RectTransform)_root.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, PlayMetrics.ObjectOffsetY);
            rect.sizeDelta = new Vector2(PlayMetrics.ObjectWidth, PlayMetrics.ObjectHeight);

            var image = _root.GetComponent<Image>();
            image.sprite = PlayArt.Circle();
            image.color = PlayPalette.Object;
            image.preserveAspect = false;
            image.type = Image.Type.Simple;

            var button = _root.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(Inspect);

            _title = host.Ui.CreateText(
                _root.transform,
                UiText.Get(host.Texts, UiTextKey.Game_Object),
                PlayMetrics.ObjectTitleFontSize,
                TextAnchor.MiddleCenter);
            _title.raycastTarget = false;
            var titleRect = _title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 0.5f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(PlayMetrics.ObjectTextPadding, 0f);
            titleRect.offsetMax = new Vector2(-PlayMetrics.ObjectTextPadding, -PlayMetrics.ObjectTitleTop);

            _hint = host.Ui.CreateText(
                _root.transform,
                UiText.Get(host.Texts, UiTextKey.Game_Object_Hint),
                PlayMetrics.ObjectHintFontSize,
                TextAnchor.MiddleCenter,
                UiFactory.MutedTextColor);
            _hint.raycastTarget = false;
            var hintRect = _hint.rectTransform;
            hintRect.anchorMin = new Vector2(0f, 0f);
            hintRect.anchorMax = new Vector2(1f, 0.5f);
            hintRect.offsetMin = new Vector2(PlayMetrics.ObjectTextPadding, PlayMetrics.ObjectHintBottom);
            hintRect.offsetMax = new Vector2(-PlayMetrics.ObjectTextPadding, -PlayMetrics.ObjectHintTop);

            _root.SetActive(false);
        }

        /// <summary>Показан ли объект.</summary>
        public bool Visible => _root.activeSelf;

        /// <summary>Показывает объект с названием, подсказкой и обработчиком.</summary>
        /// <param name="title">Название объекта.</param>
        /// <param name="hint">Подсказка действия.</param>
        /// <param name="inspect">Что выполнить по нажатию.</param>
        public void Show(string title, string hint, Action inspect)
        {
            _title.text = title;
            _hint.text = hint;
            _inspect = inspect;
            _root.SetActive(true);
        }

        /// <summary>Скрывает объект: у локации может не быть объекта.</summary>
        public void Hide()
        {
            _inspect = null;
            _root.SetActive(false);
        }

        private void Inspect() => _inspect?.Invoke();
    }
}
