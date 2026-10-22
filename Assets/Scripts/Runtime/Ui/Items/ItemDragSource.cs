#nullable enable
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Ui.Items
{
    /// <summary>Позволяет тащить предмет: пишет в контекст id и точку захвата, ведёт подпись за курсором и ловит R.</summary>
    public sealed class ItemDragSource : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private ItemDragContext _context = null!;
        private Text _ghost = null!;
        private string _instanceId = string.Empty;
        private string _label = string.Empty;
        private DragOrigin _origin;
        private int _footprintWidth = 1;
        private int _footprintHeight = 1;
        private bool _rotatable;
        private bool _rotated;

        /// <summary>Настраивает источник перетаскивания.</summary>
        /// <param name="context">Общий контекст перетаскивания.</param>
        /// <param name="ghost">Подпись, которая летит за курсором.</param>
        /// <param name="instanceId">Id экземпляра.</param>
        /// <param name="label">Что показать рядом с курсором.</param>
        /// <param name="origin">Откуда тащат.</param>
        /// <param name="footprintWidth">Ширина габарита в клетках без поворота.</param>
        /// <param name="footprintHeight">Высота габарита в клетках без поворота.</param>
        /// <param name="rotatable">Разрешён ли поворот.</param>
        /// <param name="rotated">Начальное состояние поворота.</param>
        public void Configure(
            ItemDragContext context,
            Text ghost,
            string instanceId,
            string label,
            DragOrigin origin,
            int footprintWidth,
            int footprintHeight,
            bool rotatable,
            bool rotated)
        {
            _context = context;
            _ghost = ghost;
            _instanceId = instanceId;
            _label = label;
            _origin = origin;
            _footprintWidth = footprintWidth;
            _footprintHeight = footprintHeight;
            _rotatable = rotatable;
            _rotated = rotated;
        }

        /// <inheritdoc />
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(_instanceId) || _context == null || _ghost == null)
            {
                return;
            }

            var rect = (RectTransform)transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rect,
                eventData.position,
                null,
                out var local);
            var width = Mathf.Max(1f, rect.rect.width);
            var height = Mathf.Max(1f, rect.rect.height);
            var fractionX = Mathf.Clamp01(local.x / width);
            var fractionY = Mathf.Clamp01(-local.y / height);

            _context.Begin(
                _instanceId,
                _origin,
                _footprintWidth,
                _footprintHeight,
                _rotatable,
                _rotated,
                fractionX,
                fractionY,
                eventData.position);
            _ghost.text = _label;
            _ghost.gameObject.SetActive(true);
            MoveGhost(eventData.position);
        }

        /// <inheritdoc />
        public void OnDrag(PointerEventData eventData)
        {
            if (_context != null && _context.Active)
            {
                _context.Move(eventData.position);
                MoveGhost(eventData.position);
            }
        }

        /// <inheritdoc />
        public void OnEndDrag(PointerEventData eventData)
        {
            if (_context != null)
            {
                _context.End();
            }

            if (_ghost != null)
            {
                _ghost.gameObject.SetActive(false);
            }
        }

        /// <summary>Поворачивает перетаскиваемый предмет на 90°, если это разрешено.</summary>
        /// <returns><c>true</c>, если поворот изменился.</returns>
        public bool Rotate()
        {
            if (_context == null || !_context.Active || _context.InstanceId != _instanceId)
            {
                return false;
            }

            return _context.ToggleRotation();
        }

        private void Update()
        {
            if (_context == null || !_context.Active || _context.InstanceId != _instanceId)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                Rotate();
            }
        }

        private void MoveGhost(Vector2 screenPosition)
        {
            _ghost.rectTransform.position = new Vector3(screenPosition.x, screenPosition.y - 34f, 0f);
        }
    }
}
