#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Templates;

namespace AnomalyPath.Core.Quests
{
    /// <summary>Цель «найти предметы и убить NPC»: категории предметов, пул NPC и сложность.</summary>
    public sealed class FindItemsTarget
    {
        /// <summary>Создаёт цель «найти предметы и убить NPC».</summary>
        /// <param name="itemPool">Явный пул предметов; пусто — все предметы указанных категорий.</param>
        /// <param name="categories">Категории предметов; пусто — оружие и броня.</param>
        /// <param name="itemKinds">Сколько разных предметов нужно принести.</param>
        /// <param name="itemCount">Сколько штук каждого предмета нужно принести.</param>
        /// <param name="npcPool">Явный пул NPC; пусто — все люди подходящего тира.</param>
        /// <param name="npcCount">Сколько NPC нужно убить.</param>
        /// <param name="poolSize">Сколько разных NPC попадает в конкретный квест.</param>
        /// <param name="minTier">Минимальный тир предмета и NPC.</param>
        /// <param name="maxTier">Максимальный тир предмета и NPC.</param>
        public FindItemsTarget(
            IReadOnlyList<string> itemPool,
            IReadOnlyList<ItemCategory> categories,
            int itemKinds,
            int itemCount,
            IReadOnlyList<string> npcPool,
            int npcCount,
            int poolSize,
            int minTier,
            int maxTier)
        {
            ItemPool = itemPool ?? Array.Empty<string>();
            Categories = categories ?? Array.Empty<ItemCategory>();
            ItemKinds = itemKinds;
            ItemCount = itemCount;
            NpcPool = npcPool ?? Array.Empty<string>();
            NpcCount = npcCount;
            PoolSize = poolSize;
            MinTier = minTier;
            MaxTier = maxTier;
        }

        /// <summary>Явный пул предметов.</summary>
        public IReadOnlyList<string> ItemPool { get; }

        /// <summary>Категории предметов.</summary>
        public IReadOnlyList<ItemCategory> Categories { get; }

        /// <summary>Сколько разных предметов нужно принести.</summary>
        public int ItemKinds { get; }

        /// <summary>Сколько штук каждого предмета нужно принести.</summary>
        public int ItemCount { get; }

        /// <summary>Явный пул NPC.</summary>
        public IReadOnlyList<string> NpcPool { get; }

        /// <summary>Сколько NPC нужно убить.</summary>
        public int NpcCount { get; }

        /// <summary>Сколько разных NPC попадает в конкретный квест.</summary>
        public int PoolSize { get; }

        /// <summary>Минимальный тир предмета и NPC.</summary>
        public int MinTier { get; }

        /// <summary>Максимальный тир предмета и NPC.</summary>
        public int MaxTier { get; }
    }
}
