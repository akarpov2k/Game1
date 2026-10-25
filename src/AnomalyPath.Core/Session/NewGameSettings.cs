#nullable enable
using System;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Session
{
    /// <summary>Настройки сложности, выбранные при старте новой игры и зафиксированные в сейве.</summary>
    public sealed class NewGameSettings
    {
        /// <summary>Создаёт настройки новой игры.</summary>
        /// <param name="insuranceChance">
        /// Коэффициент страховки надетой вещи, 0..1: вероятность, что конкретный надетый предмет
        /// переживёт смерть. Применяется к каждому предмету отдельным броском.
        /// </param>
        /// <param name="tradeDifficulty">Коэффициент усложнения торговли, не меньше 0.</param>
        /// <param name="nickname">Ник персонажа; пусто — сгенерировать случайный.</param>
        /// <param name="nicknameMaxLength">Предел длины ника из правил; 0 — не проверять.</param>
        /// <param name="permadeath">
        /// <c>true</c> — режим постоянной смерти: при смерти удаляется всё, включая снаряжение,
        /// а страховка не применяется. По умолчанию <c>false</c>.
        /// </param>
        /// <param name="xpLossChance">
        /// Доля опыта текущего уровня, теряемая при смерти, 0..1; по умолчанию 0 — без потери
        /// (так читаются сейвы без настройки, значение им подставляет загрузчик).
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Значение вне допустимых границ.</exception>
        public NewGameSettings(
            double insuranceChance,
            double tradeDifficulty,
            string? nickname = null,
            int nicknameMaxLength = 0,
            bool permadeath = false,
            double xpLossChance = 0)
        {
            if (insuranceChance < 0 || insuranceChance > 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(insuranceChance),
                    CoreText.Format(CoreErrors.ChanceRange, nameof(insuranceChance)));
            }

            if (tradeDifficulty < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tradeDifficulty),
                    CoreText.Format(CoreErrors.NonNegative, nameof(tradeDifficulty)));
            }

            if (xpLossChance < 0 || xpLossChance > 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(xpLossChance),
                    CoreText.Format(CoreErrors.ChanceRange, nameof(xpLossChance)));
            }

            var trimmed = nickname?.Trim() ?? string.Empty;
            if (nicknameMaxLength > 0 && trimmed.Length > nicknameMaxLength)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(nickname),
                    CoreText.Format(CoreErrors.NicknameLength, nicknameMaxLength));
            }

            InsuranceChance = insuranceChance;
            TradeDifficulty = tradeDifficulty;
            Nickname = trimmed;
            Permadeath = permadeath;
            XpLossChance = xpLossChance;
        }

        /// <summary>
        /// Коэффициент страховки надетой вещи, 0..1. При смерти каждый надетый предмет
        /// проходит собственный бросок: при успехе остаётся, иначе теряется.
        /// </summary>
        public double InsuranceChance { get; }

        /// <summary>Коэффициент усложнения торговли.</summary>
        public double TradeDifficulty { get; }

        /// <summary>
        /// Доля опыта, накопленного на текущем уровне, которая теряется при смерти.
        /// Потеря не опускает прогресс ниже нуля и не понижает уровень.
        /// </summary>
        public double XpLossChance { get; }

        /// <summary>Ник персонажа; пусто — сгенерировать случайный.</summary>
        public string Nickname { get; }

        /// <summary>
        /// <c>true</c>, если выбран режим постоянной смерти: страховка не применяется,
        /// а при смерти удаляется всё, включая снаряжение.
        /// </summary>
        public bool Permadeath { get; }
    }
}
