#nullable enable
using System.Collections;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AnomalyPath.Tests.PlayMode
{
    /// <summary>Диалог лута после победы: сетка, «Забрать всё», предмет по клику и Esc.</summary>
    public sealed class LootPlayModeTests
    {
        /// <summary>Диалог показывает лут, «Забрать всё» переносит предметы в инвентарь и закрывается.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Loot_Dialog_TakeAll_MovesItemsToInventory()
        {
            yield return StartGame();
            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден");
            var player = screen!.Session!.State.Player!;
            var before = Count(player.Inventory, "ammo_9x18");

            screen.ShowLoot(new[] { new ItemInstance("loot_test_1", "ammo_9x18", 10) });
            yield return null;

            var dialog = GameObject.Find("LootDialog");
            Assert.IsNotNull(dialog, "Диалог лута не открылся");
            Assert.IsTrue(dialog!.activeSelf, "Диалог лута скрыт");

            Click("Забрать всё");
            yield return null;

            Assert.AreEqual(before + 10, Count(player.Inventory, "ammo_9x18"), "лут должен попасть в инвентарь");
            Assert.IsFalse(DialogOpen(), "диалог должен закрыться");
        }

        /// <summary>Клик по предмету забирает его и закрывает пустой диалог.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Loot_Dialog_TakeItem_MovesOnlyIt()
        {
            yield return StartGame();
            var screen = Object.FindAnyObjectByType<GameScreen>();
            var player = screen!.Session!.State.Player!;
            var before = Count(player.Inventory, "ammo_9x18");

            screen.ShowLoot(new[] { new ItemInstance("loot_test_2", "ammo_9x18", 5) });
            yield return null;

            Click("Loot_0");
            yield return null;

            Assert.AreEqual(before + 5, Count(player.Inventory, "ammo_9x18"), "предмет по клику");
            Assert.IsFalse(DialogOpen(), "пустой диалог закрывается");
        }

        /// <summary>Esc закрывает диалог, лут не переносится.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Loot_Dialog_Escape_ClosesWithoutTaking()
        {
            yield return StartGame();
            var screen = Object.FindAnyObjectByType<GameScreen>();
            var player = screen!.Session!.State.Player!;
            var before = Count(player.Inventory, "ammo_9x18");

            screen.ShowLoot(new[] { new ItemInstance("loot_test_3", "ammo_9x18", 7) });
            yield return null;
            Assert.IsTrue(DialogOpen(), "диалог должен быть открыт");

            Assert.IsTrue(screen.HandleEscape(), "Esc должен закрыть диалог");
            yield return null;

            Assert.IsFalse(DialogOpen(), "диалог должен закрыться");
            Assert.AreEqual(before, Count(player.Inventory, "ammo_9x18"), "лут не перенесён");
        }

        /// <summary>Пустой лут не открывает диалог.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Loot_Dialog_Empty_StaysClosed()
        {
            yield return StartGame();
            var screen = Object.FindAnyObjectByType<GameScreen>();

            screen.ShowLoot(System.Array.Empty<ItemInstance>());
            yield return null;

            Assert.IsFalse(DialogOpen(), "пустой лут не открывает диалог");
        }

        private static bool DialogOpen()
        {
            var dialog = GameObject.Find("LootDialog");
            return dialog != null && dialog.activeSelf;
        }

        private static int Count(GridContainer container, string itemId)
        {
            var total = 0;
            foreach (var item in container.Items)
            {
                if (string.Equals(item.ItemId, itemId, System.StringComparison.Ordinal))
                {
                    total += item.Stack;
                }
            }

            return total;
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
