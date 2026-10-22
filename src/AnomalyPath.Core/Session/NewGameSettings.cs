#nullable enable
using System;

namespace AnomalyPath.Core.Session
{
    /// <summary>Настройки сложности, выбранные при старте новой игры и зафиксированные в сейве.</summary>
    public sealed class NewGameSettings
    {
        /// <summary>Коэффициент усложнения торговли по умолчанию.</summary>
        public const double DefaultTradeDifficulty = 1;

        /// <summary>Создаёт настройки новой игры.</summary>
        /// <param name="lootLossFactor">Коэффициент потери лута при смерти, 0..1.</param>
        /// <param name="tradeDifficulty">Коэффициент усложнения торговли, не меньше 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">Значение вне допустимых границ.</exception>
        public NewGameSettings(double lootLossFactor, double tradeDifficulty)
        {
            if (lootLossFactor < 0 || lootLossFactor > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(lootLossFactor), "Коэффициент потери лута должен быть в 0..1");
            }

            if (tradeDifficulty < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tradeDifficulty), "Коэффициент торговли не может быть отрицательным");
            }

            LootLossFactor = lootLossFactor;
            TradeDifficulty = tradeDifficulty;
        }

        /// <summary>Коэффициент потери лута при смерти, 0..1.</summary>
        public double LootLossFactor { get; }

        /// <summary>Коэффициент усложнения торговли.</summary>
        public double TradeDifficulty { get; }
    }
}
