#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Game
{
    /// <summary>
    /// Список действий основного экрана: видны четыре кнопки, остальные доступны
    /// вертикальной прокруткой (колесом мыши или полосой справа).
    /// </summary>
    public sealed class PlayActionList
    {
        private readonly IUiHost _host;
        private readonly ScrollArea _scroll;
        private readonly RectTransform _content;
        private readonly List<GameObject> _items = new List<GameObject>();

        /// <summary>Создаёт список действий и область прокрутки.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="parent">Родитель, обычно панель действий.</param>
        public PlayActionList(IUiHost host, Transform parent)
        {
            _host = host;
            var go = new GameObject("ActionScroll", typeof(RectTransform), typeof(ScrollArea));
            go.transform.SetParent(parent, false);
            Root = (RectTransform)go.transform;
            _scroll = go.GetComponent<ScrollArea>();
            _scroll.Configure(PlayMetrics.ScrollbarWidth);
            _content = _scroll.Content;
        }

        /// <summary>Корень области прокрутки: экран сам ставит её в панель.</summary>
        public RectTransform Root { get; }

        /// <summary>Область прокрутки списка.</summary>
        public ScrollArea Scroll => _scroll;

        /// <summary>Перестраивает кнопки; список любой длины.</summary>
        /// <param name="actions">Действия в порядке показа.</param>
        /// <param name="emptyText">Что показать, если действий нет.</param>
        public void SetActions(IReadOnlyList<PlayAction> actions, string emptyText)
        {
            Clear();
            if (actions.Count == 0)
            {
                AddEmpty(emptyText);
                _scroll.SetContentHeight(PlayMetrics.ActionViewportHeight);
                return;
            }

            for (var index = 0; index < actions.Count; index++)
            {
                AddButton(actions[index], index);
            }

            var height = (2f * PlayMetrics.ActionPadding) +
                         (actions.Count * PlayMetrics.ActionButtonHeight) +
                         ((actions.Count - 1) * PlayMetrics.ActionGap);
            _scroll.SetContentHeight(Mathf.Max(PlayMetrics.ActionViewportHeight, height));
        }

        private void AddButton(PlayAction action, int index)
        {
            var button = _host.Ui.CreateButton(_content, action.Label, action.Invoke);
            button.gameObject.name = action.Name;
            var rect = (RectTransform)button.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            var top = PlayMetrics.ActionPadding +
                      (index * (PlayMetrics.ActionButtonHeight + PlayMetrics.ActionGap));
            rect.offsetMin = new Vector2(PlayMetrics.ActionPadding, -(top + PlayMetrics.ActionButtonHeight));
            rect.offsetMax = new Vector2(-PlayMetrics.ActionPadding, -top);
            _items.Add(button.gameObject);
        }

        private void AddEmpty(string text)
        {
            var label = _host.Ui.CreateText(
                _content,
                text,
                PlayMetrics.ActionEmptyFontSize,
                TextAnchor.MiddleCenter,
                UiFactory.MutedTextColor);
            label.gameObject.name = "ActionsEmpty";
            var rect = label.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(
                PlayMetrics.ActionPadding,
                -(PlayMetrics.ActionPadding + PlayMetrics.ActionEmptyHeight));
            rect.offsetMax = new Vector2(-PlayMetrics.ActionPadding, -PlayMetrics.ActionPadding);
            _items.Add(label.gameObject);
        }

        private void Clear()
        {
            foreach (var item in _items)
            {
                if (item == null)
                {
                    continue;
                }

                item.SetActive(false);
                UnityEngine.Object.Destroy(item);
            }

            _items.Clear();
        }
    }
}
