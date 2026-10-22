#nullable enable
using System;

namespace AnomalyPath.Core.Session
{
    /// <summary>Настройки сложности, выбранные при старте новой игры и зафиксированные в сейве.</summary>
    public sealed class NewGameSettings
    {
        /// <summary>Коэффициент усложнения торговли по умолчанию.</summary>
        public const double DefaultTradeDifficulty = 1;

        /// <summary>Максимальная длина ника.</summary>
        public const int MaxNicknameLength = 24;

        /// <summary>Создаёт настройки новой игры.</summary>
        /// <param name="lootLossFactor">Коэффициент потери лута при смерти, 0..1.</param>
        /// <param name="tradeDifficulty">Коэффициент усложнения торговли, не меньше 0.</param>
        /// <param name="nickname">Ник персонажа; пусто — сгенерировать случайный.</param>
        /// <exception cref="ArgumentOutOfRangeException">Значение вне допустимых границ.</exception>
        public NewGameSettings(double lootLossFactor, double tradeDifficulty, string? nickname = null)
        {
            if (lootLossFactor < 0 || lootLossFactor > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(lootLossFactor), "Коэффициент потери лута должен быть в 0..1");
            }

            if (tradeDifficulty < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tradeDifficulty), "Коэффициент торговли не может быть отрицательным");
            }

            var trimmed = nickname?.Trim() ?? string.Empty;
            if (trimmed.Length > MaxNicknameLength)
            {
                throw new ArgumentOutOfRangeException(nameof(nickname), "Ник не должен быть длиннее " + MaxNicknameLength + " символов");
            }

            LootLossFactor = lootLossFactor;
            TradeDifficulty = tradeDifficulty;
            Nickname = trimmed;
        }

        /// <summary>Коэффициент потери лута при смерти, 0..1.</summary>
        public double LootLossFactor { get; }

        /// <summary>Коэффициент усложнения торговли.</summary>
        public double TradeDifficulty { get; }

        /// <summary>Ник персонажа; пусто — сгенерировать случайный.</summary>
        public string Nickname { get; }
    }
}
