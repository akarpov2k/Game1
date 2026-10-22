#nullable enable
using System;
using AnomalyPath.Core.Abstractions;

namespace AnomalyPath.Core.World
{
    /// <summary>Детерминированный генератор для мира: один seed даёт один маршрут.</summary>
    public sealed class SeededWorldRandom : IWorldRandom
    {
        private ulong _state;

        /// <summary>Создаёт генератор с seed.</summary>
        /// <param name="seed">Начальное значение; ноль заменяется на 1.</param>
        public SeededWorldRandom(int seed)
        {
            _state = seed == 0 ? 1UL : (ulong)seed;
        }

        /// <inheritdoc />
        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            }

            return minInclusive + (int)(NextDouble() * (maxExclusive - minInclusive));
        }

        /// <inheritdoc />
        public double NextDouble()
        {
            _state = _state * 6364136223846793005UL + 1442695040888963407UL;
            return (_state >> 11) / (double)(1UL << 53);
        }
    }
}
