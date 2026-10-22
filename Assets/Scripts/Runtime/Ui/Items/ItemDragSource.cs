#nullable enable
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Ui.Items
{
    /// <summary>Позволяет тащить предмет: пишет в контекст id и источник и ведёт подпись за курсором.</summary>
    public sealed class ItemDragSource : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private ItemDragContext _context = null!;
        private Text _ghost = null!;
        private string _instanceId = string.Empty;
        private string _label = string.Empty;
        private DragOrigin _origin;

        /// <summary>Настраивает источник перетаскивания.</summary>
        /// <param name="context">Общий контекст перетаскивания.</param>
        /// <param name="ghost">Подпись, которая летит за курсором.</param>
        /// <param name="instanceId">Id экземпляра.</param>
        /// <param name="label">Что показать рядом с курсором.</param>
        /// <param name="origin">Откуда тащат.</param>
        public void Configure(ItemDragContext context, Text ghost, string instanceId, string label, DragOrigin origin)
        {
            _context = context;
            _ghost = ghost;
            _instanceId = instanceId;
            _label = label;
            _origin = origin;
        }

        /// <inheritdoc />
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(_instanceId) || _context == null || _ghost == null)
            {
                return;
            }

            _context.Begin(_instanceId, _origin);
            _ghost.text = _label;
            _ghost.gameObject.SetActive(true);
            MoveGhost(eventData.position);
        }

        /// <inheritdoc />
        public void OnDrag(PointerEventData eventData)
        {
            if (_context != null && _context.Active)
            {
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

        private void MoveGhost(Vector2 screenPosition)
        {
            _ghost.rectTransform.position = new Vector3(screenPosition.x, screenPosition.y - 34f, 0f);
        }
    }
}
