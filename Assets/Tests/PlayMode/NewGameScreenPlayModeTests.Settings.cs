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
    /// <summary>Часть класса <see cref="NewGameScreenPlayModeTests"/>: настройки.</summary>
    public sealed partial class NewGameScreenPlayModeTests
    {
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
            Assert.AreEqual(2, right.transform.childCount, "В правой колонке торговля и пермасмерть");
            Assert.AreEqual(2, left.transform.childCount, "В левой колонке страховка и потеря опыта");

            var insurance = FindInPanel("InsuranceField");
            var trade = FindInPanel("TradeField");
            var xpLoss = FindInPanel("XpLossField");
            Assert.IsNotNull(insurance, "Нет поля шанса страховки");
            Assert.IsNotNull(trade, "Нет поля усложнения торговли");
            Assert.IsNotNull(xpLoss, "Нет поля потери опыта");
            Assert.IsNotNull(insurance.GetComponent<NumberField>(), "У страховки нет NumberField");
            Assert.IsNotNull(trade.GetComponent<NumberField>(), "У торговли нет NumberField");
            Assert.IsNotNull(xpLoss.GetComponent<NumberField>(), "У потери опыта нет NumberField");
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

        /// <summary>Потеря опыта по умолчанию 40%, шаг 10%, больше 100% не поднимается.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_XpLoss_StepAndMaximum()
        {
            yield return OpenNewGame();

            var xpLoss = FindInPanel("XpLossField").GetComponent<NumberField>();
            Assert.AreEqual(0.4, xpLoss.Value, 1e-6, "Потеря опыта по умолчанию");
            ClickStep(xpLoss, "SettingFieldIncrease");
            Assert.AreEqual(0.5, xpLoss.Value, 1e-6, "Шаг увеличения 0.1");
            ClickStep(xpLoss, "SettingFieldDecrease");
            Assert.AreEqual(0.4, xpLoss.Value, 1e-6, "Шаг уменьшения 0.1");

            FindInPanel("XpLossFieldInput").GetComponent<TextInputField>().SetText("5");
            Assert.AreEqual(1, xpLoss.Value, 1e-6, "Больше 100% не поднимается");
        }

        /// <summary>Выбранная потеря опыта попадает в настройки старта.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_XpLoss_PassedToSettings()
        {
            yield return OpenNewGame();

            var xpLoss = FindInPanel("XpLossField").GetComponent<NumberField>();
            ClickStep(xpLoss, "SettingFieldIncrease");
            Click("Начать");
            yield return null;

            var host = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(host.Session, "Игра не началась");
            Assert.AreEqual(0.5, host.Session!.State.Settings!.XpLossChance, 1e-6, "Потеря опыта");
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

            Click("Начать");
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
            AssertField(FindInPanel("XpLossFieldInput"), "потеря опыта");
        }
    }
}
