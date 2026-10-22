#nullable enable
namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Источник случайных чисел генерации мира; подменяется в тестах.</summary>
    public interface IWorldRandom
    {
        /// <summary>Следующее целое в диапазоне [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>).</summary>
        /// <param name="minInclusive">Нижняя граница включительно.</param>
        /// <param name="maxExclusive">Верхняя граница исключительно.</param>
        /// <returns>Случайное целое.</returns>
        int NextInt(int minInclusive, int maxExclusive);

        /// <summary>Следующее число в диапазоне [0, 1).</summary>
        /// <returns>Случайное число.</returns>
        double NextDouble();
    }
}
