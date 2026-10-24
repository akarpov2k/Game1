#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Loadouts;
using InventoryEquipment = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Core.Services
{
    /// <summary>
    /// Собирает набор снаряжения так же, как это делает новая игра, но без побочных эффектов:
    /// результат нужен экрану выбора набора, чтобы показать предметы до старта.
    /// </summary>
    public sealed class LoadoutPreviewService : ILoadoutPreviewService
    {
        private const string PlayerContainerId = "player";
        private const string DisplayContainerId = "loadout_preview";

        private readonly IGameRulesRepository _rules;
        private readonly ILoadoutRepository _loadouts;
        private readonly IItemRepository _items;
        private readonly IInventoryService _inventory;
        private readonly IEquipmentService _equipment;
        private readonly IEquipmentSettings _equipmentSettings;
        private readonly IInstanceIdGenerator _ids;

        /// <summary>Создаёт сервис раскладки наборов.</summary>
        /// <param name="rules">Правила прохождения: ширина сетки и базовые строки.</param>
        /// <param name="loadouts">Наборы снаряжения.</param>
        /// <param name="items">Репозиторий предметов.</param>
        /// <param name="inventory">Сервис инвентаря.</param>
        /// <param name="equipment">Сервис экипировки.</param>
        /// <param name="equipmentSettings">Настройки слотов экипировки.</param>
        /// <param name="ids">Генератор id экземпляров.</param>
        /// <exception cref="ArgumentNullException">Зависимость не задана.</exception>
        public LoadoutPreviewService(
            IGameRulesRepository rules,
            ILoadoutRepository loadouts,
            IItemRepository items,
            IInventoryService inventory,
            IEquipmentService equipment,
            IEquipmentSettings equipmentSettings,
            IInstanceIdGenerator ids)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _loadouts = loadouts ?? throw new ArgumentNullException(nameof(loadouts));
            _items = items ?? throw new ArgumentNullException(nameof(items));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
            _equipmentSettings = equipmentSettings ?? throw new ArgumentNullException(nameof(equipmentSettings));
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));
        }

        /// <inheritdoc />
        public LoadoutPreview Build(string loadoutId)
        {
            var loadout = _loadouts.Get(loadoutId);
            var rules = _rules.Require();
            var grid = new GridContainer(PlayerContainerId, rules.GridWidth, rules.BaseRows);
            var equipment = new InventoryEquipment(PlayerContainerId, _equipmentSettings.QuickSlots);

            var rejected = new List<KeyValuePair<EquipmentSlot, string>>();
            foreach (var pair in loadout.Equipment)
            {
                var template = _items.Get(pair.Value);
                var durability = template.ArmorDurabilityMax > 0
                    ? template.ArmorDurabilityMax
                    : template.Weapon?.DurabilityMax;
                // Стартовое оружие выдаётся заряженным; запас торговца создаётся InventoryService и остаётся разряженным.
                var instance = new ItemInstance(
                    _ids.Next(),
                    pair.Value,
                    1,
                    durability,
                    durability,
                    null,
                    template.Weapon?.Magazine);
                if (!_equipment.TryEquip(equipment, instance, pair.Key, out _))
                {
                    rejected.Add(new KeyValuePair<EquipmentSlot, string>(pair.Key, pair.Value));
                }
            }

            var rows = _equipment.InventoryRows(equipment, rules.BaseRows);
            if (rows != grid.Rows)
            {
                grid.Resize(rows);
            }

            var overflow = new List<ItemInstance>();
            foreach (var item in loadout.Items)
            {
                _inventory.TryAdd(grid, item.ItemId, item.Count, out var rest);
                if (rest.Count > 0)
                {
                    overflow.AddRange(rest);
                }
            }

            var display = BuildDisplay(loadout, grid.Width, grid.Rows, rules.PreviewMaxRows, out var displayOverflow);
            return new LoadoutPreview(loadout, grid, equipment, overflow, rejected, display, displayOverflow);
        }

        /// <summary>
        /// Собирает сетку показа без ограничения по числу ячеек: если надетые предметы и расходники
        /// не помещаются, добавляются ряды, пока всё не уляжется.
        /// </summary>
        private GridContainer BuildDisplay(
            LoadoutTemplate loadout,
            int width,
            int startRows,
            int maxRows,
            out List<ItemInstance> overflow)
        {
            var rows = Math.Max(1, startRows);
            for (var attempt = 0; attempt < maxRows; attempt++)
            {
                var display = new GridContainer(DisplayContainerId, width, rows);
                var rest = new List<ItemInstance>();
                foreach (var pair in loadout.Equipment)
                {
                    var template = _items.Get(pair.Value);
                    var durability = template.ArmorDurabilityMax > 0
                        ? template.ArmorDurabilityMax
                        : template.Weapon?.DurabilityMax;
                    _inventory.TryAdd(display, pair.Value, 1, durability, null, out var equipmentRest);
                    rest.AddRange(equipmentRest);
                }

                foreach (var item in loadout.Items)
                {
                    _inventory.TryAdd(display, item.ItemId, item.Count, out var itemsRest);
                    rest.AddRange(itemsRest);
                }

                if (rest.Count == 0 || rows >= maxRows)
                {
                    overflow = rest;
                    return display;
                }

                rows++;
            }

            overflow = new List<ItemInstance>();
            return new GridContainer(DisplayContainerId, width, Math.Max(1, startRows));
        }
    }
}
