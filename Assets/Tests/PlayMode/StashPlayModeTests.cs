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
    /// <summary>Тайник на игровом экране: объект локации, вскрытие и скрытие после вскрытия.</summary>
    public sealed class StashPlayModeTests
    {
        /// <summary>Без тайника объект локации скрыт.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Stash_WithoutStash_ObjectHidden()
        {
            yield return StartGame();
            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден");

            var locationObject = FindAnywhere("LocationObject");
            Assert.IsNotNull(locationObject, "Объект локации не создан");
            Assert.IsFalse(locationObject!.activeSelf, "Без тайника объект локации скрыт");
        }

        /// <summary>Тайник показывается и вскрывается кликом, после чего объект скрывается.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Stash_Present_ShownAndOpenedByClick()
        {
            yield return StartGame();
            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден");
            var location = screen!.Session!.State.World.CurrentLocation;
            location.MarkStashRolled();
            location.MarkStashPresent();

            screen.ShowGame(false);
            yield return null;

            var locationObject = FindAnywhere("LocationObject");
            Assert.IsNotNull(locationObject, "Объект локации не создан");
            Assert.IsTrue(locationObject!.activeSelf, "Тайник должен быть виден");

            Click("LocationObject");
            yield return null;

            Assert.IsTrue(location.StashOpened, "Тайник вскрыт");
            var after = FindAnywhere("LocationObject");
            Assert.IsFalse(after != null && after.activeSelf, "Вскрытый тайник скрывается");
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

        private static GameObject? FindAnywhere(string name)
        {
            var transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
            foreach (var transform in transforms)
            {
                if (string.Equals(transform.name, name, System.StringComparison.Ordinal))
                {
                    return transform.gameObject;
                }
            }

            return null;
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
