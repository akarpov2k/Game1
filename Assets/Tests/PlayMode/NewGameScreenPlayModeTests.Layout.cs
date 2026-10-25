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
    /// <summary>Часть класса <see cref="NewGameScreenPlayModeTests"/>: разметка экрана.</summary>
    public sealed partial class NewGameScreenPlayModeTests
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
    }
}
