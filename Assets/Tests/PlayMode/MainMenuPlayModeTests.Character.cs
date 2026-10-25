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
    /// <summary>Часть класса <see cref="MainMenuPlayModeTests"/>: экран персонажа.</summary>
    public sealed partial class MainMenuPlayModeTests
    {
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
            Click("Backpack");
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
            Click("Backpack");
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

        /// <summary>Ник заполнен по умолчанию, кубик выдаёт новое имя.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_Nickname_IsFilledAndRandomizable()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;

            var field = GameObject.Find("NicknameInput");
            Assert.IsNotNull(field, "Нет поля имени");
            var input = field.GetComponent<TextInputField>();
            Assert.IsNotNull(input, "У поля нет TextInputField");
            Assert.IsFalse(string.IsNullOrWhiteSpace(input.Text), "Имя должно быть заполнено по умолчанию");
            Assert.IsNotNull(GameObject.Find("DiceButton"), "Нет кнопки-кубика");

            Click("DiceButton");
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
            Click("Backpack");
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
    }
}
