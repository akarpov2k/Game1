#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Di;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.Services;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.Trade;
using AnomalyPath.Core.World;

namespace AnomalyPath.Tests
{
    /// <summary>Торговцы: данные, привязка к хабу, условия, запас, покупка и продажа.</summary>
    public sealed class TraderTests : TestBase
    {
        private const string TraderId = "trd_hub_smuggler";

        private readonly string _dataRoot;

        /// <summary>Создаёт категорию тестов торговцев.</summary>
        /// <param name="dataRoot">Корень данных.</param>
        public TraderTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        /// <inheritdoc />
        public override void Run()
        {
            var services = Bootstrap.Build(_dataRoot);
            var trades = services.Resolve<ITradeService>();
            var actors = services.Resolve<IActorRepository>();
            var locations = services.Resolve<ILocationRepository>();

            Check("торговцы: привязаны к хабу в данных локации", () =>
            {
                var hub = locations.Get("loc_hub_settlement");
                Equal(2, hub.Traders.Count, "торговцев в хабе");
                Equal(TraderId, hub.Traders[0], "первый торговец");
                Equal("trd_hub_medic", hub.Traders[1], "второй торговец");
                Equal(ActorType.Trader, actors.Get(TraderId).Type, "тип торговца");
                Equal("portraits/trd_hub_smuggler", actors.Get(TraderId).Portrait, "портрет");
                Equal(PortraitMode.Waist, actors.Get("trd_hub_medic").PortraitMode, "кадрирование портрета");
            });

            Check("торговцы: товар скрыт условиями", () =>
            {
                var session = NewSession(services);
                var offers = trades.GetOffers(session, TraderId);
                True(Contains(offers, "wpn_pm"), "пистолет в продаже");
                False(Contains(offers, "wpn_akm"), "автомат за квестом скрыт");
                False(Contains(offers, "wpn_sv98"), "снайперка за открытием скрыта");
            });

            Check("торговцы: выполненный квест открывает товар", () =>
            {
                var session = NewSession(services, quests: new[] { "quest_caravan_deal" });
                var offers = trades.GetOffers(session, TraderId);
                True(Contains(offers, "wpn_akm"), "автомат открыт квестом");
                False(Contains(offers, "wpn_sv98"), "снайперка всё ещё скрыта");
            });

            Check("торговцы: открытие разблокирует товар", () =>
            {
                var session = NewSession(services, unlocks: new[] { "unlock_smuggler_premium" });
                var offers = trades.GetOffers(session, TraderId);
                True(Contains(offers, "wpn_sv98"), "снайперка открыта");
            });

            Check("торговцы: цена учитывает множитель торговца", () =>
            {
                var session = NewSession(services);
                var offers = trades.GetOffers(session, TraderId);
                var pistol = Find(offers, "wpn_pm");
                Equal(275, pistol!.UnitPrice, "250 × 1.1");
            });

            Check("торговцы: покупка списывает деньги и запас", () =>
            {
                var session = NewSession(services, money: 1000);
                var before = Find(trades.GetOffers(session, TraderId), "ammo_9x18")!;
                var result = trades.Buy(session, TraderId, "ammo_9x18", 5);
                True(result.Succeeded, "покупка");
                Equal(5, result.Count, "куплено");
                Equal(30, result.Price, "6 × 5");
                Equal(970, session.State.Player!.Money, "деньги");
                Equal(5, CountItem(session.State.Player, "ammo_9x18"), "патроны в инвентаре");
                Equal(before.Stock - 5, Find(trades.GetOffers(session, TraderId), "ammo_9x18")!.Stock, "запас");
            });

            Check("торговцы: купленное оружие разряжено", () =>
            {
                var session = NewSession(services, money: 1000);
                var result = trades.Buy(session, TraderId, "wpn_pm");
                True(result.Succeeded, "покупка");
                var weapon = FindItem(session.State.Player!.Inventory, "wpn_pm");
                True(weapon != null, "оружие в инвентаре");
                Equal(0, weapon!.MagazineRounds, "магазин пуст");
                Equal(weapon.DurabilityMax, weapon.Durability, "прочность полная");
            });

            Check("торговцы: не хватает денег — товар и деньги не меняются", () =>
            {
                var session = NewSession(services, money: 0);
                var before = Find(trades.GetOffers(session, TraderId), "ammo_9x18")!;
                var result = trades.Buy(session, TraderId, "ammo_9x18");
                Equal(TradeStatus.NotEnoughMoney, result.Status, "статус");
                Equal(0, session.State.Player!.Money, "деньги");
                Equal(before.Stock, Find(trades.GetOffers(session, TraderId), "ammo_9x18")!.Stock, "запас");
            });

            Check("торговцы: нет места — покупка отменяется", () =>
            {
                var session = NewSession(services, money: 1000, width: 1, rows: 1);
                var result = trades.Buy(session, TraderId, "wpn_pm");
                Equal(TradeStatus.NoRoom, result.Status, "статус");
                Equal(1000, session.State.Player!.Money, "деньги не списаны");
                Equal(0, CountItem(session.State.Player, "wpn_pm"), "оружия нет");
            });

            Check("торговцы: запас пополняется через период", () =>
            {
                var session = NewSession(services, money: 1000);
                True(trades.Buy(session, TraderId, "wpn_pm").Succeeded, "первая покупка");
                Equal(2, Find(trades.GetOffers(session, TraderId), "wpn_pm")!.Stock, "остаток");
                session.State.World.Clock.Advance(12);
                Equal(3, Find(trades.GetOffers(session, TraderId), "wpn_pm")!.Stock, "после пополнения");
            });

            Check("торговцы: продажа добавляет деньги и убирает предмет", () =>
            {
                var session = NewSession(services, money: 1000);
                var inventory = services.Resolve<IInventoryService>();
                inventory.TryAdd(session.State.Player!.Inventory, "wpn_pm", 1, out _);
                var sell = trades.GetSellOffers(session, TraderId);
                var offer = FindSell(sell, "wpn_pm");
                True(offer != null, "предмет можно продать");
                var result = trades.Sell(session, TraderId, offer!.InstanceId);
                True(result.Succeeded, "продажа");
                Equal(50, result.Price, "100 × 0.5");
                Equal(1050, session.State.Player.Money, "деньги");
                Equal(0, CountItem(session.State.Player, "wpn_pm"), "предмет убран");
            });

            Check("торговцы: вне хаба торговец недоступен", () =>
            {
                var session = NewSession(services);
                MoveToWild(session);
                False(trades.IsAvailable(session, TraderId), "доступность");
                var result = trades.Buy(session, TraderId, "wpn_pm");
                Equal(TradeStatus.WrongLocation, result.Status, "статус");
            });

            Check("торговцы: неизвестный id и не-торговец", () =>
            {
                var session = NewSession(services);
                Equal(TradeStatus.TraderNotFound, trades.Buy(session, "missing", "wpn_pm").Status, "неизвестный");
                Equal(TradeStatus.NotTrader, trades.Buy(session, "actor_player", "wpn_pm").Status, "не торговец");
            });

            Check("торговцы: сейв сохраняет запас и время пополнения", () =>
            {
                var session = NewSession(services, money: 1000);
                True(trades.Buy(session, TraderId, "wpn_pm").Succeeded, "покупка");
                var state = session.State.GetOrCreateTrader(TraderId);
                Equal(2, state.Count("wpn_pm"), "остаток до сейва");
                Equal(10.0, state.LastRestockAt, "время пополнения");
            });
        }

        private static GameSession NewSession(
            ServiceContainer services,
            int money = 1000,
            int width = 8,
            int rows = 4,
            IReadOnlyList<string>? quests = null,
            IReadOnlyList<string>? unlocks = null)
        {
            var generator = services.Resolve<IWorldGenerator>();
            var world = WorldState.Start(generator.Generate(42));
            var grid = new GridContainer("player", width, rows);
            var equipment = new Equipment("player", 4);
            var player = new PlayerState(
                "player",
                1,
                0,
                100,
                8,
                grid,
                equipment,
                Array.Empty<string>(),
                new NeedsState(100, 100, 100, 100),
                "Тест",
                money,
                quests,
                unlocks);
            return new GameSession(new GameState(world, player));
        }

        private static bool Contains(IReadOnlyList<TraderOffer> offers, string itemId) =>
            Find(offers, itemId) != null;

        private static TraderOffer? Find(IReadOnlyList<TraderOffer> offers, string itemId)
        {
            foreach (var offer in offers)
            {
                if (offer.ItemId == itemId)
                {
                    return offer;
                }
            }

            return null;
        }

        private static TraderSellOffer? FindSell(IReadOnlyList<TraderSellOffer> offers, string itemId)
        {
            foreach (var offer in offers)
            {
                if (offer.ItemId == itemId)
                {
                    return offer;
                }
            }

            return null;
        }

        private static ItemInstance? FindItem(GridContainer grid, string itemId)
        {
            foreach (var item in grid.Items)
            {
                if (item.ItemId == itemId)
                {
                    return item;
                }
            }

            return null;
        }

        private static int CountItem(PlayerState player, string itemId) =>
            RequirementsTemplate.CountItem(player, itemId);

        private static void MoveToWild(GameSession session)
        {
            var map = session.State.World.Map;
            foreach (var location in map.Locations)
            {
                if (location.NodeType != LocationNodeType.Hub)
                {
                    session.State.World.MoveTo(location.InstanceId);
                    return;
                }
            }
        }
    }
}
