#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>
    /// Не даёт контенту прокрутки стать ниже видимой области: фреймы экрана растягиваются
    /// под высоту окна, а при переполнении контент становится выше и включается прокрутка.
    /// </summary>
    public sealed class ScrollContentMinHeight : MonoBehaviour, ILayoutElement
    {
        [Tooltip("Видимая область прокрутки, чья высота задаёт минимум контента")]
        [SerializeField]
        private RectTransform? viewport = null;

        /// <inheritdoc />
        public float minWidth => -1f;

        /// <inheritdoc />
        public float preferredWidth => -1f;

        /// <inheritdoc />
        public float flexibleWidth => -1f;

        /// <inheritdoc />
        public float minHeight => ViewportHeight;

        /// <inheritdoc />
        public float preferredHeight => ViewportHeight;

        /// <inheritdoc />
        public float flexibleHeight => -1f;

        /// <inheritdoc />
        public float maxWidth => -1f;

        /// <inheritdoc />
        public float maxHeight => -1f;

        /// <inheritdoc />
        public int layoutPriority => 0;

        /// <inheritdoc />
        public void CalculateLayoutInputHorizontal()
        {
        }

        /// <inheritdoc />
        public void CalculateLayoutInputVertical()
        {
        }

        private float ViewportHeight => viewport != null ? viewport.rect.height : 0f;
    }
}
