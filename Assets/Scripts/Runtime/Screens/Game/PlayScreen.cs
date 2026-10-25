#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Loot;
using AnomalyPath.Core.Quests;
using AnomalyPath.Core.Spawns;
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
    public sealed partial class PlayScreen : ScreenBase
    {
        private readonly IUiHost _host;
        private readonly Text _title;
        private readonly Text _locationInfo;
        private readonly PlayClock _clock;
        private readonly Text _money;
        private readonly PlayObject _object;
        private readonly TestEnemyPanel _testEnemy;
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
            // ВРЕМЕННО: тестовый противник и кнопка «Убить» — до боевого экрана (ROADMAP, блок I).
            _testEnemy = new TestEnemyPanel(host, Content, Show);
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
                _testEnemy.Hide();
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
                UiText.Get(_host.Texts, PlayText.Subtype(location.Subtype)),
                location.Danger);
            if (location.Subtype == LocationSubtype.Path)
            {
                var returnCost = _host.Topology.Topology.HubReturnCost(location.RouteStep);
                _locationInfo.text += Environment.NewLine + UiText.Format(
                    _host.Texts,
                    UiTextKey.Game_Route_Summary,
                    location.RouteStep,
                    returnCost.Stamina);
            }
            _clock.Show(world.Clock);
            _money.text = player == null
                ? string.Empty
                : UiText.Format(_host.Texts, UiTextKey.Common_Money, player.Money);

            _host.Sessions.RollLocationStash(session);
            ShowLocationObject(location);

            _testEnemy.Refresh();

            if (session.InCombat)
            {
                _actions.SetActions(Array.Empty<PlayAction>(), UiText.Get(_host.Texts, UiTextKey.Game_In_Combat));
                return;
            }

            var edges = _host.Sessions.AvailableEdges(session);
            var actions = new List<PlayAction>(edges.Count + 3);
            if (location.Subtype == LocationSubtype.Hub)
            {
                actions.Add(new PlayAction(
                    "LeaveHub",
                    UiText.Get(_host.Texts, UiTextKey.Game_Leave_Hub),
                    LeaveHub));
            }
            else if (location.Subtype == LocationSubtype.Path)
            {
                actions.Add(new PlayAction(
                    "ReturnToHub",
                    UiText.Get(_host.Texts, UiTextKey.Game_Return_Hub),
                    ReturnToHub));
            }

            if (location.Subtype != LocationSubtype.Hub)
            {
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
            }

            AddTraderActions(session, location, actions);
            AddQuestActions(session, location, actions);
            actions.Add(new PlayAction(
                "GameQuests",
                UiText.Get(_host.Texts, UiTextKey.Game_Quests),
                () => _host.ShowQuests(null)));

            _actions.SetActions(actions, UiText.Get(_host.Texts, UiTextKey.Game_No_Actions));
        }

        private void AddTraderActions(
            AnomalyPath.Core.Session.GameSession session,
            LocationInstance location,
            List<PlayAction> actions)
        {
            var template = _host.Locations.Get(location.TemplateId);
            if (template.LocationKind != LocationKind.Hub)
            {
                return;
            }

            foreach (var traderId in template.Traders)
            {
                if (!_host.Trade.IsAvailable(session, traderId))
                {
                    continue;
                }

                var actor = _host.Actors.Get(traderId);
                var label = UiText.Format(
                    _host.Texts,
                    UiTextKey.Game_Trade,
                    _host.Texts.Get(actor.NameKey));
                var id = traderId;
                actions.Add(new PlayAction("Trade_" + id, label, () => _host.ShowTrader(id)));
            }
        }

        private void AddQuestActions(
            AnomalyPath.Core.Session.GameSession session,
            LocationInstance location,
            List<PlayAction> actions)
        {
            foreach (var quest in _host.Quests.GetActive(session))
            {
                if (quest.Cleared ||
                    !string.Equals(quest.LocationInstanceId, location.InstanceId, StringComparison.Ordinal) ||
                    !_host.Quests.TryGetTemplate(quest.QuestId, out var template) ||
                    template.Type != QuestType.ClearLocation)
                {
                    continue;
                }

                var questId = quest.QuestId;
                actions.Add(new PlayAction(
                    "QuestClear_" + questId,
                    UiText.Get(_host.Texts, UiTextKey.Game_Clear_Location),
                    () => ClearQuest(questId, location.InstanceId)));
            }
        }

        private void ClearQuest(string questId, string locationInstanceId)
        {
            var session = _host.Session;
            if (session == null)
            {
                return;
            }

            _host.Quests.NotifyLocationCleared(session, locationInstanceId);
            Show();
            ShowToast(UiText.Get(_host.Texts, UiTextKey.Game_Cleared));
        }

        private void ShowLocationObject(LocationInstance location)
        {
            var current = location.Object;
            if (current == null)
            {
                if (location.StashPresent && !location.StashOpened)
                {
                    _object.Show(
                        UiText.Get(_host.Texts, UiTextKey.Game_Stash),
                        UiText.Get(_host.Texts, UiTextKey.Game_Stash_Hint),
                        OpenStash);
                }
                else
                {
                    _object.Hide();
                }

                return;
            }

            if (current.Looted)
            {
                _object.Hide();
                return;
            }

            var title = UiText.Get(_host.Texts, current.TitleKey);
            switch (current.Kind)
            {
                case LocationObjectKind.Stash:
                    _object.Show(title, UiText.Get(_host.Texts, UiTextKey.Game_Stash_Hint), OpenStash);
                    break;
                case LocationObjectKind.Loot:
                    _object.Show(
                        title,
                        UiText.Get(_host.Texts, UiTextKey.Game_Object_Hint),
                        () => OpenLootObject(location));
                    break;
                default:
                    _object.Show(
                        title,
                        UiText.Get(_host.Texts, UiTextKey.Game_Object_Hint),
                        Noop);
                    break;
            }
        }

        private void OpenLootObject(LocationInstance location)
        {
            var session = _host.Session;
            if (session == null || location.Object is not LootLocationObject lootObject)
            {
                return;
            }

            var world = session.State.World;
            var seed = StableHash.Combine(world.Map.Seed, location.InstanceId + "#object_loot");
            var loot = _host.Loot.RollTable(lootObject.LootTableId, location.Tier, seed, false);
            if (loot.Count == 0)
            {
                location.MarkObjectLooted();
                ShowToast(UiText.Get(_host.Texts, UiTextKey.Loot_Empty));
                Show();
                return;
            }

            _host.ShowLoot(loot, () =>
            {
                location.MarkObjectLooted();
                Show();
            });
        }

        private static void Noop()
        {
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
