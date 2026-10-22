#nullable enable
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Templates;
using AnomalyPath.Runtime.Ui.Core;
using AnomalyPath.Runtime.Ui.Items;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Character
{
    /// <summary>Виджеты экрана персонажа: раскладка, тексты предметов и перетаскивание.</summary>
    public static class CharacterWidgets
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
            Color color) =>
            UiWidgets.Box(host, parent, name, x, y, width, height, color);

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
            float height) =>
            UiWidgets.Icon(host, parent, name, sprite, x, y, width, height);

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
            float height) =>
            UiWidgets.Label(host, parent, name, text, size, anchor, color, x, y, width, height);

        /// <summary>Возвращает шаблон предмета или <c>null</c>, если его нет в данных.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="item">Экземпляр.</param>
        /// <returns>Шаблон или <c>null</c>.</returns>
        public static ItemTemplate? Template(IUiHost host, ItemInstance item) =>
            ItemPresentation.Template(host, item);

        /// <summary>Подпись предмета: название, количество, прочность.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="item">Экземпляр.</param>
        /// <returns>Текст для слота или клетки.</returns>
        public static string Describe(IUiHost host, ItemInstance item) => ItemPresentation.Describe(host, item);

        /// <summary>Цвет предмета по категории.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="item">Экземпляр.</param>
        /// <returns>Цвет плашки.</returns>
        public static Color ColorFor(IUiHost host, ItemInstance item) => ItemPresentation.ColorFor(host, item);

        /// <summary>Делает плашку перетаскиваемой и открывает меню по правому клику.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="frameHost">Экран персонажа.</param>
        /// <param name="target">Плашка.</param>
        /// <param name="item">Предмет.</param>
        /// <param name="slot">Слот снаряжения или <c>null</c>.</param>
        /// <param name="artifactIndex">Индекс слота артефакта или −1.</param>
        /// <param name="origin">Откуда начинается перетаскивание.</param>
        public static void AttachItem(
            IUiHost host,
            ICharacterFrameHost frameHost,
            Image target,
            ItemInstance item,
            EquipmentSlot? slot,
            int artifactIndex,
            DragOrigin origin)
        {
            var template = Template(host, item);
            var width = template != null
                ? template.FootprintWidth
                : item.Rotated ? Mathf.Max(1, item.PlacedHeight) : Mathf.Max(1, item.PlacedWidth);
            var height = template != null
                ? template.FootprintHeight
                : item.Rotated ? Mathf.Max(1, item.PlacedWidth) : Mathf.Max(1, item.PlacedHeight);

            var source = target.gameObject.AddComponent<ItemDragSource>();
            source.Configure(
                frameHost.Drag,
                frameHost.Ghost,
                item.InstanceId,
                Describe(host, item),
                origin,
                width,
                height,
                template?.Rotatable ?? false,
                item.Rotated);

            var trigger = target.gameObject.AddComponent<ItemMenuTrigger>();
            trigger.Configure(position => frameHost.OpenItemMenu(position, item, slot, artifactIndex));
        }

        /// <summary>Ставит прямоугольник по абсолютной позиции от левого верхнего угла.</summary>
        /// <param name="rect">Прямоугольник.</param>
        /// <param name="x">Отступ слева.</param>
        /// <param name="y">Отступ сверху.</param>
        /// <param name="width">Ширина.</param>
        /// <param name="height">Высота.</param>
        public static void PlaceRect(RectTransform rect, float x, float y, float width, float height) =>
            UiWidgets.Place(rect, x, y, width, height);
    }
}
