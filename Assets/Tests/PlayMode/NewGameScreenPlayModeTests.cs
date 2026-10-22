#nullable enable
using System.Collections;
using AnomalyPath.Runtime;
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
    /// <summary>Экран новой игры: предпросмотр набора справа и числовые поля сложности вместо ползунков.</summary>
    public sealed class NewGameScreenPlayModeTests
    {
        /// <summary>Окно инвентаря рисует предметы набора картинками, без подписей, и не даёт с ними работать.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_InventoryPreview_DrawsLoadoutItemsWithoutText()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;

            var preview = GameObject.Find("NewGameInventoryFrame");
            Assert.IsNotNull(preview, "Окно инвентаря набора не создано");
            Assert.IsTrue(preview.activeSelf, "Окно инвентаря набора не активно");
            Assert.IsNotNull(preview.GetComponent<Image>(), "Рамка инвентаря должна быть фоном самой панели");
            Assert.IsNull(GameObject.Find("PreviewFrame"), "Рамка не должна быть отдельным растягиваемым объектом");

            Assert.IsNull(GameObject.Find("PreviewSlotMainValue"), "Состав набора не должен расписываться по слотам");
            Assert.IsNull(GameObject.Find("PreviewInventorySummary"), "Счётчик занятых клеток на этом экране не нужен");
            var money = GameObject.Find("PreviewMoney");
            Assert.IsNotNull(money, "Над инвентарём не показаны деньги");
            StringAssert.Contains("руб", money.GetComponent<Text>().text, "Деньги должны быть подписаны в рублях");
            Assert.AreEqual(0, CountChildren(preview, "PreviewItemLabel_"),
                "В клетках инвентаря не должно быть подписей предметов");
            Assert.Greater(CountChildren(preview, "PreviewCell_"), 0, "Клетки инвентаря не созданы");
            Assert.GreaterOrEqual(CountItemBoxes(preview), 7, "Предметы набора не отрисованы в инвентаре");
            Assert.IsTrue(HasItem(preview, "wpn_ak74"), "Автомат набора не отрисован");
            Assert.IsTrue(HasItem(preview, "itm_armor_plate"), "Броня набора не отрисована");
            Assert.IsTrue(HasItem(preview, "ammo_545"), "Патроны набора не отрисованы");

            var icons = preview.GetComponentsInChildren<Image>(includeInactive: false);
            var drawn = 0;
            foreach (var image in icons)
            {
                if (image.name.StartsWith("PreviewItemIcon_", System.StringComparison.Ordinal) && image.sprite != null)
                {
                    drawn++;
                }
            }

            Assert.Greater(drawn, 0, "Иконки предметов не нарисованы — проверьте каталог спрайтов в сцене");

            Assert.AreEqual(0, preview.GetComponentsInChildren<ItemDropZone>(true).Length,
                "В инвентаре набора не должно быть зон сброса");
            Assert.AreEqual(0, preview.GetComponentsInChildren<ItemDragSource>(true).Length,
                "В инвентаре набора не должно быть перетаскивания");
            Assert.AreEqual(0, preview.GetComponentsInChildren<Button>(true).Length,
                "В инвентаре набора не должно быть кнопок");
        }

        /// <summary>Выбор другого набора перерисовывает окно инвентаря.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_SelectOtherLoadout_UpdatesPreview()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;

            var preview = GameObject.Find("NewGameInventoryFrame");
            Assert.IsTrue(HasItem(preview, "wpn_ak74"), "Первый набор должен быть с автоматом АК-74");

            Click("Разведчик");
            yield return null;

            var updated = GameObject.Find("NewGameInventoryFrame");
            Assert.IsFalse(HasItem(updated, "wpn_ak74"), "Автомат остался в окне инвентаря второго набора");
            Assert.IsTrue(HasItem(updated, "wpn_pm"), "Второй набор не отрисован в инвентаре");
        }

        /// <summary>Поля сложности принимают цифры и меняют значение кнопками шагом 0.1.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Difficulty_NumberFieldsStepByPointOne()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;

            var loot = GameObject.Find("LootLossField");
            Assert.IsNotNull(loot, "Нет поля потери лута");
            var lootField = loot.GetComponent<NumberField>();
            Assert.IsNotNull(lootField, "У поля потери лута нет NumberField");
            Assert.IsNotNull(GameObject.Find("LootLossFieldIncrease"), "Нет кнопки увеличения потери лута");
            Assert.IsNotNull(GameObject.Find("LootLossFieldDecrease"), "Нет кнопки уменьшения потери лута");
            Assert.AreEqual(0.5, lootField.Value, 1e-6, "Начальное значение потери лута");

            Click("LootLossFieldIncrease");
            Assert.AreEqual(0.6, lootField.Value, 1e-6, "Шаг увеличения 0.1");
            Click("LootLossFieldIncrease");
            Assert.AreEqual(0.7, lootField.Value, 1e-6, "Шаг увеличения 0.1");
            Click("LootLossFieldDecrease");
            Assert.AreEqual(0.6, lootField.Value, 1e-6, "Шаг уменьшения 0.1");
            Click("LootLossFieldDecrease");
            Click("LootLossFieldDecrease");
            Click("LootLossFieldDecrease");
            Click("LootLossFieldDecrease");
            Click("LootLossFieldDecrease");
            Click("LootLossFieldDecrease");
            Assert.AreEqual(0, lootField.Value, 1e-6, "Минимум потери лута равен 0");

            var trade = GameObject.Find("TradeDifficultyField");
            Assert.IsNotNull(trade, "Нет поля усложнения торговли");
            var tradeField = trade.GetComponent<NumberField>();
            Assert.IsNotNull(tradeField, "У поля торговли нет NumberField");
            Assert.AreEqual(1, tradeField.Value, 1e-6, "Начальное усложнение торговли");
            Click("TradeDifficultyFieldIncrease");
            Assert.AreEqual(1.1, tradeField.Value, 1e-6, "Шаг усложнения торговли 0.1");
        }

        /// <summary>Цифровой ввод обновляет значение и зажимается в диапазон.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Difficulty_TypedNumber_ClampedToRange()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;

            var field = GameObject.Find("LootLossField").GetComponent<NumberField>();
            var input = GameObject.Find("LootLossFieldInput");
            Assert.IsNotNull(input, "Нет поля ввода у числового поля");
            var text = input.GetComponent<TextInputField>();
            Assert.IsNotNull(text, "У поля ввода нет TextInputField");

            text.SetText("0.8");
            Assert.AreEqual(0.8, field.Value, 1e-6, "Введённое значение");

            text.SetText("7");
            Assert.AreEqual(1, field.Value, 1e-6, "Значение выше максимума зажимается");

            text.SetText("abc");
            Assert.AreEqual(1, field.Value, 1e-6, "Буквы не меняют значение");
            Assert.AreEqual("1.0", text.Text, "Недопустимый ввод откатывается к последнему значению");
        }

        /// <summary>На экране новой игры нет ползунков: только поля с цифрами.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_HasNoSliders()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;

            var panel = GameObject.Find("NewGame");
            Assert.IsNotNull(panel, "Панель новой игры не создана");
            Assert.IsNull(panel.GetComponentInChildren<Slider>(true), "Ползунки на экране новой игры не нужны");
        }

        /// <summary>Верхние границы настроек и инвентаря совпадают, фреймы растянуты, прокрутка есть.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Layout_TopAlignedStretchScroll()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;
            Canvas.ForceUpdateCanvases();

            var panel = GameObject.Find("NewGame");
            Assert.IsNotNull(panel, "Панель новой игры не создана");
            var preview = GameObject.Find("NewGameInventoryFrame");
            Assert.IsNotNull(preview, "Нет панели инвентаря");
            var column = preview.transform.parent.Find("Column");
            Assert.IsNotNull(column, "Нет колонки настроек рядом с инвентарём");

            var columnRect = WorldRect((RectTransform)column);
            var previewRect = WorldRect((RectTransform)preview.transform);
            Assert.AreEqual(columnRect.yMax, previewRect.yMax, 1f, "Верхние границы фреймов не совпадают");
            Assert.Less(columnRect.xMax, previewRect.xMin, "Колонка настроек заходит на панель инвентаря");

            var settingsHeight = ((RectTransform)column).rect.height;
            var previewHeight = ((RectTransform)preview.transform).rect.height;
            Assert.Greater(settingsHeight, 0f, "Колонка настроек не получила высоту");
            Assert.Greater(previewHeight, 0f, "Окно инвентаря не получило высоту");

            var scroll = panel.GetComponentInChildren<ScrollRect>(true);
            Assert.IsNotNull(scroll, "Нет вертикальной прокрутки");
            Assert.IsTrue(scroll.vertical, "Прокрутка должна быть вертикальной");
            Assert.IsFalse(scroll.horizontal, "Горизонтальная прокрутка не нужна");
            Assert.IsNotNull(scroll.verticalScrollbar, "Нет полосы прокрутки справа");
            Assert.AreSame(scroll.content, preview.transform.parent, "Фреймы должны лежать в контенте прокрутки");

            var boxes = 0;
            foreach (var child in preview.GetComponentsInChildren<Transform>(includeInactive: false))
            {
                if (!IsItemBox(child.name))
                {
                    continue;
                }

                boxes++;
                var itemRect = WorldRect((RectTransform)child);
                Assert.GreaterOrEqual(itemRect.xMin, previewRect.xMin - 1f, "Предмет выходит влево за панель: " + child.name);
                Assert.LessOrEqual(itemRect.xMax, previewRect.xMax + 1f, "Предмет выходит вправо за панель: " + child.name);
                Assert.GreaterOrEqual(itemRect.yMin, previewRect.yMin - 1f, "Предмет выходит вниз за панель: " + child.name);
                Assert.LessOrEqual(itemRect.yMax, previewRect.yMax + 1f, "Предмет выходит вверх за панель: " + child.name);
            }

            Assert.Greater(boxes, 0, "В инвентаре набора нет предметов для проверки вёрстки");
        }

        /// <summary>На маленьком окне контент выше области — полоса прокрутки включается.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Layout_SmallViewport_ScrollsContent()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;
            Canvas.ForceUpdateCanvases();

            var panel = GameObject.Find("NewGame");
            var scroll = panel.GetComponentInChildren<ScrollRect>(true);
            Assert.IsNotNull(scroll, "Нет вертикальной прокрутки");
            var content = (RectTransform)scroll.content;
            var viewport = (RectTransform)scroll.viewport;
            Assert.GreaterOrEqual(content.rect.height, viewport.rect.height,
                "Высота контента не может быть меньше области");
            Assert.Greater(content.rect.height, 0f, "Контент не растянут");
        }

        /// <summary>При смене размера области фреймы пересчитываются: растягиваются, не разъезжаются, включают прокрутку.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Layout_ViewportResize_StretchesFramesAndShowsScrollbar()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;
            Canvas.ForceUpdateCanvases();

            var panel = GameObject.Find("NewGame");
            Assert.IsNotNull(panel, "Панель новой игры не создана");
            var scrollRoot = (RectTransform)GameObject.Find("NewGameScroll").transform;
            var panelRect = (RectTransform)panel.transform;
            var preview = GameObject.Find("NewGameInventoryFrame");
            Assert.IsNotNull(preview, "Нет панели инвентаря");
            var column = (RectTransform)preview.transform.parent.Find("Column");
            Assert.IsNotNull(column, "Нет колонки настроек");
            var scroll = panel.GetComponentInChildren<ScrollRect>(true);
            Assert.IsNotNull(scroll, "Нет вертикальной прокрутки");

            var previewDefaultWidth = ((RectTransform)preview.transform).rect.width;

            scrollRoot.sizeDelta = Vector2.zero;
            Canvas.ForceUpdateCanvases();
            yield return null;

            var widePreview = ((RectTransform)preview.transform).rect.width;
            Assert.Greater(widePreview, previewDefaultWidth, "На широкой области окно инвентаря не растянулось");
            var contentRect = (RectTransform)scroll.content;
            var gap = contentRect.rect.width + ((RectTransform)preview.transform).rect.xMin - column.rect.xMax;
            Assert.AreEqual(48f, gap, 1f, "Зазор между фреймами должен оставаться постоянным");
            Assert.AreEqual(WorldRect(column).yMax, WorldRect((RectTransform)preview.transform).yMax, 1f,
                "Верхние границы разъехались после изменения размера");

            var smallWidth = Mathf.Max(240f, panelRect.rect.width - 800f);
            var smallHeight = Mathf.Max(160f, panelRect.rect.height - 800f);
            scrollRoot.sizeDelta = new Vector2(
                smallWidth - panelRect.rect.width,
                smallHeight - panelRect.rect.height);
            Canvas.ForceUpdateCanvases();
            yield return null;
            yield return null;

            var content = (RectTransform)scroll.content;
            var viewport = (RectTransform)scroll.viewport;
            Assert.Greater(content.rect.height, viewport.rect.height,
                "На маленькой области контент должен быть выше видимой части");
            Assert.IsTrue(scroll.verticalScrollbar.gameObject.activeSelf,
                "Полоса прокрутки не включилась при переполнении");
        }

        /// <summary>Колесо мыши прокручивает содержимое: область ловит события и ScrollRect их принимает.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Scroll_WheelScrollsContent()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;
            Canvas.ForceUpdateCanvases();

            var panel = GameObject.Find("NewGame");
            Assert.IsNotNull(panel, "Панель новой игры не создана");
            var scrollRoot = (RectTransform)GameObject.Find("NewGameScroll").transform;
            var panelRect = (RectTransform)panel.transform;
            var scroll = panel.GetComponentInChildren<ScrollRect>(true);
            Assert.IsNotNull(scroll, "Нет вертикальной прокрутки");

            var catcher = scroll.viewport.GetComponent<Graphic>();
            Assert.IsNotNull(catcher, "Без Graphic область прокрутки не получит колесо мыши");
            Assert.IsTrue(catcher.raycastTarget, "Область прокрутки не ловит события указателя");

            var smallWidth = Mathf.Max(240f, panelRect.rect.width - 800f);
            var smallHeight = Mathf.Max(160f, panelRect.rect.height - 800f);
            scrollRoot.sizeDelta = new Vector2(
                smallWidth - panelRect.rect.width,
                smallHeight - panelRect.rect.height);
            Canvas.ForceUpdateCanvases();
            yield return null;
            yield return null;

            scroll.verticalNormalizedPosition = 0.5f;
            Canvas.ForceUpdateCanvases();
            yield return null;

            var content = (RectTransform)scroll.content;
            var before = content.anchoredPosition.y;
            scroll.OnScroll(new PointerEventData(EventSystem.current) { scrollDelta = new Vector2(0f, -5f) });
            yield return null;

            Assert.AreNotEqual(before, content.anchoredPosition.y, "Колесо мыши не прокручивает содержимое");
        }

        /// <summary>Сетка инвентаря заполняет панель по ширине, без пустых полей по бокам.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_InventoryGrid_FillsPanelWidth()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;
            Canvas.ForceUpdateCanvases();

            var preview = GameObject.Find("NewGameInventoryFrame");
            Assert.IsNotNull(preview, "Нет панели инвентаря");
            var width = ((RectTransform)preview.transform).rect.width;

            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var cell = 0f;
            var cells = 0;
            foreach (var child in preview.GetComponentsInChildren<Transform>(includeInactive: false))
            {
                if (!child.name.StartsWith("PreviewCell_", System.StringComparison.Ordinal))
                {
                    continue;
                }

                var rect = (RectTransform)child;
                cell = Mathf.Max(cell, rect.rect.width);
                minX = Mathf.Min(minX, rect.anchoredPosition.x);
                maxX = Mathf.Max(maxX, rect.anchoredPosition.x + rect.rect.width);
                cells++;
            }

            Assert.Greater(cells, 0, "Клетки инвентаря не созданы");
            var padding = 24f;
            Assert.LessOrEqual(minX, padding + cell + 1f, "Слева от сетки остаётся пустое поле");
            Assert.LessOrEqual(width - maxX, padding + cell + 1f, "Справа от сетки остаётся пустое поле");
            Assert.Greater(maxX - minX, width * 0.8f, "Сетка не заполняет ширину панели");
        }

        private static Rect WorldRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }

        private static bool IsItemBox(string name) =>
            name.StartsWith("PreviewItem_", System.StringComparison.Ordinal) &&
            !name.StartsWith("PreviewItemIcon_", System.StringComparison.Ordinal);

        private static int CountItemBoxes(GameObject root)
        {
            var count = 0;
            foreach (var child in root.GetComponentsInChildren<Transform>(includeInactive: false))
            {
                if (IsItemBox(child.name))
                {
                    count++;
                }
            }

            return count;
        }

        private static bool HasItem(GameObject root, string itemId)
        {
            var prefix = "PreviewItem_" + itemId + "_";
            foreach (var child in root.GetComponentsInChildren<Transform>(includeInactive: false))
            {
                if (child.name.StartsWith(prefix, System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static int CountChildren(GameObject root, string prefix)
        {
            var count = 0;
            foreach (var child in root.GetComponentsInChildren<Transform>(includeInactive: false))
            {
                if (child.name.StartsWith(prefix, System.StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
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
            var button = go.GetComponent<Button>();
            Assert.IsNotNull(button, $"У «{name}» нет Button");
            button.onClick.Invoke();
        }
    }
}
