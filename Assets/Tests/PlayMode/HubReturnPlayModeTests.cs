#nullable enable
using System.Collections;
using AnomalyPath.Core.World;
using AnomalyPath.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AnomalyPath.Tests.PlayMode
{
    /// <summary>Возврат в хаб с игрового экрана: действие на пути, номер шага и цена в сводке.</summary>
    public sealed class HubReturnPlayModeTests
    {
        /// <summary>С пути действие «В хаб» есть, сводка показывает шаг и цену, возврат ведёт в хаб.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator HubReturn_FromPath_ShowsActionAndReturnsToHub()
        {
            yield return StartGame();
            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден");
            var world = screen!.Session!.State.World;

            Click("LeaveHub");
            yield return null;

            Assert.AreEqual(LocationSubtype.Path, world.CurrentLocation.Subtype, "стоим на пути");
            Assert.AreEqual(1, world.CurrentLocation.RouteStep, "первый шаг");
            var action = GameObject.Find("ReturnToHub");
            Assert.IsNotNull(action, "На пути нет действия «В хаб»");
            var summary = GameObject.Find("LocationSummary");
            Assert.IsNotNull(summary, "Нет сводки локации");
            StringAssert.Contains("Шаг 1", summary!.GetComponent<Text>().text, "Номер шага в сводке");
            StringAssert.Contains("2", summary.GetComponent<Text>().text, "Цена возврата в сводке");

            Click("ReturnToHub");
            yield return null;

            Assert.AreEqual(LocationSubtype.Hub, world.CurrentLocation.Subtype, "вернулись в хаб");
            Assert.IsNull(GameObject.Find("ReturnToHub"), "В хабе действия «В хаб» нет");
        }

        /// <summary>Из здания в хаб вернуться нельзя: действия нет.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator HubReturn_FromBuilding_NoAction()
        {
            yield return StartGame();
            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден");
            var world = screen!.Session!.State.World;
            var building = FindSubtype(world.Map, LocationSubtype.Building) ?? AddBuilding(world);
            world.MoveTo(building.InstanceId);
            screen.ShowGame(false);
            yield return null;

            Assert.IsNull(GameObject.Find("ReturnToHub"), "Из здания в хаб не вернуться");
        }

        private static LocationInstance AddBuilding(WorldState world)
        {
            var location = new LocationInstance(
                "locinst_test_building",
                "loc_site_house",
                1,
                "bg_site_house",
                LocationSubtype.Building,
                1,
                0.2);
            world.Map.AddLocation(location);
            return location;
        }

        private static LocationInstance? FindSubtype(WorldMap map, LocationSubtype subtype)
        {
            foreach (var location in map.Locations)
            {
                if (location.Subtype == subtype)
                {
                    return location;
                }
            }

            return null;
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
            Assert.IsNotNull(go, $"Кнопка «{name}» не найдена");
            var button = go!.GetComponent<Button>();
            Assert.IsNotNull(button, $"У «{name}» нет Button");
            button!.onClick.Invoke();
        }
    }
}
