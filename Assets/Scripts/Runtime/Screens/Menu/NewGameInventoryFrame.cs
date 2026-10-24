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
    /// Правая панель экрана старта: набор экипировки в собственной прокрутке. Клетки считаются
    /// от ширины видимой области, высота контента — от сетки; полоса прокрутки появляется только
    /// при переполнении и не перекрывает содержимое.
    /// </summary>
    public sealed class NewGameInventoryFrame : MonoBehaviour
    {
        [Tooltip("Прокрутка панели: своя у набора, независимая от остальных областей экрана")]
        [SerializeField]
        private ScrollArea scroll = null!;

        [Tooltip("Текстура окна из инспектора: работает до пересборки каталога спрайтов")]
        [SerializeField]
        private Texture2D? panelTexture = null;

        [Tooltip("Подпись денег в верхней плашке окна набора")]
        [SerializeField]
        private Text? moneyLabel = null;

        [Tooltip("Подпись пустого или ошибочного состояния")]
        [SerializeField]
        private Text? emptyLabel = null;

        [Tooltip("Контейнер динамической сетки: клетки и предметы создаются кодом")]
        [SerializeField]
        private RectTransform? gridRoot = null;

        [Tooltip("Зазор между клетками сетки")]
        [SerializeField]
        private float gridGap = 0f;

        [Tooltip("Минимальный размер клетки")]
        [SerializeField]
        private float minCell = 0f;

        [Tooltip("Внутренний отступ сетки от краёв области")]
        [SerializeField]
        private float padding = 0f;

        [Tooltip("Толщина линий сетки поверх текстуры")]
        [SerializeField]
        private float gridLine = 2f;

        private IUiHost _host = null!;
        private LoadoutPreview? _preview;
        private string? _error;
        private int _money;
        private float _renderedWidth = -1f;
        private float _renderedHeight = -1f;
        private bool _hasPanelTexture;
        private Sprite? _panelSprite;

        /// <summary>Связывает панель с хостом интерфейса.</summary>
        /// <param name="host">Хост интерфейса.</param>
        public void Bind(IUiHost host)
        {
            _host = host;
            ApplyPanelArt();
        }

        /// <summary>Перерисовывает набор под выбранный комплект.</summary>
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

        private void LateUpdate()
        {
            if (scroll == null)
            {
                return;
            }

            var width = scroll.ViewportWidth;
            var height = scroll.ViewportHeight;
            if (Mathf.Approximately(width, _renderedWidth) && Mathf.Approximately(height, _renderedHeight))
            {
                return;
            }

            if (_preview != null || _error != null)
            {
                Render();
            }
        }

        private void Render()
        {
            if (scroll == null || gridRoot == null)
            {
                return;
            }

            var width = Mathf.Max(1f, scroll.ViewportWidth);
            _renderedWidth = width;
            _renderedHeight = scroll.ViewportHeight;

            UpdateMoney();
            UpdateEmpty();

            Clear();
            if (_preview == null)
            {
                scroll.SetContentHeight(scroll.ViewportHeight);
                return;
            }

            var grid = _preview.Display;
            var cell = CellSize(width, grid.Width);
            var gridWidth = (grid.Width * cell) + ((grid.Width - 1) * gridGap);
            var gridHeight = (grid.Rows * cell) + ((grid.Rows - 1) * gridGap);
            var hidden = HiddenCount(grid, _preview.Rejected.Count);
            var noteHeight = hidden > 0 ? 20f : 0f;
            var contentHeight = Mathf.Max(
                scroll.ViewportHeight,
                padding + gridHeight + noteHeight + padding);

            scroll.SetContentHeight(contentHeight);

            var x = padding + Mathf.Max(0f, (width - (2f * padding) - gridWidth) / 2f);
            var y = padding;
            DrawCells(grid, cell, x, y);
            DrawGridLines(gridWidth, gridHeight, grid.Width, grid.Rows, cell, x, y);
            DrawItems(grid, cell, x, y);

            if (hidden > 0)
            {
                var note = UiWidgets.Label(
                    _host,
                    gridRoot!,
                    "PreviewNote",
                    UiText.Format(_host.Texts, UiTextKey.Common_Not_Placed, hidden, _preview.Rejected.Count),
                    13,
                    TextAnchor.UpperLeft,
                    UiFactory.ToastColor,
                    padding,
                    y + gridHeight + 4f,
                    width - (2f * padding),
                    noteHeight);
                note.raycastTarget = false;
            }
        }

        private static int HiddenCount(GridContainer grid, int rejected)
        {
            var hidden = rejected;
            foreach (var item in grid.Items)
            {
                if (item.Overflow || item.PlacedWidth <= 0 || item.PlacedHeight <= 0)
                {
                    hidden++;
                }
            }

            return hidden;
        }

        private void DrawCells(GridContainer grid, float cell, float originX, float originY)
        {
            var color = _hasPanelTexture ? Color.clear : MenuPalette.Cell;
            for (var row = 0; row < grid.Rows; row++)
            {
                for (var column = 0; column < grid.Width; column++)
                {
                    Box(
                        $"PreviewCell_{column}_{row}",
                        originX + (column * (cell + gridGap)),
                        originY + (row * (cell + gridGap)),
                        cell,
                        cell,
                        color);
                }
            }
        }

        private void DrawGridLines(
            float width,
            float height,
            int columns,
            int rows,
            float cell,
            float originX,
            float originY)
        {
            var line = Mathf.Max(1f, gridLine);
            var half = line / 2f;
            Line("GridTop", originX - line, originY - line, width + (line * 2f), line);
            Line("GridBottom", originX - line, originY + height, width + (line * 2f), line);
            Line("GridLeft", originX - line, originY - line, line, height + (line * 2f));
            Line("GridRight", originX + width, originY - line, line, height + (line * 2f));

            for (var column = 1; column < columns; column++)
            {
                var x = originX + (column * (cell + gridGap)) - (gridGap / 2f) - half;
                Line("GridV" + column, x, originY, line, height);
            }

            for (var row = 1; row < rows; row++)
            {
                var y = originY + (row * (cell + gridGap)) - (gridGap / 2f) - half;
                Line("GridH" + row, originX, y, width, line);
            }
        }

        private void Line(string name, float x, float y, float width, float height)
        {
            Box(name, x, y, width, height, MenuPalette.GridLine);
        }

        private void DrawItems(GridContainer grid, float cell, float originX, float originY)
        {
            foreach (var item in grid.Items)
            {
                if (item.Overflow || item.PlacedWidth <= 0 || item.PlacedHeight <= 0)
                {
                    continue;
                }

                var width = (item.PlacedWidth * cell) + ((item.PlacedWidth - 1) * gridGap);
                var height = (item.PlacedHeight * cell) + ((item.PlacedHeight - 1) * gridGap);
                var x = originX + (item.X * (cell + gridGap));
                var y = originY + (item.Y * (cell + gridGap));
                DrawItem(item, x, y, width, height);
            }
        }

        /// <summary>Рисует предмет картинкой: без названия, количества и прочности — только изображение.</summary>
        private void DrawItem(ItemInstance item, float x, float y, float width, float height)
        {
            var suffix = item.ItemId + "_" + item.InstanceId;
            var template = ItemPresentation.Template(_host, item);
            Sprite? sprite = null;
            var hasIcon = template != null && _host.Sprites.TryGet(template.Icon, out sprite) && sprite != null;
            Box(
                "PreviewItem_" + suffix,
                x,
                y,
                width,
                height,
                hasIcon ? Color.clear : ItemPresentation.ColorFor(_host, item));

            if (!hasIcon)
            {
                return;
            }

            if (Mathf.Min(width, height) <= 6f)
            {
                return;
            }

            UiWidgets.ItemIcon(
                _host,
                gridRoot!,
                "PreviewItemIcon_" + suffix,
                sprite!,
                x,
                y,
                width,
                height,
                4f);
        }

        private void ApplyPanelArt()
        {
            var image = GetComponent<Image>();
            if (image != null)
            {
                _hasPanelTexture = ApplyPanelTexture(image);
            }

            if (moneyLabel != null)
            {
                InventoryPanelArt.DecorateMoney(moneyLabel);
            }
        }

        private bool ApplyPanelTexture(Image image)
        {
            if (panelTexture == null)
            {
                return InventoryPanelArt.ApplyBackground(_host, image, MenuPalette.Frame);
            }

            if (_panelSprite == null)
            {
                _panelSprite = Sprite.Create(
                    panelTexture,
                    new Rect(0f, 0f, panelTexture.width, panelTexture.height),
                    new Vector2(0.5f, 0.5f),
                    100f,
                    0,
                    SpriteMeshType.FullRect);
                _panelSprite.hideFlags = HideFlags.DontSave;
            }

            image.sprite = _panelSprite;
            image.color = Color.white;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            return true;
        }

        private void OnDestroy()
        {
            if (_panelSprite != null)
            {
                Destroy(_panelSprite);
                _panelSprite = null;
            }
        }

        private void UpdateMoney()
        {
            if (moneyLabel == null)
            {
                return;
            }

            moneyLabel.gameObject.SetActive(true);
            moneyLabel.text = UiText.Format(_host.Texts, UiTextKey.Common_Money, _money);
        }

        private void UpdateEmpty()
        {
            if (emptyLabel == null)
            {
                return;
            }

            var show = _preview == null;
            emptyLabel.gameObject.SetActive(show);
            if (show)
            {
                emptyLabel.text = _error ?? UiText.Get(_host.Texts, UiTextKey.NewGame_Preview_Empty);
            }
        }

        private float CellSize(float width, int columns)
        {
            var available = Mathf.Max(1f, width - (2f * padding));
            var cell = Mathf.Floor((available - ((columns - 1) * gridGap)) / Mathf.Max(1, columns));
            return Mathf.Max(minCell, cell);
        }

        private Image Box(string name, float x, float y, float width, float height, Color color)
        {
            var image = UiWidgets.Box(_host, gridRoot!, name, x, y, width, height, color);
            image.raycastTarget = false;
            return image;
        }

        private void Clear()
        {
            if (gridRoot == null)
            {
                return;
            }

            foreach (Transform child in gridRoot)
            {
                child.gameObject.SetActive(false);
                Object.Destroy(child.gameObject);
            }
        }
    }
}
