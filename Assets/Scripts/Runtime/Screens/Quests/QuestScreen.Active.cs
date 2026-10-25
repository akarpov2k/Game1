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
    /// <summary>Часть класса <see cref="QuestScreen"/>: активные квесты.</summary>
    public sealed partial class QuestScreen
    {
        private void BuildActive(GameSession session)
        {
            var active = _host.Quests.GetActive(session);
            if (active.Count == 0)
            {
                AddEmpty(_activeScroll.Content, UiTextKey.Quest_Empty_Active);
                _activeScroll.SetContentHeight(QuestMetrics.ListHeight);
                return;
            }

            for (var index = 0; index < active.Count; index++)
            {
                var quest = active[index];
                if (!_host.Quests.TryGetTemplate(quest.QuestId, out var template))
                {
                    continue;
                }

                var progress = _host.Quests.GetProgress(session, quest);
                var row = Row(
                    _activeScroll.Content,
                    "Active_" + quest.QuestId,
                    index,
                    QuestMetrics.ActiveHeight,
                    progress.Ready ? QuestPalette.ReadyRow : QuestPalette.Row);
                var width = 900f - 24f;
                UiWidgets.Label(
                    _host,
                    row,
                    "Title",
                    _host.Texts.Get(template.TitleKey),
                    23,
                    TextAnchor.MiddleLeft,
                    UiFactory.TextColor,
                    16f,
                    8f,
                    width - 260f,
                    30f);
                UiWidgets.Label(
                    _host,
                    row,
                    "Progress",
                    ProgressLine(template, progress),
                    19,
                    TextAnchor.MiddleLeft,
                    progress.Ready ? UiFactory.ToastColor : UiFactory.TextColor,
                    16f,
                    40f,
                    width - 260f,
                    26f);
                UiWidgets.Label(
                    _host,
                    row,
                    "Targets",
                    TargetsLine(quest, template, progress),
                    17,
                    TextAnchor.UpperLeft,
                    UiFactory.MutedTextColor,
                    16f,
                    70f,
                    width - 260f,
                    72f);

                var cancel = _host.Ui.CreateButton(
                    row,
                    UiText.Get(_host.Texts, UiTextKey.Quest_Cancel),
                    () => CancelQuest(quest.QuestId));
                cancel.gameObject.name = "CancelQuest";
                PlaceButton((RectTransform)cancel.transform, -16f, 110f);

                if (_traderActorId != null &&
                    string.Equals(quest.GiverActorId, _traderActorId, StringComparison.Ordinal))
                {
                    var button = _host.Ui.CreateButton(
                        row,
                        UiText.Get(_host.Texts, UiTextKey.Quest_TurnIn),
                        () => TurnIn(quest.QuestId));
                    button.gameObject.name = "TurnIn";
                    button.interactable = progress.Ready;
                    PlaceButton((RectTransform)button.transform, -132f, 110f);
                }
                else if (progress.Ready)
                {
                    UiWidgets.Label(
                        _host,
                        row,
                        "Ready",
                        UiText.Get(_host.Texts, UiTextKey.Quest_Ready),
                        17,
                        TextAnchor.MiddleLeft,
                        UiFactory.ToastColor,
                        16f,
                        144f,
                        300f,
                        24f);
                }
            }

            _activeScroll.SetContentHeight(ContentHeight(active.Count, QuestMetrics.ActiveHeight));
        }
    }
}
