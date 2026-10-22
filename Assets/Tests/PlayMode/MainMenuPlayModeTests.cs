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
    /// <summary>Проверяет стартовое меню и модальный диалог выхода в сцене Main.</summary>
    public sealed class MainMenuPlayModeTests
    {
        /// <summary>Сцена Main создаёт GameScreen и активное стартовое меню.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator MainScene_Start_ShowsMainMenu()
        {
            yield return LoadMain();

            var screen = Object.FindFirstObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден в сцене Main");

            var mainMenu = GameObject.Find("MainMenu");
            Assert.IsNotNull(mainMenu, "Панель MainMenu не создана");
            Assert.IsTrue(mainMenu.activeSelf, "Стартовое меню не активно");
        }

        /// <summary>Кнопка «Выйти» в игре открывает диалог с двумя вариантами выхода.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator InGame_ExitButton_ShowsModalDialog()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;
            Click("Выйти");
            yield return null;

            var dialog = GameObject.Find("ExitDialog");
            Assert.IsNotNull(dialog, "Модальный диалог выхода не создан");
            Assert.IsTrue(dialog.activeSelf, "Модальный диалог выхода не активен");
            Assert.IsNotNull(GameObject.Find("В меню"), "Нет кнопки «В меню»");
            Assert.IsNotNull(GameObject.Find("На рабочий стол"), "Нет кнопки «На рабочий стол»");
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
