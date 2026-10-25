#nullable enable
using System;
using System.Globalization;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Loot;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Services
{
    /// <summary>
    /// Тайники: проверка появления при входе с растущим шансом и вскрытие
    /// с пулом по тиру локации либо пулом джекпота, не привязанным к тирам.
    /// </summary>
    public sealed class StashService : IStashService
    {
        private readonly StashSettings? _settings;
        private readonly ILootService _loot;

        /// <summary>Создаёт сервис тайников.</summary>
        /// <param name="loot">Таблицы лута и настройки тайников.</param>
        /// <param name="lootService">Бросок лута по таблице.</param>
        /// <exception cref="ArgumentNullException">Зависимость не задана.</exception>
        public StashService(ILootRepository loot, ILootService lootService)
        {
            if (loot == null)
            {
                throw new ArgumentNullException(nameof(loot));
            }

            _settings = loot.Settings.Stashes;
            _loot = lootService ?? throw new ArgumentNullException(nameof(lootService));
        }

        /// <inheritdoc />
        public bool Enabled => _settings != null;

        /// <inheritdoc />
        public bool RollOnEntry(WorldState world, LocationInstance location)
        {
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }

            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (_settings == null || location.NodeType == LocationNodeType.Hub || location.StashRolled)
            {
                return false;
            }

            location.MarkStashRolled();
            var checks = world.StashChecks;
            var seed = StableHash.Combine(
                world.Map.Seed,
                location.InstanceId + "#stash_check#" + checks.ToString(CultureInfo.InvariantCulture));
            if (new SeededWorldRandom(seed).NextDouble() >= _settings.Chance(checks))
            {
                world.RegisterStashCheck();
                return false;
            }

            location.MarkStashPresent();
            world.ResetStashChecks();
            return true;
        }

        /// <inheritdoc />
        public StashOpenResult Open(WorldState world, LocationInstance location)
        {
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }

            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (location.StashOpened)
            {
                return StashOpenResult.AlreadyOpened();
            }

            if (_settings == null || !location.StashPresent)
            {
                return StashOpenResult.NoStash();
            }

            var openings = world.StashOpenings;
            var serial = world.NextLootSerial();
            var seed = StableHash.Combine(
                world.Map.Seed,
                location.InstanceId
                + "#stash_open#"
                + serial.ToString(CultureInfo.InvariantCulture)
                + "#"
                + openings.ToString(CultureInfo.InvariantCulture));
            var jackpot = new SeededWorldRandom(seed).NextDouble() < _settings.JackpotChance(openings);
            location.MarkStashOpened();
            if (jackpot)
            {
                world.ResetStashOpenings();
            }
            else
            {
                world.RegisterStashOpening();
            }

            if (jackpot)
            {
                var jackpotLoot = _loot.RollTable(_settings.JackpotLootTableId, location.Tier, seed, true);
                return StashOpenResult.Opened(true, _settings.JackpotLootTableId, jackpotLoot);
            }

            var tableId = _settings.PoolForTier(location.Tier);
            if (tableId == null)
            {
                return StashOpenResult.Opened(false, _settings.JackpotLootTableId, Array.Empty<ItemInstance>());
            }

            var loot = _loot.RollTable(tableId, location.Tier, seed, true);
            return StashOpenResult.Opened(false, tableId, loot);
        }
    }
}
