#nullable enable
using System.Collections;
using AnomalyPath.Runtime.Screens.Talents;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AnomalyPath.Tests.PlayMode
{
    /// <summary>Проверяет вход в дерево талантов с экрана персонажа и возврат.</summary>
    public sealed class TalentPlayModeTests
    {
        /// <summary>С экрана персонажа открывается дерево, показываются карточки и очки.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Talent_Character_OpensTree_AndReturns()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;
            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;

            Click("Backpack");
            yield return null;
            Assert.IsNotNull(GameObject.Find("Character"), "Экран персонажа не открылся");
            Assert.IsNotNull(GameObject.Find("TalentOpen"), "Нет кнопки талантов");

            Click("TalentOpen");
            yield return null;
            Assert.IsNotNull(GameObject.Find("Talents"), "Дерево талантов не открылось");
            Assert.IsNotNull(GameObject.Find("TalentNode_tal_steady_hand"), "Нет стартового таланта");
            Assert.IsNotNull(GameObject.Find("TalentNode_tal_toughness"), "Нет правой ветки");
            Assert.IsNotNull(GameObject.Find("TalentLink"), "Нет связей между талантами");
            var points = GameObject.Find("TalentPoints");
            Assert.IsNotNull(points, "Нет подписи очков");
            Assert.IsFalse(string.IsNullOrWhiteSpace(points!.GetComponent<Text>().text), "Очки не показаны");

            Click("TalentBack");
            yield return null;
            Assert.IsNotNull(GameObject.Find("Character"), "Возврат на экран персонажа не сработал");
        }

        /// <summary>Подсказка появляется после задержания курсора и скрывается при уходе.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Talent_Hover_ShowsTooltipAfterDelay_AndHidesOnExit()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;
            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;

            Click("Backpack");
            yield return null;
            Click("TalentOpen");
            yield return null;

            var node = GameObject.Find("TalentNode_tal_steady_hand");
            Assert.IsNotNull(node, "Нет стартового таланта");
            Assert.IsNotNull(node!.GetComponent<TalentHover>(), "У карточки нет наведения");
            var data = new PointerEventData(EventSystem.current)
            {
                position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)
            };
            ExecuteEvents.Execute(node, data, ExecuteEvents.pointerEnterHandler);

            yield return new WaitForSecondsRealtime(0.5f);
            Assert.IsNull(GameObject.Find("TalentTooltip"), "Подсказка появилась раньше задержки");

            yield return new WaitForSecondsRealtime(0.6f);
            var tooltip = GameObject.Find("TalentTooltip");
            Assert.IsNotNull(tooltip, "Подсказка не появилась");
            Assert.IsTrue(tooltip!.activeSelf, "Подсказка не активна");

            ExecuteEvents.Execute(node, data, ExecuteEvents.pointerExitHandler);
            yield return null;
            Assert.IsFalse(tooltip.activeSelf, "Подсказка не скрылась");
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
