#nullable enable
using System;
using System.Collections.Generic;

namespace AnomalyPath.Core.Quests
{
    /// <summary>Пул локаций для квеста на зачистку.</summary>
    public sealed class ClearLocationTarget
    {
        /// <summary>Создаёт цель «зачистить локацию».</summary>
        /// <param name="locationPool">Явный пул шаблонов локаций; пусто — все дикие точки интереса.</param>
        /// <param name="minTier">Минимальный тир сложности.</param>
        /// <param name="maxTier">Максимальный тир сложности.</param>
        public ClearLocationTarget(
            IReadOnlyList<string> locationPool,
            int minTier,
            int maxTier)
        {
            LocationPool = locationPool ?? Array.Empty<string>();
            MinTier = minTier;
            MaxTier = maxTier;
        }

        /// <summary>Явный пул шаблонов локаций.</summary>
        public IReadOnlyList<string> LocationPool { get; }

        /// <summary>Минимальный тир сложности.</summary>
        public int MinTier { get; }

        /// <summary>Максимальный тир сложности.</summary>
        public int MaxTier { get; }
    }
}
