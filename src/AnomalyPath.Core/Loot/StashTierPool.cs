#nullable enable
namespace AnomalyPath.Core.Loot
{
    /// <summary>Пул лута тайника для одного тира локации.</summary>
    public readonly struct StashTierPool
    {
        /// <summary>Создаёт пул тайника для тира.</summary>
        /// <param name="tier">Тир локации.</param>
        /// <param name="lootTableId">Id таблицы лута.</param>
        public StashTierPool(int tier, string lootTableId)
        {
            Tier = tier;
            LootTableId = lootTableId;
        }

        /// <summary>Тир локации.</summary>
        public int Tier { get; }

        /// <summary>Id таблицы лута тайника для этого тира.</summary>
        public string LootTableId { get; }
    }
}
