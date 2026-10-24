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
    public sealed class CombatService : ICombatService
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
            return new CombatState(player, opponent, distance, scenario ?? CombatScenario.Normal);
        }

        /// <inheritdoc />
        public bool TryAttack(CombatState state, ItemInstance weapon, out CombatAttackResult result)
        {
            result = default;
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (state.IsFinished || weapon == null || weapon.IsBroken)
            {
                return false;
            }

            var attacker = state.Current;
            var defender = state.Opponent;
            if (_settings.AttackApCost > 0 && attacker.Ap < _settings.AttackApCost)
            {
                return false;
            }

            var template = _items.Get(weapon.ItemId);
            if (template.Weapon == null)
            {
                return false;
            }

            var magazine = weapon.MagazineRounds;
            if (magazine <= 0)
            {
                if (attacker.Template.Type != ActorType.Enemy || attacker.Ap < _settings.ReloadApCost)
                {
                    return false;
                }

                // У противника запас патронов бесконечный: пустой магазин заряжается полностью за ОД.
                weapon.Magazine = template.Weapon.Magazine;
                attacker.Ap -= _settings.ReloadApCost;
                magazine = template.Weapon.Magazine;
            }

            weapon.Magazine = magazine - 1;
            var chance = HitChance(state, weapon);
            var roll = _random.NextDouble();
            var hit = roll < chance;
            var damage = 0;
            if (hit)
            {
                var type = template.Weapon.DamageTypeId;
                if (!_damageTypes.Contains(type))
                {
                    throw new DataException(template.Id, CoreErrors.UnknownDamageType, type);
                }

                var protection = ProtectionOf(defender, type);
                var variables = new Dictionary<string, double>(StringComparer.Ordinal)
                {
                    ["damage"] = template.Weapon.Damage,
                    ["penetration"] = template.Weapon.ArmorPenetration,
                    ["protection"] = protection,
                    ["distance"] = (int)state.Distance
                };

                var formula = _formulas.Get(_settings.DamageFormulaId);
                var evaluated = formula.Evaluate(variables, _balance.Constants, _settings.ClampMin, _settings.ClampMax);
                damage = evaluated < 0 ? 0 : (int)evaluated;
            }

            if (hit)
            {
                attacker.PityBonus = 0;
            }
            else
            {
                attacker.PityBonus += BaseChance(attacker, template.Weapon) * 0.05;
            }

            attacker.Ap -= _settings.AttackApCost;
            defender.Health = Math.Max(0, defender.Health - damage);
            ApplyWear(weapon, defender, damage);
            result = new CombatAttackResult(hit, damage, defender.IsDead, roll, chance);
            return true;
        }

        /// <inheritdoc />
        public bool TryReload(CombatState state, ItemInstance weapon, GridContainer inventory)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (weapon == null)
            {
                throw new ArgumentNullException(nameof(weapon));
            }

            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            var attacker = state.Current;
            var template = _items.Get(weapon.ItemId);
            if (state.IsFinished || template.Weapon == null || attacker.Ap < _settings.ReloadApCost)
            {
                return false;
            }

            var size = template.Weapon.Magazine;
            var current = weapon.MagazineRounds;
            var need = size - current;
            if (need <= 0)
            {
                return false;
            }

            var taken = _inventory.TakeAmmo(inventory, template.Weapon.AmmoCaliber, need);
            if (taken <= 0)
            {
                return false;
            }

            weapon.Magazine = current + taken;
            attacker.Ap -= _settings.ReloadApCost;
            return true;
        }

        /// <inheritdoc />
        public double HitChance(CombatState state, ItemInstance weapon)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (weapon == null)
            {
                throw new ArgumentNullException(nameof(weapon));
            }

            var template = _items.Get(weapon.ItemId);
            if (template.Weapon == null)
            {
                return 0;
            }

            var attacker = state.Current;
            var defender = state.Opponent;
            var ratio = defender.Level > 0 ? attacker.Level / (double)defender.Level : attacker.Level;
            if (Math.Floor(ratio) > 5)
            {
                return 0.98;
            }

            var combat = attacker.Template.Combat;
            var accuracy = (combat?.Accuracy ?? 0.8) + (combat?.AccuracyBonus ?? 0) +
                           attacker.AccuracyBonus + attacker.PityBonus;
            var distance = template.Weapon.IgnoresDistance ? 1.0 : DistanceMultiplier(state.Distance);
            var chance = accuracy * template.Weapon.AccuracyCoefficient * distance;
            return Math.Clamp(chance, 0, 1);
        }

        private double BaseChance(Combatant attacker, WeaponBlock weapon)
        {
            var combat = attacker.Template.Combat;
            var accuracy = (combat?.Accuracy ?? 0.8) + (combat?.AccuracyBonus ?? 0) + attacker.AccuracyBonus;
            return Math.Clamp(accuracy * weapon.AccuracyCoefficient, 0, 1);
        }

        private double DistanceMultiplier(DistanceZone zone) => zone switch
        {
            DistanceZone.Melee => _settings.AccuracyClose,
            DistanceZone.Close => _settings.AccuracyClose,
            DistanceZone.Mid => _settings.AccuracyMid,
            _ => _settings.AccuracyFar
        };

        /// <inheritdoc />
        public bool TryMove(CombatState state, int deltaZones)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var shifts = Math.Abs(deltaZones);
            if (state.IsFinished || shifts == 0)
            {
                return false;
            }

            var free = Math.Max(0, _settings.ZoneShiftFree - state.ShiftUsed);
            var paid = Math.Max(0, shifts - free);
            var cost = paid * _settings.ZoneShiftApCost;
            var attacker = state.Current;
            if (cost > attacker.Ap)
            {
                return false;
            }

            var target = (int)state.Distance + (deltaZones > 0 ? shifts : -shifts);
            target = Math.Clamp(target, (int)DistanceZone.Melee, (int)DistanceZone.Far);
            if (target == (int)state.Distance)
            {
                return false;
            }

            attacker.Ap -= cost;
            state.ShiftUsed += shifts;
            state.Distance = (DistanceZone)target;
            return true;
        }

        /// <inheritdoc />
        public void EndTurn(CombatState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            state.Current = state.Opponent;
            state.ShiftUsed = 0;
            state.Current.Ap = RestoreAp(state.Current);
            if (ReferenceEquals(state.Current, state.Player))
            {
                state.Round++;
            }
        }

        private int ProtectionOf(Combatant defender, string damageType)
        {
            var own = defender.OwnProtection(damageType);
            if (defender.Equipment == null)
            {
                return own;
            }

            return own + _equipment.GetProtection(defender.Equipment, damageType);
        }

        private int RestoreAp(Combatant combatant) => _settings.ApRestore switch
        {
            ApRestoreKind.None => combatant.Ap,
            ApRestoreKind.Partial => Math.Min(
                combatant.MaxAp,
                combatant.Ap + _settings.ApPerTurn + combatant.ApRestoreBonus),
            _ => combatant.MaxAp
        };

        private void ApplyWear(ItemInstance weapon, Combatant defender, int damage)
        {
            if (weapon.Durability.HasValue && _items.Get(weapon.ItemId).Weapon is { } weaponBlock)
            {
                var wear = weaponBlock.DurabilityPerShot > 0
                    ? Math.Max(1, (int)Math.Ceiling(weaponBlock.DurabilityPerShot))
                    : 0;
                if (wear > 0)
                {
                    weapon.Durability = Math.Max(0, weapon.Durability.Value - wear);
                }
            }

            if (damage <= 0 || defender.Equipment == null)
            {
                return;
            }

            var armor = defender.Equipment.Get(EquipmentSlot.Armor);
            if (armor != null)
            {
                _wear.ApplyWear(armor, damage);
            }

            var helmet = defender.Equipment.Get(EquipmentSlot.Helmet);
            if (helmet != null)
            {
                _wear.ApplyWear(helmet, damage);
            }
        }
    }
}
