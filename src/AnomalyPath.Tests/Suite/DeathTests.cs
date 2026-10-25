#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Death;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.Services;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.Templates;
using AnomalyPath.Core.World;
using InventoryEquipment = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Tests
{
    /// <summary>Смерть: полная потеря инвентаря и независимая страховка каждого надетого предмета.</summary>
    public sealed class DeathTests : TestBase
    {
        private readonly string _dataRoot;
        private readonly IProgressionService _progression;

        /// <summary>Создаёт категорию тестов смерти.</summary>
        /// <param name="dataRoot">Корень данных проекта.</param>
        public DeathTests(string dataRoot)
        {
            _dataRoot = dataRoot;
            _progression = Bootstrap.Build(dataRoot).Resolve<IProgressionService>();
        }

        /// <inheritdoc />
        public override void Run()
        {
            RegistrationTests();
            InventoryTests();
            InsuranceTests();
            PermadeathTests();
            ExperienceTests();
            EdgeCaseTests();
        }

        private void ExperienceTests()
        {
            Check("смерть: теряет 40% опыта текущего уровня", () =>
            {
                var fixture = Fixture(chance: 0, random: new SequenceRandom(), level: 3, xp: 270, xpLoss: 0.4);
                var result = fixture.Service.Apply(fixture.State);

                Equal(20, result.XpLost, "потеряно опыта");
                Equal(250, fixture.Player.Xp, "остаток опыта");
                Equal(3, fixture.Player.Level, "уровень не понизился");
            });

            Check("смерть: 100% потери обнуляют прогресс, но не уровень", () =>
            {
                var fixture = Fixture(chance: 0, random: new SequenceRandom(), level: 3, xp: 270, xpLoss: 1);
                var result = fixture.Service.Apply(fixture.State);

                Equal(50, result.XpLost, "потеряно опыта");
                Equal(220, fixture.Player.Xp, "опыт на начале уровня");
                Equal(3, fixture.Player.Level, "уровень");
            });

            Check("смерть: без настройки опыт не теряется", () =>
            {
                var fixture = Fixture(chance: 0, random: new SequenceRandom(), level: 3, xp: 270);
                var result = fixture.Service.Apply(fixture.State);

                Equal(0, result.XpLost, "потеряно опыта");
                Equal(270, fixture.Player.Xp, "опыт не изменился");
            });
        }

        private void RegistrationTests()
        {
            Check("смерть: сервис зарегистрирован", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                True(services.IsRegistered<IDeathService>(), "IDeathService");
                True(services.IsRegistered<IDeathRandom>(), "IDeathRandom");
            });
        }

        private void InventoryTests()
        {
            Check("смерть: инвентарь теряется полностью, неутрачиваемое остаётся", () =>
            {
                var fixture = Fixture(chance: 0, random: new SequenceRandom());
                var result = fixture.Service.Apply(fixture.State);

                Equal(1, fixture.Player.Inventory.Items.Count, "в инвентаре только неутрачиваемое");
                Equal("mat_unlosable", fixture.Player.Inventory.Items[0].ItemId, "остаток");
                True(ContainsId(result.Lost, "mat_losable"), "обычный предмет потерян");
                True(ContainsId(result.Lost, "mat_overflow"), "переполнение потеряно");
                True(ContainsId(result.Kept, "mat_unlosable"), "неутрачиваемый остался");
            });

            Check("смерть: страховка не распространяется на инвентарь", () =>
            {
                var fixture = Fixture(chance: 1, random: new SequenceRandom(0.5, 0.5, 0.5));
                fixture.Service.Apply(fixture.State);

                False(ContainsItem(fixture.Player.Inventory.Items, "mat_losable"), "предмет из сетки потерян");
                False(ContainsItem(fixture.Player.Inventory.Items, "mat_overflow"), "переполнение потеряно");
            });
        }

        private void InsuranceTests()
        {
            Check("смерть: коэффициент 1 сохраняет все надетые вещи", () =>
            {
                var fixture = Fixture(chance: 1, random: new SequenceRandom(0.5, 0.5, 0.5, 0.5));
                var result = fixture.Service.Apply(fixture.State);

                True(fixture.Player.Equipment.Get(EquipmentSlot.Main) != null, "основное оружие осталось");
                True(fixture.Player.Equipment.Get(EquipmentSlot.Secondary) != null, "дополнительное осталось");
                True(fixture.Player.Equipment.GetQuick(0) != null, "быстрый слот остался");
                True(fixture.Player.Equipment.Artifacts[0] != null, "артефакт остался");
                Equal(4, result.Insured.Count, "страховка спасла четыре предмета");
            });

            Check("смерть: каждый надетый предмет страхуется отдельным броском", () =>
            {
                var random = new SequenceRandom(0.1, 0.9, 0.2, 0.8);
                var fixture = Fixture(chance: 0.5, random: random);
                var result = fixture.Service.Apply(fixture.State);

                Equal(4, random.Calls, "ровно один бросок на надетый предмет");
                Equal("worn_main", fixture.Player.Equipment.Get(EquipmentSlot.Main)!.ItemId, "основное спасено");
                True(fixture.Player.Equipment.Get(EquipmentSlot.Secondary) == null, "дополнительное потеряно");
                Equal("worn_quick", fixture.Player.Equipment.GetQuick(0)!.ItemId, "быстрый слот спасён");
                True(fixture.Player.Equipment.Artifacts[0] == null, "артефакт потерян");
                Equal(2, result.Insured.Count, "спасено страховкой");
                True(ContainsId(result.Lost, "worn_secondary"), "потеряно дополнительное");
                True(ContainsId(result.Lost, "worn_artifact"), "потерян артефакт");
            });

            Check("смерть: неутрачиваемый надетый предмет не бросает страховку", () =>
            {
                var random = new SequenceRandom(0.1, 0.1, 0.1, 0.1);
                var fixture = Fixture(chance: 0, random: random);
                var result = fixture.Service.Apply(fixture.State);

                Equal(4, random.Calls, "броски только для утрачиваемых надетых предметов");
                True(fixture.Player.Equipment.Get(EquipmentSlot.Backpack) != null, "неутрачиваемое осталось");
                False(ContainsId(result.Insured, "worn_unlosable"), "не считается страховкой");
            });
        }

        private void PermadeathTests()
        {
            Check("смерть: режим полной потери забирает всё, кроме неутрачиваемого", () =>
            {
                var random = new SequenceRandom(0.1, 0.1, 0.1);
                var fixture = Fixture(chance: 1, random: random, permadeath: true);
                var result = fixture.Service.Apply(fixture.State);

                True(result.Permadeath, "режим полной потери");
                Equal(0, random.Calls, "страховка не бросается");
                True(fixture.Player.Equipment.Get(EquipmentSlot.Main) == null, "оружие потеряно");
                True(fixture.Player.Equipment.Get(EquipmentSlot.Secondary) == null, "дополнительное потеряно");
                True(fixture.Player.Equipment.Get(EquipmentSlot.Backpack) != null, "неутрачиваемое осталось");
                True(fixture.Player.Equipment.GetQuick(0) == null, "быстрый слот потерян");
                True(fixture.Player.Equipment.Artifacts[0] == null, "артефакт потерян");
                Equal(1, fixture.Player.Inventory.Items.Count, "в инвентаре только неутрачиваемое");
                Equal(0, result.Insured.Count, "страховки нет");
            });
        }

        private void EdgeCaseTests()
        {
            Check("смерть: состояние обязательно", () =>
            {
                var fixture = Fixture(chance: 0, random: new SequenceRandom());
                Throws<ArgumentNullException>(() => fixture.Service.Apply(null!), "Apply");
            });

            Check("смерть: без игрока результат пуст", () =>
            {
                var fixture = Fixture(chance: 0, random: new SequenceRandom());
                var state = new GameState(fixture.State.World, null);
                var result = fixture.Service.Apply(state);

                Equal(0, result.Lost.Count, "потерь нет");
                Equal(0, result.Kept.Count, "остатков нет");
                Equal(0, result.Insured.Count, "страховки нет");
            });

            Check("смерть: неизвестный предмет теряется без исключения", () =>
            {
                var fixture = Fixture(chance: 1, random: new SequenceRandom());
                fixture.Player.Inventory.AddOverflow(new ItemInstance("inst_unknown", "item_missing", 1));
                var result = fixture.Service.Apply(fixture.State);

                False(ContainsItem(fixture.Player.Inventory.Items, "item_missing"), "неизвестный потерян");
                True(ContainsId(result.Lost, "item_missing"), "в списке потерь");
            });
        }

        private FixtureState Fixture(
            double chance,
            IDeathRandom random,
            bool permadeath = false,
            int level = 1,
            int xp = 0,
            double xpLoss = 0)
        {
            var items = new ItemRepository(new[]
            {
                Template(ItemJson("mat_losable", unlosable: false)),
                Template(ItemJson("mat_overflow", unlosable: false)),
                Template(ItemJson("mat_unlosable", unlosable: true)),
                Template(ItemJson("worn_main", unlosable: false)),
                Template(ItemJson("worn_secondary", unlosable: false)),
                Template(ItemJson("worn_unlosable", unlosable: true)),
                Template(ItemJson("worn_quick", unlosable: false)),
                Template(ItemJson("worn_artifact", unlosable: false))
            });

            var grid = new GridContainer("player", 8, 4);
            grid.Place(new ItemInstance("inst_losable", "mat_losable", 1), items.Get("mat_losable"), 0, 0, false);
            grid.Place(new ItemInstance("inst_unlosable", "mat_unlosable", 1), items.Get("mat_unlosable"), 1, 0, false);
            grid.AddOverflow(new ItemInstance("inst_overflow", "mat_overflow", 1));

            var equipment = new InventoryEquipment("player", 1);
            equipment.Set(EquipmentSlot.Main, new ItemInstance("inst_main", "worn_main", 1));
            equipment.Set(EquipmentSlot.Secondary, new ItemInstance("inst_secondary", "worn_secondary", 1));
            equipment.Set(EquipmentSlot.Backpack, new ItemInstance("inst_unlosable_worn", "worn_unlosable", 1));
            equipment.SetQuick(0, new ItemInstance("inst_quick", "worn_quick", 1));
            equipment.SetArtifact(0, new ItemInstance("inst_artifact", "worn_artifact", 1));

            var player = new PlayerState(
                "player",
                level,
                xp,
                10,
                10,
                grid,
                equipment,
                Array.Empty<string>(),
                new NeedsState(10, 10, 10, 10));
            return new FixtureState(
                player,
                new GameState(
                    WorldStateForTests(),
                    player,
                    new NewGameSettings(chance, 1, permadeath: permadeath, xpLossChance: xpLoss)),
                new DeathService(items, random, _progression));
        }

        private static WorldState WorldStateForTests()
        {
            var map = new WorldMap(
                1,
                new List<LocationInstance>
                {
                    new LocationInstance("loc_hub", "loc_hub", 1, "bg", LocationSubtype.Hub, 1, 1)
                },
                new List<WorldEdge>(),
                "loc_hub",
                10,
                6,
                22);
            return WorldState.Start(map);
        }

        private static string ItemJson(string id, bool unlosable) =>
            $"{{\"kind\":\"items\",\"id\":\"{id}\",\"nameKey\":\"n\",\"category\":\"material\"," +
            "\"footprint\":[1,1],\"maxStack\":1,\"tier\":1,\"icon\":\"i\"" +
            (unlosable ? ",\"unlosable\":true" : string.Empty) + "}";

        private static bool ContainsId(IReadOnlyList<ItemInstance> items, string itemId) =>
            ContainsItem(items, itemId);

        private static bool ContainsItem(IReadOnlyList<ItemInstance> items, string itemId)
        {
            foreach (var item in items)
            {
                if (item.ItemId == itemId)
                {
                    return true;
                }
            }

            return false;
        }

        private sealed class SequenceRandom : IDeathRandom
        {
            private readonly Queue<double> _values;

            internal SequenceRandom(params double[] values)
            {
                _values = new Queue<double>(values);
            }

            internal int Calls { get; private set; }

            public double NextDouble()
            {
                Calls++;
                return _values.Count > 0 ? _values.Dequeue() : 0.99;
            }
        }

        private sealed class FixtureState
        {
            internal FixtureState(PlayerState player, GameState state, DeathService service)
            {
                Player = player;
                State = state;
                Service = service;
            }

            internal PlayerState Player { get; }

            internal GameState State { get; }

            internal DeathService Service { get; }
        }
    }
}
