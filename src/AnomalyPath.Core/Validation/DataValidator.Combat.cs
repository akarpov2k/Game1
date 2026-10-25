#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Loadouts;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Loot;
using AnomalyPath.Core.Nicks;
using AnomalyPath.Core.Quests;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Rules;
using AnomalyPath.Core.Talents;
using AnomalyPath.Core.Templates;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Validation
{
    /// <summary>Часть класса <see cref="DataValidator"/>: бой.</summary>
    public sealed partial class DataValidator
    {
        private static void AddWorldIssues(LoadedData data, List<ValidationIssue> issues)
        {
            foreach (var issue in WorldValidator.Validate(data))
            {
                issues.Add(issue);
            }
        }

        private static void ValidateCombatReferences(LoadedData data, List<ValidationIssue> issues)
        {
            try
            {
                var damageTypes = DamageTypeRepository.FromLoadedData(data);
                var formulas = FormulaRepository.FromLoadedData(data);
                var actors = ActorRepository.FromLoadedData(data);
                var balanceRecords = data.All("balance");
                var constants = balanceRecords.Count > 0
                    ? BalanceTemplate.FromJson(balanceRecords[0]).Constants
                    : BalanceTemplate.Empty.Constants;

                foreach (var formula in formulas.All)
                {
                    var declared = new HashSet<string>(formula.Variables, StringComparer.Ordinal);
                    foreach (var name in formula.UsedNames)
                    {
                        if (!declared.Contains(name) && !constants.ContainsKey(name))
                        {
                            issues.Add(new ValidationIssue(
                                formula.Id,
                                CoreErrors.FormulaUnknownVariable,
                                CoreText.Format(CoreErrors.FormulaUnknownVariable, name)));
                        }
                    }
                }

                var combat = data.All("combat");
                if (combat.Count > 1)
                {
                    issues.Add(new ValidationIssue("combat", CoreErrors.DuplicateIdFiles, "combat"));
                }

                if (combat.Count == 1)
                {
                    var settings = CombatSettingsTemplate.FromJson(combat[0], "combat");
                    if (!formulas.Contains(settings.DamageFormulaId))
                    {
                        issues.Add(new ValidationIssue("combat", CoreErrors.FormulaNotFound, settings.DamageFormulaId));
                    }

                    if (!formulas.Contains(settings.XpFormulaId))
                    {
                        issues.Add(new ValidationIssue("combat", CoreErrors.FormulaNotFound, settings.XpFormulaId));
                    }
                }

                foreach (var actor in actors.All)
                {
                    var block = actor.Combat;
                    if (block == null)
                    {
                        continue;
                    }

                    foreach (var pair in block.Protection)
                    {
                        if (!damageTypes.Contains(pair.Key))
                        {
                            issues.Add(new ValidationIssue(
                                actor.Id,
                                CoreErrors.UnknownDamageType,
                                CoreText.Format(CoreErrors.UnknownDamageType, pair.Key)));
                        }
                    }
                }
            }
            catch (DataException)
            {
                // Ошибки схемы уже собраны отдельными проходами.
            }
        }

        private static void ValidateActorAttacks(LoadedData data, List<ValidationIssue> issues)
        {
            try
            {
                var damageTypes = DamageTypeRepository.FromLoadedData(data);
                var actors = ActorRepository.FromLoadedData(data);
                var items = ItemRepository.FromLoadedData(data);
                foreach (var actor in actors.All)
                {
                    if (actor.Combat != null)
                    {
                        ValidateActorAttack(actor, actor.Combat.Attack, damageTypes, items, issues);
                    }
                }
            }
            catch (DataException)
            {
                // Ошибки схемы актёров уже собраны в ValidateEntities.
            }
        }

        private static void ValidateActorAttack(
            ActorTemplate actor,
            ActorAttackTemplate? attack,
            DamageTypeRepository damageTypes,
            ItemRepository items,
            List<ValidationIssue> issues)
        {
            if (actor.Type != ActorType.Enemy)
            {
                if (attack != null)
                {
                    issues.Add(new ValidationIssue(
                        actor.Id,
                        CoreErrors.AttackOnNonEnemy,
                        CoreText.Format(CoreErrors.AttackOnNonEnemy, actor.Id)));
                }

                return;
            }

            if (attack == null)
            {
                issues.Add(new ValidationIssue(
                    actor.Id,
                    CoreErrors.ActorWithoutAttack,
                    CoreText.Format(CoreErrors.ActorWithoutAttack, actor.Id)));
                return;
            }

            if (attack.WeaponId != null)
            {
                if (!items.TryGet(attack.WeaponId, out var template))
                {
                    issues.Add(new ValidationIssue(
                        actor.Id,
                        CoreErrors.ItemNotFound,
                        CoreText.Format(CoreErrors.ItemNotFound, attack.WeaponId)));
                }
                else if (template.Weapon == null)
                {
                    issues.Add(new ValidationIssue(
                        actor.Id,
                        CoreErrors.WeaponWithoutBlock,
                        CoreText.Format(CoreErrors.WeaponWithoutBlock, attack.WeaponId)));
                }

                return;
            }

            if (attack.DamageTypeId == null || !damageTypes.Contains(attack.DamageTypeId))
            {
                issues.Add(new ValidationIssue(
                    actor.Id,
                    CoreErrors.UnknownDamageType,
                    CoreText.Format(CoreErrors.UnknownDamageType, attack.DamageTypeId ?? string.Empty)));
            }
        }
    }
}
