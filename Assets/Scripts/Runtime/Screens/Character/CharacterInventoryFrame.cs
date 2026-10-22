#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Player;
using AnomalyPath.Runtime.Screens.Menu;
using AnomalyPath.Runtime.Ui.Core;
using AnomalyPath.Runtime.Ui.Items;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Character
{
    /// <summary>Фрейм инвентаря: сетка клеток, предметы по габаритам и строка переполнения.</summary>
    public sealed class CharacterInventoryFrame
    {
        private readonly IUiHost _host;
        private readonly Transform _content;
        private readonly ICharacterFrameHost _frameHost;

        /// <summary>Создаёт фрейм инвентаря.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="content">Корень содержимого экрана.</param>
        /// <param name="frameHost">Экран персонажа.</param>
        public CharacterInventoryFrame(IUiHost host, Transform content, ICharacterFrameHost frameHost)
        {
            _host = host;
            _content = content;
            _frameHost = frameHost;
        }

        /// <summary>Рисует сетку и предметы.</summary>
        /// <param name="player">Состояние игрока.</param>
        public void Build(PlayerState player)
        {
            var grid = player.Inventory;
            var left = CharacterMetrics.FrameWidth + CharacterMetrics.Gap;
            var frame = CharacterWidgets.Box(_host, _content, "InventoryFrame", left, 0f,
                CharacterMetrics.InventoryWidth, CharacterMetrics.FrameHeight, CharacterPalette.Frame);
            var frameZone = frame.gameObject.AddComponent<ItemDropZone>();
            frameZone.Configure(_frameHost.Drag, _frameHost.DropOnInventory);

            CharacterWidgets.Label(_host, _content, "InventoryTitle", "Инвентарь", 28, TextAnchor.MiddleCenter,
                UiFactory.TextColor, left + 20f, 12f, CharacterMetrics.InventoryWidth - 40f, 36f);
            CharacterWidgets.Label(_host, _content, "InventoryMoney", SaveSlotText.Money(player.Money), 20,
                TextAnchor.MiddleRight, UiFactory.MutedTextColor,
                left + CharacterMetrics.InventoryWidth - 220f, 14f, 200f, 30f);
            var summary = $"занято {grid.UsedCells} из {grid.Width * grid.Rows} клеток";
            CharacterWidgets.Label(_host, _content, "InventorySummary", summary, 16, TextAnchor.MiddleCenter,
                UiFactory.MutedTextColor, left + 20f, 50f, CharacterMetrics.InventoryWidth - 40f, 24f);

            var gridWidth = (grid.Width * CharacterMetrics.Cell) + ((grid.Width - 1) * CharacterMetrics.CellGap);
            var gridX = left + ((CharacterMetrics.InventoryWidth - gridWidth) / 2f);
            const float gridY = 86f;

            for (var row = 0; row < grid.Rows; row++)
            {
                for (var column = 0; column < grid.Width; column++)
                {
                    var cell = CharacterWidgets.Box(_host, _content, $"Cell_{column}_{row}",
                        gridX + (column * (CharacterMetrics.Cell + CharacterMetrics.CellGap)),
                        gridY + (row * (CharacterMetrics.Cell + CharacterMetrics.CellGap)),
                        CharacterMetrics.Cell, CharacterMetrics.Cell, CharacterPalette.Cell);
                    var zone = cell.gameObject.AddComponent<ItemDropZone>();
                    zone.Configure(_frameHost.Drag, _frameHost.DropOnInventory);
                }
            }

            var gridHeight = (grid.Rows * CharacterMetrics.Cell) +
                             ((grid.Rows - 1) * CharacterMetrics.CellGap);
            DrawGridLines(gridX, gridY, gridWidth, gridHeight, grid.Width, grid.Rows);

            var overflow = new List<string>();
            foreach (var item in grid.Items)
            {
                if (item.Overflow || item.PlacedWidth <= 0 || item.PlacedHeight <= 0)
                {
                    overflow.Add(CharacterWidgets.Describe(_host, item));
                    continue;
                }

                var width = (item.PlacedWidth * CharacterMetrics.Cell) +
                            ((item.PlacedWidth - 1) * CharacterMetrics.CellGap);
                var height = (item.PlacedHeight * CharacterMetrics.Cell) +
                             ((item.PlacedHeight - 1) * CharacterMetrics.CellGap);
                var x = gridX + (item.X * (CharacterMetrics.Cell + CharacterMetrics.CellGap));
                var y = gridY + (item.Y * (CharacterMetrics.Cell + CharacterMetrics.CellGap));
                ItemBox(item, x, y, width, height);
            }

            if (overflow.Count > 0)
            {
                CharacterWidgets.Label(_host, _content, "Overflow", "Не помещается: " + string.Join(", ", overflow),
                    14, TextAnchor.UpperLeft, UiFactory.ToastColor, left + 24f,
                    gridY + ((grid.Rows * (CharacterMetrics.Cell + CharacterMetrics.CellGap)) + 8f),
                    CharacterMetrics.InventoryWidth - 48f, 40f);
            }
        }

        private void DrawGridLines(
            float gridX,
            float gridY,
            float width,
            float height,
            int columns,
            int rows)
        {
            var color = new Color(0.30f, 0.34f, 0.34f, 1f);
            var cell = CharacterMetrics.Cell;
            var gap = CharacterMetrics.CellGap;
            CharacterWidgets.Box(_host, _content, "GridTop", gridX - 1f, gridY - 1f, width + 2f, 1f, color);
            CharacterWidgets.Box(_host, _content, "GridBottom", gridX - 1f, gridY + height, width + 2f, 1f, color);
            CharacterWidgets.Box(_host, _content, "GridLeft", gridX - 1f, gridY - 1f, 1f, height + 2f, color);
            CharacterWidgets.Box(_host, _content, "GridRight", gridX + width, gridY - 1f, 1f, height + 2f, color);

            for (var column = 1; column < columns; column++)
            {
                var x = gridX + (column * (cell + gap)) - (gap / 2f) - 0.5f;
                CharacterWidgets.Box(_host, _content, "GridV" + column, x, gridY, 1f, height, color);
            }

            for (var row = 1; row < rows; row++)
            {
                var y = gridY + (row * (cell + gap)) - (gap / 2f) - 0.5f;
                CharacterWidgets.Box(_host, _content, "GridH" + row, gridX, y, width, 1f, color);
            }
        }

        private void ItemBox(ItemInstance item, float x, float y, float width, float height)
        {
            var frame = CharacterWidgets.Box(_host, _content, "Item_" + item.InstanceId, x, y, width, height,
                CharacterWidgets.ColorFor(_host, item));
            CharacterWidgets.AttachItem(_host, _frameHost, frame, item, null, -1, DragOrigin.Inventory);
            var zone = frame.gameObject.AddComponent<ItemDropZone>();
            zone.Configure(_frameHost.Drag, _frameHost.DropOnInventory);

            var template = CharacterWidgets.Template(_host, item);
            Sprite? sprite = null;
            var hasIcon = template != null && _host.Sprites.TryGet(template.Icon, out sprite) && sprite != null;
            const float labelHeight = 30f;

            if (hasIcon)
            {
                var iconArea = height - labelHeight - 6f;
                var side = Mathf.Min(width - 10f, iconArea);
                if (side > 8f)
                {
                    CharacterWidgets.Icon(_host, _content, "ItemIcon_" + item.InstanceId, sprite!,
                        x + ((width - side) / 2f), y + 4f, side, side);
                }
            }

            var text = hasIcon && template != null && template.MaxStack > 1
                ? "×" + item.Stack
                : CharacterWidgets.Describe(_host, item);
            var label = CharacterWidgets.Label(_host, _content, "ItemLabel_" + item.InstanceId,
                text, 11,
                hasIcon ? TextAnchor.LowerCenter : TextAnchor.MiddleCenter, UiFactory.TextColor,
                x + 4f,
                hasIcon ? y + height - labelHeight - 2f : y + 4f,
                width - 8f,
                hasIcon ? labelHeight : height - 8f);
            label.verticalOverflow = VerticalWrapMode.Truncate;
        }
    }
}
