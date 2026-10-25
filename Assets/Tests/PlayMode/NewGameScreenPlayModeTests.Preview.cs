#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AnomalyPath.Runtime;
using AnomalyPath.Runtime.Screens.Menu;
using AnomalyPath.Runtime.Ui.Core;
using AnomalyPath.Runtime.Ui.Items;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AnomalyPath.Tests.PlayMode
{
    /// <summary>Часть класса <see cref="NewGameScreenPlayModeTests"/>: превью набора.</summary>
    public sealed partial class NewGameScreenPlayModeTests
    {
        /// <summary>Набор рисуется картинками без подписей, денег и интерактива.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Preview_DrawsItemsWithoutText()
        {
            yield return OpenNewGame();

            var preview = FindInPanel("NewGameInventoryFrame");
            Assert.IsNotNull(preview, "Окно набора не создано");
            Assert.IsNotNull(preview.GetComponent<Image>(), "Рамка набора должна быть фоном панели");

            Assert.AreEqual(0, CountChildren(preview, "PreviewItemLabel_"),
                "В клетках набора не должно быть подписей");
            Assert.Greater(CountChildren(preview, "PreviewCell_"), 0, "Клетки сетки не созданы");
            Assert.GreaterOrEqual(CountItemBoxes(preview), 7, "Предметы набора не отрисованы");
            Assert.IsTrue(HasItem(preview, "itm_armor_plate"), "Броня набора не отрисована");
            Assert.IsTrue(HasItem(preview, "ammo_545"), "Патроны набора не отрисованы");

            var money = FindInPanel("PreviewMoney");
            Assert.IsNotNull(money, "В окне набора не показаны деньги");
            StringAssert.Contains("руб", money.GetComponent<Text>().text, "Деньги должны быть подписаны в рублях");

            Assert.AreEqual(0, preview.GetComponentsInChildren<ItemDropZone>(true).Length,
                "В наборе не должно быть зон сброса");
            Assert.AreEqual(0, preview.GetComponentsInChildren<ItemDragSource>(true).Length,
                "В наборе не должно быть перетаскивания");
            Assert.AreEqual(0, preview.GetComponentsInChildren<Button>(true).Length,
                "В наборе не должно быть кнопок");
        }

        /// <summary>Окно набора рисуется на текстуре инвентаря, даже если каталог ещё не пересобран.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Preview_UsesInventoryTexture()
        {
            yield return OpenNewGame();

            var preview = FindInPanel("NewGameInventoryFrame");
            Assert.IsNotNull(preview, "Окно набора не создано");
            var image = preview.GetComponent<Image>();
            Assert.IsNotNull(image, "У окна набора нет Image");
            Assert.IsNotNull(image.sprite, "Текстура инвентаря не применена");
            Assert.IsNotNull(image.sprite.texture, "У спрайта окна нет текстуры");
            Assert.AreEqual(344, image.sprite.texture.width, "Ширина текстуры инвентаря");
            Assert.AreEqual(458, image.sprite.texture.height, "Высота текстуры инвентаря");
        }

        /// <summary>Стартовая валюта стоит в верхней правой плашке окна набора.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Preview_Money_IsInTopRightPlate()
        {
            yield return OpenNewGame();

            var preview = FindInPanel("NewGameInventoryFrame");
            var money = FindInPanel("PreviewMoney");
            Assert.IsNotNull(preview, "Окно набора не создано");
            Assert.IsNotNull(money, "В окне набора не показаны деньги");

            var previewRect = WorldRect((RectTransform)preview.transform);
            var moneyRect = WorldRect((RectTransform)money.transform);
            Assert.Greater(moneyRect.center.x, previewRect.center.x + (previewRect.width * 0.15f),
                "Валюта должна быть в правой плашке окна");
            Assert.Greater(moneyRect.center.y, previewRect.yMin + (previewRect.height * 0.9f),
                "Валюта должна быть в верхней плашке окна");
            Assert.IsTrue(previewRect.Contains(moneyRect.center), "Валюта должна лежать внутри окна");

            var host = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(host, "GameScreen не найден");
            var selected = host!.Loadouts.All.OrderBy(loadout => loadout.Id, System.StringComparer.Ordinal).First();
            var startingMoney = selected.StartingMoney ?? host.Rules.Require().StartingMoney;
            var expected = UiText.Format(host.Texts, UiTextKey.Common_Money, startingMoney);
            Assert.AreEqual(expected, money!.GetComponent<Text>().text, "Валюта должна быть стартовой суммой набора");
        }

        /// <summary>АК-74 в наборе лежит вертикально (ствол вверх, магазин слева), а слот не закрашивает фон.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Ak74Preview_IsVerticalWithoutBackground()
        {
            yield return OpenNewGame();

            var preview = FindInPanel("NewGameInventoryFrame");
            Assert.IsNotNull(preview, "Окно набора не создано");

            var icon = FindChild(preview, "PreviewItemIcon_wpn_ak74_");
            Assert.IsNotNull(icon, "Иконка АК-74 в наборе не найдена");
            var rect = icon!.GetComponent<RectTransform>();
            Assert.Less(Quaternion.Angle(rect.localRotation, Quaternion.Euler(0f, 0f, -90f)), 0.1f,
                "Иконка АК-74 должна быть повёрнута по часовой: ствол вверх, магазин слева");
            var world = WorldRect(rect);
            Assert.Greater(world.height, world.width, "Иконка должна быть вытянута по высоте слота");

            var box = FindChild(preview, "PreviewItem_wpn_ak74_");
            Assert.IsNotNull(box, "Слот АК-74 не найден");
            Assert.AreEqual(0f, box!.GetComponent<Image>().color.a, 0.001f,
                "У слота не должно быть фона");
        }

        /// <summary>У набора своя вертикальная прокрутка, независимая от остальных областей.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Preview_HasOwnScroll()
        {
            yield return OpenNewGame();

            var scrollGo = FindInPanel("PreviewScroll");
            Assert.IsNotNull(scrollGo, "Нет прокрутки набора");
            var scroll = scrollGo.GetComponent<ScrollRect>();
            Assert.IsNotNull(scroll, "Нет ScrollRect у прокрутки набора");
            Assert.IsTrue(scroll.vertical, "Прокрутка набора должна быть вертикальной");
            Assert.IsFalse(scroll.horizontal, "Горизонтальная прокрутка набору не нужна");
            Assert.IsNotNull(scroll.verticalScrollbar, "У прокрутки набора нет полосы");
            Assert.AreEqual(ScrollRect.ScrollbarVisibility.AutoHide, scroll.verticalScrollbarVisibility,
                "Полоса должна включаться только при переполнении");

            var catcher = scroll.viewport.GetComponent<Graphic>();
            Assert.IsNotNull(catcher, "Без Graphic область прокрутки не получит колесо мыши");
            Assert.IsTrue(catcher.raycastTarget, "Область прокрутки не ловит события указателя");

            var content = (RectTransform)scroll.content;
            Assert.GreaterOrEqual(content.rect.height, scroll.viewport.rect.height - 1f,
                "Высота контента не может быть меньше видимой области");
        }
    }
}
