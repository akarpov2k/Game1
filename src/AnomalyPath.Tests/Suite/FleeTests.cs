#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Di;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.Services;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.World;

namespace AnomalyPath.Tests
{
    /// <summary>Побег из боя: ход противника, его атака, смерть и исчезновение противника.</summary>
    public sealed class FleeTests : TestBase
    {
        private readonly string _dataRoot;

        /// <summary>Создаёт категорию тестов побега.</summary>
        /// <param name="dataRoot">Корень данных проекта.</param>
        public FleeTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        /// <inheritdoc />
        public override void Run()
        {
            Check("побег: без боя попытка отклоняется", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var session = new GameSession(NewState(services));
                var sessions = NewSessionService(services, new StubRandom(0));

                False(sessions.TryFlee(session, out var result), "побег без боя");
                Equal(FleeStatus.NotInCombat, result.Status, "статус");
            });

            Check("побег: в ход противника попытка отклоняется без атаки", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services);
                MoveToWild(state);
                var session = new GameSession(state);
                var sessions = NewSessionService(services, new StubRandom(0));
                True(
                    sessions.StartCombat(session, PlayerActor(services), Enemy(services, "enemy_bandit"), DistanceZone.Mid, CombatScenario.Ambush),
                    "бой начат засадой");
                var before = session.Combat!.Player.Health;

                False(sessions.TryFlee(session, out var result), "побег в чужой ход");
                Equal(FleeStatus.NotPlayerTurn, result.Status, "статус");
                Equal(before, session.Combat!.Player.Health, "противник не атаковал");
                True(session.InCombat, "бой продолжается");
            });

            Check("побег: ход переходит противнику, он атакует из оружия и бой закрывается", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services);
                MoveToWild(state);
                var session = new GameSession(state);
                var sessions = NewSessionService(services, new StubRandom(0));
                True(sessions.StartCombat(session, PlayerActor(services), Enemy(services, "enemy_bandit"), DistanceZone.Mid), "бой начат");
                var combat = session.Combat!;

                True(sessions.TryFlee(session, out var result), "побег выполнен");

                Equal(FleeStatus.Escaped, result.Status, "статус");
                True(result.Attack.HasValue, "противник атаковал");
                True(ReferenceEquals(combat.Current, combat.Enemy), "ход перешёл противнику");
                True(result.Attack!.Value.Hit, "атака попала при броске 0");
                True(result.Health < 100, "здоровье уменьшилось");
                Equal(result.Health, state.Player!.Health, "здоровье перенесено в игрока");
                False(session.InCombat, "бой закрыт");
                Equal(29, combat.EnemyWeapon!.MagazineRounds, "патрон из магазина противника");
            });

            Check("побег: здоровье и износ вещей не восстанавливаются", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services);
                MoveToWild(state);
                var session = new GameSession(state);
                var sessions = NewSessionService(services, new StubRandom(0));
                True(sessions.StartCombat(session, PlayerActor(services), Enemy(services, "enemy_bandit"), DistanceZone.Mid), "бой начат");
                var equipment = services.Resolve<IEquipmentService>();
                True(
                    equipment.TryEquip(
                        state.Player!.Equipment,
                        new ItemInstance("armor", "itm_armor_jacket", 1, 40, 40),
                        EquipmentSlot.Armor,
                        out _),
                    "броня надета");

                True(sessions.TryFlee(session, out var result), "побег выполнен");

                Equal(result.Health, state.Player!.Health, "здоровье после побега");
                var armor = state.Player.Equipment.Get(EquipmentSlot.Armor)!;
                True(armor.Durability!.Value < 40, "износ брони сохранён");
                True(armor.Durability.Value > 0, "броня не уничтожена");
            });

            Check("побег: смерть от атаки оставляет бой завершённым и даёт обычное поражение", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services);
                MoveToWild(state);
                var session = new GameSession(state);
                var sessions = NewSessionService(services, new StubRandom(0));
                var killer = KillerEnemy();
                True(sessions.StartCombat(session, PlayerActor(services), killer, DistanceZone.Melee), "бой начат");

                True(sessions.TryFlee(session, out var result), "попытка побега");

                Equal(FleeStatus.Died, result.Status, "статус");
                True(result.Died, "игрок погиб");
                True(session.InCombat, "бой не закрыт");
                True(session.Combat!.IsFinished, "бой завершён смертью");

                var outcome = sessions.ApplyCombatOutcome(session);
                Equal(CombatOutcomeStatus.Defeat, outcome.Status, "итог боя");
                True(state.Player!.Health > 0, "обычный респавн после смерти");
            });

            Check("побег: мутант на средней дистанции не достаёт", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services);
                MoveToWild(state);
                var session = new GameSession(state);
                var sessions = NewSessionService(services, new StubRandom(0));
                True(sessions.StartCombat(session, PlayerActor(services), Enemy(services, "mutant_wolf"), DistanceZone.Mid), "бой начат");
                var before = state.Player!.Health;

                True(sessions.TryFlee(session, out var result), "побег выполнен");

                Equal(FleeStatus.Escaped, result.Status, "статус");
                False(result.Attack.HasValue, "атаки не было");
                Equal(before, state.Player!.Health, "здоровье не изменилось");
            });

            Check("побег: мутант вплотную бьёт естественной атакой", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services);
                MoveToWild(state);
                var session = new GameSession(state);
                var sessions = NewSessionService(services, new StubRandom(0));
                True(sessions.StartCombat(session, PlayerActor(services), Enemy(services, "mutant_wolf"), DistanceZone.Melee), "бой начат");
                var before = state.Player!.Health;

                True(sessions.TryFlee(session, out var result), "побег выполнен");

                True(result.Attack.HasValue, "естественная атака была");
                True(result.Attack!.Value.Hit, "атака попала");
                True(result.Health < before, "здоровье уменьшилось");
                True(result.Health <= before - 1, "урон прошёл по защите");
            });

            Check("побег: враг без атаки не наносит урона", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services);
                MoveToWild(state);
                var session = new GameSession(state);
                var sessions = NewSessionService(services, new StubRandom(0));
                var harmless = new ActorTemplate(
                    "harmless",
                    ActorType.Enemy,
                    "n",
                    new CombatBlockTemplate(1, 1, 10, 5, new Dictionary<string, int>(), "passive", 1.0, 0, 10));
                True(sessions.StartCombat(session, PlayerActor(services), harmless, DistanceZone.Melee), "бой начат");
                var before = state.Player!.Health;

                True(sessions.TryFlee(session, out var result), "побег выполнен");

                Equal(FleeStatus.Escaped, result.Status, "статус");
                False(result.Attack.HasValue, "атаки нет");
                Equal(before, state.Player!.Health, "здоровье не изменилось");
            });

            Check("побег: противник исчезает только на текущей локации", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services);
                var current = MoveToWild(state);
                var session = new GameSession(state);
                var sessions = NewSessionService(services, new StubRandom(0));
                True(sessions.StartCombat(session, PlayerActor(services), Enemy(services, "enemy_bandit"), DistanceZone.Mid), "бой начат");

                True(sessions.TryFlee(session, out _), "побег выполнен");

                True(state.World.CurrentLocation.EnemyGone, "текущая локация без противника");
                foreach (var location in state.World.Map.Locations)
                {
                    if (location.InstanceId != current)
                    {
                        False(location.EnemyGone, "другая локация сохраняет противника");
                    }
                }
            });

            Check("побег: опыт и лут не начисляются", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services);
                MoveToWild(state);
                var session = new GameSession(state);
                var sessions = NewSessionService(services, new StubRandom(0));
                True(sessions.StartCombat(session, PlayerActor(services), Enemy(services, "enemy_bandit"), DistanceZone.Mid), "бой начат");
                var lootSerial = state.World.LootSerial;

                True(sessions.TryFlee(session, out _), "побег выполнен");

                Equal(0, state.Player!.Xp, "опыт не начислен");
                Equal(lootSerial, state.World.LootSerial, "номер броска лута не вырос");
                Equal(CombatOutcomeStatus.NoCombat, sessions.ApplyCombatOutcome(session).Status, "итога боя нет");
            });

            Check("побег: повторный побег отклоняется", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var state = NewState(services);
                MoveToWild(state);
                var session = new GameSession(state);
                var sessions = NewSessionService(services, new StubRandom(0));
                True(sessions.StartCombat(session, PlayerActor(services), Enemy(services, "enemy_bandit"), DistanceZone.Mid), "бой начат");
                True(sessions.TryFlee(session, out _), "первый побег");

                False(sessions.TryFlee(session, out var result), "второй побег");
                Equal(FleeStatus.NotInCombat, result.Status, "статус");
            });

            Check("побег: детерминирован при одном seed", () =>
            {
                var first = EscapeSummary(Bootstrap.Build(_dataRoot));
                var second = EscapeSummary(Bootstrap.Build(_dataRoot));
                Equal(first, second, "повтор с тем же seed");
            });
        }

        private string EscapeSummary(ServiceContainer services)
        {
            var state = NewState(services);
            MoveToWild(state);
            var session = new GameSession(state);
            var sessions = NewSessionService(services, new SeededCombatRandom(123));
            True(sessions.StartCombat(session, PlayerActor(services), Enemy(services, "enemy_bandit"), DistanceZone.Mid), "бой начат");
            True(sessions.TryFlee(session, out var result), "побег");
            var attack = result.Attack;
            return result.Status + "|" + result.Health + "|" +
                   (attack.HasValue ? attack.Value.Hit + ":" + attack.Value.Damage : "none");
        }

        private static GameState NewState(ServiceContainer services) =>
            services.Resolve<INewGameService>().Create(42, "loadout_rifleman", new NewGameSettings(0, 1));

        private static string MoveToWild(GameState state)
        {
            foreach (var location in state.World.Map.Locations)
            {
                if (location.Subtype != LocationSubtype.Hub)
                {
                    state.World.MoveTo(location.InstanceId);
                    return location.InstanceId;
                }
            }

            throw new InvalidOperationException("В мире нет дикой локации");
        }

        private static ActorTemplate PlayerActor(ServiceContainer services) =>
            services.Resolve<IActorRepository>().Get(services.Resolve<IGameRulesRepository>().Require().PlayerActorId);

        private static ActorTemplate Enemy(ServiceContainer services, string id) =>
            services.Resolve<IActorRepository>().Get(id);

        private static ActorTemplate KillerEnemy()
        {
            var attack = ActorAttackTemplate.FromJson(
                JsonParser.Parse("{\"damage\":1000,\"damageTypeId\":\"dmg_rupture\",\"range\":\"melee\"}", "test"),
                "test");
            return new ActorTemplate(
                "killer",
                ActorType.Enemy,
                "n",
                new CombatBlockTemplate(
                    1,
                    1,
                    10,
                    5,
                    new Dictionary<string, int>(),
                    "aggressive",
                    1.0,
                    0,
                    10,
                    attack));
        }

        private static GameSessionService NewSessionService(ServiceContainer services, ICombatRandom random) =>
            new GameSessionService(
                services.Resolve<ITravelService>(),
                NewCombatService(services, random),
                NullSaves.Instance,
                services.Resolve<INewGameService>(),
                services.Resolve<IDeathService>(),
                services.Resolve<ICombatOutcomeService>());

        private static CombatService NewCombatService(ServiceContainer services, ICombatRandom random) =>
            new CombatService(
                services.Resolve<IItemRepository>(),
                services.Resolve<IDamageTypeRepository>(),
                services.Resolve<IEquipmentService>(),
                services.Resolve<IArmorWearService>(),
                services.Resolve<IFormulaRepository>(),
                Settings(),
                BalanceTemplate.Empty,
                random,
                services.Resolve<IInventoryService>());

        private static CombatSettingsTemplate Settings() => new CombatSettingsTemplate(
            "formula_damage_base",
            "formula_combat_xp",
            0,
            null,
            ApRestoreKind.Full,
            0,
            1,
            1,
            1,
            1.4,
            1.0,
            0.8,
            1);

        private sealed class NullSaves : ISaveService
        {
            public static readonly NullSaves Instance = new NullSaves();

            public void Save(
                GameState state,
                string slot,
                double playtime = 0,
                DateTimeOffset? createdAt = null,
                string? title = null)
            {
            }

            public GameState Load(string slot) => throw new NotSupportedException();

            public bool Exists(string slot) => false;

            public IReadOnlyList<SaveSlotInfo> ListSlots() => Array.Empty<SaveSlotInfo>();

            public void Delete(string slot)
            {
            }
        }

        private sealed class StubRandom : ICombatRandom
        {
            private readonly double _value;

            public StubRandom(double value)
            {
                _value = value;
            }

            public double NextDouble() => _value;
        }
    }
}
