#nullable enable
using System;
using System.Collections.Generic;
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

            Check("итог: победа начисляет опыт по тиру локации и считает лут по пулу", () =>
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
                True(outcome.Loot.Count <= 9, "лут не больше числа записей пула");
                var pool = PlayerTableItems(services);
                foreach (var item in outcome.Loot)
                {
                    True(pool.Contains(item.ItemId), "предмет из пула бандита: " + item.ItemId);
                    True(item.Stack >= 1, "стопка лута");
                }

                Equal(100, state.Player.Health, "здоровье победителя");
                False(session.InCombat, "бой завершён");
            });

            Check("итог: победа повышает уровень и выдаёт награды за него", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services);
                var player = state.Player!;
                var progression = services.Resolve<IProgressionService>();
                progression.AddXp(player, 536);
                Equal(4, player.Level, "до боя четвёртый уровень");
                MoveToTiered(state);
                var session = StartFight(services, state, "enemy_bandit", out var combat);
                Win(services.Resolve<ICombatService>(), combat);

                services.Resolve<ICombatOutcomeService>().Resolve(session);

                Equal(5, player.Level, "пятый уровень после победы");
                Equal(1, player.TalentPoints, "очко талантов за пятый уровень");
                Equal(2, player.LevelHealthBonus, "прибавка здоровья за пятый уровень");
            });

            Check("итог: поражение теряет долю опыта текущего уровня", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services, xpLoss: 0.4);
                var player = state.Player!;
                services.Resolve<IProgressionService>().AddXp(player, 270);
                Equal(3, player.Level, "третий уровень до боя");
                MoveToWild(state);
                var session = StartFight(services, state, "enemy_bandit", out var combat);
                Lose(services.Resolve<ICombatService>(), combat);

                var outcome = services.Resolve<ICombatOutcomeService>().Resolve(session);

                Equal(CombatOutcomeStatus.Defeat, outcome.Status, "статус");
                Equal(20, outcome.Death!.XpLost, "потеряно 40% от 50");
                Equal(250, player.Xp, "остаток опыта");
                Equal(3, player.Level, "уровень не понизился");
            });

            Check("итог: лут детерминирован для одного seed и состояния", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var first = LootOfVictory(services);
                var second = LootOfVictory(services);
                Equal(first, second, "повтор с тем же seed мира");
            });

            Check("итог: каждая победа берёт новый номер броска лута", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services);
                MoveToWild(state);
                Equal(0, state.World.LootSerial, "до боёв");

                var first = StartFight(services, state, "enemy_bandit", out var firstCombat);
                Win(services.Resolve<ICombatService>(), firstCombat);
                services.Resolve<ICombatOutcomeService>().Resolve(first);

                var second = StartFight(services, state, "enemy_bandit", out var secondCombat);
                Win(services.Resolve<ICombatService>(), secondCombat);
                services.Resolve<ICombatOutcomeService>().Resolve(second);

                Equal(2, state.World.LootSerial, "номер вырос на каждой победе");
            });

            Check("итог: поражение не отдаёт лут", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services);
                MoveToWild(state);
                var session = StartFight(services, state, "enemy_bandit", out var combat);
                Lose(services.Resolve<ICombatService>(), combat);

                var outcome = services.Resolve<ICombatOutcomeService>().Resolve(session);

                Equal(CombatOutcomeStatus.Defeat, outcome.Status, "статус");
                Equal(0, outcome.Loot.Count, "лута нет");
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

        private static HashSet<string> PlayerTableItems(ServiceContainer services)
        {
            var loot = services.Resolve<ILootRepository>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            CollectLoot("loot_human", loot, ids);
            return ids;
        }

        private static void CollectLoot(string tableId, ILootRepository loot, HashSet<string> ids)
        {
            var table = loot.Get(tableId);
            foreach (var entry in table.Entries)
            {
                ids.Add(entry.ItemId);
            }

            foreach (var shared in table.SharedTableIds)
            {
                CollectLoot(shared, loot, ids);
            }
        }

        private static string LootOfVictory(ServiceContainer services)
        {
            var state = NewState(services);
            MoveToWild(state);
            var session = StartFight(services, state, "enemy_bandit", out var combat);
            Win(services.Resolve<ICombatService>(), combat);
            var outcome = services.Resolve<ICombatOutcomeService>().Resolve(session);
            var parts = new List<string>();
            foreach (var item in outcome.Loot)
            {
                parts.Add(item.ItemId + ":" + item.Stack);
            }

            return string.Join(",", parts);
        }

        private static GameState NewState(
            ServiceContainer services,
            bool permadeath = false,
            double xpLoss = 0)
        {
            var settings = new NewGameSettings(0, 1, permadeath: permadeath, xpLossChance: xpLoss);
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
