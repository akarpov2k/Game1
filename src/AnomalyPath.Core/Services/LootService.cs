#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Loot;
using AnomalyPath.Core.Templates;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Services
{
    /// <summary>
    /// Бросок лута: каждая запись пула проверяется отдельно — полоса тира, затем
    /// шанс с множителем редкости противника. Запись <c>always</c> падает без бросков.
    /// </summary>
    public sealed class LootService : ILootService
    {
        private readonly ILootRepository _loot;
        private readonly IItemRepository _items;
        private readonly IInstanceIdGenerator _ids;
        private readonly int _minItemTier;
        private readonly int _maxItemTier;

        /// <summary>Создаёт сервис лута.</summary>
        /// <param name="loot">Таблицы лута и полосы тира.</param>
        /// <param name="items">Каталог предметов: тир и существование id.</param>
        /// <param name="ids">Генератор id экземпляров.</param>
        /// <exception cref="ArgumentNullException">Зависимость не задана.</exception>
        public LootService(ILootRepository loot, IItemRepository items, IInstanceIdGenerator ids)
        {
            _loot = loot ?? throw new ArgumentNullException(nameof(loot));
            _items = items ?? throw new ArgumentNullException(nameof(items));
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));

            var min = int.MaxValue;
            var max = int.MinValue;
            foreach (var item in items.All)
            {
                if (item.Tier < min)
                {
                    min = item.Tier;
                }

                if (item.Tier > max)
                {
                    max = item.Tier;
                }
            }

            _minItemTier = min == int.MaxValue ? 0 : min;
            _maxItemTier = max == int.MinValue ? -1 : max;
        }

        /// <inheritdoc />
        public IReadOnlyList<ItemInstance> Roll(LootContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (string.IsNullOrWhiteSpace(context.LootTableId))
            {
                return Array.Empty<ItemInstance>();
            }

            if (!_loot.TryGet(context.LootTableId!, out var table))
            {
                return Array.Empty<ItemInstance>();
            }

            var entries = new List<LootEntry>();
            Collect(table, entries, new HashSet<string>(StringComparer.Ordinal));
            if (entries.Count == 0)
            {
                return Array.Empty<ItemInstance>();
            }

            var random = new SeededWorldRandom(context.Seed);
            var multiplier = (int)context.EnemyRarity;
            var result = new List<ItemInstance>();

            foreach (var entry in entries)
            {
                if (!_items.TryGet(entry.ItemId, out var template))
                {
                    continue;
                }

                if (!entry.Always)
                {
                    if (!entry.IgnoreTierBand)
                    {
                        var band = _loot.Settings.ChanceForTier(
                            template.Tier,
                            context.LocationTier,
                            _minItemTier,
                            _maxItemTier);
                        if (random.NextDouble() >= band)
                        {
                            continue;
                        }
                    }

                    var chance = entry.Chance * multiplier;
                    if (chance < 1 && random.NextDouble() >= chance)
                    {
                        continue;
                    }
                }

                var count = entry.CountMax > entry.CountMin
                    ? random.NextInt(entry.CountMin, entry.CountMax + 1)
                    : entry.CountMin;
                result.Add(new ItemInstance(_ids.Next(), entry.ItemId, count));
            }

            return result;
        }

        private void Collect(LootTable table, List<LootEntry> entries, HashSet<string> visited)
        {
            if (!visited.Add(table.Id))
            {
                return;
            }

            foreach (var sharedId in table.SharedTableIds)
            {
                if (_loot.TryGet(sharedId, out var shared))
                {
                    Collect(shared, entries, visited);
                }
            }

            entries.AddRange(table.Entries);
        }
    }
}
