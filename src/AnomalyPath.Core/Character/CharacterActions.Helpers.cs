#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Templates;
using EquipmentState = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Core.Character
{
    /// <summary>Часть класса <see cref="CharacterActions"/>: поиск и удаление.</summary>
    public sealed partial class CharacterActions
    {
        private static ItemInstance? FindInGrid(GridContainer inventory, string instanceId)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
            {
                return null;
            }

            var item = inventory.Find(instanceId);
            return item == null || item.Overflow ? null : item;
        }

        private static ItemInstance? FindAnywhere(GridContainer inventory, EquipmentState equipment, string instanceId)
        {
            var inGrid = FindInGrid(inventory, instanceId);
            if (inGrid != null)
            {
                return inGrid;
            }

            if (string.IsNullOrWhiteSpace(instanceId))
            {
                return null;
            }

            foreach (var pair in equipment.Slots)
            {
                if (pair.Value.InstanceId == instanceId)
                {
                    return pair.Value;
                }
            }

            foreach (var artifact in equipment.Artifacts)
            {
                if (artifact != null && artifact.InstanceId == instanceId)
                {
                    return artifact;
                }
            }

            return null;
        }

        private CharacterActionResult RemoveAnywhere(GridContainer inventory, EquipmentState equipment, string instanceId)
        {
            Require(inventory, equipment);
            var inGrid = FindInGrid(inventory, instanceId);
            if (inGrid != null)
            {
                inventory.Remove(inGrid);
                return CharacterActionResult.Ok;
            }

            if (string.IsNullOrWhiteSpace(instanceId))
            {
                return CharacterActionResult.ItemMissing;
            }

            EquipmentSlot? equipped = null;
            foreach (var pair in equipment.Slots)
            {
                if (pair.Value.InstanceId == instanceId)
                {
                    equipped = pair.Key;
                    break;
                }
            }

            if (equipped != null)
            {
                equipment.Set(equipped.Value, null);
                return CharacterActionResult.Ok;
            }

            for (var index = 0; index < equipment.Artifacts.Count; index++)
            {
                var artifact = equipment.Artifacts[index];
                if (artifact != null && artifact.InstanceId == instanceId)
                {
                    equipment.SetArtifact(index, null);
                    return CharacterActionResult.Ok;
                }
            }

            return CharacterActionResult.ItemMissing;
        }

        private static void Require(GridContainer inventory, EquipmentState equipment)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            if (equipment == null)
            {
                throw new ArgumentNullException(nameof(equipment));
            }
        }
    }
}
