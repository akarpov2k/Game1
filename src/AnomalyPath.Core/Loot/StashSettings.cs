#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Loot
{
    /// <summary>
    /// Настройки тайников и джекпота: шанс появления с ростом, пулы по тирам локации
    /// и отдельный пул джекпота, не привязанный к тирам.
    /// </summary>
    public sealed class StashSettings
    {
        private readonly IReadOnlyList<StashTierPool> _pools;

        /// <summary>Создаёт настройки тайников.</summary>
        /// <param name="baseChance">Базовый шанс тайника 0..1.</param>
        /// <param name="chanceGrowth">Прибавка к шансу за каждую проверку без тайника 0..1.</param>
        /// <param name="chanceCap">Предел шанса тайника 0..1.</param>
        /// <param name="jackpotBaseChance">Базовый шанс джекпота 0..1.</param>
        /// <param name="jackpotChanceGrowth">Прибавка к шансу джекпота за каждое вскрытие 0..1.</param>
        /// <param name="jackpotChanceCap">Предел шанса джекпота 0..1.</param>
        /// <param name="pools">Пулы лута по тирам локации.</param>
        /// <param name="jackpotLootTableId">Таблица пула джекпота.</param>
        /// <exception cref="ArgumentNullException">Списки не заданы.</exception>
        /// <exception cref="ArgumentException">Поля вне диапазона или пулы заданы неверно.</exception>
        public StashSettings(
            double baseChance,
            double chanceGrowth,
            double chanceCap,
            double jackpotBaseChance,
            double jackpotChanceGrowth,
            double jackpotChanceCap,
            IReadOnlyList<StashTierPool> pools,
            string jackpotLootTableId)
        {
            CheckChance(baseChance, nameof(baseChance));
            CheckChance(chanceGrowth, nameof(chanceGrowth));
            CheckChance(chanceCap, nameof(chanceCap));
            CheckChance(jackpotBaseChance, nameof(jackpotBaseChance));
            CheckChance(jackpotChanceGrowth, nameof(jackpotChanceGrowth));
            CheckChance(jackpotChanceCap, nameof(jackpotChanceCap));

            _pools = pools ?? throw new ArgumentNullException(nameof(pools));
            if (_pools.Count == 0)
            {
                throw new ArgumentException(CoreText.Get(CoreErrors.StashPoolEmpty), nameof(pools));
            }

            var tiers = new HashSet<int>();
            foreach (var pool in _pools)
            {
                if (pool.Tier < 0 || string.IsNullOrWhiteSpace(pool.LootTableId))
                {
                    throw new ArgumentException(
                        CoreText.Format(CoreErrors.StashPool, pool.Tier),
                        nameof(pools));
                }

                if (!tiers.Add(pool.Tier))
                {
                    throw new ArgumentException(
                        CoreText.Format(CoreErrors.StashPoolDuplicate, pool.Tier),
                        nameof(pools));
                }
            }

            if (string.IsNullOrWhiteSpace(jackpotLootTableId))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.StashPool, "jackpot"),
                    nameof(jackpotLootTableId));
            }

            BaseChance = baseChance;
            ChanceGrowth = chanceGrowth;
            ChanceCap = chanceCap;
            JackpotBaseChance = jackpotBaseChance;
            JackpotChanceGrowth = jackpotChanceGrowth;
            JackpotChanceCap = jackpotChanceCap;
            JackpotLootTableId = jackpotLootTableId;
        }

        /// <summary>Базовый шанс появления тайника.</summary>
        public double BaseChance { get; }

        /// <summary>Прибавка к шансу тайника за проверку без тайника.</summary>
        public double ChanceGrowth { get; }

        /// <summary>Предел шанса тайника.</summary>
        public double ChanceCap { get; }

        /// <summary>Базовый шанс джекпота при вскрытии тайника.</summary>
        public double JackpotBaseChance { get; }

        /// <summary>Прибавка к шансу джекпота за вскрытие без джекпота.</summary>
        public double JackpotChanceGrowth { get; }

        /// <summary>Предел шанса джекпота.</summary>
        public double JackpotChanceCap { get; }

        /// <summary>Пулы лута по тирам локации.</summary>
        public IReadOnlyList<StashTierPool> Pools => _pools;

        /// <summary>Таблица пула джекпота; не зависит от тира локации.</summary>
        public string JackpotLootTableId { get; }

        /// <summary>Шанс тайника после указанного числа проверок без находки.</summary>
        /// <param name="checks">Проверки без тайника с последней находки.</param>
        /// <returns>Шанс 0..1 с учётом роста и предела.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Число проверок отрицательно.</exception>
        public double Chance(int checks)
        {
            if (checks < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(checks));
            }

            return Math.Min(ChanceCap, BaseChance + ChanceGrowth * checks);
        }

        /// <summary>Шанс джекпота после указанного числа вскрытий без джекпота.</summary>
        /// <param name="openings">Вскрытия без джекпота с последнего срабатывания.</param>
        /// <returns>Шанс 0..1 с учётом роста и предела.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Число вскрытий отрицательно.</exception>
        public double JackpotChance(int openings)
        {
            if (openings < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(openings));
            }

            return Math.Min(JackpotChanceCap, JackpotBaseChance + JackpotChanceGrowth * openings);
        }

        /// <summary>Таблица лута тайника для тира локации.</summary>
        /// <param name="locationTier">Тир локации.</param>
        /// <returns>
        /// Точный тир, иначе ближайший к нему; при равном расстоянии — меньший тир.
        /// </returns>
        public string? PoolForTier(int locationTier)
        {
            string? exact = null;
            string? nearest = null;
            var nearestDistance = int.MaxValue;
            var nearestTier = int.MaxValue;
            foreach (var pool in _pools)
            {
                if (pool.Tier == locationTier)
                {
                    exact = pool.LootTableId;
                }

                var distance = Math.Abs(pool.Tier - locationTier);
                if (distance < nearestDistance || (distance == nearestDistance && pool.Tier < nearestTier))
                {
                    nearestDistance = distance;
                    nearestTier = pool.Tier;
                    nearest = pool.LootTableId;
                }
            }

            return exact ?? nearest;
        }

        /// <summary>Читает блок тайников из записи настроек лута.</summary>
        /// <param name="value">Запись настроек лута.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Настройки тайников или <c>null</c>, если блок не задан.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static StashSettings? FromJson(JsonValue value, string sourcePath)
        {
            if (!value.TryGet("stashes", out var block))
            {
                return null;
            }

            if (!block.IsObject)
            {
                throw new DataException(sourcePath, CoreErrors.StashSettings, "stashes");
            }

            var baseChance = RequireChance(block, "baseChance", sourcePath);
            var chanceGrowth = RequireChance(block, "chanceGrowth", sourcePath);
            var chanceCap = RequireChance(block, "chanceCap", sourcePath);
            var jackpotBase = RequireChance(block, "jackpotBaseChance", sourcePath);
            var jackpotGrowth = RequireChance(block, "jackpotChanceGrowth", sourcePath);
            var jackpotCap = RequireChance(block, "jackpotChanceCap", sourcePath);
            var jackpotTable = DataMap.RequireString(block, "jackpotLootTableId", sourcePath);
            if (string.IsNullOrWhiteSpace(jackpotTable))
            {
                throw new DataException(sourcePath, CoreErrors.StashPool, "jackpot");
            }

            if (!block.TryGet("pools", out var poolsValue) || !poolsValue.IsArray || poolsValue.Items.Count == 0)
            {
                throw new DataException(sourcePath, CoreErrors.StashPoolEmpty);
            }

            var pools = new List<StashTierPool>(poolsValue.Items.Count);
            var tiers = new HashSet<int>();
            foreach (var item in poolsValue.Items)
            {
                if (!item.IsObject)
                {
                    throw new DataException(sourcePath, CoreErrors.StashPool, "pools");
                }

                var tier = DataMap.RequireInt(item, "tier", sourcePath);
                var tableId = DataMap.RequireString(item, "lootTableId", sourcePath);
                if (tier < 0 || string.IsNullOrWhiteSpace(tableId))
                {
                    throw new DataException(sourcePath, CoreErrors.StashPool, tier);
                }

                if (!tiers.Add(tier))
                {
                    throw new DataException(sourcePath, CoreErrors.StashPoolDuplicate, tier);
                }

                pools.Add(new StashTierPool(tier, tableId));
            }

            return new StashSettings(
                baseChance,
                chanceGrowth,
                chanceCap,
                jackpotBase,
                jackpotGrowth,
                jackpotCap,
                pools,
                jackpotTable);
        }

        private static double RequireChance(JsonValue value, string field, string sourcePath)
        {
            var chance = DataMap.RequireDouble(value, field, sourcePath);
            if (chance < 0 || chance > 1)
            {
                throw new DataException(sourcePath, CoreErrors.StashChanceRange, field);
            }

            return chance;
        }

        private static void CheckChance(double value, string field)
        {
            if (value < 0 || value > 1)
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.StashChanceRange, field),
                    field);
            }
        }
    }
}
