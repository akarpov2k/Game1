#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Character;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Templates;
using EquipmentState = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Tests
{
    public sealed class CharacterActionsTests : TestBase
    {
        private const string ArmorJson = "{\"kind\":\"items\",\"id\":\"armor_test\",\"nameKey\":\"n\",\"category\":\"armor\",\"footprint\":[2,2],\"maxStack\":1,\"tier\":1,\"icon\":\"i\",\"artifactSlots\":1,\"material\":\"light\"}";
        private const string BeltJson = "{\"kind\":\"items\",\"id\":\"belt_test\",\"nameKey\":\"n\",\"category\":\"belt\",\"footprint\":[2,1],\"maxStack\":1,\"tier\":1,\"icon\":\"i\",\"artifactSlots\":2}";
        private const string ArtifactJson = "{\"kind\":\"items\",\"id\":\"art_test\",\"nameKey\":\"n\",\"category\":\"artifact\",\"footprint\":[1,1],\"maxStack\":1,\"tier\":1,\"icon\":\"i\",\"artifactType\":\"fire\"}";
        private const string HelmetJson = "{\"kind\":\"items\",\"id\":\"helm_test\",\"nameKey\":\"n\",\"category\":\"helmet\",\"footprint\":[1,1],\"maxStack\":1,\"tier\":1,\"icon\":\"i\",\"material\":\"light\"}";
        private const string DualWeaponJson = "{\"kind\":\"items\",\"id\":\"dual_test\",\"nameKey\":\"n\",\"category\":\"weapon\",\"footprint\":[2,2],\"maxStack\":1,\"tier\":1,\"icon\":\"i\",\"equipSlots\":[\"main\",\"secondary\"],\"weapon\":{\"classId\":\"w\",\"damage\":1,\"accuracy\":0.5,\"magazine\":5,\"ammoCaliber\":\"c\",\"reloadTime\":1,\"durabilityMax\":10,\"durabilityPerShot\":0.1}}";

        public override void Run()
        {
            Check("действия: предмет надевается из инвентаря", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var rifle = Add(inventory, grid, TestData.Rifle);
                var state = NewEquipment();

                var result = actions.Equip(grid, state, rifle.InstanceId, EquipmentSlot.Main);

                Equal(CharacterActionResult.Ok, result, "итог");
                True(state.Get(EquipmentSlot.Main) != null, "предмет в слоте");
                Equal(0, grid.Items.Count, "инвентарь пуст");
            });

            Check("действия: чужой слот — ошибка, состояние не меняется", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var rifle = Add(inventory, grid, TestData.Rifle);
                var state = NewEquipment();

                var result = actions.Equip(grid, state, rifle.InstanceId, EquipmentSlot.Helmet);

                Equal(CharacterActionResult.WrongSlot, result, "итог");
                Equal(1, grid.Items.Count, "предмет остался в инвентаре");
                True(state.Get(EquipmentSlot.Helmet) == null, "слот пуст");
            });

            Check("действия: занятый слот — прежний предмет возвращается в инвентарь", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var first = Add(inventory, grid, TestData.Rifle);
                var state = NewEquipment();
                Equal(CharacterActionResult.Ok, actions.Equip(grid, state, first.InstanceId, EquipmentSlot.Main), "первое надевание");
                var second = Add(inventory, grid, TestData.Rifle);

                var result = actions.Equip(grid, state, second.InstanceId, EquipmentSlot.Main);

                Equal(CharacterActionResult.Ok, result, "итог");
                True(state.Get(EquipmentSlot.Main) != null, "слот занят новым предметом");
                Equal(1, grid.Items.Count, "прежний предмет вернулся");
                Equal(TestData.Rifle, grid.Items[0].ItemId, "вернулся тот же вид предмета");
            });

            Check("действия: нет места под прежний предмет — ошибка", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 1, 1);
                var helmet = Add(inventory, grid, "helm_test");
                var state = NewEquipment();
                state.Set(EquipmentSlot.Helmet, new ItemInstance("prev", "armor_test", 1));

                var result = actions.Equip(grid, state, helmet.InstanceId, EquipmentSlot.Helmet);

                Equal(CharacterActionResult.NoRoom, result, "итог");
                Equal(1, grid.Items.Count, "предмет остался в инвентаре");
                Equal("prev", state.Get(EquipmentSlot.Helmet)?.InstanceId, "слот не изменился");
            });

            Check("действия: снятие возвращает предмет в инвентарь", () =>
            {
                var (actions, _, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var state = NewEquipment();
                state.Set(EquipmentSlot.Main, new ItemInstance("rifle", TestData.Rifle, 1));

                var result = actions.Unequip(grid, state, EquipmentSlot.Main);

                Equal(CharacterActionResult.Ok, result, "итог");
                True(state.Get(EquipmentSlot.Main) == null, "слот пуст");
                Equal(1, grid.Items.Count, "предмет в инвентаре");
                Equal(TestData.Rifle, grid.Items[0].ItemId, "вид предмета");
            });

            Check("действия: снятие пояса освобождает слоты артефактов в инвентарь", () =>
            {
                var (actions, _, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var state = NewEquipment();
                state.Set(EquipmentSlot.Belt, new ItemInstance("belt", "belt_test", 1));
                state.SetArtifact(0, new ItemInstance("a1", "art_test", 1));
                state.SetArtifact(1, new ItemInstance("a2", "art_test", 1));

                var result = actions.Unequip(grid, state, EquipmentSlot.Belt);

                Equal(CharacterActionResult.Ok, result, "итог");
                True(state.Get(EquipmentSlot.Belt) == null, "пояс снят");
                Equal(3, grid.Items.Count, "пояс и два артефакта в инвентаре");
                True(state.Artifacts[0] == null && state.Artifacts[1] == null, "слоты артефактов свободны");
            });

            Check("действия: снятие брони оставляет артефакты в пределах пояса", () =>
            {
                var (actions, _, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var state = NewEquipment();
                state.Set(EquipmentSlot.Armor, new ItemInstance("armor", "armor_test", 1));
                state.Set(EquipmentSlot.Belt, new ItemInstance("belt", "belt_test", 1));
                state.SetArtifact(0, new ItemInstance("a1", "art_test", 1));
                state.SetArtifact(1, new ItemInstance("a2", "art_test", 1));
                state.SetArtifact(2, new ItemInstance("a3", "art_test", 1));

                var result = actions.Unequip(grid, state, EquipmentSlot.Armor);

                Equal(CharacterActionResult.Ok, result, "итог");
                True(state.Artifacts[0] != null && state.Artifacts[1] != null, "два артефакта остались");
                True(state.Artifacts[2] == null, "третий слот освобождён");
                Equal(2, grid.Items.Count, "броня и лишний артефакт в инвентаре");
            });

            Check("действия: нет места под артефакты — пояс не снимается", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 1);
                for (var index = 0; index < 6; index++)
                {
                    Add(inventory, grid, "art_test");
                }

                var state = NewEquipment();
                state.Set(EquipmentSlot.Belt, new ItemInstance("belt", "belt_test", 1));
                state.SetArtifact(0, new ItemInstance("a1", "art_test", 1));
                state.SetArtifact(1, new ItemInstance("a2", "art_test", 1));

                var result = actions.Unequip(grid, state, EquipmentSlot.Belt);

                Equal(CharacterActionResult.NoRoom, result, "итог");
                True(state.Get(EquipmentSlot.Belt) != null, "пояс остался надетым");
                True(state.Artifacts[1] != null, "артефакт остался в слоте");
            });

            Check("действия: артефакт надевается в свободный слот", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var artifact = Add(inventory, grid, "art_test");
                var state = NewEquipment();
                state.Set(EquipmentSlot.Belt, new ItemInstance("belt", "belt_test", 1));

                var result = actions.EquipArtifact(grid, state, artifact.InstanceId);

                Equal(CharacterActionResult.Ok, result, "итог");
                True(state.Artifacts[0] != null, "артефакт в слоте");
                Equal(0, grid.Items.Count, "инвентарь пуст");
            });

            Check("действия: не артефакт в слот артефакта не надевается", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var crate = Add(inventory, grid, TestData.Crate);
                var state = NewEquipment();

                var result = actions.EquipArtifact(grid, state, crate.InstanceId);

                Equal(CharacterActionResult.NotArtifact, result, "итог");
                Equal(1, grid.Items.Count, "предмет остался в инвентаре");
            });

            Check("действия: нет свободного слота артефакта — ошибка", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var third = Add(inventory, grid, "art_test");
                var state = NewEquipment();
                state.Set(EquipmentSlot.Belt, new ItemInstance("belt", "belt_test", 1));
                state.SetArtifact(0, new ItemInstance("a1", "art_test", 1));
                state.SetArtifact(1, new ItemInstance("a2", "art_test", 1));

                var result = actions.EquipArtifact(grid, state, third.InstanceId);

                Equal(CharacterActionResult.NoFreeArtifactSlot, result, "итог");
                Equal(1, grid.Items.Count, "предмет остался в инвентаре");
            });

            Check("действия: артефакт снимается в инвентарь", () =>
            {
                var (actions, _, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var state = NewEquipment();
                state.SetArtifact(0, new ItemInstance("a1", "art_test", 1));

                var result = actions.UnequipArtifact(grid, state, 0);

                Equal(CharacterActionResult.Ok, result, "итог");
                True(state.Artifacts[0] == null, "слот свободен");
                Equal(1, grid.Items.Count, "артефакт в инвентаре");
            });

            Check("действия: разрядить оружие — патроны в инвентарь", () =>
            {
                var (actions, _, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var weapon = new ItemInstance("rifle", TestData.Rifle, 1);
                weapon.EnsureMagazine(7);
                var state = NewEquipment();
                state.Set(EquipmentSlot.Main, weapon);

                var result = actions.Unload(grid, state, "rifle", out var rounds);

                Equal(CharacterActionResult.Ok, result, "итог");
                Equal(7, rounds, "патронов");
                Equal<int?>(0, weapon.Magazine, "магазин пуст");
                Equal(7, CountAmmo(grid), "патроны в инвентаре");
            });

            Check("действия: пустой магазин — разряжать нечего", () =>
            {
                var (actions, _, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var weapon = new ItemInstance("rifle", TestData.Rifle, 1);
                weapon.EnsureMagazine(0);
                var state = NewEquipment();
                state.Set(EquipmentSlot.Main, weapon);

                var result = actions.Unload(grid, state, "rifle", out var rounds);

                Equal(CharacterActionResult.NothingToUnload, result, "итог");
                Equal(0, rounds, "патронов");
            });

            Check("действия: разрядить не оружие — ошибка", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var crate = Add(inventory, grid, TestData.Crate);
                var state = NewEquipment();

                var result = actions.Unload(grid, state, crate.InstanceId, out _);

                Equal(CharacterActionResult.NotAWeapon, result, "итог");
            });

            Check("действия: нет патронов калибра — ошибка", () =>
            {
                var items = new ItemRepository(new[] { Template(TestData.RifleJson) });
                var inventory = new InventoryService(items, new SequentialInstanceIdGenerator());
                var actions = new CharacterActions(items, inventory, new EquipmentService(items, inventory));
                var grid = new GridContainer("g", 8, 8);
                var weapon = new ItemInstance("rifle", TestData.Rifle, 1);
                weapon.EnsureMagazine(3);
                var state = NewEquipment();
                state.Set(EquipmentSlot.Main, weapon);

                var result = actions.Unload(grid, state, "rifle", out var rounds);

                Equal(CharacterActionResult.AmmoTemplateMissing, result, "итог");
                Equal(0, rounds, "патронов");
                Equal<int?>(3, weapon.Magazine, "магазин не тронут");
            });

            Check("действия: разобрать удаляет предмет из инвентаря", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var crate = Add(inventory, grid, TestData.Crate);
                var state = NewEquipment();

                var result = actions.Disassemble(grid, state, crate.InstanceId);

                Equal(CharacterActionResult.Ok, result, "итог");
                Equal(0, grid.Items.Count, "инвентарь пуст");
            });

            Check("действия: выбросить удаляет надетый предмет", () =>
            {
                var (actions, _, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var state = NewEquipment();
                state.Set(EquipmentSlot.Helmet, new ItemInstance("helm", "helm_test", 1));

                var result = actions.Discard(grid, state, "helm");

                Equal(CharacterActionResult.Ok, result, "итог");
                True(state.Get(EquipmentSlot.Helmet) == null, "слот пуст");
            });

            Check("действия: отсутствующий предмет — ItemMissing", () =>
            {
                var (actions, _, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var state = NewEquipment();

                Equal(CharacterActionResult.ItemMissing, actions.Equip(grid, state, "nope", EquipmentSlot.Main), "надеть");
                Equal(CharacterActionResult.ItemMissing, actions.Unequip(grid, state, EquipmentSlot.Main), "снять");
                Equal(CharacterActionResult.ItemMissing, actions.EquipArtifact(grid, state, "nope"), "надеть артефакт");
                Equal(CharacterActionResult.ItemMissing, actions.UnequipArtifact(grid, state, 5), "снять артефакт");
                Equal(CharacterActionResult.ItemMissing, actions.Unload(grid, state, "nope", out _), "разрядить");
                Equal(CharacterActionResult.ItemMissing, actions.Discard(grid, state, "nope"), "выбросить");
            });

            Check("действия: предмет из переполнения не надевается", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 1, 1);
                Add(inventory, grid, "helm_test");
                var overflow = Add(inventory, grid, "helm_test");
                var state = NewEquipment();

                var result = actions.Equip(grid, state, overflow.InstanceId, EquipmentSlot.Helmet);

                Equal(CharacterActionResult.ItemMissing, result, "итог");
            });

            Check("действия: DefaultSlot предпочитает свободный слот", () =>
            {
                var (actions, _, _) = NewActions();
                var state = NewEquipment();
                var dual = new ItemInstance("dual", "dual_test", 1);

                Equal<EquipmentSlot?>(EquipmentSlot.Main, actions.DefaultSlot(state, dual), "оба слота свободны");
                state.Set(EquipmentSlot.Main, new ItemInstance("rifle", TestData.Rifle, 1));
                Equal<EquipmentSlot?>(EquipmentSlot.Secondary, actions.DefaultSlot(state, dual), "основной занят");
                state.Set(EquipmentSlot.Secondary, new ItemInstance("rifle2", TestData.Rifle, 1));
                Equal<EquipmentSlot?>(EquipmentSlot.Main, actions.DefaultSlot(state, dual), "оба заняты — первый");
                Equal<EquipmentSlot?>(null, actions.DefaultSlot(state, new ItemInstance("ammo", TestData.Ammo, 1)), "патроны не надеваются");
            });
        }

        private (CharacterActions Actions, InventoryService Inventory, EquipmentService Equipment) NewActions()
        {
            var items = new ItemRepository(new[]
            {
                Template(TestData.RifleJson),
                Template(TestData.AmmoJson),
                Template(TestData.CrateJson),
                Template(ArmorJson),
                Template(BeltJson),
                Template(ArtifactJson),
                Template(HelmetJson),
                Template(DualWeaponJson)
            });
            var inventory = new InventoryService(items, new SequentialInstanceIdGenerator());
            var equipment = new EquipmentService(items, inventory);
            return (new CharacterActions(items, inventory, equipment), inventory, equipment);
        }

        private static EquipmentState NewEquipment() => new EquipmentState("player", 4);

        private static ItemInstance Add(InventoryService inventory, GridContainer grid, string itemId)
        {
            inventory.TryAdd(grid, itemId, 1, out _);
            for (var index = grid.Items.Count - 1; index >= 0; index--)
            {
                if (grid.Items[index].ItemId == itemId)
                {
                    return grid.Items[index];
                }
            }

            throw new InvalidOperationException("Предмет не добавлен: " + itemId);
        }

        private static int CountAmmo(GridContainer grid)
        {
            var total = 0;
            foreach (var item in grid.Items)
            {
                if (item.ItemId == TestData.Ammo)
                {
                    total += item.Stack;
                }
            }

            return total;
        }
    }
}
