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
    /// <summary>Часть класса <see cref="MainMenuPlayModeTests"/>: меню и новая игра.</summary>
    public sealed partial class MainMenuPlayModeTests
    {
        /// <summary>Сцена Main создаёт GameScreen и активное стартовое меню.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator MainScene_Start_ShowsMainMenu()
        {
            yield return LoadMain();

            var screen = Object.FindAnyObjectByType<GameScreen>();
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
            Click("Gear");
            yield return null;
            Click("Выйти");
            yield return null;

            var dialog = GameObject.Find("ExitDialog");
            Assert.IsNotNull(dialog, "Модальный диалог выхода не создан");
            Assert.IsTrue(dialog.activeSelf, "Модальный диалог выхода не активен");
            Assert.IsNotNull(GameObject.Find("В меню"), "Нет кнопки «В меню»");
            Assert.IsNotNull(GameObject.Find("На рабочий стол"), "Нет кнопки «На рабочий стол»");
        }

        /// <summary>Экран новой игры показывает наборы по именам из данных, «Назад» возвращает в меню.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_ShowsLoadoutNames_BackReturnsToMenu()
        {
            yield return LoadMain();

            var mainMenu = GameObject.Find("MainMenu");
            Assert.IsNotNull(mainMenu, "Стартовое меню не найдено в сцене Main");

            Click("Новая игра");
            yield return null;

            var newGame = GameObject.Find("NewGame");
            Assert.IsNotNull(newGame, "Панель новой игры не создана");
            Assert.IsTrue(newGame.activeSelf, "Панель новой игры не активна");
            Assert.IsNotNull(GameObject.Find("Стрелок"), "Набор не показан по имени из каталога текстов");
            Assert.IsFalse(mainMenu.activeSelf, "Стартовое меню осталось активным при открытии новой игры");

            Click("Назад");
            yield return null;

            Assert.IsFalse(newGame.activeSelf, "Панель новой игры осталась активной");
            Assert.IsTrue(mainMenu.activeSelf, "Стартовое меню не вернулось");
        }

        /// <summary>Пункт «Об игре» открывает сведения, «Назад» возвращает в меню.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Info_Back_ReturnsToMenu()
        {
            yield return LoadMain();

            var mainMenu = GameObject.Find("MainMenu");
            Click("Об игре");
            yield return null;

            var info = GameObject.Find("Info");
            Assert.IsNotNull(info, "Панель сведений не создана");
            Assert.IsTrue(info.activeSelf, "Панель сведений не активна");
            Assert.IsFalse(mainMenu.activeSelf, "Стартовое меню осталось активным");

            Click("Назад");
            yield return null;

            Assert.IsFalse(info.activeSelf, "Панель сведений осталась активной");
            Assert.IsTrue(GameObject.Find("MainMenu").activeSelf, "Стартовое меню не вернулось");
        }

        /// <summary>Экран персонажа показывает слоты снаряжения и сетку инвентаря, «Назад» возвращает в игру.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Character_ShowsEquipmentAndInventory_BackReturnsToGame()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;
            Click("Backpack");
            yield return null;

            var character = GameObject.Find("Character");
            Assert.IsNotNull(character, "Панель персонажа не создана");
            Assert.IsTrue(character.activeSelf, "Панель персонажа не активна");
            Assert.IsNotNull(GameObject.Find("SlotMain"), "Нет слота основного оружия");
            Assert.IsNotNull(GameObject.Find("SlotSecondary"), "Нет слота дополнительного оружия");
            Assert.IsNotNull(GameObject.Find("SlotHelmet"), "Нет слота шлема");
            Assert.IsNotNull(GameObject.Find("SlotArmor"), "Нет слота брони");
            Assert.IsNotNull(GameObject.Find("SlotBackpack"), "Нет слота рюкзака");
            Assert.IsNotNull(GameObject.Find("SlotBelt"), "Нет слота пояса артефактов");
            Assert.IsNotNull(GameObject.Find("ArtifactSlot_0"), "Нет столбца слотов артефактов");
            Assert.IsNotNull(GameObject.Find("InventoryTitle"), "Нет заголовка инвентаря");
            Assert.Greater(CountChildren(character, "Cell_"), 0, "Клетки инвентаря не созданы");
            Assert.Greater(CountChildren(character, "ItemLabel_"), 0, "Предметы инвентаря не показаны");

            Click("Назад");
            yield return null;

            Assert.IsTrue(GameObject.Find("InGame").activeSelf, "Игровая панель не вернулась");
            Assert.IsFalse(character.activeSelf, "Панель персонажа осталась активной");
        }

        /// <summary>Шестерёнка открывает меню без кнопки персонажа, рюкзак — экран персонажа.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator PlayScreen_GearAndBackpack_OpenMenuAndCharacter()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;
            Assert.IsTrue(GameObject.Find("InGame").activeSelf, "Основной экран не открылся");

            Click("Gear");
            yield return null;
            var menu = GameObject.Find("GameMenu");
            Assert.IsNotNull(menu, "Шестерёнка не открыла меню игры");
            Assert.IsTrue(menu.activeSelf, "Меню игры не активно");
            Assert.IsNull(GameObject.Find("Персонаж"), "В меню не должно быть кнопки персонажа");
            Assert.IsNotNull(GameObject.Find("Продолжить"), "В меню нет кнопки «Продолжить»");

            Click("Продолжить");
            yield return null;
            Assert.IsTrue(GameObject.Find("InGame").activeSelf, "Меню не вернуло в игру");

            Click("Backpack");
            yield return null;
            Assert.IsTrue(GameObject.Find("Character").activeSelf, "Рюкзак не открыл экран персонажа");
        }

        /// <summary>Основной экран берёт непрозрачный фон локации и показывает действия перехода.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator PlayScreen_ShowsLocationBackgroundAndTravelActions()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;

            var backdrop = BackdropOf("InGame");
            Assert.IsNotNull(backdrop.sprite, "У основного экрана нет фона локации");
            StringAssert.StartsWith("bg_", backdrop.sprite!.name, "Фон должен быть картинкой локации");
            Assert.AreEqual(1f, backdrop.color.a, 0.01f, "Фон основного экрана должен быть непрозрачным");

            var summary = GameObject.Find("LocationSummary");
            Assert.IsNotNull(summary, "На экране нет сведений о локации");
            StringAssert.Contains("Тип:", summary!.GetComponent<Text>().text, "Сведения о локации не выведены");
            StringAssert.DoesNotContain("Посещена", summary.GetComponent<Text>().text, "Характеристика посещения не должна выводиться");

            Assert.IsNotNull(GameObject.Find("Clock"), "На экране нет часов");
            Assert.IsNotNull(GameObject.Find("ClockFace"), "У часов нет циферблата");
            Assert.IsNotNull(GameObject.Find("ClockHand"), "У часов нет стрелки");
            var clockLabel = GameObject.Find("Clock")!.transform.Find("Text");
            Assert.IsNotNull(clockLabel, "У часов нет численного времени");
            StringAssert.Contains("10:00", clockLabel!.GetComponent<Text>().text, "Часы не показывают время начала игры");
            Assert.AreEqual(0f, GameObject.Find("ClockHand")!.GetComponent<RectTransform>().localEulerAngles.z, 0.01f,
                "В начале дня стрелка должна стоять на верхней границе дневной половины");

            Assert.IsNull(GameObject.Find("Меню"), "У шестерёнки не должно быть подписи");
            Assert.IsNull(GameObject.Find("Персонаж"), "У рюкзака не должно быть подписи");
            Assert.IsNull(GameObject.Find("LocationObject"), "Объект не должен рисоваться без данных локации");

            var hasObjectStructure = false;
            foreach (var transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (transform.name == "LocationObject")
                {
                    hasObjectStructure = true;
                    break;
                }
            }

            Assert.IsTrue(hasObjectStructure, "Структура объекта должна оставаться в коде экрана");
            Assert.IsNotNull(FindAction("LeaveHub"), "В хабе нет действия «Покинуть хаб»");
            Assert.IsNull(FindAction("Travel_"), "В хабе не должно быть отдельных переходов: маршрут строится при выходе");
        }

        /// <summary>Кнопка «Покинуть хаб» строит маршрут и ведёт на первую дорогу.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator PlayScreen_LeaveHub_GeneratesRoute()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;

            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден в сцене Main");
            var world = screen!.Session!.State.World;
            Assert.AreEqual(LocationSubtype.Hub, world.CurrentLocation.Subtype, "Игра начинается в хабе");

            var leave = FindAction("LeaveHub");
            Assert.IsNotNull(leave, "Нет кнопки «Покинуть хаб»");
            leave!.onClick.Invoke();
            yield return null;

            Assert.AreEqual(LocationSubtype.Path, world.CurrentLocation.Subtype, "Вышли на дорогу");
            Assert.AreEqual(1, world.CurrentLocation.RouteEdge, "Первое звено маршрута");
            Assert.AreEqual(1, world.CurrentLocation.RouteStep, "Первый шаг звена");
            Assert.IsNull(FindAction("LeaveHub"), "На дороге нет действия «Покинуть хаб»");
            Assert.IsNotNull(FindAction("Travel_"), "На дороге нет кнопок переходов по маршруту");
            Assert.IsNotNull(FindAction("ReturnToHub"), "На дороге нет возврата в хаб");
        }

        /// <summary>Кнопка перехода по маршруту уводит на следующую локацию и снимает пройденную.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator PlayScreen_Travel_MovesToAnotherLocation()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;

            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден в сцене Main");
            var world = screen!.Session!.State.World;
            Click("LeaveHub");
            yield return null;
            var before = world.CurrentLocationId;
            var travel = FindAction("Travel_");
            Assert.IsNotNull(travel, "Нет кнопки перехода");
            travel!.onClick.Invoke();
            yield return null;

            Assert.AreNotEqual(before, world.CurrentLocationId, "Переход не сменил локацию");
            Assert.IsFalse(world.Map.ContainsLocation(before), "Пройденная локация осталась на карте");
            Assert.IsTrue(GameObject.Find("InGame")!.activeSelf, "После перехода пропал основной экран");
        }

        /// <summary>Пять и больше действий уходят в прокрутку, колесо мыши двигает список.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator PlayActions_MoreThanFour_ScrollsWithWheel()
        {
            yield return LoadMain();

            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден в сцене Main");
            var canvas = GameObject.Find("Canvas");
            Assert.IsNotNull(canvas, "Нет канваса");

            var go = new GameObject("ActionListTest", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            var list = new PlayActionList(screen, go.transform);
            list.Root.anchorMin = new Vector2(0.5f, 0.5f);
            list.Root.anchorMax = new Vector2(0.5f, 0.5f);
            list.Root.sizeDelta = new Vector2(600f, PlayMetrics.ActionViewportHeight);

            var actions = new List<PlayAction>();
            for (var index = 0; index < 6; index++)
            {
                var number = index;
                actions.Add(new PlayAction("Action_" + number, "Действие " + number, () => { }));
            }

            list.SetActions(actions, "Нет действий");
            Canvas.ForceUpdateCanvases();
            yield return null;
            yield return null;

            var scroll = go.GetComponentInChildren<ScrollRect>(true);
            Assert.IsNotNull(scroll, "Нет вертикальной прокрутки действий");
            Assert.IsTrue(scroll!.vertical, "Прокрутка действий должна быть вертикальной");
            Assert.IsFalse(scroll.horizontal, "Горизонтальная прокрутка действий не нужна");
            Assert.Greater(scroll.content.rect.height, scroll.viewport.rect.height,
                "При шести действиях контент должен быть выше видимой части");
            Assert.IsTrue(scroll.verticalScrollbar.gameObject.activeSelf, "Полоса прокрутки не включилась");

            var catcher = scroll.viewport.GetComponent<Graphic>();
            Assert.IsNotNull(catcher, "Без Graphic область действий не получит колесо мыши");
            Assert.IsTrue(catcher.raycastTarget, "Область действий не ловит события указателя");

            scroll.verticalNormalizedPosition = 0.5f;
            Canvas.ForceUpdateCanvases();
            yield return null;
            var before = scroll.content.anchoredPosition.y;
            scroll.OnScroll(new PointerEventData(EventSystem.current) { scrollDelta = new Vector2(0f, -5f) });
            yield return null;
            Assert.AreNotEqual(before, scroll.content.anchoredPosition.y, "Колесо мыши не прокручивает действия");

            Object.Destroy(go);
        }

        private static Button? FindAction(string prefix)
        {
            foreach (var button in Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude))
            {
                if (button.name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return button!;
                }
            }

            return null;
        }

        private static int CountChildren(GameObject root, string prefix)
        {
            var count = 0;
            foreach (var child in root.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                if (child.name.StartsWith(prefix, System.StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
