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
    /// <summary>Часть класса <see cref="CharacterActionsTests"/>: помощники для сборки действий, оборудования и инвентаря.</summary>
    public sealed partial class CharacterActionsTests
    {
        private (CharacterActions Actions, InventoryService Inventory, EquipmentService Equipment) NewActions()
        {
            var items = new ItemRepository(new[]
            {
                Template(TestData.RifleJson),
                Template(TestData.AmmoJson),
                Template(OtherAmmoJson),
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

        private static ItemInstance PlaceAmmo(GridContainer grid, ItemTemplate template, int x, int count)
        {
            var stack = new ItemInstance("ammo_" + x + "_" + count, TestData.Ammo, count);
            grid.Place(stack, template, x, 0, false);
            return stack;
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
