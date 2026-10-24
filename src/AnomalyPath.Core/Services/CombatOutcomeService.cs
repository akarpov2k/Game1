#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Session;

namespace AnomalyPath.Core.Services
{
    /// <summary>
    /// Итог боя: победа даёт опыт и лут, поражение применяет смерть и, кроме пермасмерти,
    /// возвращает игрока в последний хаб с урезанными здоровьем, едой и водой.
    /// </summary>
    public sealed class CombatOutcomeService : ICombatOutcomeService
    {
        private readonly IDeathService _death;
        private readonly IGameRulesRepository _rules;
        private readonly IFormulaRepository _formulas;
        private readonly CombatSettingsTemplate _settings;
        private readonly BalanceTemplate _balance;

        /// <summary>Создаёт сервис результата боя.</summary>
        /// <param name="death">Последствия смерти для вещей.</param>
        /// <param name="rules">Правила прохождения: доля респавна.</param>
        /// <param name="formulas">Формулы: опыт за победу.</param>
        /// <param name="settings">Настройки боя: ссылка на формулу опыта.</param>
        /// <param name="balance">Константы баланса для формулы.</param>
        /// <exception cref="ArgumentNullException">Зависимость не задана.</exception>
        public CombatOutcomeService(
            IDeathService death,
            IGameRulesRepository rules,
            IFormulaRepository formulas,
            CombatSettingsTemplate settings,
            BalanceTemplate balance)
        {
            _death = death ?? throw new ArgumentNullException(nameof(death));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _formulas = formulas ?? throw new ArgumentNullException(nameof(formulas));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _balance = balance ?? throw new ArgumentNullException(nameof(balance));
        }

        /// <inheritdoc />
        public CombatOutcome Resolve(GameSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            var combat = session.Combat;
            if (combat == null)
            {
                return CombatOutcome.NoCombat();
            }

            if (!combat.IsFinished)
            {
                return CombatOutcome.NotFinished();
            }

            var player = session.State.Player;
            if (player == null)
            {
                return CombatOutcome.NoCombat();
            }

            return combat.Player.IsDead
                ? ResolveDefeat(session, player, combat)
                : ResolveVictory(session, player, combat);
        }

        private CombatOutcome ResolveVictory(GameSession session, PlayerState player, CombatState combat)
        {
            player.SetHealth(ToHealth(combat.Player.Health));
            var xp = CalculateXp(combat.Enemy.Template, session.State.World.CurrentLocation.Tier);
            player.AddXp(xp);
            session.SetCombat(null);
            return CombatOutcome.Victory(xp, Array.Empty<ItemInstance>());
        }

        private CombatOutcome ResolveDefeat(GameSession session, PlayerState player, CombatState combat)
        {
            var death = _death.Apply(session.State);
            if (death.Permadeath)
            {
                player.SetHealth(0);
                session.SetCombat(null);
                return CombatOutcome.Defeat(death, false, null);
            }

            var rules = _rules.Require();
            var health = Math.Max(
                1,
                (int)Math.Round(combat.Player.MaxHealth * rules.RespawnPercent, MidpointRounding.AwayFromZero));
            player.SetHealth(health);

            var needsCap = Math.Max(
                0,
                (int)Math.Round(player.Needs.Max * rules.RespawnPercent, MidpointRounding.AwayFromZero));
            player.Needs.SetHunger(Math.Min(player.Needs.Hunger, needsCap));
            player.Needs.SetThirst(Math.Min(player.Needs.Thirst, needsCap));

            var hubId = session.State.World.LastHubId;
            session.State.World.MoveTo(hubId);
            session.SetCombat(null);
            return CombatOutcome.Defeat(death, true, hubId);
        }

        private int CalculateXp(ActorTemplate enemy, int locationTier)
        {
            var enemyXp = enemy.Combat?.Xp ?? 0;
            if (enemyXp <= 0)
            {
                return 0;
            }

            var variables = new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["enemyXp"] = enemyXp,
                ["locationTier"] = locationTier
            };
            var formula = _formulas.Get(_settings.XpFormulaId);
            var value = formula.Evaluate(variables, _balance.Constants, 0, null);
            return value <= 0 ? 0 : (int)value;
        }

        private static int ToHealth(double health) =>
            health <= 0 ? 0 : (int)Math.Round(health, MidpointRounding.AwayFromZero);
    }
}
