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
    /// <summary>Часть класса <see cref="CombatService"/>: оружие и перезарядка.</summary>
    public sealed partial class CombatService
    {
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
    }
}
