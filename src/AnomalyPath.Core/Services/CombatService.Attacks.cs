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
    /// <summary>Часть класса <see cref="CombatService"/>: атаки и урон.</summary>
    public sealed partial class CombatService
    {
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
                damage = EvaluateDamage(
                    defender,
                    template.Weapon.DamageTypeId,
                    template.Weapon.Damage,
                    template.Weapon.ArmorPenetration,
                    state.Distance,
                    template.Id);
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
        public bool TryEnemyAttack(CombatState state, out CombatAttackResult result)
        {
            result = default;
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (state.IsFinished || !ReferenceEquals(state.Current, state.Enemy))
            {
                return false;
            }

            var attacker = state.Enemy;
            var defender = state.Player;
            var attack = attacker.Template.Combat?.Attack;
            if (attack == null)
            {
                return false;
            }

            if (attack.WeaponId != null)
            {
                var weapon = state.EnemyWeapon;
                if (weapon == null)
                {
                    return false;
                }

                return TryAttack(state, weapon, out result);
            }

            if (state.Distance > attack.Range || attacker.Ap < _settings.AttackApCost)
            {
                return false;
            }

            var chance = NaturalChance(attacker, state.Distance);
            var roll = _random.NextDouble();
            var hit = roll < chance;
            var damage = 0;
            if (hit)
            {
                attacker.PityBonus = 0;
                damage = EvaluateDamage(
                    defender,
                    attack.DamageTypeId!,
                    attack.Damage,
                    0,
                    state.Distance,
                    attacker.Template.Id);
            }
            else
            {
                attacker.PityBonus += chance * 0.05;
            }

            attacker.Ap -= _settings.AttackApCost;
            defender.Health = Math.Max(0, defender.Health - damage);
            ApplyArmorWear(defender, damage);
            result = new CombatAttackResult(hit, damage, defender.IsDead, roll, chance);
            return true;
        }

        /// <inheritdoc />
        public bool TryFlee(CombatState state, out FleeResult result)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (state.IsFinished)
            {
                result = FleeResult.Finished();
                return false;
            }

            if (!ReferenceEquals(state.Current, state.Player))
            {
                result = FleeResult.NotPlayerTurn();
                return false;
            }

            EndTurn(state);
            var attacked = TryEnemyAttack(state, out var attack);
            var attackResult = attacked ? attack : (CombatAttackResult?)null;
            result = state.Player.IsDead
                ? FleeResult.DiedTo(attackResult)
                : FleeResult.EscapedWith(ToHealth(state.Player.Health), attackResult);
            return true;
        }

        private static int ToHealth(double health) =>
            health <= 0 ? 0 : (int)Math.Round(health, MidpointRounding.AwayFromZero);

        private double NaturalChance(Combatant attacker, DistanceZone distance)
        {
            var combat = attacker.Template.Combat;
            var accuracy = (combat?.Accuracy ?? 0.8) + (combat?.AccuracyBonus ?? 0) +
                           attacker.AccuracyBonus + attacker.PityBonus;
            return Math.Clamp(accuracy * DistanceMultiplier(distance), 0, 1);
        }

        private int EvaluateDamage(
            Combatant defender,
            string damageType,
            double baseDamage,
            double penetration,
            DistanceZone distance,
            string sourceId)
        {
            if (!_damageTypes.Contains(damageType))
            {
                throw new DataException(sourceId, CoreErrors.UnknownDamageType, damageType);
            }

            var variables = new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["damage"] = baseDamage,
                ["penetration"] = penetration,
                ["protection"] = ProtectionOf(defender, damageType),
                ["distance"] = (int)distance
            };

            var formula = _formulas.Get(_settings.DamageFormulaId);
            var evaluated = formula.Evaluate(variables, _balance.Constants, _settings.ClampMin, _settings.ClampMax);
            return evaluated < 0 ? 0 : (int)evaluated;
        }
    }
}
