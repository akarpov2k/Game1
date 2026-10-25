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
    public sealed partial class QuestScreen : ScreenBase
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
    }
}
