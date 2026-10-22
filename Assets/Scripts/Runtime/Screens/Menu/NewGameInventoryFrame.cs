#nullable enable
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Loadouts;
using AnomalyPath.Runtime.Ui.Core;
using AnomalyPath.Runtime.Ui.Items;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Menu
{
    /// <summary>
    /// Правая панель экрана новой игры: окно инвентаря, в которое разложены все предметы набора
    /// как обычные экземпляры. Панель обтягивает сетку, клетки считаются от ширины, лимита ячеек нет.
    /// </summary>
    public sealed class NewGameInventoryFrame
    {
        private const float GridGap = 4f;
        private const float TitleHeight = 34f;
        private const float TitleTop = 12f;
        private const float GridTop = 60f;
        private const float MinCell = 24f;

        private readonly IUiHost _host;
        private readonly Transform _content;
        private LoadoutPreview? _preview;
        private string? _error;
        private int _money;

        /// <summary>Создаёт окно инвентаря набора.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="parent">Контент прокрутки экрана.</param>
        public NewGameInventoryFrame(IUiHost host, Transform parent)
        {
            _host = host;
            Root = new GameObject("NewGameInventoryFrame", typeof(RectTransform), typeof(Image));
            Root.transform.SetParent(parent, false);
            var rect = Root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(MenuMetrics.PreviewMinWidth, MenuMetrics.PreviewMinHeight);

            // Рамка — это сам корень панели: она не может растянуться отдельно от содержимого.
            var background = Root.GetComponent<Image>();
            background.color = MenuPalette.Frame;
            background.raycastTarget = false;
            _content = Root.transform;
        }

        /// <summary>Корень панели.</summary>
        public GameObject Root { get; }

        /// <summary>Высота панели, при которой сетка заполняет её: заголовок, клетки по ширине и отступ.</summary>
        /// <param name="width">Ширина панели.</param>
        /// <returns>Нужная высота панели.</returns>
        public float MeasureHeight(float width)
        {
            if (_preview == null)
            {
                return MenuMetrics.PreviewMinHeight;
            }

            var grid = _preview.Display;
            var cell = CellSize(width, grid.Width);
            var gridHeight = (grid.Rows * cell) + ((grid.Rows - 1) * GridGap);
            var required = GridTop + gridHeight + MenuMetrics.PreviewPadding;
            return Mathf.Max(MenuMetrics.PreviewMinHeight, required);
        }

        /// <summary>Меняет размер панели и перерисовывает содержимое.</summary>
        /// <param name="width">Ширина.</param>
        /// <param name="height">Высота.</param>
        public void Resize(float width, float height)
        {
            var rect = (RectTransform)Root.transform;
            rect.sizeDelta = new Vector2(Mathf.Max(1f, width), Mathf.Max(1f, height));
            if (_preview != null || _error != null)
            {
                Render();
            }
        }

        /// <summary>Перерисовывает окно инвентаря под выбранный набор.</summary>
        /// <param name="preview">Разложенный набор или <c>null</c>, если показать нечего.</param>
        /// <param name="error">Сообщение об ошибке или <c>null</c>.</param>
        /// <param name="money">Стартовые деньги персонажа.</param>
        public void Show(LoadoutPreview? preview, string? error, int money = 0)
        {
            _preview = preview;
            _error = error;
            _money = money;
            Render();
        }

        private static float CellSize(float width, int columns)
        {
            var available = Mathf.Max(1f, width - (2f * MenuMetrics.PreviewPadding));
            var cell = Mathf.Floor((available - ((columns - 1) * GridGap)) / Mathf.Max(1, columns));
            return Mathf.Max(MinCell, cell);
        }

        private void Render()
        {
            Clear();
            var size = ((RectTransform)Root.transform).rect.size;
            var width = size.x <= 1f ? MenuMetrics.PreviewMinWidth : size.x;
            var height = size.y <= 1f ? MenuMetrics.PreviewMinHeight : size.y;

            Label("PreviewTitle", UiText.Get(_host.Texts, UiTextKey.NewGame_Preview_Title), 26, TextAnchor.MiddleCenter, UiFactory.TextColor,
                0f, TitleTop, width, TitleHeight);

            if (_money > 0)
            {
                Label("PreviewMoney", UiText.Format(_host.Texts, UiTextKey.Common_Money, _money), 18, TextAnchor.MiddleRight,
                    UiFactory.MutedTextColor, width - MenuMetrics.PreviewPadding - 180f, TitleTop, 180f, TitleHeight);
            }

            if (_preview == null)
            {
                Label("PreviewEmpty", _error ?? UiText.Get(_host.Texts, UiTextKey.NewGame_Preview_Empty), 18, TextAnchor.MiddleCenter,
                    UiFactory.ToastColor, MenuMetrics.PreviewPadding, 120f,
                    width - (2f * MenuMetrics.PreviewPadding), 80f);
                return;
            }

            DrawInventory(_preview, width, height, _error);
        }

        private void DrawInventory(LoadoutPreview preview, float width, float height, string? error)
        {
            var grid = preview.Display;
            var padding = MenuMetrics.PreviewPadding;
            var availableWidth = Mathf.Max(1f, width - (2f * padding));

            // Клетки считаются от ширины: сетка заполняет панель по горизонтали, без боковых полей.
            var cell = CellSize(width, grid.Width);
            var gridWidth = (grid.Width * cell) + ((grid.Width - 1) * GridGap);
            var gridHeight = (grid.Rows * cell) + ((grid.Rows - 1) * GridGap);
            var availableHeight = Mathf.Max(1f, height - GridTop - padding);
            var gridX = padding + ((availableWidth - gridWidth) / 2f);
            var gridY = GridTop + Mathf.Max(0f, (availableHeight - gridHeight) / 2f);

            for (var row = 0; row < grid.Rows; row++)
            {
                for (var column = 0; column < grid.Width; column++)
                {
                    Box($"PreviewCell_{column}_{row}",
                        gridX + (column * (cell + GridGap)),
                        gridY + (row * (cell + GridGap)),
                        cell, cell, MenuPalette.Cell);
                }
            }

            var hidden = 0;
            foreach (var item in grid.Items)
            {
                if (item.Overflow || item.PlacedWidth <= 0 || item.PlacedHeight <= 0)
                {
                    hidden++;
                    continue;
                }

                var itemWidth = (item.PlacedWidth * cell) + ((item.PlacedWidth - 1) * GridGap);
                var itemHeight = (item.PlacedHeight * cell) + ((item.PlacedHeight - 1) * GridGap);
                var x = gridX + (item.X * (cell + GridGap));
                var y = gridY + (item.Y * (cell + GridGap));
                DrawItem(item, x, y, itemWidth, itemHeight);
            }

            if (hidden > 0 || preview.Rejected.Count > 0)
            {
                Label("PreviewNote",
                    UiText.Format(_host.Texts, UiTextKey.Common_Not_Placed, hidden, preview.Rejected.Count), 13, TextAnchor.UpperLeft,
                    UiFactory.ToastColor, padding, gridY + gridHeight + 8f, availableWidth, 44f);
            }

            if (!string.IsNullOrEmpty(error))
            {
                Label("PreviewError", error, 13, TextAnchor.LowerLeft, UiFactory.ToastColor,
                    padding, height - padding - 30f, availableWidth, 30f);
            }
        }

        /// <summary>Рисует предмет картинкой: без названия, количества и прочности — только изображение.</summary>
        private void DrawItem(ItemInstance item, float x, float y, float width, float height)
        {
            var suffix = item.ItemId + "_" + item.InstanceId;
            Box("PreviewItem_" + suffix, x, y, width, height, ItemPresentation.ColorFor(_host, item));

            var template = ItemPresentation.Template(_host, item);
            if (template == null || !_host.Sprites.TryGet(template.Icon, out var sprite) || sprite == null)
            {
                return;
            }

            if (Mathf.Min(width, height) <= 6f)
            {
                return;
            }

            UiWidgets.Icon(_host, _content, "PreviewItemIcon_" + suffix, sprite,
                x + 2f, y + 2f, width - 4f, height - 4f);
        }

        private Image Box(string name, float x, float y, float width, float height, Color color)
        {
            var image = UiWidgets.Box(_host, _content, name, x, y, width, height, color);
            image.raycastTarget = false;
            return image;
        }

        private Text Label(
            string name,
            string text,
            int size,
            TextAnchor anchor,
            Color color,
            float x,
            float y,
            float width,
            float height) =>
            UiWidgets.Label(_host, _content, name, text, size, anchor, color, x, y, width, height);

        private void Clear()
        {
            foreach (Transform child in _content)
            {
                child.gameObject.SetActive(false);
                Object.Destroy(child.gameObject);
            }
        }
    }
}
