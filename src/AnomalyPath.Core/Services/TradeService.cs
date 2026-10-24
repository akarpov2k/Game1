#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.Templates;
using AnomalyPath.Core.Trade;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Services
{
    /// <summary>
    /// Торговля: доступность торговца в хабе, скрытые условиями товары, запас с пополнением
    /// по часам мира, покупка и продажа по ценам предмета и множителям торговца.
    /// </summary>
    public sealed class TradeService : ITradeService
    {
        private readonly IActorRepository _actors;
        private readonly ILocationRepository _locations;
        private readonly IItemRepository _items;
        private readonly IInventoryService _inventory;
        private readonly IEconomySettings _economy;
        private readonly IGameRulesRepository _rules;

        /// <summary>Создаёт сервис торговли.</summary>
        /// <param name="actors">Актёры.</param>
        /// <param name="locations">Шаблоны локаций: привязка торговцев к хабам.</param>
        /// <param name="items">Предметы и их цены.</param>
        /// <param name="inventory">Инвентарь: проверка места и выдача покупки.</param>
        /// <param name="economy">Округление и нижняя граница цены.</param>
        /// <param name="rules">Правила: усложнение торговли по умолчанию.</param>
        /// <exception cref="ArgumentNullException">Зависимость не задана.</exception>
        public TradeService(
            IActorRepository actors,
            ILocationRepository locations,
            IItemRepository items,
            IInventoryService inventory,
            IEconomySettings economy,
            IGameRulesRepository rules)
        {
            _actors = actors ?? throw new ArgumentNullException(nameof(actors));
            _locations = locations ?? throw new ArgumentNullException(nameof(locations));
            _items = items ?? throw new ArgumentNullException(nameof(items));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        /// <inheritdoc />
        public bool IsAvailable(GameSession session, string actorId)
        {
            if (session == null || string.IsNullOrWhiteSpace(actorId) || !_actors.Contains(actorId))
            {
                return false;
            }

            var actor = _actors.Get(actorId);
            if (actor.Type != ActorType.Trader || actor.Trader == null)
            {
                return false;
            }

            if (!actor.Trader.Requirements.IsMet(session.State.Player))
            {
                return false;
            }

            var location = _locations.Get(session.State.World.CurrentLocation.TemplateId);
            if (location.LocationKind != LocationKind.Hub)
            {
                return false;
            }

            return Contains(location.Traders, actorId);
        }

        /// <inheritdoc />
        public IReadOnlyList<TraderOffer> GetOffers(GameSession session, string actorId)
        {
            var result = new List<TraderOffer>();
            if (!IsAvailable(session, actorId))
            {
                return result;
            }

            var actor = _actors.Get(actorId);
            var block = actor.Trader!;
            var player = session.State.Player;
            if (player == null)
            {
                return result;
            }

            var state = EnsureRestock(session, actor);
            foreach (var offer in block.Assortment)
            {
                if (!offer.Requirements.IsMet(player))
                {
                    continue;
                }

                var stock = state.Count(offer.ItemId);
                if (stock <= 0)
                {
                    continue;
                }

                if (!TryUnitPrice(offer.ItemId, block.PlayerBuyModifier, isBuy: true, session, out var price))
                {
                    continue;
                }

                result.Add(new TraderOffer(offer.ItemId, price, stock));
            }

            return result;
        }

        /// <inheritdoc />
        public IReadOnlyList<TraderSellOffer> GetSellOffers(GameSession session, string actorId)
        {
            var result = new List<TraderSellOffer>();
            if (!IsAvailable(session, actorId))
            {
                return result;
            }

            var actor = _actors.Get(actorId);
            var block = actor.Trader!;
            var player = session.State.Player;
            if (player == null)
            {
                return result;
            }

            foreach (var item in player.Inventory.Items)
            {
                if (!TryUnitPrice(item.ItemId, block.PlayerSellModifier, isBuy: false, session, out var price))
                {
                    continue;
                }

                result.Add(new TraderSellOffer(item.InstanceId, item.ItemId, item.Stack, price));
            }

            return result;
        }

        /// <inheritdoc />
        public TradeResult Buy(GameSession session, string actorId, string itemId, int count = 1)
        {
            if (session == null)
            {
                return TradeResult.Error(TradeStatus.Unavailable);
            }

            if (count < 1)
            {
                return TradeResult.Error(TradeStatus.InvalidCount);
            }

            if (string.IsNullOrWhiteSpace(itemId))
            {
                return TradeResult.Error(TradeStatus.OfferNotFound);
            }

            if (!IsAvailable(session, actorId))
            {
                return TradeResult.Error(StatusForUnavailable(session, actorId));
            }

            var actor = _actors.Get(actorId);
            var block = actor.Trader!;
            var player = session.State.Player;
            if (player == null)
            {
                return TradeResult.Error(TradeStatus.Unavailable);
            }

            TraderOfferTemplate? offer = null;
            foreach (var candidate in block.Assortment)
            {
                if (string.Equals(candidate.ItemId, itemId, StringComparison.Ordinal))
                {
                    offer = candidate;
                    break;
                }
            }

            if (offer == null || !offer.Requirements.IsMet(player))
            {
                return TradeResult.Error(TradeStatus.OfferNotFound);
            }

            var state = EnsureRestock(session, actor);
            if (state.Count(itemId) < count)
            {
                return TradeResult.Error(TradeStatus.NoStock);
            }

            if (!TryUnitPrice(itemId, block.PlayerBuyModifier, isBuy: true, session, out var unitPrice))
            {
                return TradeResult.Error(TradeStatus.OfferNotFound);
            }

            var total = checked(unitPrice * count);
            if (player.Money < total)
            {
                return TradeResult.Error(TradeStatus.NotEnoughMoney);
            }

            var template = _items.Get(itemId);
            var durability = NewDurability(template);
            var probe = new ItemInstance("trade-probe", itemId, count, durability);
            if (!_inventory.CanFit(player.Inventory, new[] { probe }))
            {
                return TradeResult.Error(TradeStatus.NoRoom);
            }

            if (!player.TrySpendMoney(total))
            {
                return TradeResult.Error(TradeStatus.NotEnoughMoney);
            }

            if (!state.TryTake(itemId, count))
            {
                player.AddMoney(total);
                return TradeResult.Error(TradeStatus.NoStock);
            }

            _inventory.TryAdd(player.Inventory, itemId, count, durability, null, 0, out var overflow);
            if (overflow.Count > 0)
            {
                foreach (var item in overflow)
                {
                    player.Inventory.Remove(item);
                }

                state.Add(itemId, count);
                player.AddMoney(total);
                return TradeResult.Error(TradeStatus.NoRoom);
            }

            return new TradeResult(TradeStatus.Ok, itemId, count, total);
        }

        /// <inheritdoc />
        public TradeResult Sell(GameSession session, string actorId, string instanceId)
        {
            if (session == null)
            {
                return TradeResult.Error(TradeStatus.Unavailable);
            }

            if (!IsAvailable(session, actorId))
            {
                return TradeResult.Error(StatusForUnavailable(session, actorId));
            }

            var actor = _actors.Get(actorId);
            var block = actor.Trader!;
            var player = session.State.Player;
            if (player == null)
            {
                return TradeResult.Error(TradeStatus.Unavailable);
            }

            var item = player.Inventory.Find(instanceId);
            if (item == null)
            {
                return TradeResult.Error(TradeStatus.ItemNotFound);
            }

            if (!TryUnitPrice(item.ItemId, block.PlayerSellModifier, isBuy: false, session, out var unitPrice))
            {
                return TradeResult.Error(TradeStatus.ItemNotSellable);
            }

            var total = checked(unitPrice * item.Stack);
            var count = item.Stack;
            var itemId = item.ItemId;
            player.Inventory.Remove(item);
            player.AddMoney(total);
            return new TradeResult(TradeStatus.Ok, itemId, count, total);
        }

        private TraderState EnsureRestock(GameSession session, ActorTemplate actor)
        {
            var block = actor.Trader!;
            var state = session.State.GetOrCreateTrader(actor.Id);
            var now = session.State.World.Clock.AbsoluteHours;
            if (state.LastRestockAt < 0)
            {
                Restock(state, block, now);
                return state;
            }

            if (now < state.LastRestockAt)
            {
                // Часы откатились назад (старый сейв или смена мира): просто начинаем отсчёт заново.
                state.MarkRestocked(now);
                return state;
            }

            if (now - state.LastRestockAt >= block.RestockHours)
            {
                Restock(state, block, now);
            }

            return state;
        }

        private static void Restock(TraderState state, TraderBlockTemplate block, double now)
        {
            foreach (var offer in block.Assortment)
            {
                state.SetStock(offer.ItemId, offer.Stock);
            }

            state.MarkRestocked(now);
        }

        private bool TryUnitPrice(
            string itemId,
            double modifier,
            bool isBuy,
            GameSession session,
            out int price)
        {
            price = 0;
            if (!_items.TryGet(itemId, out var template) || template.Price == null)
            {
                return false;
            }

            var basePrice = isBuy ? template.Price.Buy ?? template.Price.Base : template.Price.Sell ?? template.Price.Base;
            if (!basePrice.HasValue)
            {
                return false;
            }

            var value = basePrice.Value * modifier;
            if (isBuy)
            {
                var difficulty = session.State.Settings?.TradeDifficulty ?? _rules.Require().TradeDifficulty;
                value *= Math.Max(difficulty, 0);
            }

            price = Round(value);
            return true;
        }

        private int Round(double value)
        {
            var step = _economy.Rounding;
            var rounded = (int)(Math.Round(value / step, MidpointRounding.AwayFromZero) * step);
            return Math.Max(rounded, _economy.MinPrice);
        }

        private static int? NewDurability(ItemTemplate template)
        {
            if (template.Weapon != null)
            {
                return template.Weapon.DurabilityMax;
            }

            return template.ArmorDurabilityMax > 0 ? template.ArmorDurabilityMax : null;
        }

        private static bool Contains(IReadOnlyList<string> values, string id)
        {
            foreach (var value in values)
            {
                if (string.Equals(value, id, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private TradeStatus StatusForUnavailable(GameSession session, string actorId)
        {
            if (session == null)
            {
                return TradeStatus.Unavailable;
            }

            if (string.IsNullOrWhiteSpace(actorId) || !_actors.Contains(actorId))
            {
                return TradeStatus.TraderNotFound;
            }

            var actor = _actors.Get(actorId);
            if (actor.Type != ActorType.Trader || actor.Trader == null)
            {
                return TradeStatus.NotTrader;
            }

            var location = _locations.Get(session.State.World.CurrentLocation.TemplateId);
            if (location.LocationKind != LocationKind.Hub || !Contains(location.Traders, actorId))
            {
                return TradeStatus.WrongLocation;
            }

            return TradeStatus.Unavailable;
        }
    }
}
