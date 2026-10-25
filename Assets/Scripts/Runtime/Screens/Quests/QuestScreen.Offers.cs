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
    /// <summary>Часть класса <see cref="QuestScreen"/>: предложения.</summary>
    public sealed partial class QuestScreen
    {
        private void BuildOffers(GameSession session)
        {
            var offers = _host.Quests.GetOffers(session, _traderActorId!);
            if (offers.Count == 0)
            {
                AddEmpty(_offersScroll.Content, UiTextKey.Quest_Empty_Offers);
                _offersScroll.SetContentHeight(QuestMetrics.ListHeight);
                return;
            }

            for (var index = 0; index < offers.Count; index++)
            {
                var offer = offers[index];
                var row = Row(
                    _offersScroll.Content,
                    "Offer_" + offer.QuestId,
                    index,
                    QuestMetrics.OfferHeight,
                    QuestPalette.Row);
                var width = QuestMetrics.LeftColumnWidth - 24f;
                UiWidgets.Label(
                    _host,
                    row,
                    "Title",
                    _host.Texts.Get(offer.Template.TitleKey),
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
                    "Description",
                    _host.Texts.Get(offer.Template.DescriptionKey),
                    18,
                    TextAnchor.UpperLeft,
                    UiFactory.MutedTextColor,
                    16f,
                    40f,
                    width - 260f,
                    48f);
                UiWidgets.Label(
                    _host,
                    row,
                    "Difficulty",
                    UiText.Format(_host.Texts, UiTextKey.Quest_Difficulty, offer.Template.Difficulty),
                    18,
                    TextAnchor.MiddleLeft,
                    UiFactory.MutedTextColor,
                    16f,
                    96f,
                    260f,
                    24f);
                UiWidgets.Label(
                    _host,
                    row,
                    "Reward",
                    UiText.Format(_host.Texts, UiTextKey.Quest_Reward, offer.Template.Rewards.Money),
                    18,
                    TextAnchor.MiddleLeft,
                    UiFactory.MutedTextColor,
                    290f,
                    96f,
                    300f,
                    24f);
                UiWidgets.Label(
                    _host,
                    row,
                    "Targets",
                    TargetsLine(
                        offer.State,
                        offer.Template,
                        new QuestProgress(offer.Template.Type, 0, offer.State.RequiredCount, false)),
                    17,
                    TextAnchor.UpperLeft,
                    UiFactory.MutedTextColor,
                    16f,
                    122f,
                    width - 260f,
                    40f);
                var button = _host.Ui.CreateButton(
                    row,
                    UiText.Get(_host.Texts, UiTextKey.Quest_Take),
                    () => Accept(offer.QuestId));
                button.gameObject.name = "Take";
                PlaceButton((RectTransform)button.transform, -16f, 110f);

                var cancel = _host.Ui.CreateButton(
                    row,
                    UiText.Get(_host.Texts, UiTextKey.Quest_Cancel),
                    () => CancelOffer(offer.QuestId));
                cancel.gameObject.name = "CancelOffer";
                PlaceButton((RectTransform)cancel.transform, -132f, 110f);
            }

            _offersScroll.SetContentHeight(ContentHeight(offers.Count, QuestMetrics.OfferHeight));
        }
    }
}
