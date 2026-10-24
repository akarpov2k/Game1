#nullable enable
using System.Collections;
using AnomalyPath.Core.Trade;
using AnomalyPath.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AnomalyPath.Tests.PlayMode
{
    /// <summary>Проверяет торговцев в хабе: действие, экран, покупку и возврат.</summary>
    public sealed class TraderPlayModeTests
    {
        /// <summary>В хабе есть торговцы, экран открывается, покупка меняет деньги и инвентарь.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Trader_Hub_OpensScreen_AndBuyWorks()
        {
            yield return LoadMain();
            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;

            Assert.IsNotNull(GameObject.Find("Trade_trd_hub_smuggler"), "Нет действия торговца-контрабандиста");
            Assert.IsNotNull(GameObject.Find("Trade_trd_hub_medic"), "Нет действия фельдшера");

            Click("Trade_trd_hub_smuggler");
            yield return null;

            var panel = GameObject.Find("Trader");
            Assert.IsNotNull(panel, "Экран торговца не открылся");
            var title = GameObject.Find("TraderTitle");
            Assert.IsNotNull(title, "Нет заголовка торговца");
            Assert.AreEqual("Контрабандист", title!.GetComponent<Text>().text, "Имя торговца");

            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден");
            var player = screen!.Session!.State.Player!;
            var moneyBefore = player.Money;
            var ammoBefore = RequirementsTemplate.CountItem(player, "ammo_9x18");
            Assert.IsNotNull(GameObject.Find("Buy_ammo_9x18"), "Нет кнопки покупки патронов");

            Click("Buy_ammo_9x18");
            yield return null;

            Assert.AreEqual(moneyBefore - 6, player.Money, "Деньги должны уменьшиться на цену патрона");
            Assert.AreEqual(ammoBefore + 1, RequirementsTemplate.CountItem(player, "ammo_9x18"), "Патрон должен появиться");
            Assert.IsNotNull(FindByPrefix(panel!.transform, "Sell_"), "Строка продажи не создана");

            Click("TraderBack");
            yield return null;
            Assert.IsNull(GameObject.Find("Trader"), "Экран торговца должен закрыться");
            Assert.IsNotNull(GameObject.Find("Trade_trd_hub_smuggler"), "Действие торговца должно вернуться");
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

        private static GameObject? FindByPrefix(Transform root, string prefix)
        {
            foreach (Transform child in root)
            {
                if (child.name.StartsWith(prefix, System.StringComparison.Ordinal))
                {
                    return child.gameObject;
                }

                var nested = FindByPrefix(child, prefix);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }
    }
}
