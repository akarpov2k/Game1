#nullable enable
using System;

namespace AnomalyPath.Core.Data
{
    /// <summary>Устойчивый к перезапуску хеш: одинаковые входные данные дают одинаковый seed.</summary>
    public static class StableHash
    {
        /// <summary>Смешивает seed и строку алгоритмом FNV-1a.</summary>
        /// <param name="seed">Начальное значение.</param>
        /// <param name="text">Строка; <c>null</c> считается пустой.</param>
        /// <returns>Положительный seed, пригодный для генератора.</returns>
        public static int Combine(int seed, string? text)
        {
            var hash = 2166136261u ^ unchecked((uint)seed);
            if (text != null)
            {
                foreach (var symbol in text)
                {
                    hash ^= symbol;
                    hash *= 16777619u;
                }
            }

            var value = (int)(hash & 0x7FFFFFFFu);
            return value == 0 ? 1 : value;
        }
    }
}
