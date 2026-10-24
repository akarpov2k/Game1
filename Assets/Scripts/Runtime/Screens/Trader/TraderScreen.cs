#nullable enable
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.Trade;
using AnomalyPath.Runtime.Screens;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Trader
{
    /// <summary>
    /// Экран торговца: портрет, деньги, ассортимент с условиями и продажа предметов игрока.
    /// Разметка строится кодом; текст берётся только из каталога строк.
    /// </summary>
    public sealed class TraderScreen : ScreenBase
    {
        private const string PanelName = "Trader";

        private readonly IUiHost _host;
        private readonly Text _title;
        private readonly Text _money;
        private readonly Image _portraitFrame;
        private readonly Image _portrait;
        private readonly Text _portraitFallback;
        private readonly Text _description;
        private readonly Text _buyHeader;
        private readonly Text _sellHeader;
        private readonly Text _toast;
        private readonly ScrollArea _buyScroll;
        private readonly ScrollArea _sellScroll;
        private string _actorId = string.Empty;

        /// <summary>Создаёт экран торговца.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="canvas">Родительский канвас.</param>
        public TraderScreen(IUiHost host, Transform canvas)
            : base(host, canvas, PanelName)
        {
            _host = host;

            _title = host.Ui.CreateTitle(Content, string.Empty);
            _title.gameObject.name = "TraderTitle";
            var titleRect = _title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.offsetMin = new Vector2(TraderMetrics.Side, -(TraderMetrics.TitleTop + TraderMetrics.TitleHeight));
            titleRect.offsetMax = new Vector2(-TraderMetrics.Side, -TraderMetrics.TitleTop);

            _money = host.Ui.CreateText(Content, string.Empty, 26, TextAnchor.MiddleRight);
            _money.gameObject.name = "TraderMoney";
            var moneyRect = _money.rectTransform;
            moneyRect.anchorMin = new Vector2(1f, 1f);
            moneyRect.anchorMax = new Vector2(1f, 1f);
            moneyRect.pivot = new Vector2(1f, 1f);
            moneyRect.anchoredPosition = new Vector2(-TraderMetrics.Side, -TraderMetrics.TitleTop);
            moneyRect.sizeDelta = new Vector2(280f, 56f);

            _portraitFrame = UiWidgets.Box(
                host,
                Content,
                "PortraitFrame",
                TraderMetrics.Side,
                TraderMetrics.PortraitTop,
                TraderMetrics.PortraitWidth,
                TraderMetrics.PortraitFullHeight,
                TraderPalette.PortraitFrame);
            _portrait = host.Ui.CreateImage("Portrait", _portraitFrame.transform, Color.white);
            _portrait.preserveAspect = true;
            _portrait.raycastTarget = false;
            _portraitFallback = UiWidgets.Label(
                host,
                _portraitFrame.transform,
                "PortraitFallback",
                string.Empty,
                20,
                TextAnchor.MiddleCenter,
                UiFactory.MutedTextColor,
                8f,
                8f,
                TraderMetrics.PortraitWidth - 16f,
                TraderMetrics.PortraitFullHeight - 16f);
            _description = host.Ui.CreateText(Content, string.Empty, 20, TextAnchor.UpperLeft, UiFactory.MutedTextColor);
            _description.gameObject.name = "TraderDescription";
            _description.raycastTarget = false;

            _buyHeader = SectionHeader("BuyHeader", UiTextKey.Trader_Section_Buy, 104f);
            _buyScroll = CreateScroll("BuyScroll", TraderMetrics.BuyTop, TraderMetrics.BuyHeight);
            _sellHeader = SectionHeader("SellHeader", UiTextKey.Trader_Section_Sell, 524f);
            _sellScroll = CreateScroll("SellScroll", TraderMetrics.SellTop, TraderMetrics.SellHeight);

            _toast = host.Ui.CreateText(Content, string.Empty, 22, TextAnchor.MiddleCenter, UiFactory.ToastColor);
            _toast.gameObject.name = "TraderToast";
            var toastRect = _toast.rectTransform;
            toastRect.anchorMin = new Vector2(0f, 0f);
            toastRect.anchorMax = new Vector2(1f, 0f);
            toastRect.pivot = new Vector2(0.5f, 0f);
            toastRect.offsetMin = new Vector2(TraderMetrics.Side, 96f);
            toastRect.offsetMax = new Vector2(-(TraderMetrics.Side + 220f), 136f);

            var back = host.Ui.CreateButton(Content, UiText.Get(host.Texts, UiTextKey.Common_Back), Back);
            back.gameObject.name = "TraderBack";
            var backRect = (RectTransform)back.transform;
            backRect.anchorMin = new Vector2(1f, 0f);
            backRect.anchorMax = new Vector2(1f, 0f);
            backRect.pivot = new Vector2(1f, 0f);
            backRect.anchoredPosition = new Vector2(-TraderMetrics.Side, 32f);
            backRect.sizeDelta = new Vector2(180f, 52f);

            var quests = host.Ui.CreateButton(Content, UiText.Get(host.Texts, UiTextKey.Trader_Quests), ShowQuests);
            quests.gameObject.name = "TraderQuests";
            var questsRect = (RectTransform)quests.transform;
            questsRect.anchorMin = new Vector2(1f, 0f);
            questsRect.anchorMax = new Vector2(1f, 0f);
            questsRect.pivot = new Vector2(1f, 0f);
            questsRect.anchoredPosition = new Vector2(-(TraderMetrics.Side + 196f), 32f);
            questsRect.sizeDelta = new Vector2(180f, 52f);
        }

        /// <summary>Открывает торговца и перестраивает экран.</summary>
        /// <param name="actorId">Идентификатор торговца.</param>
        public void Show(string actorId)
        {
            _actorId = actorId;
            ApplyTheme();
            _host.Router.Show(Panel);
            Canvas.ForceUpdateCanvases();
            Refresh();
        }

        /// <summary>Esc возвращает на основной игровой экран.</summary>
        /// <returns><c>true</c>, если Esc обработан.</returns>
        public override bool HandleEscape()
        {
            _host.ShowGame(false);
            return true;
        }

        private void Back() => _host.ShowGame(false);

        private void ShowQuests() => _host.ShowQuests(_actorId);

        private void Refresh()
        {
            var session = _host.Session;
            var texts = _host.Texts;
            if (session == null)
            {
                _title.text = UiText.Get(texts, UiTextKey.Common_No_Game);
                _money.text = string.Empty;
                _description.text = string.Empty;
                _portrait.enabled = false;
                _portraitFallback.text = string.Empty;
                Clear(_buyScroll.Content);
                Clear(_sellScroll.Content);
                _buyScroll.SetContentHeight(TraderMetrics.BuyHeight);
                _sellScroll.SetContentHeight(TraderMetrics.SellHeight);
                return;
            }

            var player = session.State.Player;
            var actor = _host.Actors.Contains(_actorId) ? _host.Actors.Get(_actorId) : null;
            var available = actor != null && _host.Trade.IsAvailable(session, _actorId);
            _title.text = available
                ? texts.Get(actor!.NameKey)
                : UiText.Get(texts, UiTextKey.Trader_Unavailable);
            _money.text = player == null ? string.Empty : UiText.Format(texts, UiTextKey.Common_Money, player.Money);
            ShowPortrait(actor);
            _description.text = actor?.DescriptionKey == null ? string.Empty : texts.Get(actor.DescriptionKey);
            _buyHeader.text = UiText.Get(texts, UiTextKey.Trader_Section_Buy);
            _sellHeader.text = UiText.Get(texts, UiTextKey.Trader_Section_Sell);
            Clear(_buyScroll.Content);
            Clear(_sellScroll.Content);
            if (!available || player == null)
            {
                _buyScroll.SetContentHeight(TraderMetrics.BuyHeight);
                _sellScroll.SetContentHeight(TraderMetrics.SellHeight);
                return;
            }

            BuildBuy(session);
            BuildSell(session);
        }

        private void ShowPortrait(ActorTemplate? actor)
        {
            var mode = actor?.PortraitMode ?? PortraitMode.Full;
            var height = mode == PortraitMode.Waist
                ? TraderMetrics.PortraitWaistHeight
                : TraderMetrics.PortraitFullHeight;
            UiWidgets.Place(
                _portraitFrame.rectTransform,
                TraderMetrics.Side,
                TraderMetrics.PortraitTop,
                TraderMetrics.PortraitWidth,
                height);
            UiWidgets.Place(
                _portrait.rectTransform,
                6f,
                6f,
                TraderMetrics.PortraitWidth - 12f,
                height - 12f);
            UiWidgets.Place(
                _portraitFallback.rectTransform,
                8f,
                8f,
                TraderMetrics.PortraitWidth - 16f,
                height - 16f);
            UiWidgets.Place(
                _description.rectTransform,
                TraderMetrics.Side,
                TraderMetrics.PortraitTop + height + 12f,
                TraderMetrics.PortraitWidth,
                96f);

            var hasSprite = actor?.Portrait != null &&
                            _host.Sprites.TryGet(actor.Portrait, out var sprite) &&
                            sprite != null;
            _portrait.sprite = hasSprite ? FoundPortrait(actor!) : TraderArt.Get(mode);
            _portrait.enabled = true;
            _portraitFallback.text = string.Empty;
        }

        private Sprite? FoundPortrait(ActorTemplate actor)
        {
            return actor.Portrait != null && _host.Sprites.TryGet(actor.Portrait, out var sprite)
                ? sprite
                : null;
        }

        private void BuildBuy(GameSession session)
        {
            var offers = _host.Trade.GetOffers(session, _actorId);
            if (offers.Count == 0)
            {
                AddEmpty(_buyScroll.Content, "BuyEmpty");
                _buyScroll.SetContentHeight(TraderMetrics.BuyHeight);
                return;
            }

            for (var index = 0; index < offers.Count; index++)
            {
                var offer = offers[index];
                TraderRow.BuildBuy(
                    _host,
                    _buyScroll.Content,
                    index,
                    offer,
                    () => Buy(offer.ItemId));
            }

            _buyScroll.SetContentHeight(ContentHeight(offers.Count, TraderMetrics.BuyHeight));
        }

        private void BuildSell(GameSession session)
        {
            var offers = _host.Trade.GetSellOffers(session, _actorId);
            if (offers.Count == 0)
            {
                AddEmpty(_sellScroll.Content, "SellEmpty");
                _sellScroll.SetContentHeight(TraderMetrics.SellHeight);
                return;
            }

            for (var index = 0; index < offers.Count; index++)
            {
                var offer = offers[index];
                TraderRow.BuildSell(
                    _host,
                    _sellScroll.Content,
                    index,
                    offer,
                    () => Sell(offer.InstanceId));
            }

            _sellScroll.SetContentHeight(ContentHeight(offers.Count, TraderMetrics.SellHeight));
        }

        private void Buy(string itemId)
        {
            var session = _host.Session;
            if (session == null)
            {
                return;
            }

            var result = _host.Trade.Buy(session, _actorId, itemId);
            if (result.Succeeded)
            {
                var name = _host.Items.TryGet(itemId, out var template) ? _host.Texts.Get(template.NameKey) : itemId;
                ShowToast(UiText.Format(_host.Texts, UiTextKey.Trader_Bought, name, result.Count, result.Price));
                Refresh();
                return;
            }

            ShowToast(UiText.Get(_host.Texts, KeyFor(result.Status)), error: true);
        }

        private void Sell(string instanceId)
        {
            var session = _host.Session;
            if (session == null)
            {
                return;
            }

            var result = _host.Trade.Sell(session, _actorId, instanceId);
            if (result.Succeeded)
            {
                var name = _host.Items.TryGet(result.ItemId, out var template)
                    ? _host.Texts.Get(template.NameKey)
                    : result.ItemId;
                ShowToast(UiText.Format(_host.Texts, UiTextKey.Trader_Sold, name, result.Price));
                Refresh();
                return;
            }

            ShowToast(UiText.Get(_host.Texts, KeyFor(result.Status)), error: true);
        }

        private void ShowToast(string text, bool error = false)
        {
            _toast.text = text;
            _toast.color = error ? TraderPalette.Error : UiFactory.ToastColor;
        }

        private Text SectionHeader(string name, UiTextKey key, float top)
        {
            var header = _host.Ui.CreateText(Content, UiText.Get(_host.Texts, key), 26, TextAnchor.MiddleLeft);
            header.gameObject.name = name;
            header.raycastTarget = false;
            var rect = header.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(TraderMetrics.ColumnLeft, -top);
            rect.sizeDelta = new Vector2(600f, TraderMetrics.SectionHeaderHeight);
            return header;
        }

        private ScrollArea CreateScroll(string name, float top, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(ScrollArea));
            go.transform.SetParent(Content, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(TraderMetrics.ColumnLeft, -(top + height));
            rect.offsetMax = new Vector2(-TraderMetrics.Side, -top);
            var scroll = go.GetComponent<ScrollArea>();
            scroll.Configure(12f);
            return scroll;
        }

        private void AddEmpty(RectTransform content, string name)
        {
            var label = UiWidgets.Label(
                _host,
                content,
                name,
                UiText.Get(_host.Texts, UiTextKey.Trader_Empty),
                20,
                TextAnchor.MiddleCenter,
                UiFactory.MutedTextColor,
                8f,
                8f,
                600f,
                32f);
            label.raycastTarget = false;
        }

        private static float ContentHeight(int count, float minHeight) =>
            Mathf.Max(minHeight, 16f + (count * (TraderMetrics.RowHeight + TraderMetrics.RowGap)));

        private static void Clear(RectTransform content)
        {
            for (var index = content.childCount - 1; index >= 0; index--)
            {
                var child = content.GetChild(index).gameObject;
                child.SetActive(false);
                UnityEngine.Object.Destroy(child);
            }
        }

        private static UiTextKey KeyFor(TradeStatus status) => status switch
        {
            TradeStatus.NotEnoughMoney => UiTextKey.Trader_Result_Not_Enough_Money,
            TradeStatus.NoStock => UiTextKey.Trader_Result_No_Stock,
            TradeStatus.NoRoom => UiTextKey.Trader_Result_No_Room,
            TradeStatus.OfferNotFound => UiTextKey.Trader_Result_Offer_Missing,
            TradeStatus.ItemNotFound => UiTextKey.Trader_Result_Item_Missing,
            TradeStatus.ItemNotSellable => UiTextKey.Trader_Result_Not_Sellable,
            TradeStatus.InvalidCount => UiTextKey.Trader_Result_Invalid_Count,
            TradeStatus.TraderNotFound => UiTextKey.Trader_Result_Unavailable,
            TradeStatus.NotTrader => UiTextKey.Trader_Result_Unavailable,
            TradeStatus.Unavailable => UiTextKey.Trader_Result_Unavailable,
            TradeStatus.WrongLocation => UiTextKey.Trader_Result_Unavailable,
            _ => UiTextKey.Trader_Result_Failed
        };
    }
}
