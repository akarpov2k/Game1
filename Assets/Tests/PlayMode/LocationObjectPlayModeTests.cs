#nullable enable
using System.Collections;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.Spawns;
using AnomalyPath.Core.World;
using AnomalyPath.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AnomalyPath.Tests.PlayMode
{
    /// <summary>Объект локации на вылазке: показ с названием из каталога, лут в сетке, хаб без объекта и объект после загрузки.</summary>
    public sealed class LocationObjectPlayModeTests
    {
        private const string SpawnId = "spawn_road_crate";
        private const string TitleKey = "spawn.spawn_road_crate.title";

        private static readonly string[] LootTables =
        {
            "loot_human",
            "loot_common",
            "loot_stash_0",
            "loot_stash_1",
            "loot_stash_2",
        };

        /// <summary>Объект на вылазке виден и носит название из каталога строк.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Object_OnExpedition_ShownWithCatalogTitle()
        {
            yield return StartGame();
            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден");
            var location = screen!.Session!.State.World.CurrentLocation;
            location.SetObject(new LootLocationObject(SpawnId, TitleKey, "loot_human"));

            screen.ShowGame(false);
            yield return null;

            var locationObject = FindAnywhere("LocationObject");
            Assert.IsNotNull(locationObject, "Объект локации не создан");
            Assert.IsTrue(locationObject!.activeSelf, "Объект лута должен быть виден на вылазке");
            var title = locationObject.GetComponentInChildren<Text>();
            Assert.IsNotNull(title, "У объекта нет названия");
            Assert.AreEqual(
                screen.Texts.Get(TitleKey),
                title!.text,
                "Название объекта берётся из каталога строк по titleKey");
        }

        /// <summary>Клик по объекту открывает лут в сетке ровно тем составом, который дал бросок.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Object_Loot_Click_ShowsRolledLootInGrid()
        {
            yield return StartGame();
            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден");
            var location = screen!.Session!.State.World.CurrentLocation;
            var rolled = RollTableWithLoot(screen.Loot, screen.Session!.State.World, location, out var tableId);
            Assert.IsNotNull(tableId, "Ни одна таблица лута не дала предметы на этом seed");

            location.SetObject(new LootLocationObject(SpawnId, TitleKey, tableId!));
            screen.ShowGame(false);
            yield return null;
            Click("LocationObject");
            yield return null;

            var dialog = GameObject.Find("LootDialog");
            Assert.IsNotNull(dialog, "Диалог лута не открылся");
            Assert.IsTrue(dialog!.activeSelf, "Диалог лута скрыт");
            Assert.AreEqual(rolled!.Count, CountLootCells(), "В сетке не тот лут, что дал бросок");

            Click("Забрать всё");
            yield return null;

            Assert.IsFalse(dialog.activeSelf, "После «Забрать всё» диалог закрывается");
            Assert.IsTrue(location.Object!.Looted, "Забранный объект отмечен забранным");
            var after = FindAnywhere("LocationObject");
            Assert.IsFalse(after != null && after.activeSelf, "Забранный объект скрывается");
        }

        /// <summary>В хабе объекта локации нет, тайник там тоже не проверяется.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Object_Hub_NotShown()
        {
            yield return StartGame();
            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден");
            var world = screen!.Session!.State.World;
            var hub = FindHub(world);
            Assert.IsNotNull(hub, "На карте нет хаба");

            world.MoveTo(hub!.InstanceId);
            screen.ShowGame(false);
            yield return null;

            var locationObject = FindAnywhere("LocationObject");
            Assert.IsFalse(locationObject != null && locationObject.activeSelf, "У хаба объекта быть не должно");
            Assert.IsNull(hub.Object, "Хаб не получает объект спавна");
            Assert.IsFalse(hub.StashPresent, "В хабе тайник не проверяется");
        }

        /// <summary>Забранный объект остаётся забранным после сохранения и загрузки.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator Object_Looted_SurvivesSaveAndLoad()
        {
            yield return StartGame();
            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден");
            var location = screen!.Session!.State.World.CurrentLocation;
            var rolled = RollTableWithLoot(screen.Loot, screen.Session!.State.World, location, out var tableId);
            Assert.IsNotNull(tableId, "Ни одна таблица лута не дала предметы на этом seed");

            location.SetObject(new LootLocationObject(SpawnId, TitleKey, tableId!));
            screen.ShowGame(false);
            yield return null;
            Click("LocationObject");
            yield return null;
            Click("Забрать всё");
            yield return null;
            Assert.IsTrue(location.Object!.Looted, "Объект не отмечен забранным");

            var slot = FreeSlot(screen);
            screen.Saves.Delete(slot);
            screen.Sessions.Save(screen.Session, slot, title: "Объект");
            var loaded = screen.Sessions.Load(slot);
            screen.Session = loaded;
            screen.ShowGame(false);
            yield return null;

            var restored = loaded.State.World.Map.GetLocation(location.InstanceId);
            Assert.IsNotNull(restored, "Локация не восстановилась из сейва");
            Assert.IsNotNull(restored!.Object, "Объект не восстановился из сейва");
            Assert.IsTrue(restored.Object!.Looted, "Забранный объект должен остаться забранным");
            var locationObject = FindAnywhere("LocationObject");
            Assert.IsFalse(
                locationObject != null && locationObject.activeSelf,
                "Забранный объект не показывается после загрузки");

            screen.Saves.Delete(slot);
        }

        /// <summary>
        /// Повторяет бросок объекта так, как его делает игровой экран: seed из seed мира и экземпляра
        /// локации, затем выбирает таблицу, дающую непустой лут, чтобы сетка была проверяемой.
        /// </summary>
        /// <param name="loot">Сервис лута.</param>
        /// <param name="world">Состояние мира: seed карты и текущая локация.</param>
        /// <param name="location">Локация, на которой лежит объект.</param>
        /// <param name="tableId">Таблица, давшая непустой лут; <c>null</c> — не нашлась.</param>
        /// <returns>Ожидаемый состав лута или <c>null</c>, если все таблицы пусты.</returns>
        private static IReadOnlyList<ItemInstance>? RollTableWithLoot(
            ILootService loot,
            WorldState world,
            LocationInstance location,
            out string? tableId)
        {
            var seed = StableHash.Combine(world.Map.Seed, location.InstanceId + "#object_loot");
            foreach (var candidate in LootTables)
            {
                var rolled = loot.RollTable(candidate, location.Tier, seed, false);
                if (rolled.Count > 0)
                {
                    tableId = candidate;
                    return rolled;
                }
            }

            tableId = null;
            return null;
        }

        private static LocationInstance? FindHub(WorldState world)
        {
            foreach (var location in world.Map.Locations)
            {
                if (location.Subtype == LocationSubtype.Hub)
                {
                    return location;
                }
            }

            return null;
        }

        private static int CountLootCells()
        {
            var grid = GameObject.Find("LootGrid");
            if (grid == null)
            {
                return 0;
            }

            var total = 0;
            foreach (Transform child in grid.transform)
            {
                if (child.name.StartsWith("Loot_"))
                {
                    total++;
                }
            }

            return total;
        }

        /// <summary>Ищет свободный слот сохранения вдали от пользовательских, чтобы не затирать их.</summary>
        /// <param name="screen">Игровой экран с сервисом сохранений.</param>
        /// <returns>Имя свободного слота.</returns>
        private static string FreeSlot(GameScreen screen)
        {
            for (var number = 40; number <= 60; number++)
            {
                var candidate = SaveSlots.Slot(number);
                if (!screen.Saves.Exists(candidate))
                {
                    return candidate;
                }
            }

            Assert.Fail("Нет свободного слота сохранения для теста");
            return string.Empty;
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
