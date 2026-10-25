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
    /// <summary>Часть класса <see cref="QuestScreen"/>: действия.</summary>
    public sealed partial class QuestScreen
    {
        private void Accept(string questId)
        {
            var session = _host.Session;
            if (session == null || _traderActorId == null)
            {
                return;
            }

            var result = _host.Quests.Accept(session, _traderActorId, questId);
            if (result.Succeeded && _host.Quests.TryGetTemplate(questId, out var template))
            {
                ShowToast(UiText.Format(
                    _host.Texts,
                    UiTextKey.Quest_Accepted,
                    _host.Texts.Get(template.TitleKey)));
                Refresh();
                return;
            }

            ShowToast(UiText.Get(_host.Texts, KeyFor(result.Status)), error: true);
        }

        private void TurnIn(string questId)
        {
            var session = _host.Session;
            if (session == null || _traderActorId == null)
            {
                return;
            }

            var result = _host.Quests.TurnIn(session, _traderActorId, questId);
            if (result.Succeeded && _host.Quests.TryGetTemplate(questId, out var template))
            {
                ShowToast(UiText.Format(
                    _host.Texts,
                    UiTextKey.Quest_Completed,
                    _host.Texts.Get(template.TitleKey),
                    result.Money));
                Refresh();
                return;
            }

            ShowToast(UiText.Get(_host.Texts, KeyFor(result.Status)), error: true);
        }

        private void CancelOffer(string questId)
        {
            var session = _host.Session;
            if (session == null || _traderActorId == null)
            {
                return;
            }

            var result = _host.Quests.CancelOffer(session, _traderActorId, questId);
            if (result.Succeeded)
            {
                ShowToast(UiText.Get(_host.Texts, UiTextKey.Quest_Offer_Cancelled));
                Refresh();
                return;
            }

            ShowToast(UiText.Get(_host.Texts, KeyFor(result.Status)), error: true);
        }

        private void CancelQuest(string questId)
        {
            var session = _host.Session;
            if (session == null)
            {
                return;
            }

            var result = _host.Quests.CancelQuest(session, questId);
            if (result.Succeeded)
            {
                ShowToast(UiText.Get(_host.Texts, UiTextKey.Quest_Cancelled));
                Refresh();
                return;
            }

            ShowToast(UiText.Get(_host.Texts, KeyFor(result.Status)), error: true);
        }
    }
}
