#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Death;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Save;
using InventoryEquipment = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Core.Services
{
    /// <summary>
    /// Последствия смерти для вещей: инвентарь теряется полностью, кроме помеченных
    /// неутрачиваемыми, а каждый надетый предмет страхуется отдельным броском.
    /// </summary>
    public sealed class DeathService : IDeathService
    {
        private readonly IItemRepository _items;
        private readonly IDeathRandom _random;

        /// <summary>Создаёт сервис смерти.</summary>
        /// <param name="items">Репозиторий предметов: нужен признак «не утрачиваемый».</param>
        /// <param name="random">Источник бросков страховки.</param>
        /// <exception cref="ArgumentNullException">Зависимость не задана.</exception>
        public DeathService(
            IItemRepository items,
            IDeathRandom random)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <inheritdoc />
        public DeathResult Apply(GameState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var permadeath = state.Settings?.Permadeath == true;
            var lost = new List<ItemInstance>();
            var kept = new List<ItemInstance>();
            var insured = new List<ItemInstance>();
            var player = state.Player;
            if (player == null)
            {
                return new DeathResult(lost, kept, insured, permadeath);
            }

            // Инвентарь теряется целиком: страховка на него не распространяется.
            foreach (var item in new List<ItemInstance>(player.Inventory.Items))
            {
                if (IsUnlosable(item))
                {
                    kept.Add(item);
                    continue;
                }

                player.Inventory.Remove(item);
                lost.Add(item);
            }

            var chance = state.Settings?.InsuranceChance ?? 0;
            ApplyToEquipment(player.Equipment, permadeath, chance, lost, kept, insured);
            return new DeathResult(lost, kept, insured, permadeath);
        }

        private void ApplyToEquipment(
            InventoryEquipment equipment,
            bool permadeath,
            double chance,
            List<ItemInstance> lost,
            List<ItemInstance> kept,
            List<ItemInstance> insured)
        {
            var slots = new List<KeyValuePair<EquipmentSlot, ItemInstance>>(equipment.Slots);
            slots.Sort((left, right) => left.Key.CompareTo(right.Key));
            foreach (var pair in slots)
            {
                if (!KeepWorn(pair.Value, permadeath, chance, lost, kept, insured))
                {
                    equipment.Set(pair.Key, null);
                }
            }

            for (var index = 0; index < equipment.QuickSlotCount; index++)
            {
                var item = equipment.GetQuick(index);
                if (item != null && !KeepWorn(item, permadeath, chance, lost, kept, insured))
                {
                    equipment.SetQuick(index, null);
                }
            }

            for (var index = 0; index < equipment.Artifacts.Count; index++)
            {
                var item = equipment.Artifacts[index];
                if (item != null && !KeepWorn(item, permadeath, chance, lost, kept, insured))
                {
                    equipment.SetArtifact(index, null);
                }
            }
        }

        private bool KeepWorn(
            ItemInstance item,
            bool permadeath,
            double chance,
            List<ItemInstance> lost,
            List<ItemInstance> kept,
            List<ItemInstance> insured)
        {
            if (IsUnlosable(item))
            {
                kept.Add(item);
                return true;
            }

            if (permadeath)
            {
                lost.Add(item);
                return false;
            }

            if (_random.NextDouble() < chance)
            {
                kept.Add(item);
                insured.Add(item);
                return true;
            }

            lost.Add(item);
            return false;
        }

        private bool IsUnlosable(ItemInstance item) =>
            _items.TryGet(item.ItemId, out var template) && template.Unlosable;
    }
}
