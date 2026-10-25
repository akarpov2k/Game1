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
    /// <summary>Часть класса <see cref="QuestService"/>: состояние квестов.</summary>
    public sealed partial class QuestService
    {
        /// <inheritdoc />
        public bool TryGetTemplate(string questId, out QuestTemplate template) =>
            _quests.TryGet(questId, out template);

        /// <inheritdoc />
        public IReadOnlyList<QuestState> GetActive(GameSession session)
        {
            var result = new List<QuestState>();
            if (session == null)
            {
                return result;
            }

            foreach (var quest in session.State.Quests)
            {
                if (quest.Status == QuestStatus.Active)
                {
                    result.Add(quest);
                }
            }

            return result;
        }

        /// <inheritdoc />
        public QuestProgress GetProgress(GameSession session, QuestState quest)
        {
            if (session == null || quest == null)
            {
                return new QuestProgress(QuestType.ClearLocation, 0, 1, false);
            }

            if (!_quests.TryGet(quest.QuestId, out var template))
            {
                return new QuestProgress(QuestType.ClearLocation, 0, 1, false);
            }

            var player = session.State.Player;
            if (player == null)
            {
                return new QuestProgress(template.Type, 0, quest.RequiredCount, false);
            }

            switch (template.Type)
            {
                case QuestType.ClearLocation:
                {
                    var cleared = quest.Cleared;
                    if (!cleared && quest.LocationInstanceId != null &&
                        session.State.World.Map.TryGetLocation(quest.LocationInstanceId, out var location) &&
                        location != null)
                    {
                        cleared = location.Cleared;
                    }

                    return new QuestProgress(template.Type, cleared ? 1 : 0, 1, cleared);
                }

                case QuestType.KillMutants:
                    return new QuestProgress(
                        template.Type,
                        quest.KillCount,
                        quest.RequiredCount,
                        quest.KillCount >= quest.RequiredCount);

                case QuestType.GatherArtifacts:
                {
                    var items = new List<QuestItemProgress>();
                    var total = 0;
                    foreach (var target in quest.Items)
                    {
                        var current = CountInventory(player, target.ItemId);
                        total += current;
                        items.Add(new QuestItemProgress(target.ItemId, current, target.Count));
                    }

                    return new QuestProgress(
                        template.Type,
                        total,
                        quest.RequiredCount,
                        total >= quest.RequiredCount,
                        items);
                }

                case QuestType.FindItems:
                {
                    var items = new List<QuestItemProgress>();
                    var itemsReady = true;
                    foreach (var target in quest.Items)
                    {
                        var current = CountInventory(player, target.ItemId);
                        var met = current >= target.Count;
                        itemsReady &= met;
                        items.Add(new QuestItemProgress(target.ItemId, current, target.Count));
                    }

                    var npcReady = quest.KillCount >= quest.RequiredCount;
                    return new QuestProgress(
                        template.Type,
                        quest.KillCount,
                        quest.RequiredCount,
                        itemsReady && npcReady,
                        items,
                        quest.KillCount,
                        quest.RequiredCount);
                }

                default:
                    return new QuestProgress(template.Type, 0, quest.RequiredCount, false);
            }
        }

        /// <inheritdoc />
        public void NotifyActorKilled(GameSession session, string actorId)
        {
            if (session == null || string.IsNullOrWhiteSpace(actorId))
            {
                return;
            }

            foreach (var quest in session.State.Quests)
            {
                if (quest.Status != QuestStatus.Active || !Contains(quest.ActorIds, actorId))
                {
                    continue;
                }

                if (!_quests.TryGet(quest.QuestId, out var template))
                {
                    continue;
                }

                if ((template.Type == QuestType.KillMutants || template.Type == QuestType.FindItems) &&
                    quest.KillCount < quest.RequiredCount)
                {
                    quest.AddKill();
                }
            }
        }

        /// <inheritdoc />
        public void NotifyLocationCleared(GameSession session, string locationInstanceId)
        {
            if (session == null || string.IsNullOrWhiteSpace(locationInstanceId))
            {
                return;
            }

            if (session.State.World.Map.TryGetLocation(locationInstanceId, out var location) && location != null)
            {
                location.MarkCleared();
            }

            foreach (var quest in session.State.Quests)
            {
                if (quest.Status != QuestStatus.Active || quest.Cleared)
                {
                    continue;
                }

                if (!_quests.TryGet(quest.QuestId, out var template) ||
                    template.Type != QuestType.ClearLocation)
                {
                    continue;
                }

                if (string.Equals(quest.LocationInstanceId, locationInstanceId, StringComparison.Ordinal))
                {
                    quest.MarkCleared();
                }
            }
        }
    }
}
