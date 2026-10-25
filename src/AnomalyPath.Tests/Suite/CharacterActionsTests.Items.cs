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
    /// <summary>Часть класса <see cref="CharacterActionsTests"/>: разборка, выброс, перемещение и отсутствующие предметы.</summary>
    public sealed partial class CharacterActionsTests
    {
        private void RunDisassemble()
        {
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
        }

        private void RunDiscard()
        {
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
        }

        private void RunMove()
        {
            Check("действия: предмет перекладывается в свободную клетку с поворотом", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var rifle = Add(inventory, grid, TestData.Rifle);

                var result = actions.Move(grid, rifle.InstanceId, 1, 2, rotated: true);

                Equal(CharacterActionResult.Ok, result, "итог");
                Equal(1, rifle.X, "X");
                Equal(2, rifle.Y, "Y");
                True(rifle.Rotated, "предмет повёрнут");
                Equal(3, rifle.PlacedWidth, "ширина после поворота");
                Equal(2, rifle.PlacedHeight, "высота после поворота");
            });

            Check("действия: перемещение на занятую клетку — NoRoom", () =>
            {
                var (actions, inventory, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                Add(inventory, grid, TestData.Crate);
                var second = Add(inventory, grid, TestData.Crate);
                var x = second.X;
                var y = second.Y;

                var result = actions.Move(grid, second.InstanceId, 0, 0, rotated: false);

                Equal(CharacterActionResult.NoRoom, result, "итог");
                Equal(x, second.X, "X не изменился");
                Equal(y, second.Y, "Y не изменился");
            });

            Check("действия: перемещение отсутствующего предмета — ItemMissing", () =>
            {
                var (actions, _, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);

                Equal(CharacterActionResult.ItemMissing, actions.Move(grid, "nope", 0, 0, false), "итог");
            });
        }

        private void RunMissingItem()
        {
            Check("действия: отсутствующий предмет — ItemMissing", () =>
            {
                var (actions, _, _) = NewActions();
                var grid = new GridContainer("g", 8, 8);
                var state = NewEquipment();

                Equal(CharacterActionResult.ItemMissing, actions.Equip(grid, state, "nope", EquipmentSlot.Main), "надеть");
                Equal(CharacterActionResult.ItemMissing, actions.Unequip(grid, state, EquipmentSlot.Main), "снять");
                Equal(CharacterActionResult.ItemMissing, actions.EquipArtifact(grid, state, "nope"), "надеть артефакт");
                Equal(CharacterActionResult.ItemMissing, actions.UnequipArtifact(grid, state, 5), "снять артефакт");
                Equal(CharacterActionResult.ItemMissing, actions.Load(grid, state, "nope", out _), "зарядить");
                Equal(CharacterActionResult.ItemMissing, actions.LoadFrom(grid, state, "nope", "nope", out _), "зарядить из стопки");
                Equal(CharacterActionResult.ItemMissing, actions.Unload(grid, state, "nope", out _), "разрядить");
                Equal(CharacterActionResult.ItemMissing, actions.Discard(grid, state, "nope"), "выбросить");
            });
        }

        private void RunOverflowEquip()
        {
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
        }
    }
}
