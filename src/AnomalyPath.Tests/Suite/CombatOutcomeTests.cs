#nullable enable
using System;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Di;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.World;

namespace AnomalyPath.Tests
{
    /// <summary>Результат боя: опыт, лут, смерть, респавн и пермасмерть.</summary>
    public sealed class CombatOutcomeTests : TestBase
    {
        private readonly string _dataRoot;

        /// <summary>Создаёт категорию тестов результата боя.</summary>
        /// <param name="dataRoot">Корень данных проекта.</param>
        public CombatOutcomeTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        /// <inheritdoc />
        public override void Run()
        {
            Check("итог: сервис зарегистрирован", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                True(services.IsRegistered<ICombatOutcomeService>(), "ICombatOutcomeService");
            });

            Check("итог: отрицательные опыт и здоровье отклоняются", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var player = NewState(services).Player!;
                Throws<ArgumentOutOfRangeException>(() => player.AddXp(-1), "отрицательный опыт");
                Throws<ArgumentOutOfRangeException>(() => player.SetHealth(-1), "отрицательное здоровье");
            });

            Check("итог: без боя ничего не меняется", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services);
                var session = new GameSession(state);

                var outcome = services.Resolve<ICombatOutcomeService>().Resolve(session);

                Equal(CombatOutcomeStatus.NoCombat, outcome.Status, "статус");
                Equal(0, state.Player!.Xp, "опыт");
            });

            Check("итог: незавершённый бой не завершается", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services);
                MoveToWild(state);
                var session = StartFight(services, state, "enemy_bandit", out var combat);
                var outcome = services.Resolve<ICombatOutcomeService>().Resolve(session);

                Equal(CombatOutcomeStatus.NotFinished, outcome.Status, "статус");
                True(session.InCombat, "бой продолжается");
                Equal(0, state.Player!.Xp, "опыт не начислен");
                False(combat.IsFinished, "бой не завершён");
            });

            Check("итог: победа начисляет опыт по тиру локации и отдаёт пустой лут", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services);
                var tier = MoveToTiered(state);
                var session = StartFight(services, state, "enemy_bandit", out var combat);
                Win(services.Resolve<ICombatService>(), combat);

                var outcome = services.Resolve<ICombatOutcomeService>().Resolve(session);

                var expected = (int)Math.Round(
                    services.Resolve<IActorRepository>().Get("enemy_bandit").Combat!.Xp *
                    (1 + tier * 5 / 100.0),
                    MidpointRounding.AwayFromZero);

                Equal(CombatOutcomeStatus.Victory, outcome.Status, "статус");
                Equal(expected, outcome.XpGained, "опыт за победу");
                Equal(expected, state.Player!.Xp, "опыт игрока");
                Equal(0, outcome.Loot.Count, "лут пока пуст");
                Equal(100, state.Player.Health, "здоровье победителя");
                False(session.InCombat, "бой завершён");
            });

            Check("итог: повторный разбор победы не начисляет опыт второй раз", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services);
                MoveToWild(state);
                var session = StartFight(services, state, "enemy_bandit", out var combat);
                Win(services.Resolve<ICombatService>(), combat);
                var outcomes = services.Resolve<ICombatOutcomeService>();

                var first = outcomes.Resolve(session);
                var second = outcomes.Resolve(session);

                Equal(CombatOutcomeStatus.Victory, first.Status, "первый разбор");
                Equal(CombatOutcomeStatus.NoCombat, second.Status, "второй разбор");
                Equal(first.XpGained, state.Player!.Xp, "опыт не удвоился");
            });

            Check("итог: поражение возвращает в последний хаб и срезает здоровье и потребности", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services);
                var hubId = state.World.LastHubId;
                var wildId = MoveToWild(state);
                True(wildId != hubId, "бой не в хабе");
                state.Player!.SetHealth(5);
                state.Player.Needs.SetHunger(10);
                state.Player.Needs.SetThirst(20);
                var session = StartFight(services, state, "enemy_bandit", out var combat);
                Lose(services.Resolve<ICombatService>(), combat);

                var outcome = services.Resolve<ICombatOutcomeService>().Resolve(session);

                Equal(CombatOutcomeStatus.Defeat, outcome.Status, "статус");
                True(outcome.Respawned, "игрок возродился");
                Equal(hubId, outcome.RespawnLocationId, "хаб возврата");
                Equal(hubId, state.World.CurrentLocationId, "текущая локация");
                Equal(30, state.Player.Health, "здоровье всегда 30%");
                Equal(10, state.Player.Needs.Hunger, "еда ниже 30% осталась");
                Equal(20, state.Player.Needs.Thirst, "вода ниже 30% осталась");
                Equal(100, state.Player.Needs.Sleep, "сон не тронут");
                True(outcome.Death != null, "последствия смерти");
                False(session.InCombat, "бой завершён");
            });

            Check("итог: еда и вода срезаются до 30%, если было больше", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services);
                MoveToWild(state);
                var session = StartFight(services, state, "enemy_bandit", out var combat);
                Lose(services.Resolve<ICombatService>(), combat);

                services.Resolve<ICombatOutcomeService>().Resolve(session);

                Equal(30, state.Player!.Needs.Hunger, "еда");
                Equal(30, state.Player.Needs.Thirst, "вода");
                Equal(30, state.Player.Health, "здоровье");
            });

            Check("итог: пермасмерть не возвращает в хаб", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services, permadeath: true);
                var wildId = MoveToWild(state);
                var session = StartFight(services, state, "enemy_bandit", out var combat);
                Lose(services.Resolve<ICombatService>(), combat);

                var outcome = services.Resolve<ICombatOutcomeService>().Resolve(session);

                Equal(CombatOutcomeStatus.Defeat, outcome.Status, "статус");
                False(outcome.Respawned, "возврата нет");
                True(outcome.Death!.Permadeath, "режим полной смерти");
                Equal(wildId, state.World.CurrentLocationId, "игрок остался на месте");
                Equal(0, state.Player!.Health, "здоровье");
                False(session.InCombat, "бой завершён");
            });
        }

        private static GameState NewState(ServiceContainer services, bool permadeath = false)
        {
            var settings = new NewGameSettings(0, 1, permadeath: permadeath);
            return services.Resolve<INewGameService>().Create(42, "loadout_rifleman", settings);
        }

        private static string MoveToWild(GameState state)
        {
            foreach (var location in state.World.Map.Locations)
            {
                if (location.NodeType != LocationNodeType.Hub)
                {
                    state.World.MoveTo(location.InstanceId);
                    return location.InstanceId;
                }
            }

            throw new InvalidOperationException("В мире нет дикой локации");
        }

        private static int MoveToTiered(GameState state)
        {
            foreach (var location in state.World.Map.Locations)
            {
                if (location.NodeType != LocationNodeType.Hub && location.Tier > 0)
                {
                    state.World.MoveTo(location.InstanceId);
                    return location.Tier;
                }
            }

            throw new InvalidOperationException("В мире нет дикой локации с тиром");
        }

        private static GameSession StartFight(
            ServiceContainer services,
            GameState state,
            string enemyId,
            out CombatState combat)
        {
            var actors = services.Resolve<IActorRepository>();
            var rules = services.Resolve<IGameRulesRepository>().Require();
            var playerActor = actors.Get(rules.PlayerActorId);
            combat = services.Resolve<ICombatService>().Start(
                playerActor,
                state.Player!.Equipment,
                actors.Get(enemyId),
                DistanceZone.Mid);
            return new GameSession(state, combat);
        }

        private static void Win(ICombatService combat, CombatState state)
        {
            var weapon = new ItemInstance("test_player_weapon", "wpn_sv98", 1, 100, 100, null, 10);
            for (var guard = 0; guard < 40 && !state.IsFinished; guard++)
            {
                if (ReferenceEquals(state.Current, state.Player))
                {
                    if (!combat.TryAttack(state, weapon, out _))
                    {
                        combat.EndTurn(state);
                    }
                }
                else
                {
                    combat.EndTurn(state);
                }
            }
        }

        private static void Lose(ICombatService combat, CombatState state)
        {
            var weapon = new ItemInstance("test_enemy_weapon", "wpn_ak74", 1, 100, 100, null, 30);
            for (var guard = 0; guard < 200 && !state.IsFinished; guard++)
            {
                if (ReferenceEquals(state.Current, state.Player))
                {
                    combat.EndTurn(state);
                    continue;
                }

                if (!combat.TryAttack(state, weapon, out _))
                {
                    combat.EndTurn(state);
                    continue;
                }

                if (!state.IsFinished && state.Current.Ap <= 0)
                {
                    combat.EndTurn(state);
                }
            }
        }
    }
}
