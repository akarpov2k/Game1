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
    /// <summary>Часть класса <see cref="CharacterActionsTests"/>: артефакты.</summary>
    public sealed partial class CharacterActionsTests
    {
        private void RunArtifacts()
        {
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
        }
    }
}
