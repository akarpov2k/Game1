#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Quests;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.Trade;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Services
{
    /// <summary>Сохранение мира и игрока в JSON-снимок и восстановление из него.</summary>
    public sealed class SaveService : ISaveService
    {
        private readonly ISaveStorage _storage;
        private readonly IItemRepository _items;

        /// <summary>Создаёт сервис поверх хранилища.</summary>
        /// <param name="storage">Хранилище слотов.</param>
        /// <param name="items">Репозиторий предметов для восстановления инвентаря.</param>
        /// <exception cref="ArgumentNullException">Зависимость не задана.</exception>
        public SaveService(ISaveStorage storage, IItemRepository items)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _items = items ?? throw new ArgumentNullException(nameof(items));
        }

        /// <inheritdoc />
        public void Save(
            GameState state,
            string slot,
            double playtime = 0,
            DateTimeOffset? createdAt = null,
            string? title = null)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var now = createdAt ?? DateTimeOffset.UtcNow;
            var world = state.World;
            var worldData = new WorldSaveData(
                world.Map.StartLocationId,
                world.CurrentLocationId,
                world.Stamina,
                world.StaminaMax,
                world.Map.StartingStamina,
                world.Clock.Day,
                world.Clock.Hours,
                world.Clock.DayStartHour,
                world.Clock.NightStartHour,
                WeatherCodec.ToCode(world.Weather),
                ToLocationData(world.Map),
                ToEdgeData(world.Map),
                state.Settings == null
                    ? null
                    : new NewGameSettingsSaveData(
                        state.Settings.InsuranceChance,
                        state.Settings.TradeDifficulty,
                        state.Settings.Permadeath),
                ToTraderData(state.Traders),
                ToQuestData(state.Quests),
                state.QuestSerial,
                world.LastHubId);
            var game = new SaveGame(
                SaveGame.CurrentSchemaVersion,
                SaveGame.CurrentSaveVersion,
                now.ToString("O", CultureInfo.InvariantCulture),
                playtime,
                world.Map.Seed,
                worldData,
                state.Player == null ? null : PlayerSaveMapper.ToSaveData(state.Player),
                title);
            _storage.Write(slot, JsonWriter.Write(game.ToJson()));
        }

        /// <inheritdoc />
        public GameState Load(string slot)
        {
            var text = _storage.Read(slot);
            var game = SaveGame.FromJson(JsonParser.Parse(text, slot), slot);
            var world = game.World;
            var locations = new List<LocationInstance>();
            foreach (var item in world.Locations)
            {
                var location = new LocationInstance(
                    item.InstanceId,
                    item.TemplateId,
                    item.Seed,
                    item.Background,
                    item.NodeType,
                    item.Tier,
                    item.Danger);
                if (item.Visited)
                {
                    location.MarkVisited();
                }

                if (item.Cleared)
                {
                    location.MarkCleared();
                }

                locations.Add(location);
            }

            var edges = new List<WorldEdge>();
            foreach (var item in world.Edges)
            {
                edges.Add(new WorldEdge(
                    item.EdgeId,
                    item.FromId,
                    item.ToId,
                    item.Via,
                    item.Length,
                    new TravelCost(item.Stamina, item.Hours),
                    item.OneWay,
                    item.Hidden));
            }

            var map = new WorldMap(
                game.Seed,
                locations,
                edges,
                world.StartLocationId,
                world.StartingStamina,
                world.DayStartHour,
                world.NightStartHour);
            var clock = new WorldClock(world.Day, world.Hours, world.DayStartHour, world.NightStartHour);
            var weather = WeatherCodec.FromCode(world.Weather, slot);
            var state = new WorldState(
                map,
                world.CurrentLocationId,
                world.Stamina,
                world.StaminaMax,
                clock,
                weather,
                world.LastHubId);
            var player = game.Player == null
                ? null
                : PlayerSaveMapper.FromSaveData(game.Player, _items, slot);
            var settings = game.World.Settings == null
                ? null
                : new NewGameSettings(
                    game.World.Settings.InsuranceChance,
                    game.World.Settings.TradeDifficulty,
                    permadeath: game.World.Settings.Permadeath);
            return new GameState(state, player, settings, LoadTraders(game.World.Traders), LoadQuests(game.World.Quests), game.World.QuestSerial);
        }

        /// <inheritdoc />
        public bool Exists(string slot) => _storage.Exists(slot);

        /// <inheritdoc />
        public IReadOnlyList<SaveSlotInfo> ListSlots()
        {
            var result = new List<SaveSlotInfo>();
            foreach (var slot in _storage.List())
            {
                try
                {
                    var text = _storage.Read(slot);
                    var game = SaveGame.FromJson(JsonParser.Parse(text, slot), slot);
                    result.Add(new SaveSlotInfo(
                        slot,
                        game.CreatedAt,
                        game.Playtime,
                        game.Seed,
                        game.World.CurrentLocationId,
                        game.World.Day,
                        game.World.Hours,
                        true,
                        game.Title,
                        game.Player?.Level ?? 0,
                        game.Player?.Money ?? 0));
                }
                catch (DataException)
                {
                    result.Add(new SaveSlotInfo(slot, null, 0, 0, null, 0, 0, false));
                }
            }

            return result;
        }

        /// <inheritdoc />
        public void Delete(string slot) => _storage.Delete(slot);

        private static IReadOnlyList<LocationSaveData> ToLocationData(WorldMap map)
        {
            var result = new List<LocationSaveData>();
            foreach (var location in map.Locations)
            {
                result.Add(new LocationSaveData(
                    location.InstanceId,
                    location.TemplateId,
                    location.Seed,
                    location.Background,
                    location.NodeType,
                    location.Tier,
                    location.Danger,
                    location.Visited,
                    location.Cleared));
            }

            return result;
        }

        private static IReadOnlyList<EdgeSaveData> ToEdgeData(WorldMap map)
        {
            var result = new List<EdgeSaveData>();
            foreach (var edge in map.Edges)
            {
                result.Add(new EdgeSaveData(
                    edge.EdgeId,
                    edge.FromId,
                    edge.ToId,
                    edge.Via,
                    edge.Length,
                    edge.Cost.Stamina,
                    edge.Cost.Hours,
                    edge.OneWay,
                    edge.Hidden));
            }

            return result;
        }

        private static IReadOnlyList<TraderSaveData> ToTraderData(IReadOnlyList<TraderState> traders)
        {
            var result = new List<TraderSaveData>();
            foreach (var trader in traders)
            {
                var stock = new List<TraderStockSaveData>();
                foreach (var pair in trader.Stock)
                {
                    stock.Add(new TraderStockSaveData(pair.Key, pair.Value));
                }

                stock.Sort((left, right) => string.CompareOrdinal(left.ItemId, right.ItemId));
                result.Add(new TraderSaveData(
                    trader.ActorId,
                    trader.LastRestockAt,
                    stock,
                    trader.QuestSlotAvailableAt));
            }

            result.Sort((left, right) => string.CompareOrdinal(left.ActorId, right.ActorId));
            return result;
        }

        private static IReadOnlyList<TraderState> LoadTraders(IReadOnlyList<TraderSaveData> traders)
        {
            var result = new List<TraderState>();
            foreach (var data in traders)
            {
                var state = new TraderState(data.ActorId, data.QuestSlots);
                foreach (var item in data.Stock)
                {
                    state.SetStock(item.ItemId, item.Count);
                }

                state.MarkRestocked(data.LastRestockAt);
                result.Add(state);
            }

            return result;
        }

        private static IReadOnlyList<QuestSaveData> ToQuestData(IReadOnlyList<QuestState> quests)
        {
            var result = new List<QuestSaveData>();
            foreach (var quest in quests)
            {
                var items = new List<QuestItemSaveData>();
                foreach (var item in quest.Items)
                {
                    items.Add(new QuestItemSaveData(item.ItemId, item.Count));
                }

                result.Add(new QuestSaveData(
                    quest.QuestId,
                    quest.GiverActorId,
                    quest.Status,
                    quest.AcceptedAt,
                    quest.CooldownUntil,
                    quest.CompletedAt,
                    quest.LocationTemplateId,
                    quest.LocationInstanceId,
                    quest.ActorIds,
                    items,
                    quest.KillCount,
                    quest.Cleared,
                    quest.RequiredCount,
                    quest.Slot));
            }

            result.Sort((left, right) => string.CompareOrdinal(left.QuestId, right.QuestId));
            return result;
        }

        private static IReadOnlyList<QuestState> LoadQuests(IReadOnlyList<QuestSaveData> quests)
        {
            var result = new List<QuestState>();
            foreach (var data in quests)
            {
                var items = new List<QuestItemTarget>();
                foreach (var item in data.Items)
                {
                    items.Add(new QuestItemTarget(item.ItemId, item.Count));
                }

                result.Add(new QuestState(
                    data.QuestId,
                    data.GiverActorId,
                    data.Status,
                    data.AcceptedAt,
                    data.CooldownUntil,
                    data.CompletedAt,
                    data.LocationTemplateId,
                    data.LocationInstanceId,
                    data.ActorIds,
                    items,
                    data.KillCount,
                    data.Cleared,
                    data.RequiredCount,
                    data.Slot));
            }

            return result;
        }
    }
}
