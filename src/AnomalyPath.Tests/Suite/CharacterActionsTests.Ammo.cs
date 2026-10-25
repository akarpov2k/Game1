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
    /// <summary>Часть класса <see cref="CharacterActionsTests"/>: перезарядка и патроны.</summary>
    public sealed partial class CharacterActionsTests
    {
        private void RunUnload()
        {
            Check("действия: разрядить оружие — патроны в инвентарь", () =>
            {
                var (actions, _, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var weapon = new ItemInstance("rifle", TestData.Rifle, 1, magazine: 7);
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
                var weapon = new ItemInstance("rifle", TestData.Rifle, 1, magazine: 0);
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
                var weapon = new ItemInstance("rifle", TestData.Rifle, 1, magazine: 3);
                var state = NewEquipment();
                state.Set(EquipmentSlot.Main, weapon);

                var result = actions.Unload(grid, state, "rifle", out var rounds);

                Equal(CharacterActionResult.AmmoTemplateMissing, result, "итог");
                Equal(0, rounds, "патронов");
                Equal<int?>(3, weapon.Magazine, "магазин не тронут");
            });
        }

        private void RunLoad()
        {
            Check("действия: зарядка досылает патроны до полного магазина", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                inventory.TryAdd(grid, TestData.Ammo, 20, out _);
                var weapon = new ItemInstance("rifle", TestData.Rifle, 1, magazine: 7);
                var state = NewEquipment();
                state.Set(EquipmentSlot.Main, weapon);

                var result = actions.Load(grid, state, "rifle", out var rounds);

                Equal(CharacterActionResult.Ok, result, "итог");
                Equal(3, rounds, "дослано");
                Equal(10, weapon.MagazineRounds, "магазин полон");
                Equal(17, CountAmmo(grid), "патроны в инвентаре");
            });

            Check("действия: полный магазин не заряжается", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var ammo = Add(inventory, grid, TestData.Ammo);
                var weapon = new ItemInstance("rifle", TestData.Rifle, 1, magazine: 10);
                var state = NewEquipment();
                state.Set(EquipmentSlot.Main, weapon);

                var result = actions.Load(grid, state, "rifle", out var rounds);

                Equal(CharacterActionResult.MagazineFull, result, "итог");
                Equal(0, rounds, "дослано");
                Equal(10, weapon.MagazineRounds, "магазин не изменился");
                Equal(1, grid.Items.Count, "патроны не тронуты");
                True(ReferenceEquals(ammo, grid.Items[0]), "та же стопка");
            });

            Check("действия: зарядка без патронов — NoAmmo", () =>
            {
                var (actions, _, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var weapon = new ItemInstance("rifle", TestData.Rifle, 1, magazine: 3);
                var state = NewEquipment();
                state.Set(EquipmentSlot.Main, weapon);

                var result = actions.Load(grid, state, "rifle", out var rounds);

                Equal(CharacterActionResult.NoAmmo, result, "итог");
                Equal(0, rounds, "дослано");
                Equal(3, weapon.MagazineRounds, "магазин не изменился");
            });

            Check("действия: зарядка не оружия — NotAWeapon", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var crate = Add(inventory, grid, TestData.Crate);
                var state = NewEquipment();

                var result = actions.Load(grid, state, crate.InstanceId, out _);

                Equal(CharacterActionResult.NotAWeapon, result, "итог");
            });

            Check("действия: зарядка оружия в инвентаре", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var weapon = Add(inventory, grid, TestData.Rifle);
                Equal(0, weapon.MagazineRounds, "новое оружие разряжено");
                inventory.TryAdd(grid, TestData.Ammo, 20, out _);

                var result = actions.Load(grid, NewEquipment(), weapon.InstanceId, out var rounds);

                Equal(CharacterActionResult.Ok, result, "итог");
                Equal(10, rounds, "дослано");
                Equal(10, weapon.MagazineRounds, "магазин полон");
                Equal(10, CountAmmo(grid), "остаток патронов");
            });
        }

        private void RunLoadFrom()
        {
            Check("действия: зарядка берёт патроны из самых маленьких стопок", () =>
            {
                var (actions, _, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var ammoTemplate = Template(TestData.AmmoJson);
                var larger = PlaceAmmo(grid, ammoTemplate, 0, 5);
                var smaller = PlaceAmmo(grid, ammoTemplate, 1, 2);
                var weapon = new ItemInstance("rifle", TestData.Rifle, 1, magazine: 7);
                var state = NewEquipment();
                state.Set(EquipmentSlot.Main, weapon);

                var result = actions.Load(grid, state, "rifle", out var rounds);

                Equal(CharacterActionResult.Ok, result, "итог");
                Equal(3, rounds, "дослано");
                Equal(4, larger.Stack, "большая стопка уменьшилась на остаток");
                True(grid.Find(smaller.InstanceId) == null, "маленькая стопка израсходована");
                Equal(4, CountAmmo(grid), "патроны в инвентаре");
            });

            Check("действия: зарядка из конкретной стопки не трогает другие", () =>
            {
                var (actions, _, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var ammoTemplate = Template(TestData.AmmoJson);
                var other = PlaceAmmo(grid, ammoTemplate, 0, 4);
                var chosen = PlaceAmmo(grid, ammoTemplate, 1, 20);
                var weapon = new ItemInstance("rifle", TestData.Rifle, 1, magazine: 5);
                var state = NewEquipment();
                state.Set(EquipmentSlot.Main, weapon);

                var result = actions.LoadFrom(grid, state, "rifle", chosen.InstanceId, out var rounds);

                Equal(CharacterActionResult.Ok, result, "итог");
                Equal(5, rounds, "дослано");
                Equal(10, weapon.MagazineRounds, "магазин полон");
                Equal(15, chosen.Stack, "выбранная стопка");
                Equal(4, other.Stack, "другая стопка не тронута");
            });

            Check("действия: зарядка из стопки другого калибра — ошибка", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var ammo = Add(inventory, grid, "ammo_other");
                var weapon = new ItemInstance("rifle", TestData.Rifle, 1, magazine: 2);
                var state = NewEquipment();
                state.Set(EquipmentSlot.Main, weapon);

                var result = actions.LoadFrom(grid, state, "rifle", ammo.InstanceId, out var rounds);

                Equal(CharacterActionResult.AmmoCaliberMismatch, result, "итог");
                Equal(0, rounds, "дослано");
                Equal(2, weapon.MagazineRounds, "магазин не изменился");
                Equal(1, grid.Items.Count, "патроны не тронуты");
            });

            Check("действия: зарядка из не-патронов — NotAmmo", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var crate = Add(inventory, grid, TestData.Crate);
                var weapon = new ItemInstance("rifle", TestData.Rifle, 1, magazine: 2);
                var state = NewEquipment();
                state.Set(EquipmentSlot.Main, weapon);

                var result = actions.LoadFrom(grid, state, "rifle", crate.InstanceId, out _);

                Equal(CharacterActionResult.NotAmmo, result, "итог");
            });

            Check("действия: зарядка из стопки в полный магазин — MagazineFull", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var ammo = Add(inventory, grid, TestData.Ammo);
                var weapon = new ItemInstance("rifle", TestData.Rifle, 1, magazine: 10);
                var state = NewEquipment();
                state.Set(EquipmentSlot.Main, weapon);

                var result = actions.LoadFrom(grid, state, "rifle", ammo.InstanceId, out var rounds);

                Equal(CharacterActionResult.MagazineFull, result, "итог");
                Equal(0, rounds, "дослано");
                Equal(1, grid.Items.Count, "патроны не тронуты");
            });

            Check("действия: зарядка из стопки не оружия — NotAWeapon", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var ammo = Add(inventory, grid, TestData.Ammo);
                var crate = Add(inventory, grid, TestData.Crate);
                var state = NewEquipment();

                var result = actions.LoadFrom(grid, state, crate.InstanceId, ammo.InstanceId, out _);

                Equal(CharacterActionResult.NotAWeapon, result, "итог");
            });
        }

        private void RunMagazinePreservation()
        {
            Check("действия: снятие и надевание сохраняют патроны в магазине", () =>
            {
                var (actions, _, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var state = NewEquipment();
                state.Set(EquipmentSlot.Main, new ItemInstance("rifle", TestData.Rifle, 1, magazine: 6));

                Equal(CharacterActionResult.Ok, actions.Unequip(grid, state, EquipmentSlot.Main), "снятие");
                var returned = grid.Items[0];
                Equal(6, returned.MagazineRounds, "патроны сохранились");

                Equal(CharacterActionResult.Ok, actions.Equip(grid, state, returned.InstanceId, EquipmentSlot.Main), "надевание");
                Equal(6, state.Get(EquipmentSlot.Main)!.MagazineRounds, "патроны на месте");
            });

            Check("действия: замена оружия возвращает прежнее с патронами", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var replacement = Add(inventory, grid, TestData.Rifle);
                var state = NewEquipment();
                state.Set(EquipmentSlot.Main, new ItemInstance("old", TestData.Rifle, 1, magazine: 4));

                var result = actions.Equip(grid, state, replacement.InstanceId, EquipmentSlot.Main);

                Equal(CharacterActionResult.Ok, result, "итог");
                Equal(1, grid.Items.Count, "прежнее оружие в инвентаре");
                Equal(4, grid.Items[0].MagazineRounds, "патроны прежнего оружия");
            });
        }
    }
}
