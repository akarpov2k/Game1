#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using EquipmentState = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Core.Character
{
    /// <summary>
    /// Действия с предметами игрока: надеть и снять предмет, надеть и снять артефакт, разрядить оружие,
    /// разобрать и выбросить. Все проверки делаются до изменения состояния, чтобы ошибка ничего не ломала.
    /// </summary>
    public sealed class CharacterActions : ICharacterActions
    {
        private readonly IItemRepository _items;
        private readonly IInventoryService _inventory;
        private readonly IEquipmentService _equipment;

        /// <summary>Создаёт действия поверх репозитория предметов, инвентаря и экипировки.</summary>
        /// <param name="items">Репозиторий шаблонов.</param>
        /// <param name="inventory">Сервис инвентаря.</param>
        /// <param name="equipment">Сервис экипировки.</param>
        /// <exception cref="ArgumentNullException">Зависимость не задана.</exception>
        public CharacterActions(IItemRepository items, IInventoryService inventory, IEquipmentService equipment)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
        }

        /// <inheritdoc />
        public CharacterActionResult Equip(GridContainer inventory, EquipmentState equipment, string instanceId, EquipmentSlot slot)
        {
            Require(inventory, equipment);
            var item = FindInGrid(inventory, instanceId);
            if (item == null)
            {
                return CharacterActionResult.ItemMissing;
            }

            if (!_equipment.CanEquip(item, slot))
            {
                return CharacterActionResult.WrongSlot;
            }

            var previous = equipment.Get(slot);
            if (previous != null)
            {
                var probe = inventory.Clone();
                var clone = probe.Find(instanceId);
                if (clone != null)
                {
                    probe.Remove(clone);
                }

                if (!_inventory.CanFit(probe, new List<ItemInstance> { previous }))
                {
                    return CharacterActionResult.NoRoom;
                }
            }

            inventory.Remove(item);
            equipment.Set(slot, item);
            if (previous != null)
            {
                _inventory.TryAdd(inventory, previous.ItemId, previous.Stack, previous.Durability, previous.VariantKey, out _);
            }

            return CharacterActionResult.Ok;
        }

        /// <inheritdoc />
        public CharacterActionResult Unequip(GridContainer inventory, EquipmentState equipment, EquipmentSlot slot)
        {
            Require(inventory, equipment);
            var item = equipment.Get(slot);
            if (item == null)
            {
                return CharacterActionResult.ItemMissing;
            }

            var moving = new List<ItemInstance> { item };
            var freed = ArtifactsFreedByRemoving(equipment, slot);
            foreach (var index in freed)
            {
                var artifact = equipment.Artifacts[index];
                if (artifact != null)
                {
                    moving.Add(artifact);
                }
            }

            var probe = inventory.Clone();
            foreach (var entry in moving)
            {
                _inventory.TryAdd(probe, entry.ItemId, entry.Stack, entry.Durability, entry.VariantKey, out var overflow);
                if (overflow.Count > 0)
                {
                    return CharacterActionResult.NoRoom;
                }
            }

            equipment.Set(slot, null);
            foreach (var index in freed)
            {
                equipment.SetArtifact(index, null);
            }

            foreach (var entry in moving)
            {
                _inventory.TryAdd(inventory, entry.ItemId, entry.Stack, entry.Durability, entry.VariantKey, out _);
            }

            return CharacterActionResult.Ok;
        }

        /// <inheritdoc />
        public CharacterActionResult EquipArtifact(GridContainer inventory, EquipmentState equipment, string instanceId)
        {
            Require(inventory, equipment);
            var item = FindInGrid(inventory, instanceId);
            if (item == null)
            {
                return CharacterActionResult.ItemMissing;
            }

            var template = _items.Get(item.ItemId);
            if (string.IsNullOrWhiteSpace(template.ArtifactType))
            {
                return CharacterActionResult.NotArtifact;
            }

            if (!_equipment.TryEquipArtifact(equipment, item, out _))
            {
                return CharacterActionResult.NoFreeArtifactSlot;
            }

            inventory.Remove(item);
            return CharacterActionResult.Ok;
        }

        /// <inheritdoc />
        public CharacterActionResult UnequipArtifact(GridContainer inventory, EquipmentState equipment, int index)
        {
            Require(inventory, equipment);
            if (index < 0 || index >= equipment.Artifacts.Count)
            {
                return CharacterActionResult.ItemMissing;
            }

            var artifact = equipment.Artifacts[index];
            if (artifact == null)
            {
                return CharacterActionResult.ItemMissing;
            }

            if (!_inventory.CanFit(inventory, new List<ItemInstance> { artifact }))
            {
                return CharacterActionResult.NoRoom;
            }

            equipment.SetArtifact(index, null);
            _inventory.TryAdd(inventory, artifact.ItemId, artifact.Stack, artifact.Durability, artifact.VariantKey, out _);
            return CharacterActionResult.Ok;
        }

        /// <inheritdoc />
        public CharacterActionResult Unload(GridContainer inventory, EquipmentState equipment, string instanceId, out int rounds)
        {
            Require(inventory, equipment);
            rounds = 0;
            var item = FindAnywhere(inventory, equipment, instanceId);
            if (item == null)
            {
                return CharacterActionResult.ItemMissing;
            }

            var template = _items.Get(item.ItemId);
            if (template.Weapon == null)
            {
                return CharacterActionResult.NotAWeapon;
            }

            var inMagazine = item.Magazine ?? 0;
            if (inMagazine <= 0)
            {
                return CharacterActionResult.NothingToUnload;
            }

            var ammoId = FindAmmoId(template.Weapon.AmmoCaliber);
            if (ammoId == null)
            {
                return CharacterActionResult.AmmoTemplateMissing;
            }

            var probe = new ItemInstance("probe", ammoId, inMagazine);
            if (!_inventory.CanFit(inventory, new List<ItemInstance> { probe }))
            {
                return CharacterActionResult.NoRoom;
            }

            _inventory.TryAdd(inventory, ammoId, inMagazine, out _);
            item.Magazine = 0;
            rounds = inMagazine;
            return CharacterActionResult.Ok;
        }

        /// <inheritdoc />
        public CharacterActionResult Disassemble(GridContainer inventory, EquipmentState equipment, string instanceId) =>
            RemoveAnywhere(inventory, equipment, instanceId);

        /// <inheritdoc />
        public CharacterActionResult Discard(GridContainer inventory, EquipmentState equipment, string instanceId) =>
            RemoveAnywhere(inventory, equipment, instanceId);

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
                if (template.Category == "ammo" && string.Equals(template.Caliber, caliber, StringComparison.Ordinal))
                {
                    return template.Id;
                }
            }

            return null;
        }

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
