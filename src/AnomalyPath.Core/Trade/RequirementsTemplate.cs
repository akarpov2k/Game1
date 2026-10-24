#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Player;

namespace AnomalyPath.Core.Trade
{
    /// <summary>
    /// Условия доступа к торговцу и к отдельным товарам: предметы, репутация,
    /// завершённые квесты, открытия и минимальный тир. Все условия объединяются через «И».
    /// </summary>
    public sealed class RequirementsTemplate
    {
        /// <summary>Условия, которые выполняются всегда.</summary>
        public static readonly RequirementsTemplate Empty = new RequirementsTemplate(
            Array.Empty<RequirementItem>(),
            Array.Empty<RequirementReputation>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            0);

        /// <summary>Создаёт набор условий.</summary>
        /// <param name="items">Предметы в инвентаре.</param>
        /// <param name="reputation">Пороги репутации.</param>
        /// <param name="quests">Завершённые квесты.</param>
        /// <param name="unlocks">Полученные открытия.</param>
        /// <param name="tier">Минимальный уровень игрока.</param>
        public RequirementsTemplate(
            IReadOnlyList<RequirementItem> items,
            IReadOnlyList<RequirementReputation> reputation,
            IReadOnlyList<string> quests,
            IReadOnlyList<string> unlocks,
            int tier)
        {
            Items = items;
            Reputation = reputation;
            Quests = quests;
            Unlocks = unlocks;
            Tier = tier;
        }

        /// <summary>Предметы, которые нужно иметь.</summary>
        public IReadOnlyList<RequirementItem> Items { get; }

        /// <summary>Пороги репутации по фракциям.</summary>
        public IReadOnlyList<RequirementReputation> Reputation { get; }

        /// <summary>Завершённые квесты.</summary>
        public IReadOnlyList<string> Quests { get; }

        /// <summary>Полученные открытия.</summary>
        public IReadOnlyList<string> Unlocks { get; }

        /// <summary>Минимальный уровень игрока.</summary>
        public int Tier { get; }

        /// <summary>Проверяет, выполнены ли условия игроком.</summary>
        /// <param name="player">Состояние игрока; <c>null</c> — условия не выполнены.</param>
        /// <returns><c>true</c>, если все условия выполнены.</returns>
        public bool IsMet(PlayerState? player)
        {
            if (player == null)
            {
                return false;
            }

            if (player.Level < Tier)
            {
                return false;
            }

            foreach (var quest in Quests)
            {
                if (!player.HasCompletedQuest(quest))
                {
                    return false;
                }
            }

            foreach (var unlock in Unlocks)
            {
                if (!player.HasUnlock(unlock))
                {
                    return false;
                }
            }

            foreach (var condition in Reputation)
            {
                if (player.ReputationWith(condition.FactionId) < condition.Min)
                {
                    return false;
                }
            }

            foreach (var condition in Items)
            {
                if (CountItem(player, condition.ItemId) < condition.Count)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Читает условия из необязательного блока.</summary>
        /// <param name="value">Объект, в котором ищется поле <c>requirements</c>.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Условия; пустые, если поля нет.</returns>
        /// <exception cref="DataException">Блок условий задан неверно.</exception>
        public static RequirementsTemplate Read(JsonValue value, string sourcePath)
        {
            if (!value.TryGet("requirements", out var block) || block.Kind == JsonKind.Null)
            {
                return Empty;
            }

            if (!block.IsObject)
            {
                throw new DataException(sourcePath, CoreErrors.JsonNotObject);
            }

            var items = ReadItems(block, sourcePath);
            var reputation = ReadReputation(block, sourcePath);
            var quests = DataMap.OptionalStringArray(block, "quests");
            var unlocks = DataMap.OptionalStringArray(block, "unlocks");
            var tier = DataMap.OptionalInt(block, "tier", 0);
            if (tier < 0)
            {
                throw new DataException(sourcePath, CoreErrors.NonNegative, "tier");
            }

            return new RequirementsTemplate(items, reputation, quests, unlocks, tier);
        }

        /// <summary>Считает предмет в инвентаре и экипировке игрока.</summary>
        /// <param name="player">Игрок.</param>
        /// <param name="itemId">Идентификатор предмета.</param>
        /// <returns>Суммарное количество.</returns>
        public static int CountItem(PlayerState player, string itemId)
        {
            var total = 0;
            foreach (var item in player.Inventory.Items)
            {
                if (string.Equals(item.ItemId, itemId, StringComparison.Ordinal))
                {
                    total += item.Stack;
                }
            }

            foreach (var pair in player.Equipment.Slots)
            {
                if (string.Equals(pair.Value.ItemId, itemId, StringComparison.Ordinal))
                {
                    total += pair.Value.Stack;
                }
            }

            foreach (var item in player.Equipment.QuickSlots)
            {
                if (item != null && string.Equals(item.ItemId, itemId, StringComparison.Ordinal))
                {
                    total += item.Stack;
                }
            }

            foreach (var item in player.Equipment.Artifacts)
            {
                if (item != null && string.Equals(item.ItemId, itemId, StringComparison.Ordinal))
                {
                    total += item.Stack;
                }
            }

            return total;
        }

        private static IReadOnlyList<RequirementItem> ReadItems(JsonValue value, string sourcePath)
        {
            var result = new List<RequirementItem>();
            if (!value.TryGet("items", out var items) || !items.IsArray)
            {
                return result;
            }

            foreach (var item in items.Items)
            {
                if (!item.IsObject)
                {
                    throw new DataException(sourcePath, CoreErrors.JsonNotObject);
                }

                var itemId = DataMap.RequireString(item, "itemId", sourcePath);
                var count = DataMap.RequireInt(item, "count", sourcePath);
                if (count < 1)
                {
                    throw new DataException(sourcePath, CoreErrors.MinimumOne, "count");
                }

                result.Add(new RequirementItem(itemId, count));
            }

            return result;
        }

        private static IReadOnlyList<RequirementReputation> ReadReputation(JsonValue value, string sourcePath)
        {
            var result = new List<RequirementReputation>();
            if (!value.TryGet("reputation", out var reputation) || !reputation.IsArray)
            {
                return result;
            }

            foreach (var item in reputation.Items)
            {
                if (!item.IsObject)
                {
                    throw new DataException(sourcePath, CoreErrors.JsonNotObject);
                }

                result.Add(new RequirementReputation(
                    DataMap.RequireString(item, "factionId", sourcePath),
                    DataMap.RequireInt(item, "min", sourcePath)));
            }

            return result;
        }
    }
}
