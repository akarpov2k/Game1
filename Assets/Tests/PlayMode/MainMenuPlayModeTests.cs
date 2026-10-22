#nullable enable
using System.Collections;
using System.Collections.Generic;
using AnomalyPath.Core.Save;
using AnomalyPath.Runtime;
using AnomalyPath.Runtime.Screens.Menu;
using AnomalyPath.Runtime.Ui.Core;
using AnomalyPath.Runtime.Ui.Items;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
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
            Click("Персонаж");
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

        /// <summary>Правый клик по слоту открывает меню, «Снять» возвращает предмет в инвентарь.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Character_RightClickSlot_ShowsMenuAndUnequips()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;
            Click("Персонаж");
            yield return null;

            var slot = GameObject.Find("SlotHelmet");
            Assert.IsNotNull(slot, "Нет слота шлема");
            Assert.IsNotNull(slot.GetComponent<ItemMenuTrigger>(), "У слота нет меню предмета");
            RightClick(slot, new Vector2(400f, 300f));
            yield return null;

            Assert.IsNotNull(GameObject.Find("ItemMenu"), "Меню не открылось");
            var unequip = GameObject.Find("Снять");
            Assert.IsNotNull(unequip, "В меню нет пункта «Снять»");
            unequip.GetComponent<Button>().onClick.Invoke();
            yield return null;

            var value = GameObject.Find("SlotHelmetValue");
            Assert.IsNotNull(value, "Нет подписи слота шлема");
            Assert.AreEqual("—", value.GetComponent<Text>().text, "Шлем не снялся");
        }

        /// <summary>Перетаскивание из слота в сетку инвентаря снимает предмет.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Character_DragFromSlotToInventory_Unequips()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;
            Click("Персонаж");
            yield return null;

            var slot = GameObject.Find("SlotMain");
            var inventory = GameObject.Find("InventoryFrame");
            Assert.IsNotNull(slot, "Нет слота основного оружия");
            Assert.IsNotNull(inventory, "Нет фрейма инвентаря");
            Assert.IsNotNull(slot.GetComponent<ItemDragSource>(), "У слота нет источника перетаскивания");
            Assert.IsNotNull(inventory.GetComponent<ItemDropZone>(), "У инвентаря нет зоны сброса");

            var data = new PointerEventData(EventSystem.current)
            {
                position = new Vector2(960f, 540f),
                button = PointerEventData.InputButton.Left
            };
            ExecuteEvents.Execute(slot, data, ExecuteEvents.beginDragHandler);
            ExecuteEvents.Execute(inventory, data, ExecuteEvents.dropHandler);
            yield return null;

            var value = GameObject.Find("SlotMainValue");
            Assert.IsNotNull(value, "Нет подписи слота основного оружия");
            Assert.AreEqual("—", value.GetComponent<Text>().text, "Оружие не снялось перетаскиванием");
        }

        /// <summary>Поле имени заполнено по умолчанию и обновляется кнопкой случайного имени.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Nickname_IsFilledAndRandomizable()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;

            var field = GameObject.Find("Input");
            Assert.IsNotNull(field, "Нет поля имени");
            var input = field.GetComponent<TextInputField>();
            Assert.IsNotNull(input, "У поля нет TextInputField");
            Assert.IsFalse(string.IsNullOrWhiteSpace(input.Text), "Имя должно быть заполнено по умолчанию");
            Assert.IsNotNull(GameObject.Find("Случайное имя"), "Нет кнопки случайного имени");

            Click("Случайное имя");
            yield return null;

            Assert.IsFalse(string.IsNullOrWhiteSpace(input.Text), "Имя после генерации");
            Assert.LessOrEqual(input.Text.Length, 24, "Длина имени");
        }

        /// <summary>Характеристики внутри фрейма снаряжения показывают числа, ник и уровень — над слотами.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Character_StatsInsideEquipmentFrame_ShowNumericValues()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;
            Click("Начать");
            yield return null;
            Click("Персонаж");
            yield return null;

            Assert.IsNotNull(GameObject.Find("Nickname"), "Нет ника на экране персонажа");
            Assert.IsNotNull(GameObject.Find("Level"), "Нет уровня на экране персонажа");
            foreach (var key in new[] { "Health", "Food", "Water", "Stamina" })
            {
                var label = GameObject.Find("Stat" + key + "Label");
                Assert.IsNotNull(label, "Нет подписи характеристики " + key);
                var value = GameObject.Find("Stat" + key + "Value");
                Assert.IsNotNull(value, "Нет значения характеристики " + key);
                StringAssert.Contains("/", value.GetComponent<Text>().text,
                    "Значение " + key + " должно быть числом вида 100/100");
            }
        }

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

            Click("Персонаж");
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
            Assert.IsFalse(screen.HandleEscape(), "Esc не должен срабатывать на игровой панели");
            Assert.IsNotNull(screen.Router.Current, "Нет активного экрана");
            Assert.AreEqual("InGame", screen.Router.Current!.name, "Экран сменился по Esc в игре");
        }

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
            Click("Выйти");
            yield return null;

            var dialog = GameObject.Find("ExitDialog");
            Assert.IsNotNull(dialog, "Нет диалога выхода");
            Assert.IsTrue(dialog.activeSelf, "Диалог выхода не открылся");
            Assert.IsTrue(screen.HandleEscape(), "Esc в диалоге выхода не обработан");
            yield return null;

            Assert.IsFalse(dialog.activeSelf, "Esc не закрыл диалог выхода");
            Assert.IsTrue(GameObject.Find("InGame").activeSelf, "Игровая панель пропала после закрытия диалога");
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

            Click("Сохранить игру");
            yield return null;
            AssertPageNavigation("сохранения");

            Click("Назад");
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

        private static int CountSaveTiles()
        {
            var root = GameObject.Find("SaveSlots");
            if (root == null)
            {
                return 0;
            }

            var count = 0;
            foreach (Transform child in root.transform)
            {
                if (child.gameObject.activeSelf && child.name.StartsWith("SaveSlot_"))
                {
                    count++;
                }
            }

            return count;
        }

        private static GameObject? FindNewSlotTile()
        {
            var root = GameObject.Find("SaveSlots");
            if (root == null)
            {
                return null;
            }

            foreach (Transform child in root.transform)
            {
                if (!child.gameObject.activeSelf || !child.name.StartsWith("SaveSlot_"))
                {
                    continue;
                }

                var label = child.GetComponentInChildren<Text>();
                if (label != null && label.text.Contains("Новое сохранение"))
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        private static Image BackdropOf(string panelName)
        {
            var panel = GameObject.Find(panelName);
            Assert.IsNotNull(panel, $"Панель «{panelName}» не найдена");
            var backdrop = panel.transform.Find("ScreenBackdrop");
            Assert.IsNotNull(backdrop, $"У окна «{panelName}» нет задника");
            var image = backdrop.GetComponent<Image>();
            Assert.IsNotNull(image, $"У задника «{panelName}» нет Image");
            return image;
        }

        private static void RightClick(GameObject target, Vector2 position)
        {
            var data = new PointerEventData(EventSystem.current)
            {
                position = position,
                button = PointerEventData.InputButton.Right
            };
            ExecuteEvents.Execute(target, data, ExecuteEvents.pointerClickHandler);
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
