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
    /// <summary>Часть класса <see cref="CharacterActionsTests"/>: экипировка, снятие и слоты по умолчанию.</summary>
    public sealed partial class CharacterActionsTests
    {
        private void RunEquip()
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

        }

        private void RunUnequip()
        {
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
        }

        private void RunDefaultSlot()
        {
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
    }
}
