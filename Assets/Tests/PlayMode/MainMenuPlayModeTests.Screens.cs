#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.World;
using AnomalyPath.Runtime;
using AnomalyPath.Runtime.Screens.Game;
using AnomalyPath.Runtime.Screens.Menu;
using AnomalyPath.Runtime.Ui.Core;
using AnomalyPath.Runtime.Ui.Items;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AnomalyPath.Tests.PlayMode
{
    /// <summary>Часть класса <see cref="MainMenuPlayModeTests"/>: экраны и фон.</summary>
    public sealed partial class MainMenuPlayModeTests
    {
        /// <summary>У всех окон есть тематический задник; на экране персонажа он подменяется фоном локации.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Screens_HaveThemedBackdrop()
        {
            yield return LoadMain();

            var menu = BackdropOf("MainMenu");
            Assert.IsNotNull(menu.sprite, "У стартового меню нет тематического задника");
            Assert.AreEqual(1f, menu.color.a, 0.01f, "Задник меню должен быть полностью непрозрачным");

            Click("Об игре");
            yield return null;
            Assert.IsNotNull(BackdropOf("Info").sprite, "У экрана сведений нет задника");
            Click("Назад");
            yield return null;

            Click("Новая игра");
            yield return null;
            Assert.IsNotNull(BackdropOf("NewGame").sprite, "У экрана новой игры нет задника");

            Click("Начать");
            yield return null;
            Assert.IsNotNull(BackdropOf("InGame").sprite, "У игровой панели нет задника");

            Click("Backpack");
            yield return null;
            var character = BackdropOf("Character");
            Assert.IsNotNull(character.sprite, "У экрана персонажа нет задника локации");
            Assert.Less(character.color.a, menu.color.a, "Фон локации должен быть приглушён");
        }

        /// <summary>Esc возвращает из настроек новой игры и из «Авторов», но не из игровой панели.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Escape_ReturnsToMenu_WhereDeclared()
        {
            yield return LoadMain();

            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден в сцене Main");

            Click("Авторы");
            yield return null;
            Assert.IsTrue(screen.HandleEscape(), "Esc из сведений не обработан");
            yield return null;
            Assert.IsTrue(GameObject.Find("MainMenu").activeSelf, "Esc не вернул в меню из «Авторов»");

            Click("Новая игра");
            yield return null;
            Assert.IsTrue(screen.HandleEscape(), "Esc из настроек новой игры не обработан");
            yield return null;
            Assert.IsTrue(GameObject.Find("MainMenu").activeSelf, "Esc не вернул в меню из новой игры");

            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;
            Assert.IsTrue(screen.HandleEscape(), "Esc на игровой панели не открыл меню");
            yield return null;
            Assert.IsTrue(GameObject.Find("GameMenu").activeSelf, "Esc не открыл меню игры");
            Assert.IsNull(GameObject.Find("InGame"), "Игровая панель осталась поверх меню");

            Assert.IsTrue(screen.HandleEscape(), "Esc в меню игры не обработан");
            yield return null;
            Assert.IsTrue(GameObject.Find("InGame").activeSelf, "Esc не вернул из меню в игру");
        }

        /// <summary>Esc на экране персонажа возвращает в игровое поле.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Escape_Character_ReturnsToGame()
        {
            yield return LoadMain();

            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден в сцене Main");

            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;
            Click("Backpack");
            yield return null;
            Assert.IsTrue(GameObject.Find("Character").activeSelf, "Экран персонажа не открылся");

            Assert.IsTrue(screen.HandleEscape(), "Esc на экране персонажа не обработан");
            yield return null;
            Assert.IsTrue(GameObject.Find("InGame").activeSelf, "Esc не вернул из персонажа в игру");
            Assert.IsNull(GameObject.Find("Character"), "Экран персонажа остался активным");
        }

        /// <summary>Стрелка часов идёт по дневной и ночной половинам и совпадает с временем.</summary>
        [Test]
        public void PlayClock_AngleFollowsDayAndNight()
        {
            Assert.AreEqual(0f, PlayClock.Angle(new WorldClock(1, 10, 10, 22)), 0.01f, "начало дня");
            Assert.AreEqual(300f, PlayClock.Angle(new WorldClock(1, 14, 10, 22)), 0.01f, "2 часа дня");
            Assert.AreEqual(180f, PlayClock.Angle(new WorldClock(1, 22, 10, 22)), 0.01f, "конец дня, начало ночи");
            Assert.AreEqual(120f, PlayClock.Angle(new WorldClock(1, 2, 10, 22)), 0.01f, "2 часа ночи");
        }
    }
}
