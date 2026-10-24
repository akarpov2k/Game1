#nullable enable
namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Правила округления и нижней границы цен из данных.</summary>
    public interface IEconomySettings
    {
        /// <summary>Шаг округления цены, не меньше единицы.</summary>
        int Rounding { get; }

        /// <summary>Минимальная цена, не меньше нуля.</summary>
        int MinPrice { get; }
    }
}
