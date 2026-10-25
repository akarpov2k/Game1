#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Spawns;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Services
{
    /// <summary>
    /// Выбор объекта локации: тайник проверяется первым, иначе взвешенный выбор
    /// из записей, подходящих по тегам, тиру и роли локации.
    /// </summary>
    public sealed class LocationObjectService : ILocationObjectService
    {
        private readonly ISpawnRepository _spawns;
        private readonly ILocationRepository _locations;
        private readonly IStashService? _stashes;

        /// <summary>Создаёт сервис выбора объекта.</summary>
        /// <param name="spawns">Записи спавна.</param>
        /// <param name="locations">Шаблоны локаций: теги, тир и роль.</param>
        /// <param name="stashes">Тайники; <c>null</c> — без тайников.</param>
        /// <exception cref="ArgumentNullException">Обязательная зависимость не задана.</exception>
        public LocationObjectService(
            ISpawnRepository spawns,
            ILocationRepository locations,
            IStashService? stashes = null)
        {
            _spawns = spawns ?? throw new ArgumentNullException(nameof(spawns));
            _locations = locations ?? throw new ArgumentNullException(nameof(locations));
            _stashes = stashes;
        }

        /// <inheritdoc />
        public LocationObject? Select(WorldState? world, LocationInstance location, int seed)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (location.Object != null)
            {
                return location.Object;
            }

            if (location.Subtype == LocationSubtype.Hub)
            {
                return null;
            }

            if (world != null && _stashes != null && _stashes.RollOnEntry(world, location))
            {
                var stash = StashLocationObject.CreateDefault();
                location.SetObject(stash);
                return stash;
            }

            var template = _locations.Get(location.TemplateId);
            var candidates = new List<SpawnEntry>();
            var total = 0.0;
            foreach (var entry in _spawns.All)
            {
                if (entry.Matches(template.Tags, location.Tier, location.Subtype))
                {
                    candidates.Add(entry);
                    total += entry.Weight;
                }
            }

            if (candidates.Count == 0 || total <= 0)
            {
                return null;
            }

            var random = new SeededWorldRandom(StableHash.Combine(seed, location.InstanceId));
            var roll = random.NextDouble() * total;
            var chosen = candidates[candidates.Count - 1];
            foreach (var entry in candidates)
            {
                roll -= entry.Weight;
                if (roll < 0)
                {
                    chosen = entry;
                    break;
                }
            }

            var created = chosen.CreateObject();
            location.SetObject(created);
            return created;
        }
    }
}
