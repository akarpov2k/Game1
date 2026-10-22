#nullable enable
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnomalyPath.Runtime.Ui.Items
{
    /// <summary>Принимает брошенный предмет и передаёт id экземпляра обработчику экрана.</summary>
    public sealed class ItemDropZone : MonoBehaviour, IDropHandler
    {
        private ItemDragContext _context = null!;
        private Action<string> _onDrop = null!;

        /// <summary>Настраивает зону сброса.</summary>
        /// <param name="context">Общий контекст перетаскивания.</param>
        /// <param name="onDrop">Что делать с id брошенного предмета.</param>
        public void Configure(ItemDragContext context, Action<string> onDrop)
        {
            _context = context;
            _onDrop = onDrop;
        }

        /// <inheritdoc />
        public void OnDrop(PointerEventData eventData)
        {
            if (_context == null || !_context.Active || _onDrop == null)
            {
                return;
            }

            _onDrop(_context.InstanceId);
        }
    }
}
