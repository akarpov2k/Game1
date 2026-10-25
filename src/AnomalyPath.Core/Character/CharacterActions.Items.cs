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
    /// <summary>Часть класса <see cref="CharacterActions"/>: предметы в сетке.</summary>
    public sealed partial class CharacterActions
    {
        /// <inheritdoc />
        public CharacterActionResult Disassemble(GridContainer inventory, EquipmentState equipment, string instanceId) =>
            RemoveAnywhere(inventory, equipment, instanceId);

        /// <inheritdoc />
        public CharacterActionResult Discard(GridContainer inventory, EquipmentState equipment, string instanceId) =>
            RemoveAnywhere(inventory, equipment, instanceId);

        /// <inheritdoc />
        public CharacterActionResult Move(GridContainer inventory, string instanceId, int x, int y, bool rotated)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            var item = FindInGrid(inventory, instanceId);
            if (item == null)
            {
                return CharacterActionResult.ItemMissing;
            }

            return _inventory.TryMove(inventory, instanceId, x, y, rotated)
                ? CharacterActionResult.Ok
                : CharacterActionResult.NoRoom;
        }

        /// <inheritdoc />
        public EquipmentSlot? DefaultSlot(EquipmentState equipment, ItemInstance item)
        {
            if (equipment == null)
            {
                throw new ArgumentNullException(nameof(equipment));
            }

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var template = _items.Get(item.ItemId);
            EquipmentSlot? first = null;
            foreach (var id in template.EquipSlots)
            {
                if (!EquipmentSlots.TryParse(id, out var slot))
                {
                    continue;
                }

                first ??= slot;
                if (equipment.Get(slot) == null)
                {
                    return slot;
                }
            }

            return first;
        }

        private List<int> ArtifactsFreedByRemoving(EquipmentState equipment, EquipmentSlot slot)
        {
            var freed = new List<int>();
            if (slot != EquipmentSlot.Belt && slot != EquipmentSlot.Armor)
            {
                return freed;
            }

            var other = slot == EquipmentSlot.Belt ? EquipmentSlot.Armor : EquipmentSlot.Belt;
            var otherItem = equipment.Get(other);
            var capacity = otherItem == null ? 0 : _items.Get(otherItem.ItemId).ArtifactSlots;
            for (var index = capacity; index < equipment.Artifacts.Count; index++)
            {
                if (equipment.Artifacts[index] != null)
                {
                    freed.Add(index);
                }
            }

            return freed;
        }

        private string? FindAmmoId(string caliber)
        {
            foreach (var template in _items.All)
            {
                if (template.Category == ItemCategory.Ammo && string.Equals(template.Caliber, caliber, StringComparison.Ordinal))
                {
                    return template.Id;
                }
            }

            return null;
        }
    }
}
