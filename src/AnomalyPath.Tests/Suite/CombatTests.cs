#nullable enable
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Di;
using System.Collections.Generic;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Services;

namespace AnomalyPath.Tests
{
    /// <summary>Бой: актёры, меткость, урон, износ, ОД, дистанция и ходы.</summary>
    public sealed class CombatTests : TestBase
    {
        private readonly string _dataRoot;

        public CombatTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        public override void Run()
        {
            var services = Bootstrap.Build(_dataRoot);
            var actors = services.Resolve<IActorRepository>();
            var combat = services.Resolve<ICombatService>();
            var equipment = services.Resolve<IEquipmentService>();
            var settings = services.Resolve<IEquipmentSettings>();

            Check("бой: актёры загружены", () =>
            {
                Equal(3, actors.All.Count, "актёры");
                Equal(45, actors.Get("enemy_bandit").Combat!.Health, "здоровье бандита");
                Equal(0.8, actors.Get("actor_player").Combat!.Accuracy, "меткость игрока");
            });

            Check("бой: игрок ходит первым", () =>
            {
                var state = Start(combat, actors, equipment, settings);
                True(ReferenceEquals(state.Current, state.Player), "первый ход игрока");
            });

            Check("меткость: автомат на средней дистанции 80%", () =>
            {
                var state = Start(combat, actors, equipment, settings);
                Equal(0.8, combat.HitChance(state, Weapon("wpn_ak74")), "0.8 × 1.0 × 1.0");
            });

            Check("меткость: вблизи растёт и упирается в 100%", () =>
            {
                var state = combat.Start(
                    actors.Get("actor_player"), new Equipment("player", settings.QuickSlots),
                    actors.Get("enemy_bandit"), DistanceZone.Melee);
                Equal(1.0, combat.HitChance(state, Weapon("wpn_ak74")), "0.8 × 1.4");
            });

            Check("меткость: вдали падает", () =>
            {
                var state = combat.Start(
                    actors.Get("actor_player"), new Equipment("player", settings.QuickSlots),
                    actors.Get("enemy_bandit"), DistanceZone.Far);
                Equal(0.64, System.Math.Round(combat.HitChance(state, Weapon("wpn_ak74")), 6), "0.8 × 0.8");
                Equal(0.512, System.Math.Round(combat.HitChance(state, Weapon("wpn_pm")), 6), "0.8 × 0.8 × 0.8");
            });

            Check("меткость: снайперка игнорирует дистанцию", () =>
            {
                var state = combat.Start(
                    actors.Get("actor_player"), new Equipment("player", settings.QuickSlots),
                    actors.Get("enemy_bandit"), DistanceZone.Far);
                Equal(1.0, combat.HitChance(state, Weapon("wpn_sv98")), "0.8 × 1.5");
            });

            Check("бой: урон вычитает защиту цели", () =>
            {
                var state = Start(combat, actors, equipment, settings);
                True(combat.TryAttack(state, Weapon("wpn_sv98"), out var result), "атака");
                True(result.Hit, "попадание гарантировано");
                Equal(28, result.Damage, "30 − 2");
                Equal(17.0, state.Enemy.Health, "здоровье бандита");
            });

            Check("бой: атака тратит ОД и изнашивает оружие", () =>
            {
                var state = Start(combat, actors, equipment, settings);
                var weapon = Weapon("wpn_ak74");
                combat.TryAttack(state, weapon, out _);
                Equal(7, state.Player.Ap, "ОД");
                Equal(99, weapon.Durability!.Value, "прочность оружия");
            });

            Check("бой: промах не наносит урон, но тратит выстрел", () =>
            {
                var service = Custom(services, new StubRandom(0.99));
                var state = service.Start(
                    actors.Get("actor_player"), new Equipment("player", settings.QuickSlots),
                    actors.Get("enemy_bandit"), DistanceZone.Mid);
                var weapon = Weapon("wpn_ak74");
                True(service.TryAttack(state, weapon, out var result), "атака");
                False(result.Hit, "промах");
                Equal(0, result.Damage, "урон");
                Equal(45.0, state.Enemy.Health, "здоровье не изменилось");
                Equal(99, weapon.Durability!.Value, "оружие всё равно износилось");
                Equal(7, state.Player.Ap, "ОД потрачены");
            });

            Check("бой: сломанное оружие не стреляет", () =>
            {
                var state = Start(combat, actors, equipment, settings);
                var broken = new ItemInstance("w", "wpn_ak74", 1, 0, 100);
                False(combat.TryAttack(state, broken, out _), "атака");
            });

            Check("бой: без ОД атака недоступна", () =>
            {
                var state = Start(combat, actors, equipment, settings);
                var pistol = Weapon("wpn_pm");
                for (var i = 0; i < state.Player.MaxAp; i++)
                {
                    combat.TryAttack(state, pistol, out _);
                }

                Equal(0, state.Player.Ap, "ОД кончились");
                False(combat.TryAttack(state, pistol, out _), "атака без ОД");
            });

            Check("бой: первая смена зоны бесплатна, вторая стоит ОД", () =>
            {
                var state = Start(combat, actors, equipment, settings);
                True(combat.TryMove(state, 1), "бесплатный сдвиг");
                Equal(DistanceZone.Far, state.Distance, "дистанция");
                Equal(8, state.Player.Ap, "ОД не тронуты");
                True(combat.TryMove(state, -1), "платный сдвиг");
                Equal(7, state.Player.Ap, "цена сдвига");
            });

            Check("бой: смена хода восстанавливает ОД и считает раунды", () =>
            {
                var state = Start(combat, actors, equipment, settings);
                combat.TryAttack(state, Weapon("wpn_ak74"), out _);
                combat.EndTurn(state);
                True(ReferenceEquals(state.Current, state.Enemy), "ходит враг");
                Equal(1, state.Round, "раунд");
                combat.EndTurn(state);
                True(ReferenceEquals(state.Current, state.Player), "снова игрок");
                Equal(8, state.Player.Ap, "ОД восстановлены");
                Equal(2, state.Round, "новый раунд");
            });

            Check("бой: попадание изнашивает броню и шлем", () =>
            {
                var state = Start(combat, actors, equipment, settings);
                equipment.TryEquip(state.Player.Equipment!, new ItemInstance("a", "itm_armor_jacket", 1, 40, 40), EquipmentSlot.Armor, out _);
                equipment.TryEquip(state.Player.Equipment!, new ItemInstance("h", "itm_helmet_light", 1, 25, 25), EquipmentSlot.Helmet, out _);
                combat.EndTurn(state);
                True(combat.TryAttack(state, Weapon("wpn_sv98"), out var result), "атака врага");
                True(result.Hit, "попадание");
                Equal(22, result.Damage, "30 − (6 + 2)");
                Equal(38, state.Player.Equipment!.Get(EquipmentSlot.Armor)!.Durability!.Value, "броня");
                Equal(22, state.Player.Equipment.Get(EquipmentSlot.Helmet)!.Durability!.Value, "шлем");
            });

            Check("бой: смерть цели завершает бой", () =>
            {
                var state = Start(combat, actors, equipment, settings);
                var killed = false;
                for (var i = 0; i < 5 && !killed; i++)
                {
                    combat.TryAttack(state, Weapon("wpn_sv98"), out var result);
                    killed = result.Killed;
                }

                True(killed, "бандит убит");
                True(state.IsFinished, "бой завершён");
            });

            Check("бой: выстрел тратит патрон из магазина", () =>
            {
                var state = Start(combat, actors, equipment, settings);
                var weapon = Weapon("wpn_ak74");
                combat.TryAttack(state, weapon, out _);
                Equal(29, weapon.Magazine!.Value, "патронов в магазине");
            });

            Check("перезарядка берёт патроны из неполного стака и тратит ОД", () =>
            {
                var state = Start(combat, actors, equipment, settings);
                var inventory = services.Resolve<IInventoryService>();
                var grid = new GridContainer("ammo", 4, 4);
                inventory.TryAdd(grid, "ammo_545", 3, out _);
                inventory.TryAdd(grid, "ammo_545", 120, out _);
                var weapon = Weapon("wpn_ak74");
                combat.TryAttack(state, weapon, out _);
                Equal(29, weapon.Magazine!.Value, "после выстрела");
                True(combat.TryReload(state, weapon, grid), "перезарядка");
                Equal(30, weapon.Magazine!.Value, "магазин полон");
                Equal(6, state.Player.Ap, "ОД за перезарядку");
                Equal(122, inventory.Count(grid, "ammo_545"), "патроны взяты из неполного стака");
            });

            Check("перезарядка без патронов не выполняется", () =>
            {
                var state = Start(combat, actors, equipment, settings);
                var grid = new GridContainer("ammo", 4, 4);
                var weapon = Weapon("wpn_ak74");
                combat.TryAttack(state, weapon, out _);
                False(combat.TryReload(state, weapon, grid), "перезарядка");
                Equal(7, state.Player.Ap, "ОД не потрачены");
            });

            Check("перезарядка врага: пустой магазин заряжается полностью за ОД", () =>
            {
                var shooterActor = new ActorTemplate(
                    "shooter", "enemy", "n",
                    new CombatBlockTemplate(1, 1, 50, 12, new Dictionary<string, int>(), "aggressive", 0.8, 0));
                var state = combat.Start(
                    actors.Get("actor_player"), new Equipment("player", settings.QuickSlots),
                    shooterActor, DistanceZone.Mid);
                combat.EndTurn(state);
                var pistol = Weapon("wpn_pm");
                for (var i = 0; i < 8; i++)
                {
                    combat.TryAttack(state, pistol, out _);
                }

                Equal(0, pistol.Magazine!.Value, "магазин пуст");
                Equal(4, state.Enemy.Ap, "ОД после восьми выстрелов");
                True(combat.TryAttack(state, pistol, out _), "атака после автоперезарядки");
                Equal(7, pistol.Magazine!.Value, "полный магазин минус выстрел");
                Equal(2, state.Enemy.Ap, "перезарядка и атака стоили 2 ОД");
            });

            Check("перезарядка игрока: пустой магазин не заряжается сам", () =>
            {
                var strongActor = new ActorTemplate(
                    "strong", "npc", "n",
                    new CombatBlockTemplate(1, 1, 100, 12, new Dictionary<string, int>(), "passive", 0.8, 0));
                var state = combat.Start(
                    strongActor, new Equipment("player", settings.QuickSlots),
                    actors.Get("enemy_bandit"), DistanceZone.Mid);
                var pistol = Weapon("wpn_pm");
                for (var i = 0; i < 8; i++)
                {
                    combat.TryAttack(state, pistol, out _);
                }

                False(combat.TryAttack(state, pistol, out _), "нужна ручная перезарядка");
                Equal(0, pistol.Magazine!.Value, "магазин пуст");
            });

            Check("меткость: отношение уровней больше 5 даёт 98%", () =>
            {
                var boss = new ActorTemplate(
                    "boss",
                    "enemy",
                    "n",
                    new CombatBlockTemplate(1, 6, 10, 5, new Dictionary<string, int>(), "aggressive", 0.8, 0));
                var state = combat.Start(
                    actors.Get("actor_player"), new Equipment("player", settings.QuickSlots),
                    boss, DistanceZone.Mid);
                combat.EndTurn(state);
                Equal(0.98, combat.HitChance(state, Weapon("wpn_ak74")), "уровень 6 против 1");
            });

            Check("меткость: отношение уровней ровно 5 не даёт 98%", () =>
            {
                var boss = new ActorTemplate(
                    "boss",
                    "enemy",
                    "n",
                    new CombatBlockTemplate(1, 5, 10, 5, new Dictionary<string, int>(), "aggressive", 0.8, 0));
                var state = combat.Start(
                    actors.Get("actor_player"), new Equipment("player", settings.QuickSlots),
                    boss, DistanceZone.Mid);
                combat.EndTurn(state);
                Equal(0.8, System.Math.Round(combat.HitChance(state, Weapon("wpn_ak74")), 6), "порог не превышен");
            });

            Check("меткость: промах повышает шанс на 5% базового", () =>
            {
                var service = Custom(services, new StubRandom(0.99));
                var state = service.Start(
                    actors.Get("actor_player"), new Equipment("player", settings.QuickSlots),
                    actors.Get("enemy_bandit"), DistanceZone.Mid);
                var weapon = Weapon("wpn_ak74");
                service.TryAttack(state, weapon, out var first);
                False(first.Hit, "промах");
                Equal(0.84, System.Math.Round(service.HitChance(state, weapon), 6), "0.8 + 0.04");
            });

            Check("DI: боевой сервис доступен", () =>
            {
                True(services.IsRegistered<ICombatService>(), "зарегистрирован");
            });
        }

        private static CombatService Custom(ServiceContainer services, ICombatRandom random)
        {
            return new CombatService(
                services.Resolve<IItemRepository>(),
                services.Resolve<IDamageTypeRepository>(),
                services.Resolve<IEquipmentService>(),
                services.Resolve<IArmorWearService>(),
                services.Resolve<IFormulaRepository>(),
                CombatSettingsTemplate.Default,
                BalanceTemplate.Empty,
                random,
                services.Resolve<IInventoryService>());
        }

        private static ItemInstance Weapon(string id) => new ItemInstance("w", id, 1, 100, 100);

        private static CombatState Start(
            ICombatService combat,
            IActorRepository actors,
            IEquipmentService equipment,
            IEquipmentSettings settings)
        {
            var state = new Equipment("player", settings.QuickSlots);
            return combat.Start(
                actors.Get("actor_player"),
                state,
                actors.Get("enemy_bandit"),
                DistanceZone.Mid);
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
