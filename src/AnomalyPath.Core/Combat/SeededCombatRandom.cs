#nullable enable
using AnomalyPath.Core.Abstractions;

namespace AnomalyPath.Core.Combat
{
    /// <summary>Детерминированный линейный конгруэнтный генератор для бросков боя.</summary>
    public sealed class SeededCombatRandom : ICombatRandom
    {
        private ulong _state;

        /// <summary>Создаёт генератор с seed.</summary>
        /// <param name="seed">Начальное значение; ноль заменяется на 1.</param>
        public SeededCombatRandom(int seed = 1)
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
