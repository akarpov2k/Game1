#nullable enable
namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Источник случайных чисел для смерти и страховки; подменяется в тестах.</summary>
    public interface IDeathRandom
    {
        /// <summary>Следующее число в диапазоне [0, 1).</summary>
        /// <returns>Случайное число.</returns>
        double NextDouble();
    }
}
