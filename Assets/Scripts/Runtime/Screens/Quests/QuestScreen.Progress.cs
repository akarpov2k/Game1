#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Quests;
using AnomalyPath.Core.Session;
using AnomalyPath.Runtime.Screens;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Quests
{
    /// <summary>Часть класса <see cref="QuestScreen"/>: прогресс.</summary>
    public sealed partial class QuestScreen
    {
        private string ProgressLine(QuestTemplate template, QuestProgress progress)
        {
            switch (template.Type)
            {
                case QuestType.ClearLocation:
                    return progress.Ready
                        ? UiText.Get(_host.Texts, UiTextKey.Quest_Ready)
                        : UiText.Format(_host.Texts, UiTextKey.Quest_Progress, progress.Current, progress.Required);
                case QuestType.FindItems:
                    return UiText.Format(_host.Texts, UiTextKey.Quest_Npc_Progress, progress.NpcCurrent, progress.NpcRequired);
                default:
                    return UiText.Format(_host.Texts, UiTextKey.Quest_Progress, progress.Current, progress.Required);
            }
        }

        private string TargetsLine(QuestState quest, QuestTemplate template, QuestProgress progress)
        {
            var parts = new List<string>();
            if (quest.LocationTemplateId != null && _host.Locations.Contains(quest.LocationTemplateId))
            {
                var location = _host.Locations.Get(quest.LocationTemplateId);
                parts.Add(UiText.Format(
                    _host.Texts,
                    UiTextKey.Quest_Target_Location,
                    _host.Texts.Get(location.TitleKey)));
            }

            if (quest.ActorIds.Count > 0)
            {
                var names = new List<string>();
                foreach (var actorId in quest.ActorIds)
                {
                    if (_host.Actors.Contains(actorId))
                    {
                        names.Add(_host.Texts.Get(_host.Actors.Get(actorId).NameKey));
                    }
                }

                if (names.Count > 0)
                {
                    parts.Add(UiText.Format(_host.Texts, UiTextKey.Quest_Target_Actors, string.Join(", ", names)));
                }
            }

            if (quest.Items.Count > 0)
            {
                var names = new List<string>();
                foreach (var item in quest.Items)
                {
                    var name = _host.Items.TryGet(item.ItemId, out var itemTemplate)
                        ? _host.Texts.Get(itemTemplate.NameKey)
                        : item.ItemId;
                    names.Add(name + " " + CurrentItem(progress, item.ItemId) + "/" + item.Count);
                }

                parts.Add(UiText.Format(_host.Texts, UiTextKey.Quest_Target_Items, string.Join(", ", names)));
            }

            return string.Join("\n", parts);
        }

        private static int CurrentItem(QuestProgress progress, string itemId)
        {
            foreach (var item in progress.Items)
            {
                if (string.Equals(item.ItemId, itemId, StringComparison.Ordinal))
                {
                    return item.Current;
                }
            }

            return 0;
        }
    }
}
