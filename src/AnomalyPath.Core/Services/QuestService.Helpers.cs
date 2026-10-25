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
    /// <summary>Часть класса <see cref="QuestService"/>: служебные помощники.</summary>
    public sealed partial class QuestService
    {
        private static bool IsActive(GameSession session, string questId) =>
            session.State.TryGetActiveQuest(questId, out _);

        private bool IsOnCooldown(GameSession session, string questId, double now)
        {
            foreach (var quest in session.State.Quests)
            {
                if (string.Equals(quest.QuestId, questId, StringComparison.Ordinal) &&
                    quest.Status == QuestStatus.Completed &&
                    now < quest.CooldownUntil)
                {
                    return true;
                }
            }

            return false;
        }

        private static int CountActive(GameSession session)
        {
            var count = 0;
            foreach (var quest in session.State.Quests)
            {
                if (quest.Status == QuestStatus.Active)
                {
                    count++;
                }
            }

            return count;
        }

        private static List<string> PickDistinct(
            IReadOnlyList<string> candidates,
            int count,
            IWorldRandom random)
        {
            var remaining = new List<string>(candidates);
            var result = new List<string>();
            var take = Math.Min(Math.Max(1, count), remaining.Count);
            for (var index = 0; index < take; index++)
            {
                var pick = random.NextInt(0, remaining.Count);
                result.Add(remaining[pick]);
                remaining.RemoveAt(pick);
            }

            return result;
        }

        private static bool Contains(IReadOnlyList<string> values, string value)
        {
            foreach (var item in values)
            {
                if (string.Equals(item, value, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsCategory(IReadOnlyList<ItemCategory> values, ItemCategory value)
        {
            foreach (var item in values)
            {
                if (item == value)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
