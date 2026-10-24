#nullable enable
using System;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Talents;

namespace AnomalyPath.Core.Services
{
    /// <summary>
    /// Прокачка через дерево талантов: очки, ранги, пороги открытия и собранные бонусы.
    /// Очки выдаются за уровни персонажа снаружи, сервис их только тратит и начисляет.
    /// </summary>
    public sealed class TalentService : ITalentService
    {
        private readonly ITalentRepository _repository;

        /// <summary>Создаёт сервис прокачки.</summary>
        /// <param name="repository">Репозиторий дерева талантов.</param>
        /// <exception cref="ArgumentNullException">Репозиторий не задан.</exception>
        public TalentService(ITalentRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <inheritdoc />
        public TalentTreeTemplate Tree => _repository.Tree;

        /// <inheritdoc />
        public int SpentPoints(PlayerState player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            var total = 0;
            foreach (var node in Tree.Nodes)
            {
                total += RankOf(player, node.Id);
            }

            return total;
        }

        /// <inheritdoc />
        public int AvailablePoints(PlayerState player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            return player.TalentPoints;
        }

        /// <inheritdoc />
        public int RankOf(PlayerState player, string talentId)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (string.IsNullOrWhiteSpace(talentId) || !Tree.TryGetNode(talentId, out var node) || node == null)
            {
                return 0;
            }

            return Math.Clamp(player.GetTalentRank(talentId), 0, node.MaxRank);
        }

        /// <inheritdoc />
        public bool IsUnlocked(PlayerState player, TalentNodeTemplate node)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return SpentPoints(player) >= node.RequiredPoints;
        }

        /// <inheritdoc />
        public TalentResult Learn(PlayerState player, string talentId)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (!Tree.TryGetNode(talentId, out var node) || node == null)
            {
                return TalentResult.Error(TalentResultStatus.UnknownTalent, talentId);
            }

            var rank = RankOf(player, talentId);
            var spent = SpentPoints(player);
            var available = player.TalentPoints;
            if (rank >= node.MaxRank)
            {
                return TalentResult.Error(
                    TalentResultStatus.MaxRank,
                    talentId,
                    rank,
                    spent,
                    available,
                    node.RequiredPoints);
            }

            if (spent < node.RequiredPoints)
            {
                return TalentResult.Error(
                    TalentResultStatus.Locked,
                    talentId,
                    rank,
                    spent,
                    available,
                    node.RequiredPoints);
            }

            if (available <= 0 || !player.TrySpendTalentPoint())
            {
                return TalentResult.Error(
                    TalentResultStatus.NoPoints,
                    talentId,
                    rank,
                    spent,
                    available,
                    node.RequiredPoints);
            }

            player.SetTalentRank(talentId, rank + 1);
            return TalentResult.Ok(
                talentId,
                rank + 1,
                SpentPoints(player),
                player.TalentPoints,
                node.RequiredPoints);
        }

        /// <inheritdoc />
        public TalentModifiers GetModifiers(PlayerState player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            var apRestore = 0;
            var apMax = 0;
            var healthMax = 0;
            var accuracy = 0.0;
            var staminaMax = 0;
            var luck = 0;
            var carryRows = 0;
            foreach (var node in Tree.Nodes)
            {
                var rank = RankOf(player, node.Id);
                if (rank == 0)
                {
                    continue;
                }

                foreach (var effect in node.Effects)
                {
                    var value = effect.PerRank * rank;
                    switch (effect.Stat)
                    {
                        case TalentStat.ApRestore:
                            apRestore += ToInt(value);
                            break;
                        case TalentStat.ApMax:
                            apMax += ToInt(value);
                            break;
                        case TalentStat.HealthMax:
                            healthMax += ToInt(value);
                            break;
                        case TalentStat.Accuracy:
                            accuracy += value;
                            break;
                        case TalentStat.StaminaMax:
                            staminaMax += ToInt(value);
                            break;
                        case TalentStat.Luck:
                            luck += ToInt(value);
                            break;
                        case TalentStat.CarryRows:
                            carryRows += ToInt(value);
                            break;
                        default:
                            throw new DataException(node.Id, CoreErrors.UnknownTalentStat, effect.Stat);
                    }
                }
            }

            return new TalentModifiers(apRestore, apMax, healthMax, accuracy, staminaMax, luck, carryRows);
        }

        /// <inheritdoc />
        public void GrantPoints(PlayerState player, int amount)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            player.AddTalentPoints(amount);
        }

        private static int ToInt(double value) => (int)Math.Round(value);
    }
}
