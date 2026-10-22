#nullable enable
using System;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.World;
using InventoryEquipment = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Core.Services
{
    /// <summary>Сборка новой игры: мир, игрок, выбранный набор снаряжения и настройки сложности.</summary>
    public sealed class NewGameService : INewGameService
    {
        private readonly IGameRulesRepository _rules;
        private readonly IWorldGenerator _world;
        private readonly IActorRepository _actors;
        private readonly ILoadoutRepository _loadouts;
        private readonly IItemRepository _items;
        private readonly IInventoryService _inventory;
        private readonly IEquipmentService _equipment;
        private readonly IEquipmentSettings _equipmentSettings;
        private readonly IInstanceIdGenerator _ids;

        /// <summary>Создаёт сервис новой игры.</summary>
        /// <param name="rules">Правила прохождения.</param>
        /// <param name="world">Генератор мира.</param>
        /// <param name="actors">Репозиторий актёров.</param>
        /// <param name="loadouts">Наборы снаряжения.</param>
        /// <param name="items">Репозиторий предметов.</param>
        /// <param name="inventory">Сервис инвентаря.</param>
        /// <param name="equipment">Сервис экипировки.</param>
        /// <param name="equipmentSettings">Настройки слотов экипировки.</param>
        /// <param name="ids">Генератор id экземпляров.</param>
        /// <exception cref="ArgumentNullException">Зависимость не задана.</exception>
        public NewGameService(
            IGameRulesRepository rules,
            IWorldGenerator world,
            IActorRepository actors,
            ILoadoutRepository loadouts,
            IItemRepository items,
            IInventoryService inventory,
            IEquipmentService equipment,
            IEquipmentSettings equipmentSettings,
            IInstanceIdGenerator ids)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _actors = actors ?? throw new ArgumentNullException(nameof(actors));
            _loadouts = loadouts ?? throw new ArgumentNullException(nameof(loadouts));
            _items = items ?? throw new ArgumentNullException(nameof(items));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
            _equipmentSettings = equipmentSettings ?? throw new ArgumentNullException(nameof(equipmentSettings));
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));
        }

        /// <inheritdoc />
        public GameState Create(int seed, string loadoutId, NewGameSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var rules = _rules.Require();
            var loadout = _loadouts.Get(loadoutId);
            var map = _world.Generate(seed);
            var world = WorldState.Start(map);

            var actor = _actors.Get(rules.PlayerActorId);
            var combat = actor.Combat ?? throw new DataException(actor.Id, CoreErrors.ActorWithoutCombat);
            var grid = new GridContainer("player", rules.GridWidth, rules.BaseRows);
            var equipment = new InventoryEquipment("player", _equipmentSettings.QuickSlots);
            var needs = new NeedsState(rules.NeedsMax, rules.NeedsMax, rules.NeedsMax, rules.NeedsMax);
            var player = new PlayerState(
                "player",
                combat.Level,
                0,
                combat.Health,
                combat.Ap,
                grid,
                equipment,
                new string[0],
                needs);

            foreach (var item in loadout.Items)
            {
                _inventory.TryAdd(grid, item.ItemId, item.Count, out var overflow);
                if (overflow.Count > 0)
                {
                    throw new DataException(loadout.Id, CoreErrors.LoadoutOverflow, item.ItemId);
                }
            }

            foreach (var pair in loadout.Equipment)
            {
                var template = _items.Get(pair.Value);
                var durability = template.ArmorDurabilityMax > 0
                    ? template.ArmorDurabilityMax
                    : template.Weapon?.DurabilityMax;
                var instance = new ItemInstance(_ids.Next(), pair.Value, 1, durability, durability, null);
                if (!_equipment.TryEquip(equipment, instance, pair.Key, out _))
                {
                    throw new DataException(
                        loadout.Id,
                        CoreErrors.LoadoutSlotMismatch,
                        pair.Value,
                        EquipmentSlots.Id(pair.Key));
                }
            }

            return new GameState(world, player, settings);
        }
    }
}
