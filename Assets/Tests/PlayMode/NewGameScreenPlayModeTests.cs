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
    /// <summary>
    /// Экран старта игры по прототипу: ник с кубиком сверху, наборы в две колонки с маркером,
    /// настройки из общей строки и набор экипировки в собственной прокрутке.
    /// </summary>
    public sealed class NewGameScreenPlayModeTests
    {
        /// <summary>Ник и кубик стоят выше выбора наборов.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_NicknameAboveLoadouts()
        {
            yield return OpenNewGame();

            var nickname = FindInPanel("NicknameRow");
            var loadouts = FindInPanel("LoadoutBlock");
            Assert.IsNotNull(nickname, "Нет строки с ником");
            Assert.IsNotNull(loadouts, "Нет блока выбора набора");

            var nicknameRect = WorldRect((RectTransform)nickname.transform);
            var loadoutsRect = WorldRect((RectTransform)loadouts.transform);
            Assert.Greater(nicknameRect.yMin, loadoutsRect.yMax, "Ник должен быть выше выбора наборов");
        }

        /// <summary>Кубик показывает грань точками и выдаёт имя по нажатию.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Dice_RollsName()
        {
            yield return OpenNewGame();

            var dice = FindInPanel("DiceButton");
            Assert.IsNotNull(dice, "Нет кнопки-кубика");
            var button = dice.GetComponent<Button>();
            Assert.IsNotNull(button, "У кубика нет Button");
            var diceFace = dice.GetComponent<DiceButton>();
            Assert.IsNotNull(diceFace, "У кубика нет DiceButton");

            var pips = 0;
            foreach (var image in dice.GetComponentsInChildren<Image>(true))
            {
                if (image.name.StartsWith("Pip_", System.StringComparison.Ordinal))
                {
                    pips++;
                }
            }

            Assert.AreEqual(9, pips, "У кубика должно быть девять точек грани");
            Assert.Greater(diceFace.Face, 0, "Грань кубика не показана");

            var nickname = FindInPanel("NicknameInput").GetComponent<TextInputField>();
            var before = nickname.Text;
            button.onClick.Invoke();
            yield return null;

            Assert.IsFalse(string.IsNullOrWhiteSpace(nickname.Text), "Кубик не выдал имя");
            Assert.AreNotEqual(before, nickname.Text, "Кубик должен обновлять имя");
        }

        /// <summary>Наборы идут двумя колонками, маркер показывает ровно один выбранный.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Loadouts_TwoColumnsAndSingleMarker()
        {
            yield return OpenNewGame();

            var left = FindInPanel("LoadoutColumnLeft");
            var right = FindInPanel("LoadoutColumnRight");
            Assert.IsNotNull(left, "Нет левой колонки наборов");
            Assert.IsNotNull(right, "Нет правой колонки наборов");
            Assert.AreEqual(2, CountLoadoutButtons(), "В данных два набора — должно быть две кнопки");
            Assert.AreEqual(1, CountMarkers(), "Маркер выбранного набора должен быть один");

            var before = MarkerParent();
            Click("Разведчик");
            yield return null;

            Assert.AreEqual(1, CountMarkers(), "После выбора маркер должен остаться один");
            Assert.AreNotEqual(before, MarkerParent(), "Маркер не перешёл на выбранный набор");
        }

        /// <summary>Выбор другого набора перерисовывает окно набора экипировки.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_SelectOtherLoadout_UpdatesPreview()
        {
            yield return OpenNewGame();

            var preview = FindInPanel("NewGameInventoryFrame");
            Assert.IsTrue(HasItem(preview, "wpn_ak74"), "Первый набор должен быть с автоматом АК-74");

            Click("Разведчик");
            yield return null;

            var updated = FindInPanel("NewGameInventoryFrame");
            Assert.IsFalse(HasItem(updated, "wpn_ak74"), "Автомат остался в окне второго набора");
            Assert.IsTrue(HasItem(updated, "wpn_pm"), "Второй набор не отрисован в окне набора");
        }

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

        /// <summary>АК-74 в наборе лежит вертикально, а слот не закрашивает фон.</summary>
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
            Assert.Less(Quaternion.Angle(rect.localRotation, Quaternion.Euler(0f, 0f, 90f)), 0.1f,
                "Иконка АК-74 должна быть повёрнута");
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

        /// <summary>Строки настроек собираются из одной заготовки и стоят по две в ряд.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Settings_RowsFromTemplate()
        {
            yield return OpenNewGame();

            var left = FindInPanel("SettingsColumnLeft");
            var right = FindInPanel("SettingsColumnRight");
            Assert.IsNotNull(left, "Нет левой колонки настроек");
            Assert.IsNotNull(right, "Нет правой колонки настроек");
            Assert.AreEqual(1, right.transform.childCount, "В правой колонке одна настройка");
            Assert.AreEqual(2, left.transform.childCount, "В левой колонке две настройки: страховка и пермасмерть");

            var insurance = FindInPanel("InsuranceField");
            var trade = FindInPanel("TradeField");
            Assert.IsNotNull(insurance, "Нет поля шанса страховки");
            Assert.IsNotNull(trade, "Нет поля усложнения торговли");
            Assert.IsNotNull(insurance.GetComponent<NumberField>(), "У страховки нет NumberField");
            Assert.IsNotNull(trade.GetComponent<NumberField>(), "У торговли нет NumberField");
        }

        /// <summary>Поля настроек меняют значение кнопками шагом 0.05.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Settings_NumberFieldsStepByPointZeroFive()
        {
            yield return OpenNewGame();

            var insurance = FindInPanel("InsuranceField").GetComponent<NumberField>();
            Assert.AreEqual(0.15, insurance.Value, 1e-6, "Начальный шанс страховки");
            ClickStep(insurance, "SettingFieldIncrease");
            Assert.AreEqual(0.2, insurance.Value, 1e-6, "Шаг увеличения 0.05");
            ClickStep(insurance, "SettingFieldDecrease");
            ClickStep(insurance, "SettingFieldDecrease");
            Assert.AreEqual(0.1, insurance.Value, 1e-6, "Шаг уменьшения 0.05");

            var trade = FindInPanel("TradeField").GetComponent<NumberField>();
            Assert.AreEqual(1, trade.Value, 1e-6, "Начальное усложнение торговли");
            ClickStep(trade, "SettingFieldIncrease");
            Assert.AreEqual(1.1, trade.Value, 1e-6, "Шаг усложнения торговли 0.1");
        }

        /// <summary>Цифровой ввод обновляет значение и зажимается в диапазон.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Settings_TypedNumber_ClampedToRange()
        {
            yield return OpenNewGame();

            var field = FindInPanel("InsuranceField").GetComponent<NumberField>();
            var input = FindInPanel("InsuranceFieldInput");
            Assert.IsNotNull(input, "Нет поля ввода у настройки");
            var text = input.GetComponent<TextInputField>();
            Assert.IsNotNull(text, "У поля ввода нет TextInputField");

            text.SetText("0.8");
            Assert.AreEqual(0.8, field.Value, 1e-6, "Введённое значение");

            text.SetText("7");
            Assert.AreEqual(1, field.Value, 1e-6, "Значение выше максимума зажимается");

            text.SetText("abc");
            Assert.AreEqual(1, field.Value, 1e-6, "Буквы не меняют значение");
            Assert.AreEqual("1.00", text.Text, "Недопустимый ввод откатывается к последнему значению");
        }

        /// <summary>Чекбокс пермасмерти выключен по умолчанию, переключается и попадает в настройки старта.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Permadeath_CheckboxPassedToSettings()
        {
            yield return OpenNewGame();

            var row = FindInPanel("PermadeathRow");
            Assert.IsNotNull(row, "Нет строки пермасмерти");
            var checkbox = row.GetComponent<CheckboxRow>();
            Assert.IsNotNull(checkbox, "У строки пермасмерти нет CheckboxRow");
            Assert.IsFalse(checkbox.Value, "Пермасмерть по умолчанию выключена");

            var toggle = row.GetComponentInChildren<Toggle>(true);
            Assert.IsNotNull(toggle, "Нет чекбокса пермасмерти");
            toggle.isOn = true;
            yield return null;
            Assert.IsTrue(checkbox.Value, "Чекбокс не переключился");

            Click("Начать игру");
            yield return null;

            var host = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(host.Session, "Игра не началась");
            var settings = host.Session!.State.Settings;
            Assert.IsNotNull(settings, "Настройки старта не сохранены в сессии");
            Assert.IsTrue(settings!.Permadeath, "Пермасмерть не попала в настройки старта");
        }

        /// <summary>Ползунков на экране нет: только поля с цифрами.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_HasNoSliders()
        {
            yield return OpenNewGame();

            var panel = GameObject.Find("NewGame");
            Assert.IsNotNull(panel, "Панель старта игры не создана");
            Assert.IsNull(panel.GetComponentInChildren<Slider>(true), "Ползунки на экране не нужны");
        }

        /// <summary>Пустой ник не даёт начать игру: показывается строка состояния.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Start_EmptyName_ShowsStatus()
        {
            yield return OpenNewGame();

            FindInPanel("NicknameInput").GetComponent<TextInputField>().SetText(string.Empty);
            Click("Начать");
            yield return null;

            var status = FindInPanel("Status");
            Assert.IsNotNull(status, "Нет строки состояния");
            Assert.IsTrue(status.activeSelf, "Строка состояния не показана");
            var host = Object.FindAnyObjectByType<GameScreen>();
            Assert.AreEqual(
                UiText.Get(host.Texts, UiTextKey.NewGame_Status_Enter_Name),
                status.GetComponent<Text>().text,
                "Статус не взят из каталога текстов");
        }

        /// <summary>Текст в полях ввода — насыщенный чёрный на светлом фоне.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Inputs_BlackTextOnLightField()
        {
            yield return OpenNewGame();

            AssertField(FindInPanel("NicknameInput"), "ник");
            AssertField(FindInPanel("InsuranceFieldInput"), "страховка");
        }

        private static void AssertField(GameObject input, string label)
        {
            Assert.IsNotNull(input, "Нет поля ввода: " + label);
            var background = input.GetComponent<Image>();
            Assert.IsNotNull(background, "У поля нет фона: " + label);
            Assert.Greater(background.color.r, 0.8f, "Фон поля должен быть светлым: " + label);

            var text = input.transform.GetChild(0).GetComponent<Text>();
            Assert.IsNotNull(text, "У поля нет текста: " + label);
            Assert.Less(text.color.r, 0.2f, "Текст поля должен быть чёрным: " + label);
            Assert.Less(text.color.g, 0.2f, "Текст поля должен быть чёрным: " + label);
            Assert.Less(text.color.b, 0.2f, "Текст поля должен быть чёрным: " + label);
            Assert.Greater(text.color.a, 0.9f, "Текст поля должен быть непрозрачным: " + label);
        }

        private static IEnumerator OpenNewGame()
        {
            yield return LoadMain();
            Click("Новая игра");
            yield return null;
            Canvas.ForceUpdateCanvases();
            yield return null;
        }

        private static int CountLoadoutButtons()
        {
            var count = 0;
            foreach (var columnName in new[] { "LoadoutColumnLeft", "LoadoutColumnRight" })
            {
                var column = FindInPanel(columnName);
                if (column == null)
                {
                    continue;
                }

                foreach (Transform child in column.transform)
                {
                    if (child.gameObject.activeSelf && child.GetComponent<Button>() != null)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static int CountMarkers()
        {
            var count = 0;
            foreach (var marker in FindMarkers())
            {
                if (marker.gameObject.activeSelf)
                {
                    count++;
                }
            }

            return count;
        }

        private static string? MarkerParent()
        {
            foreach (var marker in FindMarkers())
            {
                if (marker.gameObject.activeSelf)
                {
                    return marker.parent.name;
                }
            }

            return null;
        }

        private static List<Transform> FindMarkers()
        {
            var markers = new List<Transform>();
            var panel = GameObject.Find("NewGame");
            foreach (var child in panel.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "SelectedMarker")
                {
                    markers.Add(child);
                }
            }

            return markers;
        }

        private static void ClickStep(NumberField field, string name)
        {
            var button = field.transform.Find(name);
            Assert.IsNotNull(button, $"Нет кнопки {name} у поля {field.name}");
            button.GetComponent<Button>().onClick.Invoke();
        }

        private static GameObject FindInPanel(string name)
        {
            var panel = GameObject.Find("NewGame");
            Assert.IsNotNull(panel, "Панель старта игры не создана");
            foreach (var child in panel.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                {
                    return child.gameObject;
                }
            }

            return null!;
        }

        private static Transform? FindChild(GameObject root, string prefix)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name.StartsWith(prefix, System.StringComparison.Ordinal))
                {
                    return child;
                }
            }

            return null;
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
