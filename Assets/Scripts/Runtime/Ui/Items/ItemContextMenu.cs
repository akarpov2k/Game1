#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Ui.Items
{
    /// <summary>Всплывающее контекстное меню предмета: затемнение, список действий, закрытие кликом мимо.</summary>
    public sealed class ItemContextMenu
    {
        private const float MenuWidth = 300f;
        private const float RowHeight = 64f;

        private readonly UiFactory _ui;
        private readonly RectTransform _canvas;
        private readonly GameObject _root;
        private readonly RectTransform _column;
        private readonly List<GameObject> _entries = new List<GameObject>();

        /// <summary>Создаёт скрытое меню поверх канваса.</summary>
        /// <param name="canvas">Родительский канвас.</param>
        /// <param name="ui">Фабрика элементов.</param>
        public ItemContextMenu(Transform canvas, UiFactory ui)
        {
            _ui = ui;
            _canvas = (RectTransform)canvas;
            _root = ui.CreatePanel("ItemMenu", canvas);
            var blocker = _root.GetComponent<Image>();
            blocker.color = new Color(0f, 0f, 0f, 0.45f);
            var close = _root.AddComponent<Button>();
            close.targetGraphic = blocker;
            close.onClick.AddListener(Close);

            _column = (RectTransform)ui.CreateColumn(_root.transform, MenuWidth);
            _column.anchorMin = new Vector2(0f, 1f);
            _column.anchorMax = new Vector2(0f, 1f);
            _column.pivot = new Vector2(0f, 1f);
            _column.sizeDelta = new Vector2(MenuWidth, 0f);
            _root.SetActive(false);
        }

        /// <summary>Открыто ли меню.</summary>
        public bool IsOpen => _root.activeSelf;

        /// <summary>Корневой объект меню (для тестов).</summary>
        public GameObject Panel => _root;

        /// <summary>Показывает меню рядом с курсором.</summary>
        /// <param name="screenPosition">Позиция курсора.</param>
        /// <param name="entries">Подписи и действия.</param>
        public void Open(Vector2 screenPosition, IReadOnlyList<KeyValuePair<string, Action>> entries)
        {
            Clear();
            foreach (var entry in entries)
            {
                var action = entry.Value;
                var button = _ui.CreateButton(_column, entry.Key, () =>
                {
                    Close();
                    action();
                });
                _entries.Add(button.gameObject);
            }

            _root.SetActive(true);
            _root.transform.SetAsLastSibling();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas, screenPosition, null, out var local);
            var size = _canvas.rect.size;
            var height = (entries.Count * RowHeight) + 40f;
            var x = Mathf.Clamp(local.x + (size.x * 0.5f), 0f, Mathf.Max(0f, size.x - MenuWidth));
            var y = Mathf.Clamp(local.y - (size.y * 0.5f), -Mathf.Max(0f, size.y - height), 0f);
            _column.anchoredPosition = new Vector2(x, y);
        }

        /// <summary>Закрывает меню и убирает кнопки.</summary>
        public void Close()
        {
            _root.SetActive(false);
            Clear();
        }

        private void Clear()
        {
            foreach (var entry in _entries)
            {
                UnityEngine.Object.Destroy(entry);
            }

            _entries.Clear();
        }
    }
}
