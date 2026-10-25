#nullable enable
using System;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Progression;

namespace AnomalyPath.Core.Services
{
    /// <summary>
    /// Прогрессия по формулам из данных: требование уровня растёт на процент от предыдущего,
    /// награда за убийство линейна по рангу и растёт на процент редкости. Повышение уровня
    /// переносит излишек опыта, выдаёт очки талантов и прибавку к максимуму здоровья.
    /// </summary>
    public sealed class ProgressionService : IProgressionService
    {
        private readonly ProgressionSettings _settings;

        /// <summary>Создаёт сервис прогрессии.</summary>
        /// <param name="settings">Настройки кривой опыта и награды за убийство.</param>
        /// <exception cref="ArgumentNullException">Настройки не заданы.</exception>
        public ProgressionService(ProgressionSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <inheritdoc/>
        public int MaxRank => _settings.MaxRank;

        /// <inheritdoc/>
        public int MaxRarity => _settings.MaxRarity;

        /// <inheritdoc/>
        public int RequiredXp(int level)
        {
            RequireLevel(level);
            return Requirement(RawRequired(level));
        }

        /// <inheritdoc/>
        public int TotalXpToLevel(int level)
        {
            RequireLevel(level);
            if (level == 1)
            {
                return 0;
            }

            // каждое требование не меньше базового, поэтому заведомо переполняющие суммы считаются сразу
            var minimal = (long)_settings.LevelBaseXp * (level - 1);
            if (minimal > int.MaxValue)
            {
                return int.MaxValue;
            }

            if (_settings.LevelGrowthPercent <= 0)
            {
                return (int)minimal;
            }

            var growth = Growth();
            var factor = 1.0;
            var total = 0L;
            for (var step = 1; step < level; step++)
            {
                var requirement = Requirement(_settings.LevelBaseXp * factor);
                if (requirement >= int.MaxValue || total + requirement > int.MaxValue)
                {
                    return int.MaxValue;
                }

                total += requirement;
                factor *= growth;
            }

            return (int)total;
        }

        /// <inheritdoc/>
        public int KillXp(int rank, int rarity)
        {
            RequireRank(rank);
            RequireRarity(rarity);

            var bonus = _settings.RarityBonusPercent * rarity / 100.0;
            return Requirement(_settings.KillXpPerRank * rank * (1.0 + bonus));
        }

        /// <inheritdoc/>
        public int KillsForLevel(int level, int rank, int rarity)
        {
            var required = RequiredXp(level);
            var reward = KillXp(rank, rarity);
            return reward <= 0 ? int.MaxValue : (required + reward - 1) / reward;
        }

        /// <inheritdoc/>
        public int KillsToLevel(int level, int rank, int rarity)
        {
            var total = TotalXpToLevel(level);
            var reward = KillXp(rank, rarity);
            return reward <= 0 ? int.MaxValue : (total + reward - 1) / reward;
        }

        /// <inheritdoc/>
        public int LevelForXp(int totalXp)
        {
            if (totalXp < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(totalXp),
                    CoreText.Format(CoreErrors.NonNegative, nameof(totalXp)));
            }

            if (_settings.LevelGrowthPercent <= 0)
            {
                return 1 + totalXp / _settings.LevelBaseXp;
            }

            var growth = Growth();
            var factor = 1.0;
            var spent = 0L;
            var level = 1;
            while (true)
            {
                var requirement = Requirement(_settings.LevelBaseXp * factor);
                if (requirement >= int.MaxValue || spent + requirement > totalXp)
                {
                    return level;
                }

                spent += requirement;
                level++;
                factor *= growth;
            }
        }

        /// <inheritdoc/>
        public int SyncLevel(PlayerState player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            return ApplyLevelUps(player, player.Level);
        }

        /// <inheritdoc/>
        public int AddXp(PlayerState player, int amount)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    CoreText.Format(CoreErrors.NonNegative, nameof(amount)));
            }

            player.AddXp(amount);
            return ApplyLevelUps(player, player.Level);
        }

        /// <inheritdoc/>
        public int LoseXpOnDeath(PlayerState player, double fraction)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (double.IsNaN(fraction) || fraction < 0 || fraction > 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(fraction),
                    CoreText.Format(CoreErrors.ChanceRange, nameof(fraction)));
            }

            if (fraction <= 0)
            {
                return 0;
            }

            var levelFloor = TotalXpToLevel(player.Level);
            var progress = player.Xp - levelFloor;
            if (progress <= 0)
            {
                return 0;
            }

            var loss = (int)Math.Round(progress * fraction, MidpointRounding.AwayFromZero);
            return loss <= 0 ? 0 : player.RemoveXp(Math.Min(loss, progress));
        }

        private int ApplyLevelUps(PlayerState player, int currentLevel)
        {
            var target = LevelForXp(player.Xp);
            if (target <= currentLevel)
            {
                return 0;
            }

            for (var level = currentLevel + 1; level <= target; level++)
            {
                if (level < _settings.LevelRewardStartLevel)
                {
                    continue;
                }

                if (_settings.LevelUpTalentPoints > 0)
                {
                    player.AddTalentPoints(_settings.LevelUpTalentPoints);
                }

                if (_settings.LevelUpHealth > 0)
                {
                    player.AddLevelHealthBonus(_settings.LevelUpHealth);
                }
            }

            player.SetLevel(target);
            return target - currentLevel;
        }

        private double Growth() => 1.0 + _settings.LevelGrowthPercent / 100.0;

        private double RawRequired(int level) =>
            _settings.LevelGrowthPercent <= 0
                ? _settings.LevelBaseXp
                : _settings.LevelBaseXp * Math.Pow(Growth(), level - 1);

        private static int Requirement(double value)
        {
            if (double.IsNaN(value) || value >= int.MaxValue)
            {
                return int.MaxValue;
            }

            return value <= 0 ? 0 : (int)Math.Round(value, MidpointRounding.AwayFromZero);
        }

        private static void RequireLevel(int level)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(level),
                    CoreText.Format(CoreErrors.LevelOutOfRange, level));
            }
        }

        private void RequireRank(int rank)
        {
            if (rank < 1 || rank > _settings.MaxRank)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rank),
                    CoreText.Format(CoreErrors.RankOutOfRange, rank, _settings.MaxRank));
            }
        }

        private void RequireRarity(int rarity)
        {
            if (rarity < 1 || rarity > _settings.MaxRarity)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rarity),
                    CoreText.Format(CoreErrors.RarityOutOfRange, rarity, _settings.MaxRarity));
            }
        }
    }
}
