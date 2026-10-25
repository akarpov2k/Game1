#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>Виджеты с абсолютной раскладкой от левого верхнего угла: рамки, подписи и иконки.</summary>
    public static class UiWidgets
    {
        /// <summary>Создаёт прямоугольник с абсолютной позицией от левого верхнего угла.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="parent">Родитель.</param>
        /// <param name="name">Имя объекта.</param>
        /// <param name="x">Отступ слева.</param>
        /// <param name="y">Отступ сверху.</param>
        /// <param name="width">Ширина.</param>
        /// <param name="height">Высота.</param>
        /// <param name="color">Цвет.</param>
        /// <returns>Созданное изображение.</returns>
        public static Image Box(
            IUiHost host,
            Transform parent,
            string name,
            float x,
            float y,
            float width,
            float height,
            Color color)
        {
            var image = host.Ui.CreateImage(name, parent, color);
            Place(image.rectTransform, x, y, width, height);
            return image;
        }

        /// <summary>Создаёт изображение спрайта с абсолютной позицией от левого верхнего угла.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="parent">Родитель.</param>
        /// <param name="name">Имя объекта.</param>
        /// <param name="sprite">Спрайт.</param>
        /// <param name="x">Отступ слева.</param>
        /// <param name="y">Отступ сверху.</param>
        /// <param name="width">Ширина.</param>
        /// <param name="height">Высота.</param>
        /// <returns>Созданное изображение.</returns>
        public static Image Icon(
            IUiHost host,
            Transform parent,
            string name,
            Sprite sprite,
            float x,
            float y,
            float width,
            float height)
        {
            var image = host.Ui.CreateImage(name, parent, Color.white);
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            Place(image.rectTransform, x, y, width, height);
            return image;
        }

        /// <summary>
        /// Создаёт иконку предмета, вписанную в слот по длинной стороне; вытянутый слот поворачивает её.
        /// </summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="parent">Родитель.</param>
        /// <param name="name">Имя объекта.</param>
        /// <param name="sprite">Спрайт предмета.</param>
        /// <param name="x">Отступ слева.</param>
        /// <param name="y">Отступ сверху.</param>
        /// <param name="width">Ширина слота.</param>
        /// <param name="height">Высота слота.</param>
        /// <param name="padding">Отступ иконки от краёв слота.</param>
        /// <returns>Созданное изображение; размер может быть нулевым, если слот слишком мал.</returns>
        public static Image ItemIcon(
            IUiHost host,
            Transform parent,
            string name,
            Sprite sprite,
            float x,
            float y,
            float width,
            float height,
            float padding)
        {
            var fit = IconFit.Compute(width, height, sprite.rect.width, sprite.rect.height, padding);
            var image = host.Ui.CreateImage(name, parent, Color.white);
            image.sprite = sprite;
            image.preserveAspect = false;
            image.raycastTarget = false;

            var rect = image.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x + (width / 2f), -(y + (height / 2f)));
            rect.sizeDelta = fit.Size;
            if (fit.Rotated)
            {
                // Оружие нарисовано стволом влево, магазином вниз. Поворот против часовой
                // (минус 90°) поднимает ствол вверх и оставляет магазин слева — правило проекта.
                rect.localRotation = Quaternion.Euler(0f, 0f, -90f);
            }

            return image;
        }

        /// <summary>Создаёт подпись с абсолютной позицией от левого верхнего угла.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="parent">Родитель.</param>
        /// <param name="name">Имя объекта.</param>
        /// <param name="text">Текст.</param>
        /// <param name="size">Размер шрифта.</param>
        /// <param name="anchor">Выравнивание.</param>
        /// <param name="color">Цвет.</param>
        /// <param name="x">Отступ слева.</param>
        /// <param name="y">Отступ сверху.</param>
        /// <param name="width">Ширина.</param>
        /// <param name="height">Высота.</param>
        /// <returns>Созданный текст.</returns>
        public static Text Label(
            IUiHost host,
            Transform parent,
            string name,
            string text,
            int size,
            TextAnchor anchor,
            Color color,
            float x,
            float y,
            float width,
            float height)
        {
            var label = host.Ui.CreateText(parent, text, size, anchor, color);
            label.gameObject.name = name;
            label.raycastTarget = false;
            Place(label.rectTransform, x, y, width, height);
            return label;
        }

        /// <summary>Ставит прямоугольник по абсолютной позиции от левого верхнего угла.</summary>
        /// <param name="rect">Прямоугольник.</param>
        /// <param name="x">Отступ слева.</param>
        /// <param name="y">Отступ сверху.</param>
        /// <param name="width">Ширина.</param>
        /// <param name="height">Высота.</param>
        public static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }
    }
}
