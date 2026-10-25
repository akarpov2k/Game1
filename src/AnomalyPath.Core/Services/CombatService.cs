#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Talents;
using AnomalyPath.Core.Templates;
using EquipmentState = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Core.Services
{
    /// <summary>
    /// Боевой расчёт: урон по формуле из данных, защита, износ оружия и брони, ходы и дистанция.
    /// </summary>
    public sealed partial class CombatService : ICombatService
    {
        private readonly IItemRepository _items;
        private readonly IDamageTypeRepository _damageTypes;
        private readonly IEquipmentService _equipment;
        private readonly IArmorWearService _wear;
        private readonly IFormulaRepository _formulas;
        private readonly CombatSettingsTemplate _settings;
        private readonly BalanceTemplate _balance;
        private readonly ICombatRandom _random;
        private readonly IInventoryService _inventory;

        /// <summary>Создаёт боевой сервис.</summary>
        /// <param name="items">Репозиторий предметов.</param>
        /// <param name="damageTypes">Репозиторий типов урона.</param>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="wear">Износ.</param>
        /// <param name="formulas">Формулы.</param>
        /// <param name="settings">Настройки боя.</param>
        /// <param name="balance">Константы баланса.</param>
        /// <param name="random">Источник бросков.</param>
        /// <param name="inventory">Инвентарь для патронов.</param>
        public CombatService(
            IItemRepository items,
            IDamageTypeRepository damageTypes,
            IEquipmentService equipment,
            IArmorWearService wear,
            IFormulaRepository formulas,
            CombatSettingsTemplate settings,
            BalanceTemplate balance,
            ICombatRandom random,
            IInventoryService inventory)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
            _damageTypes = damageTypes ?? throw new ArgumentNullException(nameof(damageTypes));
            _equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
            _wear = wear ?? throw new ArgumentNullException(nameof(wear));
            _formulas = formulas ?? throw new ArgumentNullException(nameof(formulas));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _balance = balance ?? throw new ArgumentNullException(nameof(balance));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        }

        /// <inheritdoc />
        public CombatState Start(
            ActorTemplate playerActor,
            EquipmentState playerEquipment,
            ActorTemplate enemy,
            DistanceZone distance,
            CombatScenario? scenario = null,
            TalentModifiers? playerModifiers = null)
        {
            if (playerActor == null)
            {
                throw new ArgumentNullException(nameof(playerActor));
            }

            if (playerEquipment == null)
            {
                throw new ArgumentNullException(nameof(playerEquipment));
            }

            if (enemy == null)
            {
                throw new ArgumentNullException(nameof(enemy));
            }

            var player = new Combatant(playerActor, playerEquipment, playerModifiers);
            var opponent = new Combatant(enemy);
            return new CombatState(player, opponent, distance, scenario ?? CombatScenario.Normal, CreateEnemyWeapon(enemy));
        }

        /// <summary>Создаёт оружие противника из данных; у естественной атаки оружия нет.</summary>
        private ItemInstance? CreateEnemyWeapon(ActorTemplate enemy)
        {
            var attack = enemy.Combat?.Attack;
            if (attack?.WeaponId is not { } weaponId)
            {
                return null;
            }

            var template = _items.Get(weaponId);
            if (template.Weapon == null)
            {
                throw new DataException(enemy.Id, CoreErrors.WeaponWithoutBlock, weaponId);
            }

            return new ItemInstance("enemy_weapon", weaponId, 1, null, null, null, template.Weapon.Magazine);
        }
    }
}
