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
    /// <summary>
    /// ВРЕМЕННО: тестовый противник и кнопка «Убить» на основном экране.
    /// Удаляется вместе с пунктом ROADMAP «Временная тестовая обвязка» (блок I).
    /// </summary>
    public sealed class TestEnemyPlayModeTests
    {
        /// <summary>Кнопка «Убить» завершает бой победой, начисляет опыт и не убивает игрока.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator TestEnemy_Kill_FinishesFightWithVictory()
        {
            yield return StartGame();
            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден");
            var session = screen!.Session!;
            var player = session.State.Player!;
            var xpBefore = player.Xp;

            var kill = GameObject.Find("TestEnemyKill");
            Assert.IsNotNull(kill, "Кнопка тестового противника не найдена");
            var button = kill!.GetComponent<Button>();
            Assert.IsNotNull(button, "У кнопки тестового противника нет Button");

            button!.onClick.Invoke();
            yield return null;

            Assert.IsFalse(session.InCombat, "бой должен завершиться");
            Assert.Greater(player.Xp, xpBefore, "опыт за победу должен начислиться");
            Assert.Greater(player.Health, 0, "тестовый бой не должен убивать игрока");
        }

        /// <summary>Тестового противника можно убить повторно: блок остаётся на экране.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator TestEnemy_KillTwice_KeepsWorking()
        {
            yield return StartGame();
            var screen = Object.FindAnyObjectByType<GameScreen>();
            var session = screen!.Session!;
            var player = session.State.Player!;
            Click("TestEnemyKill");
            yield return null;
            var xpAfterFirst = player.Xp;

            Click("TestEnemyKill");
            yield return null;

            Assert.IsFalse(session.InCombat, "второй бой тоже завершается");
            Assert.Greater(player.Xp, xpAfterFirst, "опыт начисляется и за второй бой");
        }

        private static IEnumerator StartGame()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;
            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;
        }

        private static void Click(string name)
        {
            var go = GameObject.Find(name);
            Assert.IsNotNull(go, $"Объект «{name}» не найден");
            var button = go!.GetComponent<Button>();
            Assert.IsNotNull(button, $"У «{name}» нет Button");
            button!.onClick.Invoke();
        }
    }
}
