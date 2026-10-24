#nullable enable
using System;
using System.Collections.Generic;

namespace AnomalyPath.Core.Quests
{
    /// <summary>Цель «убить мутантов»: пул актёров, количество и сложность.</summary>
    public sealed class KillMutantsTarget
    {
        /// <summary>Создаёт цель «убить мутантов».</summary>
        /// <param name="actorPool">Явный пул мутантов; пусто — все мутанты подходящего тира.</param>
        /// <param name="count">Сколько мутантов нужно убить.</param>
        /// <param name="poolSize">Сколько разных мутантов попадает в конкретный квест.</param>
        /// <param name="minTier">Минимальный тир мутанта.</param>
        /// <param name="maxTier">Максимальный тир мутанта.</param>
        public KillMutantsTarget(
            IReadOnlyList<string> actorPool,
            int count,
            int poolSize,
            int minTier,
            int maxTier)
        {
            ActorPool = actorPool ?? Array.Empty<string>();
            Count = count;
            PoolSize = poolSize;
            MinTier = minTier;
            MaxTier = maxTier;
        }

        /// <summary>Явный пул мутантов.</summary>
        public IReadOnlyList<string> ActorPool { get; }

        /// <summary>Сколько мутантов нужно убить.</summary>
        public int Count { get; }

        /// <summary>Сколько разных мутантов попадает в конкретный квест.</summary>
        public int PoolSize { get; }

        /// <summary>Минимальный тир мутанта.</summary>
        public int MinTier { get; }

        /// <summary>Максимальный тир мутанта.</summary>
        public int MaxTier { get; }
    }
}
