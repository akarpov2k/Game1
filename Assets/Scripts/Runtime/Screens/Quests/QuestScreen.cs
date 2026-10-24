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
    /// <summary>
    /// Журнал заданий: предложения торговца, активные задания, прогресс и сдача.
    /// Разметка строится кодом; текст берётся только из каталога строк.
    /// </summary>
    public sealed class QuestScreen : ScreenBase
    {
        private const string PanelName = "Quests";

        private readonly IUiHost _host;
        private readonly Text _title;
        private readonly Text _money;
        private readonly Text _offersHeader;
        private readonly Text _activeHeader;
        private readonly Text _toast;
        private readonly ScrollArea _offersScroll;
        private readonly ScrollArea _activeScroll;
        private string? _traderActorId;

        /// <summary>Создаёт экран заданий.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="canvas">Родительский канвас.</param>
        public QuestScreen(IUiHost host, Transform canvas)
            : base(host, canvas, PanelName)
        {
            _host = host;

            _title = host.Ui.CreateTitle(Content, string.Empty);
            _title.gameObject.name = "QuestTitle";
            PlaceTop(_title.rectTransform, QuestMetrics.TitleTop, QuestMetrics.TitleHeight);

            _money = host.Ui.CreateText(Content, string.Empty, 26, TextAnchor.MiddleRight);
            _money.gameObject.name = "QuestMoney";
            var moneyRect = _money.rectTransform;
            moneyRect.anchorMin = new Vector2(1f, 1f);
            moneyRect.anchorMax = new Vector2(1f, 1f);
            moneyRect.pivot = new Vector2(1f, 1f);
            moneyRect.anchoredPosition = new Vector2(-QuestMetrics.Side, -QuestMetrics.TitleTop);
            moneyRect.sizeDelta = new Vector2(280f, 56f);

            _offersHeader = host.Ui.CreateText(Content, string.Empty, 26, TextAnchor.MiddleLeft);
            _offersHeader.gameObject.name = "QuestOffersHeader";
            _offersHeader.raycastTarget = false;
            PlaceSection(_offersHeader.rectTransform, QuestMetrics.LeftColumn, QuestMetrics.SectionTop);
            _activeHeader = host.Ui.CreateText(Content, string.Empty, 26, TextAnchor.MiddleLeft);
            _activeHeader.gameObject.name = "QuestActiveHeader";
            _activeHeader.raycastTarget = false;
            PlaceSection(_activeHeader.rectTransform, QuestMetrics.RightColumn, QuestMetrics.SectionTop);

            _offersScroll = CreateScroll("QuestOffers", QuestMetrics.LeftColumn, QuestMetrics.LeftColumnWidth);
            _activeScroll = CreateScroll("QuestActive", QuestMetrics.RightColumn, 900f);

            _toast = host.Ui.CreateText(Content, string.Empty, 22, TextAnchor.MiddleCenter, UiFactory.ToastColor);
            _toast.gameObject.name = "QuestToast";
            var toastRect = _toast.rectTransform;
            toastRect.anchorMin = new Vector2(0f, 0f);
            toastRect.anchorMax = new Vector2(1f, 0f);
            toastRect.pivot = new Vector2(0.5f, 0f);
            toastRect.offsetMin = new Vector2(QuestMetrics.Side, 96f);
            toastRect.offsetMax = new Vector2(-QuestMetrics.Side, 136f);

            var back = host.Ui.CreateButton(Content, UiText.Get(host.Texts, UiTextKey.Common_Back), Back);
            back.gameObject.name = "QuestBack";
            var backRect = (RectTransform)back.transform;
            backRect.anchorMin = new Vector2(1f, 0f);
            backRect.anchorMax = new Vector2(1f, 0f);
            backRect.pivot = new Vector2(1f, 0f);
            backRect.anchoredPosition = new Vector2(-QuestMetrics.Side, 32f);
            backRect.sizeDelta = new Vector2(180f, QuestMetrics.BottomButtonHeight);
        }

        /// <summary>Открывает журнал для торговца или без выдачи.</summary>
        /// <param name="traderActorId">Торговец-квестодатель или <c>null</c>.</param>
        public void Show(string? traderActorId)
        {
            _traderActorId = traderActorId;
            ApplyTheme();
            _host.Router.Show(Panel);
            Canvas.ForceUpdateCanvases();
            Refresh();
        }

        /// <summary>Esc возвращает к торговцу или на игровой экран.</summary>
        /// <returns><c>true</c>, если Esc обработан.</returns>
        public override bool HandleEscape()
        {
            Back();
            return true;
        }

        private void Back()
        {
            if (_traderActorId != null)
            {
                _host.ShowTrader(_traderActorId);
                return;
            }

            _host.ShowGame(false);
        }

        private void Refresh()
        {
            var session = _host.Session;
            var texts = _host.Texts;
            if (session == null)
            {
                _title.text = UiText.Get(texts, UiTextKey.Common_No_Game);
                _money.text = string.Empty;
                _offersHeader.text = string.Empty;
                _activeHeader.text = string.Empty;
                Clear(_offersScroll.Content);
                Clear(_activeScroll.Content);
                _offersScroll.SetContentHeight(QuestMetrics.ListHeight);
                _activeScroll.SetContentHeight(QuestMetrics.ListHeight);
                return;
            }

            var player = session.State.Player;
            _money.text = player == null ? string.Empty : UiText.Format(texts, UiTextKey.Common_Money, player.Money);
            _offersHeader.text = UiText.Get(texts, UiTextKey.Quest_Section_Offers);
            _activeHeader.text = UiText.Get(texts, UiTextKey.Quest_Section_Active);
            Clear(_offersScroll.Content);
            Clear(_activeScroll.Content);

            if (_traderActorId != null && _host.Actors.Contains(_traderActorId))
            {
                var actor = _host.Actors.Get(_traderActorId);
                _title.text = UiText.Format(texts, UiTextKey.Quest_Trader_Title, texts.Get(actor.NameKey));
                BuildOffers(session);
            }
            else
            {
                _title.text = UiText.Get(texts, UiTextKey.Quest_Title);
                AddEmpty(_offersScroll.Content, UiTextKey.Quest_No_Trader);
                _offersScroll.SetContentHeight(QuestMetrics.ListHeight);
            }

            BuildActive(session);
        }

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

        private void ShowToast(string text, bool error = false)
        {
            _toast.text = text;
            _toast.color = error ? QuestPalette.Error : UiFactory.ToastColor;
        }

        private Transform Row(Transform content, string name, int index, float height, Color color)
        {
            var row = _host.Ui.CreateImage(name, content, color);
            row.raycastTarget = false;
            var rect = row.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            var top = index * (height + QuestMetrics.RowGap);
            rect.offsetMin = new Vector2(0f, -(top + height));
            rect.offsetMax = new Vector2(0f, -top);
            return row.transform;
        }

        private ScrollArea CreateScroll(string name, float left, float width)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(ScrollArea));
            go.transform.SetParent(Content, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(left, -QuestMetrics.ListTop);
            rect.sizeDelta = new Vector2(width, QuestMetrics.ListHeight);
            var scroll = go.GetComponent<ScrollArea>();
            scroll.Configure(QuestMetrics.RowGap);
            return scroll;
        }

        private void AddEmpty(RectTransform content, UiTextKey key)
        {
            var label = UiWidgets.Label(
                _host,
                content,
                "Empty",
                UiText.Get(_host.Texts, key),
                20,
                TextAnchor.MiddleCenter,
                UiFactory.MutedTextColor,
                8f,
                8f,
                600f,
                32f);
            label.raycastTarget = false;
        }

        private static float ContentHeight(int count, float rowHeight) =>
            Mathf.Max(QuestMetrics.ListHeight, 16f + (count * (rowHeight + QuestMetrics.RowGap)));

        private static void Clear(RectTransform content)
        {
            for (var index = content.childCount - 1; index >= 0; index--)
            {
                var child = content.GetChild(index).gameObject;
                child.SetActive(false);
                UnityEngine.Object.Destroy(child);
            }
        }

        private static void PlaceTop(RectTransform rect, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(QuestMetrics.Side, -(top + height));
            rect.offsetMax = new Vector2(-QuestMetrics.Side, -top);
        }

        private static void PlaceSection(RectTransform rect, float left, float top)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(left, -top);
            rect.sizeDelta = new Vector2(600f, QuestMetrics.SectionHeight);
        }

        private static void PlaceButton(RectTransform rect, float right, float width)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(right, -48f);
            rect.sizeDelta = new Vector2(width, QuestMetrics.ButtonHeight);
        }

        private static UiTextKey KeyFor(QuestResultStatus status) => status switch
        {
            QuestResultStatus.QuestNotFound => UiTextKey.Quest_Result_Not_Found,
            QuestResultStatus.NotRepeatable => UiTextKey.Quest_Result_Not_Repeatable,
            QuestResultStatus.NotOffered => UiTextKey.Quest_Result_Not_Offered,
            QuestResultStatus.TraderNotFound => UiTextKey.Quest_Result_Trader_Not_Found,
            QuestResultStatus.NotTrader => UiTextKey.Quest_Result_Not_Trader,
            QuestResultStatus.WrongLocation => UiTextKey.Quest_Result_Wrong_Location,
            QuestResultStatus.Unavailable => UiTextKey.Quest_Result_Unavailable,
            QuestResultStatus.RequirementsNotMet => UiTextKey.Quest_Result_Requirements,
            QuestResultStatus.AlreadyActive => UiTextKey.Quest_Result_Already_Active,
            QuestResultStatus.ActiveLimit => UiTextKey.Quest_Result_Active_Limit,
            QuestResultStatus.Cooldown => UiTextKey.Quest_Result_Cooldown,
            QuestResultStatus.NoTargets => UiTextKey.Quest_Result_No_Targets,
            QuestResultStatus.NotReady => UiTextKey.Quest_Result_Not_Ready,
            QuestResultStatus.WrongGiver => UiTextKey.Quest_Result_Wrong_Giver,
            QuestResultStatus.NoRoom => UiTextKey.Quest_Result_No_Room,
            _ => UiTextKey.Quest_Result_Invalid
        };
    }
}
