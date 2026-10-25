#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Quests;
using AnomalyPath.Core.Rules;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.Templates;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Services
{
    /// <summary>Часть класса <see cref="QuestService"/>: награды и предметы.</summary>
    public sealed partial class QuestService
    {
        private void AddEdge(
            WorldMap map,
            string edgeId,
            string fromId,
            string toId,
            string via)
        {
            var cost = _topology.Topology.GetCost(via).Scale(1);
            map.AddEdge(new WorldEdge(edgeId, fromId, toId, via, 1, cost, oneWay: false, hidden: false));
            map.AddEdge(new WorldEdge(edgeId + "_back", toId, fromId, via, 1, cost, oneWay: false, hidden: false));
        }

        private bool CanFitRewards(PlayerState player, QuestRewardTemplate rewards)
        {
            if (rewards.Items.Count == 0)
            {
                return true;
            }

            var probes = new List<ItemInstance>();
            foreach (var item in rewards.Items)
            {
                if (!_items.TryGet(item.ItemId, out var template))
                {
                    return false;
                }

                probes.Add(new ItemInstance(
                    "quest-probe",
                    item.ItemId,
                    item.Count,
                    NewDurability(template),
                    null));
            }

            return _inventory.CanFit(player.Inventory, probes);
        }

        private bool RemoveRequiredItems(PlayerState player, QuestTemplate template, QuestState quest)
        {
            switch (template.Type)
            {
                case QuestType.GatherArtifacts:
                {
                    var remaining = quest.RequiredCount;
                    foreach (var target in quest.Items)
                    {
                        remaining -= RemoveInventory(player, target.ItemId, remaining);
                        if (remaining <= 0)
                        {
                            return true;
                        }
                    }

                    return remaining <= 0;
                }

                case QuestType.FindItems:
                {
                    foreach (var target in quest.Items)
                    {
                        if (RemoveInventory(player, target.ItemId, target.Count) < target.Count)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                default:
                    return true;
            }
        }

        private static int RemoveInventory(PlayerState player, string itemId, int count)
        {
            var remaining = count;
            var items = player.Inventory.Items;
            for (var index = items.Count - 1; index >= 0 && remaining > 0; index--)
            {
                var item = items[index];
                if (!string.Equals(item.ItemId, itemId, StringComparison.Ordinal))
                {
                    continue;
                }

                var take = Math.Min(item.Stack, remaining);
                item.Stack -= take;
                remaining -= take;
                if (item.Stack <= 0)
                {
                    player.Inventory.Remove(item);
                }
            }

            return count - remaining;
        }

        private static int CountInventory(PlayerState player, string itemId)
        {
            var total = 0;
            foreach (var item in player.Inventory.Items)
            {
                if (string.Equals(item.ItemId, itemId, StringComparison.Ordinal))
                {
                    total += item.Stack;
                }
            }

            return total;
        }

        private static int? NewDurability(ItemTemplate template)
        {
            if (template.Weapon != null)
            {
                return template.Weapon.DurabilityMax;
            }

            return template.ArmorDurabilityMax > 0 ? template.ArmorDurabilityMax : null;
        }

        private static int RequiredCount(QuestTemplate template)
        {
            switch (template.Type)
            {
                case QuestType.ClearLocation:
                    return 1;
                case QuestType.KillMutants:
                    return template.KillMutants?.Count ?? 1;
                case QuestType.GatherArtifacts:
                    return template.GatherArtifacts?.Count ?? 1;
                case QuestType.FindItems:
                    return template.FindItems?.NpcCount ?? 1;
                default:
                    return 1;
            }
        }
    }
}
