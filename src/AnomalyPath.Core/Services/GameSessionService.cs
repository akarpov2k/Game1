#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Death;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Loot;
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
        private readonly IProgressionService? _progression;
        private readonly IStashService? _stashes;
        private readonly ILocationObjectService? _locationObjects;
        private readonly IWorldGenerator? _worldGenerator;
        private readonly ITopologyRepository? _topology;

        /// <summary>Создаёт сервис сессии.</summary>
        /// <param name="travel">Переходы по миру.</param>
        /// <param name="combat">Боевой расчёт.</param>
        /// <param name="saves">Сохранение и загрузка состояния.</param>
        /// <param name="newGame">Создание новой игры.</param>
        /// <param name="death">Последствия смерти для вещей.</param>
        /// <param name="outcomes">Итог боя: опыт, лут и респавн.</param>
        /// <param name="quests">Квесты: учёт убийств в бою; <c>null</c> — не учитывать.</param>
        /// <param name="talents">Таланты: бонусы игрока в бою; <c>null</c> — без бонусов.</param>
        /// <param name="progression">
        /// Прогрессия: подтягивание уровня при загрузке; <c>null</c> — не подтягивать.
        /// </param>
        /// <param name="stashes">Тайники на локациях; <c>null</c> — тайников нет.</param>
        /// <param name="locationObjects">Объекты локаций; <c>null</c> — только тайники.</param>
        /// <param name="worldGenerator">Достройка маршрута при переходах; <c>null</c> — маршрут не растёт.</param>
        /// <param name="topology">Топология: цена возврата в хаб; <c>null</c> — бесплатно.</param>
        /// <exception cref="ArgumentNullException">Зависимость не задана.</exception>
        public GameSessionService(
            ITravelService travel,
            ICombatService combat,
            ISaveService saves,
            INewGameService newGame,
            IDeathService death,
            ICombatOutcomeService outcomes,
            IQuestService? quests = null,
            ITalentService? talents = null,
            IProgressionService? progression = null,
            IStashService? stashes = null,
            ILocationObjectService? locationObjects = null,
            IWorldGenerator? worldGenerator = null,
            ITopologyRepository? topology = null)
        {
            _travel = travel ?? throw new ArgumentNullException(nameof(travel));
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            _saves = saves ?? throw new ArgumentNullException(nameof(saves));
            _newGame = newGame ?? throw new ArgumentNullException(nameof(newGame));
            _death = death ?? throw new ArgumentNullException(nameof(death));
            _outcomes = outcomes ?? throw new ArgumentNullException(nameof(outcomes));
            _quests = quests;
            _talents = talents;
            _progression = progression;
            _stashes = stashes;
            _locationObjects = locationObjects;
            _worldGenerator = worldGenerator;
            _topology = topology;
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
        public GameSession Load(string slot)
        {
            var state = _saves.Load(slot);
            if (_progression != null && state.Player != null)
            {
                // Старые сейвы копили опыт без повышения уровня: подтягиваем уровень и награды.
                _progression.SyncLevel(state.Player);
            }

            return new GameSession(state);
        }

        /// <inheritdoc />
        public GameSession StartNew(int seed, string loadoutId, NewGameSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var session = new GameSession(_newGame.Create(seed, loadoutId, settings));
            RollLocationStash(session);
            return session;
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

            if (session.InCombat)
            {
                return new TravelResult(TravelStatus.InCombat);
            }

            var previous = session.State.World.CurrentLocation;
            var result = _travel.Travel(session.State.World, edgeId);
            if (result.Succeeded)
            {
                var world = session.State.World;
                if (world.CurrentLocation.RouteEdge != previous.RouteEdge)
                {
                    _worldGenerator?.Extend(world);
                }

                PrunePassed(world, previous);
                EnsureLocationObject(session);
            }

            return result;
        }

        /// <inheritdoc />
        public TravelResult LeaveHub(GameSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (session.InCombat)
            {
                return new TravelResult(TravelStatus.InCombat);
            }

            var world = session.State.World;
            if (world.CurrentLocation.Subtype != LocationSubtype.Hub)
            {
                return new TravelResult(TravelStatus.WrongLocation);
            }

            if (_worldGenerator == null)
            {
                return new TravelResult(TravelStatus.EdgeNotFound);
            }

            _worldGenerator.BeginRoute(world);
            var edge = FirstRouteEdge(world);
            if (edge == null)
            {
                return new TravelResult(TravelStatus.EdgeNotFound);
            }

            var previous = world.CurrentLocation;
            var result = _travel.Travel(world, edge.EdgeId);
            if (result.Succeeded)
            {
                PrunePassed(world, previous);
                EnsureLocationObject(session);
            }

            return result;
        }

        /// <inheritdoc />
        public TravelResult ReturnToHub(GameSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (session.InCombat)
            {
                return new TravelResult(TravelStatus.InCombat);
            }

            var world = session.State.World;
            var location = world.CurrentLocation;
            if (location.Subtype != LocationSubtype.Path)
            {
                return new TravelResult(TravelStatus.HubUnavailable);
            }

            var cost = _topology != null ? _topology.Topology.HubReturnCost(location.RouteStep) : TravelCost.Zero;
            if (world.Stamina < cost.Stamina)
            {
                return new TravelResult(TravelStatus.NotEnoughStamina);
            }

            if (cost.Stamina > 0)
            {
                world.TrySpendStamina(cost.Stamina);
            }

            world.Clock.Advance(cost.Hours);
            var previous = location;
            world.MoveTo(world.LastHubId);
            PrunePassed(world, previous);
            EnsureLocationObject(session);
            return new TravelResult(TravelStatus.Success);
        }

        /// <inheritdoc />
        public bool RollLocationStash(GameSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            EnsureLocationObject(session);
            return session.State.World.CurrentLocation.StashPresent;
        }

        private void EnsureLocationObject(GameSession session)
        {
            var world = session.State.World;
            var location = world.CurrentLocation;
            if (location.ObjectSelected)
            {
                return;
            }

            if (_locationObjects != null)
            {
                _locationObjects.Select(world, location, NextObjectSeed(world.ObjectSeed));
            }
            else
            {
                _stashes?.RollOnEntry(world, location);
            }

            location.MarkObjectSelected();
        }

        /// <summary>
        /// Снимает с карты пройденное: покинутую локацию и всё, что осталось позади текущей.
        /// Хаб и цели квестов остаются, чтобы в хаб можно было вернуться.
        /// </summary>
        private static void PrunePassed(WorldState world, LocationInstance previous)
        {
            if (previous.InstanceId != world.CurrentLocationId)
            {
                world.Map.RemoveEdgesBetween(previous.InstanceId, world.CurrentLocationId);
            }

            if (previous.Subtype != LocationSubtype.Hub &&
                previous.InstanceId != world.Map.StartLocationId &&
                previous.InstanceId != world.CurrentLocationId)
            {
                world.Map.RemoveLocation(previous.InstanceId);
            }

            world.Map.PrunePassed(world.CurrentLocationId);
        }

        /// <summary>Первое ребро маршрута из хаба: в первую локацию пути звена 1.</summary>
        private static WorldEdge? FirstRouteEdge(WorldState world)
        {
            foreach (var edge in world.Map.EdgesFrom(world.CurrentLocationId))
            {
                var target = world.Map.GetLocation(edge.ToId);
                if (target.Subtype == LocationSubtype.Path && target.RouteEdge == 1 && target.RouteStep == 1)
                {
                    return edge;
                }
            }

            return null;
        }

        private static int NextObjectSeed(string objectSeed)
        {
            var ticks = DateTime.UtcNow.Ticks;
            var noise = Guid.NewGuid().GetHashCode();
            var mixed = unchecked((int)(ticks ^ (ticks >> 32)) ^ noise);
            return StableHash.Combine(mixed, objectSeed);
        }

        /// <inheritdoc />
        public StashOpenResult OpenStash(GameSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            return _stashes == null
                ? StashOpenResult.NoStash()
                : _stashes.Open(session.State.World, session.State.World.CurrentLocation);
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
                _talents?.GetModifiers(player),
                player.LevelHealthBonus));
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
        public bool TryFlee(GameSession session, out FleeResult result)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            var combat = session.Combat;
            if (combat == null)
            {
                result = FleeResult.NotInCombat();
                return false;
            }

            if (!_combat.TryFlee(combat, out result))
            {
                return false;
            }

            if (result.Died)
            {
                // Бой остаётся завершённым: вызывающий применяет обычный итог поражения.
                return true;
            }

            session.State.Player?.SetHealth(result.Health);
            session.State.World.CurrentLocation.MarkEnemyGone();
            session.SetCombat(null);
            return true;
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
