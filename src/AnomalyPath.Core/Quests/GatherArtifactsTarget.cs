#nullable enable
using System;
using System.Collections.Generic;

namespace AnomalyPath.Core.Quests
{
    /// <summary>Цель «собрать артефакты»: пул предметов, количество и сложность.</summary>
    public sealed class GatherArtifactsTarget
    {
        /// <summary>Создаёт цель «собрать артефакты».</summary>
        /// <param name="itemPool">Явный пул артефактов; пусто — все артефакты подходящего тира.</param>
        /// <param name="count">Сколько артефактов нужно собрать.</param>
        /// <param name="poolSize">Сколько разных артефактов попадает в конкретный квест.</param>
        /// <param name="minTier">Минимальный тир артефакта.</param>
        /// <param name="maxTier">Максимальный тир артефакта.</param>
        public GatherArtifactsTarget(
            IReadOnlyList<string> itemPool,
            int count,
            int poolSize,
            int minTier,
            int maxTier)
        {
            ItemPool = itemPool ?? Array.Empty<string>();
            Count = count;
            PoolSize = poolSize;
            MinTier = minTier;
            MaxTier = maxTier;
        }

        /// <summary>Явный пул артефактов.</summary>
        public IReadOnlyList<string> ItemPool { get; }

        /// <summary>Сколько артефактов нужно собрать.</summary>
        public int Count { get; }

        /// <summary>Сколько разных артефактов попадает в конкретный квест.</summary>
        public int PoolSize { get; }

        /// <summary>Минимальный тир артефакта.</summary>
        public int MinTier { get; }

        /// <summary>Максимальный тир артефакта.</summary>
        public int MaxTier { get; }
    }
}
