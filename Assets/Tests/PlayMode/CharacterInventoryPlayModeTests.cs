#nullable enable
using System.Collections;
using AnomalyPath.Core.Character;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Templates;
using AnomalyPath.Runtime;
using AnomalyPath.Runtime.Screens.Character;
using AnomalyPath.Runtime.Ui.Core;
using AnomalyPath.Runtime.Ui.Items;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AnomalyPath.Tests.PlayMode
{
    /// <summary>Проверяет единообразие сетки, перетаскивание и поворот предметов на экране персонажа.</summary>
    public sealed class CharacterInventoryPlayModeTests
    {
        /// <summary>Все клетки инвентаря — одинаковые квадраты 1×1 с постоянным шагом.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator CharacterInventory_GridCells_AreUniformSquares()
        {
            yield return OpenCharacter();

            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден");
            var grid = screen.Session!.State.Player!.Inventory;
            var root = GameObject.Find("InventoryGrid");
            var canvas = GameObject.Find("Canvas");
            Assert.IsNotNull(root, "Нет корня сетки InventoryGrid");
            Assert.IsNotNull(canvas, "Нет канваса");
            Assert.IsTrue(canvas!.GetComponent<Canvas>().pixelPerfect, "Канвас должен включать pixel perfect для чётких линий");

            var top = root!.transform.Find("GridTop");
            Assert.IsNotNull(top, "Нет верхней линии сетки");
            Assert.AreEqual(CharacterMetrics.GridLine, ((RectTransform)top!).rect.height, 0.01f, "Толщина линии сетки");

            var cells = 0;
            foreach (Transform child in root.transform)
            {
                if (!child.name.StartsWith("Cell_"))
                {
                    continue;
                }

                var parts = child.name.Split('_');
                var column = int.Parse(parts[1]);
                var row = int.Parse(parts[2]);
                var rect = (RectTransform)child;
                Assert.AreEqual(CharacterMetrics.Cell, rect.rect.width, 0.01f, "Клетка должна быть квадратом по ширине");
                Assert.AreEqual(CharacterMetrics.Cell, rect.rect.height, 0.01f, "Клетка должна быть квадратом по высоте");
                Assert.AreEqual(column * CharacterMetrics.CellStep, rect.anchoredPosition.x, 0.01f, "Шаг колонок");
                Assert.AreEqual(-row * CharacterMetrics.CellStep, rect.anchoredPosition.y, 0.01f, "Шаг строк");
                cells++;
            }

            Assert.AreEqual(grid.Width * grid.Rows, cells, "Число клеток должно совпадать с сеткой");
        }

        /// <summary>Валюта стоит в верхней правой плашке окна инвентаря и равна деньгам игрока.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator CharacterInventory_Money_IsInTopRightPlate()
        {
            yield return OpenCharacter();

            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден");
            var frame = GameObject.Find("InventoryFrame");
            var money = GameObject.Find("InventoryMoney");
            Assert.IsNotNull(frame, "Нет окна инвентаря");
            Assert.IsNotNull(money, "Нет подписи валюты");

            var frameRect = WorldRect((RectTransform)frame.transform);
            var moneyRect = WorldRect((RectTransform)money.transform);
            Assert.Greater(moneyRect.center.x, frameRect.center.x + (frameRect.width * 0.15f),
                "Валюта должна быть в правой плашке окна");
            Assert.Greater(moneyRect.center.y, frameRect.yMin + (frameRect.height * 0.9f),
                "Валюта должна быть в верхней плашке окна");
            Assert.IsTrue(frameRect.Contains(moneyRect.center), "Валюта должна лежать внутри окна");

            var player = screen!.Session!.State.Player!;
            Assert.AreEqual(
                UiText.Format(screen.Texts, UiTextKey.Common_Money, player.Money),
                money!.GetComponent<Text>().text,
                "Валюта должна быть равна деньгам игрока");
        }

        /// <summary>Предмет из сетки перетаскивается в свободную клетку с сохранением точки захвата.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator CharacterInventory_DragItem_ToCell_MovesItem()
        {
            yield return OpenCharacter();

            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден");
            var grid = screen.Session!.State.Player!.Inventory;
            var item = FindPlaced(grid, 1, 1);
            Assert.IsNotNull(item, "В стартовом инвентаре нет предмета 1×1");
            Assert.IsTrue(TryFindFreeCell(grid, out var targetX, out var targetY), "В сетке нет свободной клетки");

            var itemObject = GameObject.Find("Item_" + item!.InstanceId);
            var gridObject = GameObject.Find("InventoryGrid");
            var targetCell = GameObject.Find("Cell_" + targetX + "_" + targetY);
            Assert.IsNotNull(itemObject, "Плашка предмета не создана");
            Assert.IsNotNull(gridObject, "Нет сетки для сброса");
            Assert.IsNotNull(targetCell, "Целевая клетка не создана");

            var data = new PointerEventData(EventSystem.current)
            {
                position = ScreenCenter((RectTransform)itemObject!.transform),
                button = PointerEventData.InputButton.Left
            };
            ExecuteEvents.Execute(itemObject, data, ExecuteEvents.beginDragHandler);
            data.position = ScreenCenter((RectTransform)targetCell!.transform);
            ExecuteEvents.Execute(itemObject, data, ExecuteEvents.dragHandler);
            ExecuteEvents.Execute(gridObject!, data, ExecuteEvents.dropHandler);
            yield return null;

            Assert.AreEqual(targetX, item.X, "Колонка после перетаскивания");
            Assert.AreEqual(targetY, item.Y, "Строка после перетаскивания");
            Assert.AreSame(item, grid.ItemAt(targetX, targetY), "В целевой клетке должен лежать тот же предмет");
        }

        /// <summary>Во время перетаскивания предмет поворачивается, и сброс фиксирует поворот.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator CharacterInventory_RotateDuringDrag_CommitsRotation()
        {
            yield return OpenCharacter();

            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден");
            var player = screen.Session!.State.Player!;
            var main = player.Equipment.Get(EquipmentSlot.Main);
            Assert.IsNotNull(main, "В основном слоте нет оружия");

            Assert.AreEqual(
                CharacterActionResult.Ok,
                screen.Actions.Unequip(player.Inventory, player.Equipment, EquipmentSlot.Main),
                "Оружие не снялось в инвентарь");
            screen.ShowCharacter();
            yield return null;

            var grid = player.Inventory;
            var item = FindPlaced(grid, 2, 3);
            Assert.IsNotNull(item, "В инвентаре нет предмета 2×3");
            Assert.IsTrue(screen.Items.TryGet(item!.ItemId, out var template), "Нет шаблона предмета");
            Assert.IsTrue(template!.Rotatable, "Предмет должен поддерживать поворот");

            var initialRotated = item.Rotated;
            var expectedRotated = !initialRotated;
            Assert.IsTrue(
                TryFindRotatedTarget(grid, template, item.InstanceId, expectedRotated, out var targetX, out var targetY),
                "Нет свободного места под повёрнутый предмет");

            var itemObject = GameObject.Find("Item_" + item.InstanceId);
            var targetCell = GameObject.Find("Cell_" + (targetX + 1) + "_" + (targetY + 1));
            var gridObject = GameObject.Find("InventoryGrid");
            Assert.IsNotNull(itemObject, "Плашка оружия не создана");
            Assert.IsNotNull(targetCell, "Клетка под курсором не создана");
            Assert.IsNotNull(gridObject, "Нет сетки для сброса");

            var source = itemObject!.GetComponent<ItemDragSource>();
            Assert.IsNotNull(source, "У предмета нет источника перетаскивания");
            var data = new PointerEventData(EventSystem.current)
            {
                position = ScreenCenter((RectTransform)itemObject.transform),
                button = PointerEventData.InputButton.Left
            };
            ExecuteEvents.Execute(itemObject, data, ExecuteEvents.beginDragHandler);
            Assert.IsTrue(source!.Rotate(), "Поворот во время перетаскивания не сработал");
            data.position = ScreenCenter((RectTransform)targetCell!.transform);
            ExecuteEvents.Execute(itemObject, data, ExecuteEvents.dragHandler);
            ExecuteEvents.Execute(gridObject!, data, ExecuteEvents.dropHandler);
            yield return null;

            Assert.AreEqual(expectedRotated, item.Rotated, "Сброс не зафиксировал поворот");
            Assert.AreEqual(expectedRotated ? template.FootprintHeight : template.FootprintWidth, item.PlacedWidth, "Ширина после поворота");
            Assert.AreEqual(expectedRotated ? template.FootprintWidth : template.FootprintHeight, item.PlacedHeight, "Высота после поворота");
            Assert.AreEqual(targetX, item.X, $"Колонка после поворота: target={targetX}, item={item.X}");
            Assert.AreEqual(targetY, item.Y, $"Строка после поворота: target={targetY}, item={item.Y}");
        }

        /// <summary>Поворот доступен только во время перетаскивания и только для rotatable-предмета.</summary>
        [Test]
        public void ItemDragContext_ToggleRotation_OnlyWhileDraggingAndRotatable()
        {
            var context = new ItemDragContext();
            Assert.IsFalse(context.ToggleRotation(), "Без перетаскивания поворот запрещён");

            context.Begin("a", DragOrigin.Inventory, 2, 3, rotatable: true, rotated: false, 0.5f, 0.5f, Vector2.zero);
            Assert.AreEqual(2, context.EffectiveWidth, "Ширина до поворота");
            Assert.AreEqual(3, context.EffectiveHeight, "Высота до поворота");
            Assert.IsTrue(context.ToggleRotation(), "Поворот разрешённого предмета");
            Assert.AreEqual(3, context.EffectiveWidth, "Ширина после поворота");
            Assert.AreEqual(2, context.EffectiveHeight, "Высота после поворота");
            context.End();
            Assert.IsFalse(context.ToggleRotation(), "После окончания перетаскивания поворот запрещён");

            context.Begin("b", DragOrigin.Inventory, 2, 3, rotatable: false, rotated: false, 0.5f, 0.5f, Vector2.zero);
            Assert.IsFalse(context.ToggleRotation(), "Неповорачиваемый предмет не должен поворачиваться");
        }

        private static bool TryFindFreeCell(GridContainer grid, out int x, out int y)
        {
            for (var row = 0; row < grid.Rows; row++)
            {
                for (var column = 0; column < grid.Width; column++)
                {
                    if (grid.ItemAt(column, row) == null)
                    {
                        x = column;
                        y = row;
                        return true;
                    }
                }
            }

            x = 0;
            y = 0;
            return false;
        }

        private static ItemInstance? FindPlaced(GridContainer grid, int width, int height)
        {
            foreach (var item in grid.Items)
            {
                if (!item.Overflow && item.PlacedWidth == width && item.PlacedHeight == height)
                {
                    return item;
                }
            }

            return null;
        }

        private static bool TryFindRotatedTarget(
            GridContainer grid,
            ItemTemplate template,
            string instanceId,
            bool rotated,
            out int x,
            out int y)
        {
            for (var row = 0; row + 1 < grid.Rows; row++)
            {
                for (var column = 0; column + 1 < grid.Width; column++)
                {
                    if (grid.CanPlace(template, column, row, rotated, instanceId))
                    {
                        x = column;
                        y = row;
                        return true;
                    }
                }
            }

            x = 0;
            y = 0;
            return false;
        }

        private static Vector2 ScreenCenter(RectTransform rect) =>
            RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));

        private static Rect WorldRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }

        private static IEnumerator OpenCharacter()
        {
            yield return LoadMain();
            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;
            Click("Backpack");
            yield return null;
        }

        private static IEnumerator LoadMain()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;
        }

        private static void Click(string name)
        {
            var go = GameObject.Find(name);
            Assert.IsNotNull(go, $"Кнопка «{name}» не найдена");
            var button = go!.GetComponent<Button>();
            Assert.IsNotNull(button, $"У «{name}» нет Button");
            button!.onClick.Invoke();
        }
    }
}
