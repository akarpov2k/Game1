#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Death;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Services
{
    /// <summary>Действия сессии поверх сохранений, переходов и боя; состояние хранится в <see cref="GameSession"/>.</summary>
    public sealed class GameSessionService : IGameSessionService
    {
        private readonly ITravelService _travel;
        private readonly ICombatService _combat;
        private readonly ISaveService _saves;
        private readonly INewGameService _newGame;
        private readonly IDeathService _death;
        private readonly ICombatOutcomeService _outcomes;
        private readonly IQuestService? _quests;
        private readonly ITalentService? _talents;

        /// <summary>Создаёт сервис сессии.</summary>
        /// <param name="travel">Переходы по миру.</param>
        /// <param name="combat">Боевой расчёт.</param>
        /// <param name="saves">Сохранение и загрузка состояния.</param>
        /// <param name="newGame">Создание новой игры.</param>
        /// <param name="death">Последствия смерти для вещей.</param>
        /// <param name="outcomes">Итог боя: опыт, лут и респавн.</param>
        /// <param name="quests">Квесты: учёт убийств в бою; <c>null</c> — не учитывать.</param>
        /// <param name="talents">Таланты: бонусы игрока в бою; <c>null</c> — без бонусов.</param>
        /// <exception cref="ArgumentNullException">Зависимость не задана.</exception>
        public GameSessionService(
            ITravelService travel,
            ICombatService combat,
            ISaveService saves,
            INewGameService newGame,
            IDeathService death,
            ICombatOutcomeService outcomes,
            IQuestService? quests = null,
            ITalentService? talents = null)
        {
            _travel = travel ?? throw new ArgumentNullException(nameof(travel));
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            _saves = saves ?? throw new ArgumentNullException(nameof(saves));
            _newGame = newGame ?? throw new ArgumentNullException(nameof(newGame));
            _death = death ?? throw new ArgumentNullException(nameof(death));
            _outcomes = outcomes ?? throw new ArgumentNullException(nameof(outcomes));
            _quests = quests;
            _talents = talents;
        }

        /// <inheritdoc />
        public void Save(
            GameSession session,
            string slot,
            double playtime = 0,
            DateTimeOffset? createdAt = null,
            string? title = null)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            _saves.Save(session.State, slot, playtime, createdAt, title);
        }

        /// <inheritdoc />
        public GameSession Load(string slot) => new GameSession(_saves.Load(slot));

        /// <inheritdoc />
        public GameSession StartNew(int seed, string loadoutId, NewGameSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return new GameSession(_newGame.Create(seed, loadoutId, settings));
        }

        /// <inheritdoc />
        public DeathResult ApplyDeath(GameSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            return _death.Apply(session.State);
        }

        /// <inheritdoc />
        public CombatOutcome ApplyCombatOutcome(GameSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            return _outcomes.Resolve(session);
        }

        /// <inheritdoc />
        public IReadOnlyList<WorldEdge> AvailableEdges(GameSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            return session.InCombat
                ? new List<WorldEdge>()
                : _travel.GetAvailableEdges(session.State.World);
        }

        /// <inheritdoc />
        public TravelResult Travel(GameSession session, string edgeId)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            return session.InCombat
                ? new TravelResult(TravelStatus.InCombat)
                : _travel.Travel(session.State.World, edgeId);
        }

        /// <inheritdoc />
        public bool StartCombat(
            GameSession session,
            ActorTemplate playerActor,
            ActorTemplate enemy,
            DistanceZone distance,
            CombatScenario? scenario = null)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (session.InCombat)
            {
                return false;
            }

            var player = session.State.Player;
            if (player == null)
            {
                return false;
            }

            session.SetCombat(_combat.Start(
                playerActor,
                player.Equipment,
                enemy,
                distance,
                scenario,
                _talents?.GetModifiers(player)));
            return true;
        }

        /// <inheritdoc />
        public bool TryAttack(GameSession session, ItemInstance weapon, out CombatAttackResult result)
        {
            result = default;
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            var attacked = session.Combat != null && _combat.TryAttack(session.Combat, weapon, out result);
            if (attacked && result.Killed && _quests != null && session.Combat != null)
            {
                _quests.NotifyActorKilled(session, session.Combat.Enemy.Template.Id);
            }

            return attacked;
        }

        /// <inheritdoc />
        public bool TryReload(GameSession session, ItemInstance weapon)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            var player = session.State.Player;
            return session.Combat != null &&
                   player != null &&
                   _combat.TryReload(session.Combat, weapon, player.Inventory);
        }

        /// <inheritdoc />
        public bool TryMove(GameSession session, int deltaZones)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            return session.Combat != null && _combat.TryMove(session.Combat, deltaZones);
        }

        /// <inheritdoc />
        public void EndTurn(GameSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (session.Combat != null)
            {
                _combat.EndTurn(session.Combat);
            }
        }

        /// <inheritdoc />
        public void EndCombat(GameSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            session.SetCombat(null);
        }
    }
}
