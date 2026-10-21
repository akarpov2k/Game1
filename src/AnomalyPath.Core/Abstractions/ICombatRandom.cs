#nullable enable
namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Источник случайных чисел боя; подменяется в тестах и на seed мира.</summary>
    public interface ICombatRandom
    {
        /// <summary>Следующее число в диапазоне [0, 1).</summary>
        /// <returns>Случайное число.</returns>
        double NextDouble();
    }
}
