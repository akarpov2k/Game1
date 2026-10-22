#nullable enable
using System;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Session
{
    /// <summary>Настройки сложности, выбранные при старте новой игры и зафиксированные в сейве.</summary>
    public sealed class NewGameSettings
    {
        /// <summary>Создаёт настройки новой игры.</summary>
        /// <param name="lootLossFactor">Коэффициент потери лута при смерти, 0..1.</param>
        /// <param name="tradeDifficulty">Коэффициент усложнения торговли, не меньше 0.</param>
        /// <param name="nickname">Ник персонажа; пусто — сгенерировать случайный.</param>
        /// <param name="nicknameMaxLength">Предел длины ника из правил; 0 — не проверять.</param>
        /// <exception cref="ArgumentOutOfRangeException">Значение вне допустимых границ.</exception>
        public NewGameSettings(
            double lootLossFactor,
            double tradeDifficulty,
            string? nickname = null,
            int nicknameMaxLength = 0)
        {
            if (lootLossFactor < 0 || lootLossFactor > 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(lootLossFactor),
                    CoreText.Format(CoreErrors.ChanceRange, nameof(lootLossFactor)));
            }

            if (tradeDifficulty < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tradeDifficulty),
                    CoreText.Format(CoreErrors.NonNegative, nameof(tradeDifficulty)));
            }

            var trimmed = nickname?.Trim() ?? string.Empty;
            if (nicknameMaxLength > 0 && trimmed.Length > nicknameMaxLength)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(nickname),
                    CoreText.Format(CoreErrors.NicknameLength, nicknameMaxLength));
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
