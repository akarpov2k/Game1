#nullable enable
using System;
using UnityEngine;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>Сообщает, что прямоугольник изменил размер — на этом строятся адаптивные раскладки.</summary>
    public sealed class RectResizeWatcher : MonoBehaviour
    {
        private Vector2 _size = new Vector2(-1f, -1f);

        /// <summary>Размер прямоугольника изменился.</summary>
        public event Action? Resized;

        private void OnRectTransformDimensionsChange()
        {
            if (transform is not RectTransform rect)
            {
                return;
            }

            var size = rect.rect.size;
            if (Mathf.Approximately(size.x, _size.x) && Mathf.Approximately(size.y, _size.y))
            {
                return;
            }

            _size = size;
            Resized?.Invoke();
        }
    }
}
