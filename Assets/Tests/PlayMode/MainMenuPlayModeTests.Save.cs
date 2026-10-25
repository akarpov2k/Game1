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
    /// <summary>Часть класса <see cref="MainMenuPlayModeTests"/>: сохранения.</summary>
    public sealed partial class MainMenuPlayModeTests
    {
        /// <summary>Esc закрывает окно загрузки и модальный диалог выхода.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Escape_LoadAndExitDialog()
        {
            yield return LoadMain();

            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден в сцене Main");

            Click("Загрузить");
            yield return null;
            Assert.IsTrue(GameObject.Find("LoadGame").activeSelf, "Окно загрузки не открылось");
            Assert.IsTrue(screen.HandleEscape(), "Esc в окне загрузки не обработан");
            yield return null;
            Assert.IsTrue(GameObject.Find("MainMenu").activeSelf, "Esc не вернул из загрузки в меню");

            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;
            Click("Gear");
            yield return null;
            Click("Выйти");
            yield return null;

            var dialog = GameObject.Find("ExitDialog");
            Assert.IsNotNull(dialog, "Нет диалога выхода");
            Assert.IsTrue(dialog.activeSelf, "Диалог выхода не открылся");
            Assert.IsTrue(screen.HandleEscape(), "Esc в диалоге выхода не обработан");
            yield return null;

            Assert.IsFalse(dialog.activeSelf, "Esc не закрыл диалог выхода");
            Assert.IsTrue(GameObject.Find("GameMenu").activeSelf, "Меню игры пропало после закрытия диалога");
            Assert.IsTrue(screen.HandleEscape(), "Esc в меню игры не обработан");
            yield return null;
            Assert.IsTrue(GameObject.Find("InGame").activeSelf, "Esc не вернул из меню в игру");
        }

        /// <summary>Экран сохранения пишет сейв с названием и показывает уровень и деньги в списке загрузки.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator SaveScreen_SavesNamedSlot_AndLoadListShowsProgress()
        {
            yield return LoadMain();

            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден в сцене Main");
            screen.Saves.Delete(SaveSlots.Slot(1));
            var slot = string.Empty;
            for (var number = 1; number <= 50 && slot.Length == 0; number++)
            {
                var candidate = SaveSlots.Slot(number);
                if (!screen.Saves.Exists(candidate))
                {
                    slot = candidate;
                }
            }

            Assert.IsFalse(string.IsNullOrEmpty(slot), "Не найден свободный слот сохранения");

            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;
            Click("Gear");
            yield return null;
            Click("Сохранить игру");
            yield return null;

            var panel = GameObject.Find("SaveGame");
            Assert.IsNotNull(panel, "Экран сохранения не создан");
            Assert.IsTrue(panel.activeSelf, "Экран сохранения не открылся");
            Assert.IsNull(GameObject.Find("SaveNameInput"), "Поля названия на экране быть не должно");
            Assert.IsNull(GameObject.Find("Сохранить"), "Кнопки сохранения на экране быть не должно");
            var newSlotTile = GameObject.Find("SaveSlot_" + slot);
            Assert.IsNotNull(newSlotTile, "Нет ячейки «Новое сохранение»");
            StringAssert.Contains("Новое сохранение", newSlotTile.GetComponentInChildren<Text>().text,
                "Свободная ячейка должна называться «Новое сохранение»");

            Assert.AreEqual(3, SaveSlotLayout.ColumnsForWidth(1000f), "На широком экране — 3 ячейки в строке");
            Assert.AreEqual(2, SaveSlotLayout.ColumnsForWidth(600f), "На узком экране — 2 ячейки в строке");

            var slotsRoot = GameObject.Find("SaveSlots");
            Assert.IsNotNull(slotsRoot, "Нет сетки ячеек сохранения");
            var tiles = new List<RectTransform>();
            foreach (Transform child in slotsRoot.transform)
            {
                if (child.gameObject.activeSelf && child.name.StartsWith("SaveSlot_"))
                {
                    tiles.Add((RectTransform)child);
                }
            }

            Assert.GreaterOrEqual(tiles.Count, 1, "Ячеек сохранения нет");
            var columns = new List<int>();
            var rows = new List<int>();
            foreach (var tile in tiles)
            {
                Assert.AreEqual(tile.rect.width, tile.rect.height, 0.5f, "Ячейка сохранения должна быть квадратом");
                var x = Mathf.RoundToInt(tile.anchoredPosition.x);
                var y = Mathf.RoundToInt(tile.anchoredPosition.y);
                if (!columns.Contains(x))
                {
                    columns.Add(x);
                }

                if (!rows.Contains(y))
                {
                    rows.Add(y);
                }
            }

            var panelWidth = ((RectTransform)panel.transform).rect.width;
            var expectedColumns = SaveSlotLayout.ColumnsForWidth(Mathf.Min(1200f, panelWidth - 160f));
            Assert.AreEqual(Mathf.Min(expectedColumns, tiles.Count), columns.Count, "Число квадратов в строке не по ширине экрана");
            Assert.AreEqual(Mathf.CeilToInt(tiles.Count / (float)Mathf.Min(expectedColumns, tiles.Count)), rows.Count, "Число рядов сетки");

            var name = GameObject.Find("SaveNameInput");
            Assert.IsNull(name, "Поле названия на экране быть не должно");

            Click("SaveSlot_" + slot);
            yield return null;
            var dialog = GameObject.Find("SaveNameDialog");
            Assert.IsNotNull(dialog, "Диалог сохранения не открылся");
            Assert.IsTrue(dialog.activeSelf, "Диалог сохранения не активен");
            var nameInput = GameObject.Find("SaveDialogName");
            Assert.IsNotNull(nameInput, "В диалоге нет поля названия");
            var input = nameInput.GetComponent<TextInputField>();
            StringAssert.StartsWith("Новое сохранение ", input.Text, "Не подставлено название по умолчанию");

            Assert.IsTrue(screen.HandleEscape(), "Esc в диалоге сохранения не обработан");
            yield return null;
            Assert.IsFalse(dialog.activeSelf, "Esc не закрыл диалог сохранения");
            Assert.IsTrue(GameObject.Find("SaveGame").activeSelf, "Esc закрыл и экран сохранения");
            Assert.IsFalse(screen.Saves.Exists(slot), "Esc сохранил игру");

            Click("SaveSlot_" + slot);
            yield return null;
            input = GameObject.Find("SaveDialogName").GetComponent<TextInputField>();
            input.SetText("Тестовый сейв");
            Click("Сохранить");
            yield return null;

            Assert.IsTrue(screen.Saves.Exists(slot), "Сохранение не записано");
            Assert.IsTrue(GameObject.Find("InGame").activeSelf, "После сохранения не вернулись в игру");

            SaveSlotInfo? saved = null;
            foreach (var item in screen.Saves.ListSlots())
            {
                if (item.Slot == slot)
                {
                    saved = item;
                    break;
                }
            }

            Assert.IsNotNull(saved, "Сохранение не видно в списке");
            var player = screen.Session!.State.Player!;
            Assert.AreEqual("Тестовый сейв", saved!.Title, "Название сохранения");
            Assert.AreEqual(player.Level, saved.Level, "Уровень в сохранении");
            Assert.AreEqual(player.Money, saved.Money, "Деньги в сохранении");
            Assert.Greater(saved.Money, 0, "Деньги должны быть заданы условно");

            Click("Gear");
            yield return null;
            Click("Загрузить игру");
            yield return null;

            var loadPanel = GameObject.Find("LoadGame");
            Assert.IsNotNull(loadPanel, "Экран загрузки не создан");
            Assert.IsTrue(loadPanel.activeSelf, "Экран загрузки не открылся");
            var loadRoot = GameObject.Find("LoadSlots");
            Assert.IsNotNull(loadRoot, "Нет сетки ячеек загрузки");
            var loadTiles = new List<RectTransform>();
            foreach (Transform child in loadRoot.transform)
            {
                if (child.gameObject.activeSelf && child.name.StartsWith("LoadSlot_"))
                {
                    loadTiles.Add((RectTransform)child);
                }
            }

            Assert.Greater(loadTiles.Count, 0, "В загрузке нет плиток сохранений");
            var loadColumns = new List<int>();
            foreach (var tile in loadTiles)
            {
                Assert.AreEqual(tile.rect.width, tile.rect.height, 0.5f, "Ячейка загрузки должна быть квадратом");
                var x = Mathf.RoundToInt(tile.anchoredPosition.x);
                if (!loadColumns.Contains(x))
                {
                    loadColumns.Add(x);
                }
            }

            var loadWidth = ((RectTransform)loadPanel.transform).rect.width;
            var loadExpected = SaveSlotLayout.ColumnsForWidth(Mathf.Min(1200f, loadWidth - 160f));
            Assert.AreEqual(Mathf.Min(loadExpected, loadTiles.Count), loadColumns.Count,
                "Число квадратов загрузки не по ширине экрана");

            var hasProgress = false;
            foreach (var label in loadPanel.GetComponentsInChildren<Text>())
            {
                if (label.text.Contains("Ур.") && label.text.Contains("руб"))
                {
                    hasProgress = true;
                    break;
                }
            }

            Assert.IsTrue(hasProgress, "В плитке загрузки нет уровня и денег");
            screen.Saves.Delete(slot);
        }

        /// <summary>Удаление сохранения спрашивает подтверждение; Esc и «Отмена» его закрывают, «Удалить» удаляет.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator LoadScreen_DeleteSave_AsksConfirmation()
        {
            yield return LoadMain();

            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден в сцене Main");
            var slot = SaveSlots.Slot(2);
            screen.Saves.Delete(slot);

            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;
            screen.Sessions.Save(screen.Session!, slot, title: "Удаляемый");
            Click("Gear");
            yield return null;
            Click("Загрузить игру");
            yield return null;

            Assert.IsNotNull(GameObject.Find("LoadSlot_" + slot), "Нет плитки сохранения");
            Click("DeleteSave_" + slot);
            yield return null;

            var dialog = GameObject.Find("ConfirmDialog");
            Assert.IsNotNull(dialog, "Нет подтверждения удаления");
            Assert.IsTrue(dialog.activeSelf, "Подтверждение не открылось");
            Assert.IsTrue(GameObject.Find("LoadGame").activeSelf, "Кнопка удаления сразу загрузила игру");

            Assert.IsTrue(screen.HandleEscape(), "Esc в подтверждении не обработан");
            yield return null;
            Assert.IsFalse(dialog.activeSelf, "Esc не закрыл подтверждение");
            Assert.IsTrue(GameObject.Find("LoadGame").activeSelf, "Esc закрыл и экран загрузки");
            Assert.IsTrue(screen.Saves.Exists(slot), "Esc удалил сохранение");

            Click("DeleteSave_" + slot);
            yield return null;
            Click("Отмена");
            yield return null;
            Assert.IsFalse(dialog.activeSelf, "Отмена не закрыла подтверждение");
            Assert.IsTrue(screen.Saves.Exists(slot), "Отмена удалила сохранение");

            Click("DeleteSave_" + slot);
            yield return null;
            Click("Удалить");
            yield return null;
            Assert.IsFalse(screen.Saves.Exists(slot), "Подтверждение не удалило сохранение");
            Assert.IsTrue(GameObject.Find("LoadGame").activeSelf, "После удаления ушли с экрана загрузки");
        }

        /// <summary>Страницы сохранений: «&lt;» и «&gt;» только при нескольких страницах, номер между ними.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator SaveAndLoad_Pagination_ShowsPageArrowsOnlyWhenNeeded()
        {
            yield return LoadMain();

            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден в сцене Main");
            for (var number = 1; number <= 12; number++)
            {
                screen.Saves.Delete(SaveSlots.Slot(number));
            }

            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;
            for (var number = 1; number <= 7; number++)
            {
                screen.Sessions.Save(screen.Session!, SaveSlots.Slot(number), title: "Сейв " + number);
            }

            Click("Gear");
            yield return null;
            Click("Сохранить игру");
            yield return null;
            AssertPageNavigation("сохранения");

            Click("Назад");
            yield return null;
            Click("Gear");
            yield return null;
            Click("Загрузить игру");
            yield return null;
            AssertPageNavigation("загрузки");

            for (var number = 1; number <= 12; number++)
            {
                screen.Saves.Delete(SaveSlots.Slot(number));
            }
        }

        private static void AssertPageNavigation(string screenName)
        {
            var guard = 0;
            while (GameObject.Find("PrevPage") != null && guard < 12)
            {
                Click("PrevPage");
                guard++;
            }

            var label = GameObject.Find("PageLabel");
            Assert.IsNotNull(label, $"На экране {screenName} нет номера страницы");
            StringAssert.StartsWith("1 / ", label.GetComponent<Text>().text, "Первая страница не первая");
            Assert.IsNull(GameObject.Find("PrevPage"), $"На первой странице {screenName} не должно быть «<»");
            Assert.IsNotNull(GameObject.Find("NextPage"), $"На первой странице {screenName} должна быть «>»");

            Click("NextPage");
            Assert.IsNotNull(GameObject.Find("PrevPage"), $"На второй странице {screenName} нет «<»");

            guard = 0;
            while (GameObject.Find("NextPage") != null && guard < 12)
            {
                Click("NextPage");
                guard++;
            }

            Assert.IsNull(GameObject.Find("NextPage"), $"На последней странице {screenName} не должно быть «>»");
            Assert.IsNotNull(GameObject.Find("PrevPage"), $"На последней странице {screenName} должна быть «<»");
        }

        /// <summary>Экран сохранения: сначала существующие сохранения, «Новое сохранение» — на следующей странице.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator SaveScreen_ExistingSavesFirst_NewSlotOnNextPage()
        {
            yield return LoadMain();

            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден в сцене Main");
            for (var number = 1; number <= 12; number++)
            {
                screen.Saves.Delete(SaveSlots.Slot(number));
            }

            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;
            for (var number = 1; number <= 6; number++)
            {
                screen.Sessions.Save(screen.Session!, SaveSlots.Slot(number), title: "Сейв " + number);
            }

            Click("Gear");
            yield return null;
            Click("Сохранить игру");
            yield return null;

            var label = GameObject.Find("PageLabel");
            Assert.IsNotNull(label, "Нет номера страницы");
            Assert.AreEqual("1 / 2", label.GetComponent<Text>().text, "Экран должен открываться на первой странице");
            Assert.IsNull(GameObject.Find("PrevPage"), "На первой странице не должно быть «<»");
            Assert.IsNotNull(GameObject.Find("NextPage"), "На первой странице должна быть «>»");
            for (var number = 1; number <= 6; number++)
            {
                var tile = GameObject.Find("SaveSlot_" + SaveSlots.Slot(number));
                Assert.IsNotNull(tile, $"На первой странице нет сохранения {number}");
                StringAssert.Contains("Сейв " + number, tile.GetComponentInChildren<Text>().text,
                    "Первая страница должна быть заполнена существующими сохранениями");
            }

            Assert.AreEqual(6, CountSaveTiles(), "На первой странице должно быть ровно 6 существующих сохранений");
            Assert.IsNull(FindNewSlotTile(), "На заполненной первой странице не должно быть «Нового сохранения»");

            Click("NextPage");
            yield return null;
            Assert.AreEqual("2 / 2", label.GetComponent<Text>().text, "Не переключились на вторую страницу");
            var newTile = FindNewSlotTile();
            Assert.IsNotNull(newTile, "На новой странице нет ячейки «Новое сохранение»");
            StringAssert.Contains("Новое сохранение", newTile!.GetComponentInChildren<Text>().text, "Подпись новой ячейки");
            Assert.GreaterOrEqual(CountSaveTiles(), 1, "На новой странице должна быть ячейка нового сохранения");

            Click("Назад");
            yield return null;
            for (var number = 1; number <= 7; number++)
            {
                screen.Saves.Delete(SaveSlots.Slot(number));
            }
        }
    }
}
