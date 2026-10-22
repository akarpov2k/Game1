#nullable enable
using System;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Ui.Items
{
    /// <summary>
    /// Принимает предмет в конкретную клетку сетки, показывает контур будущего места
    /// и передаёт позицию и поворот обработчику экрана.
    /// </summary>
    public sealed class ItemGridDropZone : MonoBehaviour, IDropHandler
    {
        private ItemDragContext _context = null!;
        private int _columns;
        private int _rows;
        private float _cell;
        private float _gap;
        private float _step;
        private Func<string, int, int, bool, bool> _canPlace = null!;
        private Action<string, int, int, bool> _onDrop = null!;
        private RectTransform _rect = null!;
        private Image _highlight = null!;

        /// <summary>Настраивает сетку как цель перетаскивания.</summary>
        /// <param name="context">Общий контекст перетаскивания.</param>
        /// <param name="columns">Число колонок.</param>
        /// <param name="rows">Число строк.</param>
        /// <param name="cell">Сторона клетки в пикселях.</param>
        /// <param name="gap">Зазор между клетками.</param>
        /// <param name="canPlace">Проверка, помещается ли предмет в клетку с поворотом.</param>
        /// <param name="onDrop">Обработчик сброса: id экземпляра, колонка, строка, поворот.</param>
        public void Configure(
            ItemDragContext context,
            int columns,
            int rows,
            float cell,
            float gap,
            Func<string, int, int, bool, bool> canPlace,
            Action<string, int, int, bool> onDrop)
        {
            _context = context;
            _columns = Mathf.Max(1, columns);
            _rows = Mathf.Max(1, rows);
            _cell = Mathf.Max(1f, cell);
            _gap = Mathf.Max(0f, gap);
            _step = _cell + _gap;
            _canPlace = canPlace;
            _onDrop = onDrop;
            _rect = (RectTransform)transform;

            var preview = new GameObject("DropPreview", typeof(RectTransform), typeof(Image));
            preview.transform.SetParent(transform, false);
            _highlight = preview.GetComponent<Image>();
            _highlight.color = ItemPalette.DropAllowed;
            _highlight.raycastTarget = false;
            _highlight.enabled = false;
        }

        /// <inheritdoc />
        public void OnDrop(PointerEventData eventData)
        {
            if (_context == null || !_context.Active || _onDrop == null)
            {
                return;
            }

            if (_context.Origin != DragOrigin.Inventory)
            {
                HidePreview();
                _onDrop(_context.InstanceId, -1, -1, false);
                return;
            }

            _context.Move(eventData.position);
            if (!TryTarget(eventData.position, out var x, out var y))
            {
                HidePreview();
                return;
            }

            HidePreview();
            _onDrop(_context.InstanceId, x, y, _context.Rotated);
        }

        private void Update()
        {
            if (_context == null || _canPlace == null)
            {
                return;
            }

            if (!_context.Active || _context.Origin != DragOrigin.Inventory ||
                !TryTarget(_context.Pointer, out var x, out var y))
            {
                HidePreview();
                return;
            }

            var width = (_context.EffectiveWidth * _cell) + ((_context.EffectiveWidth - 1) * _gap);
            var height = (_context.EffectiveHeight * _cell) + ((_context.EffectiveHeight - 1) * _gap);
            UiWidgets.Place((RectTransform)_highlight.transform, x * _step, y * _step, width, height);
            _highlight.color = _canPlace(_context.InstanceId, x, y, _context.Rotated)
                ? ItemPalette.DropAllowed
                : ItemPalette.DropDenied;
            _highlight.enabled = true;
            _highlight.transform.SetAsLastSibling();
        }

        private bool TryTarget(Vector2 screenPosition, out int x, out int y)
        {
            x = 0;
            y = 0;
            if (_rect == null ||
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rect,
                    screenPosition,
                    null,
                    out var local))
            {
                return false;
            }

            var pointerX = Mathf.FloorToInt(local.x / _step);
            var pointerY = Mathf.FloorToInt(-local.y / _step);
            if (pointerX < 0 || pointerY < 0 || pointerX >= _columns || pointerY >= _rows)
            {
                return false;
            }

            var width = _context.EffectiveWidth;
            var height = _context.EffectiveHeight;
            if (width > _columns || height > _rows)
            {
                return false;
            }

            x = Mathf.Clamp(pointerX - _context.GrabOffsetX, 0, _columns - width);
            y = Mathf.Clamp(pointerY - _context.GrabOffsetY, 0, _rows - height);
            return true;
        }

        private void HidePreview()
        {
            if (_highlight != null)
            {
                _highlight.enabled = false;
            }
        }
    }
}
