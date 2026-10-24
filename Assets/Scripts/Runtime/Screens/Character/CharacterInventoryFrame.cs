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
    /// <summary>Фрейм инвентаря: одинаковая сетка клеток 1×1, предметы по габаритам и строка переполнения.</summary>
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
            var hasPanel = InventoryPanelArt.ApplyBackground(_host, frame, CharacterPalette.Frame);
            var frameZone = frame.gameObject.AddComponent<ItemDropZone>();
            frameZone.Configure(_frameHost.Drag, instanceId => _frameHost.DropOnInventory(instanceId, -1, -1, false));

            CharacterWidgets.Label(_host, _content, "InventoryTitle", UiText.Get(_host.Texts, UiTextKey.Character_Inventory_Title), 24, TextAnchor.MiddleLeft,
                UiFactory.TextColor, left + 40f, 14f, 300f, 36f);
            var plate = InventoryPanelArt.ToAbsolute(
                InventoryPanelArt.CurrencyPlate,
                CharacterMetrics.InventoryWidth,
                CharacterMetrics.FrameHeight,
                left,
                0f);
            var money = CharacterWidgets.Label(_host, _content, "InventoryMoney", SaveSlotText.Money(_host.Texts, player.Money), 24,
                TextAnchor.MiddleCenter, UiFactory.TextColor, plate.x, plate.y, plate.width, plate.height);
            InventoryPanelArt.DecorateMoney(money);
            var summary = UiText.Format(_host.Texts, UiTextKey.Common_Count_Of, grid.UsedCells, grid.Width * grid.Rows);
            CharacterWidgets.Label(_host, _content, "InventorySummary", summary, 14, TextAnchor.MiddleLeft,
                UiFactory.MutedTextColor, left + 40f, 54f, 300f, 22f);

            var gridWidth = CharacterMetrics.GridWidth(grid.Width);
            var gridHeight = CharacterMetrics.GridHeight(grid.Rows);
            var gridX = left + ((CharacterMetrics.InventoryWidth - gridWidth) / 2f);
            const float gridY = 100f;

            var gridRoot = new GameObject("InventoryGrid", typeof(RectTransform), typeof(Image));
            gridRoot.transform.SetParent(_content, false);
            var gridRect = (RectTransform)gridRoot.transform;
            UiWidgets.Place(gridRect, gridX, gridY, gridWidth, gridHeight);
            var gridImage = gridRoot.GetComponent<Image>();
            gridImage.color = Color.clear;
            gridImage.raycastTarget = true;
            var gridZone = gridRoot.AddComponent<ItemGridDropZone>();
            gridZone.Configure(
                _frameHost.Drag,
                grid.Width,
                grid.Rows,
                CharacterMetrics.Cell,
                CharacterMetrics.CellGap,
                (instanceId, x, y, rotated) => CanPlace(grid, instanceId, x, y, rotated),
                _frameHost.DropOnInventory);

            var cellColor = hasPanel ? Color.clear : CharacterPalette.Cell;
            for (var row = 0; row < grid.Rows; row++)
            {
                for (var column = 0; column < grid.Width; column++)
                {
                    var cell = CharacterWidgets.Box(_host, gridRoot.transform, $"Cell_{column}_{row}",
                        column * CharacterMetrics.CellStep,
                        row * CharacterMetrics.CellStep,
                        CharacterMetrics.Cell,
                        CharacterMetrics.Cell,
                        cellColor);
                    cell.raycastTarget = false;
                }
            }

            DrawGridLines(gridRoot.transform, gridWidth, gridHeight, grid.Width, grid.Rows);

            var overflow = new List<string>();
            foreach (var item in grid.Items)
            {
                if (item.Overflow || item.PlacedWidth <= 0 || item.PlacedHeight <= 0)
                {
                    overflow.Add(CharacterWidgets.Describe(_host, item));
                    continue;
                }

                var size = CharacterMetrics.ItemSize(item.PlacedWidth, item.PlacedHeight);
                ItemBox(
                    item,
                    gridRoot.transform,
                    item.X * CharacterMetrics.CellStep,
                    item.Y * CharacterMetrics.CellStep,
                    size.x,
                    size.y);
            }

            if (overflow.Count > 0)
            {
                CharacterWidgets.Label(_host, _content, "Overflow", UiText.Format(_host.Texts, UiTextKey.Character_Overflow, string.Join(", ", overflow)),
                    14, TextAnchor.UpperLeft, UiFactory.ToastColor, left + 24f,
                    gridY + gridHeight + 8f,
                    CharacterMetrics.InventoryWidth - 48f, 40f);
            }
        }

        private bool CanPlace(GridContainer grid, string instanceId, int x, int y, bool rotated)
        {
            var item = grid.Find(instanceId);
            if (item == null || item.Overflow)
            {
                return false;
            }

            var template = CharacterWidgets.Template(_host, item);
            return template != null && grid.CanPlace(template, x, y, rotated, instanceId);
        }

        private void DrawGridLines(
            Transform parent,
            float width,
            float height,
            int columns,
            int rows)
        {
            var color = CharacterPalette.GridLine;
            var cell = CharacterMetrics.Cell;
            var gap = CharacterMetrics.CellGap;
            var line = CharacterMetrics.GridLine;
            var half = line / 2f;
            Line(parent, "GridTop", -line, -line, width + (line * 2f), line, color);
            Line(parent, "GridBottom", -line, height, width + (line * 2f), line, color);
            Line(parent, "GridLeft", -line, -line, line, height + (line * 2f), color);
            Line(parent, "GridRight", width, -line, line, height + (line * 2f), color);

            for (var column = 1; column < columns; column++)
            {
                var x = (column * (cell + gap)) - (gap / 2f) - half;
                Line(parent, "GridV" + column, x, 0f, line, height, color);
            }

            for (var row = 1; row < rows; row++)
            {
                var y = (row * (cell + gap)) - (gap / 2f) - half;
                Line(parent, "GridH" + row, 0f, y, width, line, color);
            }
        }

        private void Line(Transform parent, string name, float x, float y, float width, float height, Color color)
        {
            var image = CharacterWidgets.Box(_host, parent, name, x, y, width, height, color);
            image.raycastTarget = false;
        }

        private void ItemBox(ItemInstance item, Transform parent, float x, float y, float width, float height)
        {
            var template = CharacterWidgets.Template(_host, item);
            Sprite? sprite = null;
            var hasIcon = template != null && _host.Sprites.TryGet(template.Icon, out sprite) && sprite != null;
            var frame = CharacterWidgets.Box(_host, parent, "Item_" + item.InstanceId, x, y, width, height,
                hasIcon ? Color.clear : CharacterWidgets.ColorFor(_host, item));
            frame.raycastTarget = true;
            CharacterWidgets.AttachItem(_host, _frameHost, frame, item, null, -1, DragOrigin.Inventory);

            const float labelHeight = 30f;
            if (hasIcon)
            {
                var iconHeight = height - labelHeight - 6f;
                if (iconHeight > 8f)
                {
                    UiWidgets.ItemIcon(_host, parent, "ItemIcon_" + item.InstanceId, sprite!,
                        x, y, width, iconHeight, 4f);
                }
            }

            var text = hasIcon && template != null && template.MaxStack > 1
                ? "×" + item.Stack
                : CharacterWidgets.Describe(_host, item);
            var label = CharacterWidgets.Label(_host, parent, "ItemLabel_" + item.InstanceId,
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
