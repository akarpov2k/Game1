#nullable enable
using AnomalyPath.Core.Abstractions;

namespace AnomalyPath.Core.Death
{
    /// <summary>Детерминированный генератор бросков смерти и страховки.</summary>
    public sealed class SeededDeathRandom : IDeathRandom
    {
        private ulong _state;

        /// <summary>Создаёт генератор с seed.</summary>
        /// <param name="seed">Начальное значение; ноль заменяется на 1.</param>
        public SeededDeathRandom(int seed = 1)
        {
            _state = seed == 0 ? 1UL : (ulong)seed;
        }

        /// <inheritdoc />
        public double NextDouble()
        {
            _state = _state * 6364136223846793005UL + 1442695040888963407UL;
            return (_state >> 11) / (double)(1UL << 53);
        }
    }
}
