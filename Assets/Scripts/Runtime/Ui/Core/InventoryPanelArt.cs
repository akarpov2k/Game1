#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>
    /// Текстура окна инвентаря: id спрайта, положение плашки валюты и общие настройки изображения.
    /// Текстура отзеркалена при подготовке ассета, поэтому плашка валюты находится справа.
    /// </summary>
    public static class InventoryPanelArt
    {
        /// <summary>Id текстуры в каталоге спрайтов: файл <c>Assets/Art/Backdrops/inventory.png</c>.</summary>
        public const string SpriteId = "backdrops/inventory";

        /// <summary>Путь текстуры для инспектора префаба и редакторного сборщика.</summary>
        public const string TexturePath = "Assets/Art/Backdrops/inventory.png";

        /// <summary>
        /// Плашка валюты в долях текстуры: левый нижний угол и размер.
        /// Координаты посчитаны после зеркалирования: в исходном DDS плашка была слева.
        /// </summary>
        public static readonly Rect CurrencyPlate = new Rect(0.462f, 0.93f, 0.48f, 0.061f);

        /// <summary>Применяет текстуру окна; без спрайта оставляет запасной цвет.</summary>
        /// <param name="catalog">Каталог спрайтов.</param>
        /// <param name="image">Изображение окна.</param>
        /// <param name="fallback">Запасной цвет, если спрайта нет.</param>
        /// <returns><c>true</c>, если текстура найдена и применена.</returns>
        public static bool ApplyBackground(ISpriteCatalog catalog, Image image, Color fallback)
        {
            if (catalog.TryGet(SpriteId, out var sprite) && sprite != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
                return true;
            }

            image.sprite = null;
            image.color = fallback;
            return false;
        }

        /// <summary>Применяет текстуру окна через хост интерфейса.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="image">Изображение окна.</param>
        /// <param name="fallback">Запасной цвет, если спрайта нет.</param>
        /// <returns><c>true</c>, если текстура найдена и применена.</returns>
        public static bool ApplyBackground(IUiHost host, Image image, Color fallback) =>
            ApplyBackground(host.Sprites, image, fallback);

        /// <summary>Переводит долю текстуры в прямоугольник от левого верхнего угла.</summary>
        /// <param name="normalized">Доля текстуры, отсчёт снизу.</param>
        /// <param name="width">Ширина окна.</param>
        /// <param name="height">Высота окна.</param>
        /// <param name="offsetX">Смещение окна слева.</param>
        /// <param name="offsetY">Смещение окна сверху.</param>
        /// <returns>Прямоугольник с координатами от левого верхнего угла.</returns>
        public static Rect ToAbsolute(Rect normalized, float width, float height, float offsetX, float offsetY) =>
            new Rect(
                offsetX + (normalized.xMin * width),
                offsetY + ((1f - normalized.yMax) * height),
                normalized.width * width,
                normalized.height * height);

        /// <summary>Ставит подпись валюты по центру плашки и добавляет контур для читаемости.</summary>
        /// <param name="label">Подпись валюты.</param>
        public static void DecorateMoney(Text label)
        {
            label.alignment = TextAnchor.MiddleCenter;
            var outline = label.GetComponent<Outline>();
            if (outline == null)
            {
                outline = label.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
        }
    }
}
