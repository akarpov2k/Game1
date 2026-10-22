#nullable enable
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnomalyPath.Runtime.Ui.Items
{
    /// <summary>Открывает контекстное меню предмета по правому клику.</summary>
    public sealed class ItemMenuTrigger : MonoBehaviour, IPointerClickHandler
    {
        private Action<Vector2> _onRightClick = null!;

        /// <summary>Настраивает обработчик правого клика.</summary>
        /// <param name="onRightClick">Куда передать позицию курсора.</param>
        public void Configure(Action<Vector2> onRightClick)
        {
            _onRightClick = onRightClick;
        }

        /// <inheritdoc />
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right || _onRightClick == null)
            {
                return;
            }

            _onRightClick(eventData.position);
        }
    }
}
