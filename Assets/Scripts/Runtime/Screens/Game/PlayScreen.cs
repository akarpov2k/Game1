#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.World;
using AnomalyPath.Runtime.Screens;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Game
{
    /// <summary>
    /// Основной игровой экран: фон текущей локации, объект взаимодействия и панель действий.
    /// Один и тот же экран используется в хабе и в исследовании мира; действия строятся из состояния сессии.
    /// </summary>
    public sealed class PlayScreen : ScreenBase
    {
        private readonly IUiHost _host;
        private readonly Text _title;
        private readonly Text _locationInfo;
        private readonly PlayClock _clock;
        private readonly Text _money;
        private readonly PlayObject _object;
        private PlayActionList _actions = null!;
        private readonly Text _toast;

        /// <summary>Создаёт основной игровой экран.</summary>
        /// <param name="host">Хост интерфейса: сервисы и переходы.</param>
        /// <param name="canvas">Родительский канвас.</param>
        public PlayScreen(IUiHost host, Transform canvas)
            : base(host, canvas, "InGame")
        {
            _host = host;

            _title = host.Ui.CreateTitle(Content, string.Empty);
            PlaceTop(
                _title.rectTransform,
                0f,
                PlayMetrics.TitleTop,
                PlayMetrics.TitleWidth,
                PlayMetrics.TitleHeight);

            _locationInfo = host.Ui.CreateText(
                Content,
                string.Empty,
                PlayMetrics.LocationSummaryFontSize,
                TextAnchor.MiddleCenter,
                UiFactory.MutedTextColor);
            _locationInfo.gameObject.name = "LocationSummary";
            PlaceTop(
                _locationInfo.rectTransform,
                0f,
                PlayMetrics.LocationSummaryTop,
                PlayMetrics.LocationSummaryWidth,
                PlayMetrics.LocationSummaryHeight);

            _clock = new PlayClock(host, Content);

            _money = host.Ui.CreateText(
                Content,
                string.Empty,
                PlayMetrics.MoneyFontSize,
                TextAnchor.MiddleRight);
            PlaceTopRight(
                _money.rectTransform,
                PlayMetrics.SidePadding,
                PlayMetrics.MoneyTop,
                PlayMetrics.MoneyWidth,
                PlayMetrics.MoneyHeight);

            _object = new PlayObject(host, Content);
            BuildActionPanel();
            BuildBottomBar();

            _toast = host.Ui.CreateText(
                Content,
                string.Empty,
                PlayMetrics.ToastFontSize,
                TextAnchor.MiddleCenter,
                UiFactory.ToastColor);
            var toastRect = _toast.rectTransform;
            toastRect.anchorMin = new Vector2(0.5f, 0f);
            toastRect.anchorMax = new Vector2(0.5f, 0f);
            toastRect.pivot = new Vector2(0.5f, 0f);
            toastRect.anchoredPosition = new Vector2(0f, PlayMetrics.ToastBottom);
            toastRect.sizeDelta = new Vector2(PlayMetrics.ToastWidth, PlayMetrics.ToastHeight);
        }

        /// <summary>Фон экрана — картинка текущей локации во всю яркость.</summary>
        protected override ScreenTheme Theme
        {
            get
            {
                var world = Host.Session?.State.World;
                return world == null
                    ? ScreenTheme.Default
                    : ScreenTheme.LocationFull(world.CurrentLocation.Background);
            }
        }

        /// <summary>Показывает экран и перестраивает его по текущему состоянию.</summary>
        public void Show()
        {
            ApplyTheme();
            _host.Router.Show(Panel);
            Canvas.ForceUpdateCanvases();
            Refresh();
        }

        /// <summary>Esc открывает меню игры; повторный Esc в меню возвращает сюда.</summary>
        /// <returns><c>true</c>, если Esc обработан.</returns>
        public override bool HandleEscape()
        {
            if (_host.Session == null)
            {
                return false;
            }

            _host.ShowGameMenu();
            return true;
        }

        /// <summary>Показывает всплывающее сообщение в нижней полосе.</summary>
        /// <param name="text">Текст сообщения.</param>
        /// <param name="error">Показать сообщение как ошибку.</param>
        public void ShowToast(string text, bool error = false)
        {
            _toast.text = text;
            _toast.color = error ? PlayPalette.Error : UiFactory.ToastColor;
        }

        private void BuildActionPanel()
        {
            var panel = _host.Ui.CreateImage("ActionPanel", Content, PlayPalette.ActionPanel);
            var panelRect = panel.rectTransform;
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.offsetMin = new Vector2(0f, PlayMetrics.BottomBarHeight);
            panelRect.offsetMax = new Vector2(0f, PlayMetrics.BottomBarHeight + PlayMetrics.ActionPanelHeight);

            var header = _host.Ui.CreateText(
                panel.transform,
                UiText.Get(_host.Texts, UiTextKey.Game_Actions),
                PlayMetrics.ActionHeaderFontSize,
                TextAnchor.MiddleLeft);
            header.raycastTarget = false;
            UiWidgets.Place(
                header.rectTransform,
                PlayMetrics.ActionHeaderX,
                PlayMetrics.ActionHeaderY,
                PlayMetrics.ActionHeaderWidth,
                PlayMetrics.ActionHeaderTextHeight);

            _actions = new PlayActionList(_host, panel.transform);
            _actions.Root.anchorMin = Vector2.zero;
            _actions.Root.anchorMax = Vector2.one;
            _actions.Root.offsetMin = new Vector2(
                PlayMetrics.ActionPanelSidePadding,
                PlayMetrics.ActionPanelBottomPadding);
            _actions.Root.offsetMax = new Vector2(
                -PlayMetrics.ActionPanelSidePadding,
                -PlayMetrics.ActionHeaderHeight);
        }

        private void BuildBottomBar()
        {
            var bar = _host.Ui.CreateImage("BottomBar", Content, PlayPalette.BottomBar);
            var barRect = bar.rectTransform;
            barRect.anchorMin = new Vector2(0f, 0f);
            barRect.anchorMax = new Vector2(1f, 0f);
            barRect.pivot = new Vector2(0.5f, 0f);
            barRect.offsetMin = Vector2.zero;
            barRect.offsetMax = new Vector2(0f, PlayMetrics.BottomBarHeight);

            var gear = _host.Ui.CreateButton(bar.transform, string.Empty, _host.ShowGameMenu);
            gear.gameObject.name = "Gear";
            var gearRect = (RectTransform)gear.transform;
            gearRect.anchorMin = new Vector2(0f, 0f);
            gearRect.anchorMax = new Vector2(0f, 0f);
            gearRect.pivot = new Vector2(0f, 0f);
            gearRect.anchoredPosition = new Vector2(PlayMetrics.SidePadding, PlayMetrics.IconPadding);
            gearRect.sizeDelta = new Vector2(PlayMetrics.IconButtonSize, PlayMetrics.IconButtonSize);
            var gearImage = gear.GetComponent<Image>();
            gearImage.sprite = PlayArt.Gear();
            gearImage.preserveAspect = true;
            gearImage.color = Color.white;

            var backpack = _host.Ui.CreateButton(bar.transform, string.Empty, _host.ShowCharacter);
            backpack.gameObject.name = "Backpack";
            var backpackRect = (RectTransform)backpack.transform;
            backpackRect.anchorMin = new Vector2(1f, 0f);
            backpackRect.anchorMax = new Vector2(1f, 0f);
            backpackRect.pivot = new Vector2(1f, 0f);
            backpackRect.anchoredPosition = new Vector2(-PlayMetrics.SidePadding, PlayMetrics.IconPadding);
            backpackRect.sizeDelta = new Vector2(PlayMetrics.IconButtonSize, PlayMetrics.IconButtonSize);
            var backpackImage = backpack.GetComponent<Image>();
            if (_host.Sprites.TryGet("icons/itm_backpack_small", out var sprite) && sprite != null)
            {
                backpackImage.sprite = sprite;
                backpackImage.preserveAspect = true;
            }
            else
            {
                backpackImage.color = UiFactory.ButtonColor;
            }
        }

        private void Refresh()
        {
            var session = _host.Session;
            if (session == null)
            {
                var noGame = UiText.Get(_host.Texts, UiTextKey.Common_No_Game);
                _title.text = noGame;
                _locationInfo.text = string.Empty;
                _clock.Hide();
                _money.text = string.Empty;
                _object.Hide();
                _actions.SetActions(Array.Empty<PlayAction>(), noGame);
                return;
            }

            var world = session.State.World;
            var location = world.CurrentLocation;
            var title = LocationText.Title(_host, location);
            var player = session.State.Player;

            _title.text = title;
            _locationInfo.text = UiText.Format(
                _host.Texts,
                UiTextKey.Game_Location_Summary,
                UiText.Get(_host.Texts, PlayText.Node(location.NodeType)),
                location.Danger);
            _clock.Show(world.Clock);
            _money.text = player == null
                ? string.Empty
                : UiText.Format(_host.Texts, UiTextKey.Common_Money, player.Money);

            // В данных пока нет экземпляров объектов локации: структура есть, отрисовки нет.
            _object.Hide();

            if (session.InCombat)
            {
                _actions.SetActions(Array.Empty<PlayAction>(), UiText.Get(_host.Texts, UiTextKey.Game_In_Combat));
                return;
            }

            var edges = _host.Sessions.AvailableEdges(session);
            var actions = new List<PlayAction>(edges.Count);
            foreach (var edge in edges)
            {
                var destination = world.Map.GetLocation(edge.ToId);
                var label = UiText.Format(
                    _host.Texts,
                    UiTextKey.Game_Travel,
                    LocationText.Title(_host, destination));
                var edgeId = edge.EdgeId;
                actions.Add(new PlayAction("Travel_" + edgeId, label, () => Travel(edgeId)));
            }

            _actions.SetActions(actions, UiText.Get(_host.Texts, UiTextKey.Game_No_Actions));
        }

        private void Travel(string edgeId)
        {
            var session = _host.Session;
            if (session == null)
            {
                ShowToast(UiText.Get(_host.Texts, UiTextKey.Common_No_Game), error: true);
                return;
            }

            TravelResult result;
            try
            {
                result = _host.Sessions.Travel(session, edgeId);
            }
            catch (Exception error)
            {
                ShowToast(UiText.Format(_host.Texts, UiTextKey.Game_Travel_Error, error.Message), error: true);
                return;
            }

            if (!result.Succeeded)
            {
                ShowToast(UiText.Get(_host.Texts, PlayText.Travel(result.Status)), error: true);
                return;
            }

            Show();
            ShowToast(UiText.Get(_host.Texts, UiTextKey.Game_Travel_Success));
        }

        private static void PlaceTop(RectTransform rect, float x, float top, float width, float height)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(x, -top);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void PlaceTopRight(RectTransform rect, float right, float top, float width, float height)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-right, -top);
            rect.sizeDelta = new Vector2(width, height);
        }
    }
}
