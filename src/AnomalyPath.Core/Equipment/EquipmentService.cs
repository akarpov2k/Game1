#nullable enable
using System;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Inventory;
using EquipmentState = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Core.Equipment
{
    /// <summary>
    /// Экипировка: проверка слота по данным предмета, быстрый доступ, сумма слотов артефактов
    /// и строк инвентаря. Само состояние хранится в <see cref="Equipment"/>.
    /// </summary>
    public sealed class EquipmentService : IEquipmentService
    {
        private readonly IItemRepository _items;
        private readonly IInventoryService _inventory;

        /// <summary>Создаёт сервис поверх репозитория предметов и инвентаря.</summary>
        /// <param name="items">Репозиторий шаблонов.</param>
        /// <param name="inventory">Сервис инвентаря для перекладывания артефактов.</param>
        public EquipmentService(IItemRepository items, IInventoryService inventory)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        }

        /// <inheritdoc />
        public bool CanEquip(ItemInstance item, EquipmentSlot slot)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var template = _items.Get(item.ItemId);
            var slotId = EquipmentSlots.Id(slot);
            foreach (var allowed in template.EquipSlots)
            {
                if (string.Equals(allowed, slotId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public bool TryEquip(
            EquipmentState equipment,
            ItemInstance item,
            EquipmentSlot slot,
            out ItemInstance? replaced)
        {
            if (equipment == null)
            {
                throw new ArgumentNullException(nameof(equipment));
            }

            replaced = equipment.Get(slot);
            if (!CanEquip(item, slot))
            {
                return false;
            }

            equipment.Set(slot, item);
            return true;
        }

        /// <inheritdoc />
        public bool TryUnequip(EquipmentState equipment, EquipmentSlot slot, GridContainer inventory, out ItemInstance? removed)
        {
            if (equipment == null)
            {
                throw new ArgumentNullException(nameof(equipment));
            }

            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            removed = equipment.Get(slot);
            if (removed == null)
            {
                return true;
            }

            if (slot == EquipmentSlot.Belt || slot == EquipmentSlot.Armor)
            {
                var capacityAfter = ArtifactCapacityAfter(equipment, slot);
                var extra = new System.Collections.Generic.List<(int Index, ItemInstance Item)>();
                for (var index = capacityAfter; index < equipment.Artifacts.Count; index++)
                {
                    var artifact = equipment.Artifacts[index];
                    if (artifact != null)
                    {
                        extra.Add((index, artifact));
                    }
                }

                if (extra.Count > 0)
                {
                    var candidates = new System.Collections.Generic.List<ItemInstance>();
                    foreach (var entry in extra)
                    {
                        candidates.Add(entry.Item);
                    }

                    if (!_inventory.CanFit(inventory, candidates))
                    {
                        return false;
                    }

                    foreach (var entry in extra)
                    {
                        _inventory.TryAdd(inventory, entry.Item.ItemId, entry.Item.Stack, entry.Item.Durability, entry.Item.VariantKey, out _);
                        equipment.SetArtifact(entry.Index, null);
                    }
                }
            }

            equipment.Set(slot, null);
            return true;
        }

        private int ArtifactCapacityAfter(EquipmentState equipment, EquipmentSlot removing)
        {
            var other = removing == EquipmentSlot.Belt ? EquipmentSlot.Armor : EquipmentSlot.Belt;
            var item = equipment.Get(other);
            return item == null ? 0 : _items.Get(item.ItemId).ArtifactSlots;
        }

        /// <inheritdoc />
        public ItemInstance? Unequip(EquipmentState equipment, EquipmentSlot slot)
        {
            if (equipment == null)
            {
                throw new ArgumentNullException(nameof(equipment));
            }

            var item = equipment.Get(slot);
            equipment.Set(slot, null);
            return item;
        }

        /// <inheritdoc />
        public bool CanUseQuick(ItemInstance item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var template = _items.Get(item.ItemId);
            return template.Category == "weapon" || template.Category == "consumable";
        }

        /// <inheritdoc />
        public bool TryPutQuick(
            EquipmentState equipment,
            int index,
            ItemInstance item,
            out ItemInstance? replaced)
        {
            if (equipment == null)
            {
                throw new ArgumentNullException(nameof(equipment));
            }

            if (index < 0 || index >= equipment.QuickSlotCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Индекс быстрого слота вне диапазона");
            }

            replaced = equipment.GetQuick(index);
            if (!CanUseQuick(item))
            {
                return false;
            }

            equipment.SetQuick(index, item);
            return true;
        }

        /// <inheritdoc />
        public ItemInstance? TakeQuick(EquipmentState equipment, int index)
        {
            if (equipment == null)
            {
                throw new ArgumentNullException(nameof(equipment));
            }

            if (index < 0 || index >= equipment.QuickSlotCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Индекс быстрого слота вне диапазона");
            }

            var item = equipment.GetQuick(index);
            equipment.SetQuick(index, null);
            return item;
        }

        /// <inheritdoc />
        public int ArtifactCapacity(EquipmentState equipment) => TotalArtifactSlots(equipment);

        /// <inheritdoc />
        public bool TryEquipArtifact(EquipmentState equipment, ItemInstance artifact, out int index)
        {
            if (equipment == null)
            {
                throw new ArgumentNullException(nameof(equipment));
            }

            var template = _items.Get(artifact.ItemId);
            if (string.IsNullOrWhiteSpace(template.ArtifactType))
            {
                index = -1;
                return false;
            }

            var capacity = TotalArtifactSlots(equipment);
            if (capacity == 0)
            {
                index = -1;
                return false;
            }

            equipment.EnsureArtifactSlots(capacity);
            for (var slot = 0; slot < capacity; slot++)
            {
                if (equipment.Artifacts[slot] == null)
                {
                    equipment.SetArtifact(slot, artifact);
                    index = slot;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        /// <inheritdoc />
        public ItemInstance? TakeArtifact(EquipmentState equipment, int index)
        {
            if (equipment == null)
            {
                throw new ArgumentNullException(nameof(equipment));
            }

            if (index < 0 || index >= equipment.Artifacts.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Индекс артефакта вне диапазона");
            }

            var artifact = equipment.Artifacts[index];
            equipment.SetArtifact(index, null);
            return artifact;
        }

        /// <inheritdoc />
        public System.Collections.Generic.IReadOnlyList<ItemInstance> TrimArtifacts(EquipmentState equipment)
        {
            if (equipment == null)
            {
                throw new ArgumentNullException(nameof(equipment));
            }

            var capacity = TotalArtifactSlots(equipment);
            var removed = new System.Collections.Generic.List<ItemInstance>();
            for (var slot = capacity; slot < equipment.Artifacts.Count; slot++)
            {
                var artifact = equipment.Artifacts[slot];
                if (artifact != null)
                {
                    removed.Add(artifact);
                    equipment.SetArtifact(slot, null);
                }
            }

            return removed;
        }

        /// <inheritdoc />
        public int TotalArtifactSlots(EquipmentState equipment)
        {
            if (equipment == null)
            {
                throw new ArgumentNullException(nameof(equipment));
            }

            return ArtifactSlotsOf(equipment.Get(EquipmentSlot.Armor)) +
                   ArtifactSlotsOf(equipment.Get(EquipmentSlot.Belt));
        }

        /// <inheritdoc />
        /// <inheritdoc />
        public int GetProtection(EquipmentState equipment, string damageType)
        {
            if (equipment == null)
            {
                throw new ArgumentNullException(nameof(equipment));
            }

            if (string.IsNullOrEmpty(damageType))
            {
                return 0;
            }

            var total = 0;
            total += ProtectionOf(equipment.Get(EquipmentSlot.Helmet), damageType);
            total += ProtectionOf(equipment.Get(EquipmentSlot.Armor), damageType);
            return total;
        }

        private int ProtectionOf(ItemInstance? item, string damageType)
        {
            if (item == null || item.IsBroken)
            {
                return 0;
            }

            var template = _items.Get(item.ItemId);
            return template.Protection.TryGetValue(damageType, out var value) ? value : 0;
        }

        /// <inheritdoc />
        public int InventoryRows(EquipmentState equipment, int baseRows)
        {
            if (equipment == null)
            {
                throw new ArgumentNullException(nameof(equipment));
            }

            var backpack = equipment.Get(EquipmentSlot.Backpack);
            return baseRows + (backpack == null ? 0 : _items.Get(backpack.ItemId).ExtraRows);
        }

        private int ArtifactSlotsOf(ItemInstance? item) =>
            item == null ? 0 : _items.Get(item.ItemId).ArtifactSlots;
    }
}
