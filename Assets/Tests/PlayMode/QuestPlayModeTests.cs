#nullable enable
using System.Collections;
using AnomalyPath.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AnomalyPath.Tests.PlayMode
{
    /// <summary>Проверяет журнал заданий: открытие с торговца, взятие и возврат.</summary>
    public sealed class QuestPlayModeTests
    {
        /// <summary>У торговца открывается журнал, задание берётся и появляется среди активных.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Quest_Trader_AcceptsQuest_AndReturns()
        {
            yield return LoadMain();
            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;

            Click("Trade_trd_hub_smuggler");
            yield return null;
            Assert.IsNotNull(GameObject.Find("TraderQuests"), "Нет кнопки заданий у торговца");

            Click("TraderQuests");
            yield return null;

            Assert.IsNotNull(GameObject.Find("Quests"), "Экран заданий не открылся");
            var offers = FindByPrefix(GameObject.Find("Quests")!.transform, "Offer_");
            Assert.IsNotNull(offers, "Нет доступных заданий");
            var take = FindByPrefix(offers!.transform, "Take");
            Assert.IsNotNull(take, "Нет кнопки «Взять»");
            take!.GetComponent<Button>().onClick.Invoke();
            yield return null;

            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден");
            Assert.AreEqual(1, screen!.Quests.GetActive(screen.Session!).Count, "Задание должно быть активным");
            Assert.IsNotNull(FindByPrefix(GameObject.Find("Quests")!.transform, "Active_"), "Нет активного задания");

            Click("QuestBack");
            yield return null;
            Assert.IsNotNull(GameObject.Find("Trader"), "Возврат к торговцу не сработал");
            Click("TraderBack");
            yield return null;
            Assert.IsNull(GameObject.Find("Quests"), "Журнал должен закрыться");
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
