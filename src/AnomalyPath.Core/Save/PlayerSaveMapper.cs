#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Templates;
using InventoryEquipment = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Core.Save
{
    /// <summary>Преобразование состояния игрока в DTO сохранения и обратно.</summary>
    public static class PlayerSaveMapper
    {
        /// <summary>Собирает снимок игрока.</summary>
        /// <param name="player">Состояние игрока.</param>
        /// <returns>Снимок.</returns>
        /// <exception cref="ArgumentNullException">Игрок не задан.</exception>
        public static PlayerSaveData ToSaveData(PlayerState player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            var items = new Dictionary<string, ItemSaveData>(StringComparer.Ordinal);
            foreach (var item in player.Inventory.Items)
            {
                AddItem(items, item);
            }

            foreach (var pair in player.Equipment.Slots)
            {
                AddItem(items, pair.Value);
            }

            foreach (var item in player.Equipment.QuickSlots)
            {
                AddItem(items, item);
            }

            foreach (var item in player.Equipment.Artifacts)
            {
                AddItem(items, item);
            }

            var orderedItems = new List<ItemSaveData>(items.Values);
            orderedItems.Sort((left, right) => string.CompareOrdinal(left.InstanceId, right.InstanceId));

            var placements = new List<PlacedItemSaveData>();
            foreach (var item in player.Inventory.Items)
            {
                placements.Add(new PlacedItemSaveData(item.InstanceId, item.X, item.Y, item.Rotated, item.Overflow));
            }

            var slots = new List<EquipmentSlotSaveData>();
            var slotPairs = new List<KeyValuePair<EquipmentSlot, ItemInstance>>(player.Equipment.Slots);
            slotPairs.Sort((left, right) => string.CompareOrdinal(left.Key.ToString(), right.Key.ToString()));
            foreach (var pair in slotPairs)
            {
                slots.Add(new EquipmentSlotSaveData(pair.Key.ToString().ToLowerInvariant(), pair.Value.InstanceId));
            }

            var quick = new List<string?>();
            foreach (var item in player.Equipment.QuickSlots)
            {
                quick.Add(item?.InstanceId);
            }

            var artifacts = new List<string?>();
            foreach (var item in player.Equipment.Artifacts)
            {
                artifacts.Add(item?.InstanceId);
            }

            return new PlayerSaveData(
                player.Id,
                player.Name,
                player.Level,
                player.Xp,
                player.Health,
                player.ApMax,
                orderedItems,
                new InventorySaveData(player.Inventory.Id, player.Inventory.Width, player.Inventory.Rows, placements),
                new EquipmentSaveData(player.Equipment.OwnerId, slots, quick, artifacts),
                player.Skills,
                new NeedsSaveData(player.Needs.Hunger, player.Needs.Thirst, player.Needs.Sleep, player.Needs.Max),
                player.Money,
                player.CompletedQuests,
                player.Unlocks,
                player.Reputation,
                player.TalentPoints,
                player.TalentRanks);
        }

        /// <summary>Восстанавливает игрока из снимка.</summary>
        /// <param name="data">Снимок игрока.</param>
        /// <param name="items">Репозиторий шаблонов предметов.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Состояние игрока.</returns>
        /// <exception cref="DataException">Снимок ссылается на отсутствующие предметы или слоты.</exception>
        public static PlayerState FromSaveData(PlayerSaveData data, IItemRepository items, string sourcePath)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var instances = new Dictionary<string, ItemInstance>(StringComparer.Ordinal);
            foreach (var item in data.Items)
            {
                if (instances.ContainsKey(item.InstanceId))
                {
                    throw new DataException(sourcePath, CoreErrors.SaveMissingField, "item");
                }

                var instance = new ItemInstance(
                    item.InstanceId,
                    item.ItemId,
                    item.Stack,
                    item.Durability,
                    item.DurabilityMax,
                    item.VariantKey);
                if (item.Magazine.HasValue)
                {
                    instance.Magazine = item.Magazine;
                }
                else if (items.TryGet(item.ItemId, out var template) && template.Weapon != null)
                {
                    // Старые сейвы не хранили магазин: оружие в них считается разряженным.
                    instance.Magazine = 0;
                }

                instances[item.InstanceId] = instance;
            }

            var inventory = new GridContainer(data.Inventory.Id, data.Inventory.Width, data.Inventory.Rows);
            foreach (var placement in data.Inventory.Placements)
            {
                if (!instances.TryGetValue(placement.InstanceId, out var item))
                {
                    throw new DataException(sourcePath, CoreErrors.SaveMissingField, "placement");
                }

                if (placement.Overflow)
                {
                    inventory.AddOverflow(item);
                    continue;
                }

                if (items.TryGet(item.ItemId, out var template) &&
                    TryPlace(inventory, item, template, placement))
                {
                    continue;
                }

                inventory.AddOverflow(item);
            }

            var equipment = new InventoryEquipment(data.Equipment.OwnerId, data.Equipment.QuickSlots.Count);
            foreach (var slot in data.Equipment.Slots)
            {
                if (!Enum.TryParse<EquipmentSlot>(slot.Slot, ignoreCase: true, out var parsed) ||
                    !instances.TryGetValue(slot.InstanceId, out var item))
                {
                    throw new DataException(sourcePath, CoreErrors.SaveMissingField, "slot");
                }

                equipment.Set(parsed, item);
            }

            for (var index = 0; index < data.Equipment.QuickSlots.Count; index++)
            {
                equipment.SetQuick(index, Resolve(data.Equipment.QuickSlots[index], instances, sourcePath));
            }

            for (var index = 0; index < data.Equipment.Artifacts.Count; index++)
            {
                equipment.SetArtifact(index, Resolve(data.Equipment.Artifacts[index], instances, sourcePath));
            }

            return new PlayerState(
                data.Id,
                data.Level,
                data.Xp,
                data.Health,
                data.ApMax,
                inventory,
                equipment,
                data.Skills,
                new NeedsState(data.Needs.Hunger, data.Needs.Thirst, data.Needs.Sleep, data.Needs.Max),
                data.Name,
                data.Money,
                data.CompletedQuests,
                data.Unlocks,
                data.Reputation,
                data.TalentPoints,
                data.TalentRanks);
        }

        private static bool TryPlace(
            GridContainer grid,
            ItemInstance item,
            ItemTemplate template,
            PlacedItemSaveData placement)
        {
            try
            {
                grid.Place(item, template, placement.X, placement.Y, placement.Rotated);
                return true;
            }
            catch (DataException)
            {
                return false;
            }
        }

        private static ItemInstance? Resolve(
            string? instanceId,
            IReadOnlyDictionary<string, ItemInstance> instances,
            string sourcePath)
        {
            if (instanceId == null)
            {
                return null;
            }

            if (!instances.TryGetValue(instanceId, out var item))
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "item");
            }

            return item;
        }

        private static void AddItem(Dictionary<string, ItemSaveData> items, ItemInstance? item)
        {
            if (item == null || items.ContainsKey(item.InstanceId))
            {
                return;
            }

            items[item.InstanceId] = new ItemSaveData(
                item.InstanceId,
                item.ItemId,
                item.Stack,
                item.Durability,
                item.DurabilityMax,
                item.VariantKey,
                item.Magazine);
        }
    }
}
