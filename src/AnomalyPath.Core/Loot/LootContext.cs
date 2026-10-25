#nullable enable
using AnomalyPath.Core.Templates;

namespace AnomalyPath.Core.Loot
{
    /// <summary>Входные данные броска лута: кто погиб, где и с каким seed.</summary>
    public sealed class LootContext
    {
        /// <summary>Создаёт контекст броска лута.</summary>
        /// <param name="enemyId">Id противника.</param>
        /// <param name="enemyRarity">Редкость противника: множитель шанса.</param>
        /// <param name="lootTableId">Таблица лута или <c>null</c>, если лута нет.</param>
        /// <param name="locationTier">Тир локации, где произошёл бой.</param>
        /// <param name="seed">Seed броска: одинаковый seed даёт одинаковый лут.</param>
        public LootContext(
            string enemyId,
            Rarity enemyRarity,
            string? lootTableId,
            int locationTier,
            int seed)
        {
            EnemyId = enemyId;
            EnemyRarity = enemyRarity;
            LootTableId = lootTableId;
            LocationTier = locationTier;
            Seed = seed;
        }

        /// <summary>Id противника.</summary>
        public string EnemyId { get; }

        /// <summary>Редкость противника; числовое значение — множитель шанса.</summary>
        public Rarity EnemyRarity { get; }

        /// <summary>Таблица лута или <c>null</c>.</summary>
        public string? LootTableId { get; }

        /// <summary>Тир локации.</summary>
        public int LocationTier { get; }

        /// <summary>Seed броска.</summary>
        public int Seed { get; }
    }
}
