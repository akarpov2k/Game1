#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Player;
using AnomalyPath.Runtime.Ui.Core;
using AnomalyPath.Runtime.Ui.Items;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Ui.Dialogs
{
    /// <summary>
    /// Модальный диалог лута: сетка выпавших предметов, кнопка «Забрать всё» и «Закрыть».
    /// Клик по предмету забирает его; не влезшее в инвентарь уходит в переполнение.
    /// </summary>
    public sealed class LootDialog
    {
        private readonly IUiHost _host;
        private readonly IInventoryService _inventory;
        private readonly Text _hint;
        private readonly Text _empty;
        private readonly RectTransform _grid;
        private readonly LayoutElement _gridLayout;
        private readonly Button _takeAll;
        private IReadOnlyList<ItemInstance> _loot = Array.Empty<ItemInstance>();
        private PlayerState? _player;
        private Action? _onClosed;

        /// <summary>Создаёт диалог и прячет его.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="inventory">Сервис инвентаря: перенос забранного лута.</param>
        /// <param name="parent">Родитель, обычно панель экрана.</param>
        /// <exception cref="ArgumentNullException">Хост или сервис не заданы.</exception>
        public LootDialog(IUiHost host, IInventoryService inventory, Transform parent)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));

            Panel = host.Ui.CreatePanel("LootDialog", parent);
            var overlay = Panel.GetComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.7f);
            overlay.raycastTarget = true;

            var box = host.Ui.CreateColumn(Panel.transform, LootMetrics.Width);
            var boxImage = box.gameObject.AddComponent<Image>();
            boxImage.color = LootMetrics.BoxColor;
            boxImage.raycastTarget = true;

            host.Ui.CreateTitle(box, UiText.Get(host.Texts, UiTextKey.Loot_Title));
            _hint = host.Ui.CreateText(
                box,
                UiText.Get(host.Texts, UiTextKey.Loot_Hint),
                LootMetrics.HintFontSize,
                TextAnchor.MiddleCenter);
            _hint.GetComponent<LayoutElement>().preferredHeight = LootMetrics.HintHeight;

            var gridGo = new GameObject("LootGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridGo.transform.SetParent(box, false);
            _grid = (RectTransform)gridGo.transform;
            var gridLayout = gridGo.GetComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(LootMetrics.Cell, LootMetrics.Cell);
            gridLayout.spacing = new Vector2(LootMetrics.Gap, LootMetrics.Gap);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = LootMetrics.Columns;
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            _gridLayout = gridGo.AddComponent<LayoutElement>();
            _gridLayout.preferredHeight = LootMetrics.Cell;

            _empty = host.Ui.CreateText(
                box,
                UiText.Get(host.Texts, UiTextKey.Loot_Empty),
                LootMetrics.HintFontSize,
                TextAnchor.MiddleCenter);
            _empty.GetComponent<LayoutElement>().preferredHeight = LootMetrics.HintHeight;
            _empty.gameObject.SetActive(false);

            var row = new GameObject("LootButtons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(box, false);
            var rowLayout = row.GetComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = LootMetrics.Gap;
            rowLayout.childControlWidth = true;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandHeight = false;
            row.AddComponent<LayoutElement>().preferredHeight = LootMetrics.ButtonHeight;

            _takeAll = host.Ui.CreateButton(
                row.transform,
                UiText.Get(host.Texts, UiTextKey.Loot_Take_All),
                TakeAll);
            _takeAll.GetComponent<Image>().color = LootMetrics.ConfirmColor;
            host.Ui.CreateButton(
                row.transform,
                UiText.Get(host.Texts, UiTextKey.Loot_Close),
                Close);

            Panel.transform.SetAsLastSibling();
            Panel.SetActive(false);
        }

        /// <summary>Панель диалога.</summary>
        public GameObject Panel { get; }

        /// <summary><c>true</c>, если диалог открыт.</summary>
        public bool IsOpen => Panel.activeSelf;

        /// <summary>Открывает диалог с выпавшим лутом.</summary>
        /// <param name="player">Состояние игрока: куда переносить предметы.</param>
        /// <param name="loot">Выпавшие экземпляры.</param>
        /// <param name="onClosed">Что выполнить после закрытия; <c>null</c> — ничего.</param>
        /// <exception cref="ArgumentNullException">Состояние игрока не задано.</exception>
        public void Show(PlayerState player, IReadOnlyList<ItemInstance> loot, Action? onClosed = null)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _loot = loot ?? Array.Empty<ItemInstance>();
            _onClosed = onClosed;
            Rebuild();
            Panel.SetActive(true);
            Panel.transform.SetAsLastSibling();
        }

        /// <summary>Esc закрывает диалог; незабранный лут пропадает.</summary>
        /// <returns><c>true</c>, если диалог был открыт и закрыт.</returns>
        public bool HandleEscape()
        {
            if (!Panel.activeSelf)
            {
                return false;
            }

            Close();
            return true;
        }

        /// <summary>Забирает весь лут в инвентарь и закрывает диалог.</summary>
        public void TakeAll()
        {
            if (_player == null)
            {
                return;
            }

            var overflowed = false;
            foreach (var item in _loot)
            {
                overflowed |= Take(item) > 0;
            }

            _loot = Array.Empty<ItemInstance>();
            Rebuild();
            if (overflowed)
            {
                _host.Toast(UiText.Get(_host.Texts, UiTextKey.Loot_Full));
            }

            Close();
        }

        private int Take(ItemInstance item)
        {
            _inventory.TryAdd(
                _player!.Inventory,
                item.ItemId,
                item.Stack,
                item.Durability,
                item.VariantKey,
                item.Magazine,
                out var overflow);
            return overflow.Count;
        }

        private void TakeOne(ItemInstance item)
        {
            if (_player == null)
            {
                return;
            }

            if (Take(item) > 0)
            {
                _host.Toast(UiText.Get(_host.Texts, UiTextKey.Loot_Full));
            }

            var remaining = new List<ItemInstance>(_loot);
            remaining.Remove(item);
            _loot = remaining;
            Rebuild();
            if (_loot.Count == 0)
            {
                Close();
            }
        }

        /// <summary>Скрывает диалог, не выполняя действие закрытия.</summary>
        public void Hide()
        {
            _onClosed = null;
            _player = null;
            _loot = Array.Empty<ItemInstance>();
            Panel.SetActive(false);
        }

        private void Close()
        {
            var action = _onClosed;
            Hide();
            action?.Invoke();
        }

        private void Rebuild()
        {
            foreach (Transform child in _grid)
            {
                child.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(child.gameObject);
            }

            for (var index = 0; index < _loot.Count; index++)
            {
                Cell(_loot[index], index);
            }

            var rows = Math.Max(1, (_loot.Count + LootMetrics.Columns - 1) / LootMetrics.Columns);
            _gridLayout.preferredHeight = (rows * LootMetrics.Cell) + ((rows - 1) * LootMetrics.Gap);
            _empty.gameObject.SetActive(_loot.Count == 0);
            _hint.gameObject.SetActive(_loot.Count > 0);
            _takeAll.interactable = _loot.Count > 0;
        }

        private void Cell(ItemInstance item, int index)
        {
            var cell = new GameObject("Loot_" + index, typeof(RectTransform), typeof(Image), typeof(Button));
            cell.transform.SetParent(_grid, false);
            var image = cell.GetComponent<Image>();
            image.raycastTarget = true;

            var template = ItemPresentation.Template(_host, item);
            Sprite? sprite = null;
            var hasIcon = template != null && _host.Sprites.TryGet(template.Icon, out sprite) && sprite != null;
            if (hasIcon)
            {
                image.sprite = sprite;
                image.color = Color.white;
                image.preserveAspect = true;
            }
            else
            {
                image.color = ItemPresentation.ColorFor(_host, item);
            }

            var button = cell.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => TakeOne(item));

            var label = _host.Ui.CreateText(
                cell.transform,
                ItemPresentation.Describe(_host, item),
                LootMetrics.CaptionFontSize,
                TextAnchor.LowerCenter);
            var labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(4f, 4f);
            labelRect.offsetMax = new Vector2(-4f, -4f);
            label.verticalOverflow = VerticalWrapMode.Truncate;
            var outline = label.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1f, -1f);
        }
    }
}
