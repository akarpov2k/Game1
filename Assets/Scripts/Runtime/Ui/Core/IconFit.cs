#nullable enable
using UnityEngine;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>Подбор размера и поворота иконки предмета под прямоугольник слота.</summary>
    public readonly struct IconFit
    {
        /// <summary>Создаёт результат подбора.</summary>
        /// <param name="rotated">Нужно ли повернуть изображение на 90°.</param>
        /// <param name="size">Размер прямоугольника изображения до поворота.</param>
        public IconFit(bool rotated, Vector2 size)
        {
            Rotated = rotated;
            Size = size;
        }

        /// <summary><c>true</c>, если иконку нужно повернуть на 90°.</summary>
        public bool Rotated { get; }

        /// <summary>Размер изображения до поворота; после поворота ширина и высота меняются местами.</summary>
        public Vector2 Size { get; }

        /// <summary>
        /// Вписывает спрайт в слот по длинной стороне: вытянутый слот получает повёрнутую иконку.
        /// </summary>
        /// <param name="slotWidth">Ширина слота.</param>
        /// <param name="slotHeight">Высота слота.</param>
        /// <param name="spriteWidth">Ширина спрайта.</param>
        /// <param name="spriteHeight">Высота спрайта.</param>
        /// <param name="padding">Отступ от краёв слота.</param>
        /// <returns>Размер и признак поворота; нулевой размер, если вписывать нечего.</returns>
        public static IconFit Compute(
            float slotWidth,
            float slotHeight,
            float spriteWidth,
            float spriteHeight,
            float padding)
        {
            var width = Mathf.Max(0f, slotWidth - (2f * padding));
            var height = Mathf.Max(0f, slotHeight - (2f * padding));
            if (width <= 0f || height <= 0f || spriteWidth <= 0f || spriteHeight <= 0f)
            {
                return new IconFit(false, Vector2.zero);
            }

            var squareSprite = Mathf.Approximately(spriteWidth, spriteHeight);
            var squareSlot = Mathf.Approximately(width, height);
            var rotated = !squareSprite && !squareSlot && (spriteWidth > spriteHeight) != (width > height);
            var fitWidth = rotated ? spriteHeight : spriteWidth;
            var fitHeight = rotated ? spriteWidth : spriteHeight;
            var scale = Mathf.Min(width / fitWidth, height / fitHeight);
            return new IconFit(rotated, new Vector2(spriteWidth * scale, spriteHeight * scale));
        }
    }
}
