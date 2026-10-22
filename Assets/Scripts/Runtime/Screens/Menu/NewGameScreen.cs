#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Loadouts;
using AnomalyPath.Core.Session;
using AnomalyPath.Runtime.Screens;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Menu
{
    /// <summary>
    /// Экран новой игры: слева ник, набор снаряжения и числовые настройки сложности,
    /// справа — неинтерактивное окно инвентаря с предметами выбранного набора.
    /// Обе зоны начинаются от верхней границы контента и растягиваются под окно;
    /// если контент выше экрана, включается вертикальная прокрутка с полосой справа.
    /// </summary>
    public sealed class NewGameScreen : ScreenBase
    {
        private const float DefaultLootShare = 0.5f;
        private const double LootStep = 0.1;
        private const double TradeStep = 0.1;
        private const double TradeMax = 3;

        private readonly IUiHost _host;
        private readonly List<Button> _loadoutButtons = new List<Button>();
        private readonly List<string> _loadoutIds = new List<string>();
        private readonly List<string> _loadoutNames = new List<string>();
        private readonly ScrollArea _scroll;
        private readonly RectTransform _settings;
        private readonly Text _status;
        private readonly TextInputField _nickname;
        private readonly NumberField _lootLoss;
        private readonly NumberField _tradeDifficulty;
        private readonly NewGameInventoryFrame _preview;
        private bool _layingOut;
        private string _selectedLoadout = string.Empty;

        /// <summary>Создаёт панель новой игры.</summary>
        /// <param name="host">Хост интерфейса: сервисы и переходы.</param>
        /// <param name="canvas">Родительский канвас.</param>
        public NewGameScreen(IUiHost host, Transform canvas)
            : base(host, canvas, "NewGame")
        {
            _host = host;

            var title = host.Ui.CreateTitle(Content, "Новая игра");
            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -24f);
            titleRect.sizeDelta = new Vector2(600f, 70f);

            var scrollGo = new GameObject("NewGameScroll", typeof(RectTransform), typeof(ScrollArea));
            scrollGo.transform.SetParent(Content, false);
            var scrollRect = (RectTransform)scrollGo.transform;
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(MenuMetrics.SidePadding, MenuMetrics.BottomPadding);
            scrollRect.offsetMax = new Vector2(-MenuMetrics.SidePadding, -MenuMetrics.TopPadding);

            _scroll = scrollGo.GetComponent<ScrollArea>();
            _scroll.Configure(MenuMetrics.ScrollbarWidth);
            _scroll.Resized += LayoutContent;

            var column = host.Ui.CreateColumn(_scroll.Content, MenuMetrics.SettingsWidth);
            _settings = (RectTransform)column;
            _settings.anchorMin = new Vector2(0f, 1f);
            _settings.anchorMax = new Vector2(0f, 1f);
            _settings.pivot = new Vector2(0f, 1f);
            _settings.anchoredPosition = Vector2.zero;

            host.Ui.CreateText(column, "Имя персонажа", 18, TextAnchor.MiddleLeft);
            _nickname = host.Ui.CreateInputField(column, "Введите имя", string.Empty);
            host.Ui.CreateButton(column, "Случайное имя", GenerateNickname);
            host.Ui.CreateText(column, "Выберите набор снаряжения", 22, TextAnchor.MiddleCenter);

            var loadouts = SortedLoadouts(host.Loadouts);
            foreach (var loadout in loadouts)
            {
                var id = loadout.Id;
                _loadoutIds.Add(id);
                _loadoutNames.Add(host.Texts.Get(loadout.NameKey));
                _loadoutButtons.Add(host.Ui.CreateButton(column, host.Texts.Get(loadout.NameKey), () => Select(id)));
            }

            _status = host.Ui.CreateText(column, string.Empty, 18, TextAnchor.MiddleCenter);
            host.Ui.CreateText(column, "Потеря лута при смерти", 18, TextAnchor.MiddleLeft);
            _lootLoss = host.Ui.CreateNumberField(column, "LootLossField", 0, 1, DefaultLootShare, LootStep);
            host.Ui.CreateText(column, "Усложнение торговли", 18, TextAnchor.MiddleLeft);
            _tradeDifficulty = host.Ui.CreateNumberField(
                column,
                "TradeDifficultyField",
                0,
                TradeMax,
                NewGameSettings.DefaultTradeDifficulty,
                TradeStep);

            host.Ui.CreateButton(column, "Начать", StartGame);
            host.Ui.CreateButton(column, "Назад", host.ShowMainMenu);

            _preview = new NewGameInventoryFrame(host, _scroll.Content);
        }

        /// <summary>Готовит экран к показу: выбор первого набора, настройки по умолчанию и ник.</summary>
        public void Show()
        {
            _selectedLoadout = _loadoutIds.Count > 0 ? _loadoutIds[0] : string.Empty;
            ResetDifficulty();
            _nickname.SetText(NextNickname());
            _status.text = string.Empty;
            UpdateButtons();
            _host.Router.Show(Panel);
            Canvas.ForceUpdateCanvases();
            RefreshPreview();
            LayoutContent();
        }

        /// <summary>Esc из настроек новой игры возвращает в меню; в фокусе поля ввода Esc снимает фокус.</summary>
        /// <returns><c>true</c>, если Esc обработан.</returns>
        public override bool HandleEscape()
        {
            if (_nickname.IsFocused)
            {
                return false;
            }

            _host.ShowMainMenu();
            return true;
        }

        private void GenerateNickname()
        {
            _nickname.SetText(NextNickname());
        }

        private string NextNickname() => _host.Nicknames.Generate(unchecked((int)DateTime.UtcNow.Ticks));

        private void ResetDifficulty()
        {
            _lootLoss.SetValue(DefaultLootShare);
            var rules = _host.Rules.Rules;
            if (rules != null)
            {
                foreach (var option in rules.LootLossOptions)
                {
                    if (option.Id == rules.DefaultLootLoss)
                    {
                        _lootLoss.SetValue(option.Share);
                        break;
                    }
                }
            }

            _tradeDifficulty.SetValue(NewGameSettings.DefaultTradeDifficulty);
        }

        private void Select(string id)
        {
            _selectedLoadout = id;
            _status.text = string.Empty;
            UpdateButtons();
            RefreshPreview();
            LayoutContent();
        }

        /// <summary>
        /// Считает раскладку под текущий размер окна: обе зоны от верхней границы,
        /// ширины делят свободное место, высота контента — по более высокому фрейму.
        /// </summary>
        private void LayoutContent()
        {
            if (_layingOut || _preview == null)
            {
                return;
            }

            var viewportWidth = _scroll.ViewportWidth;
            var viewportHeight = _scroll.ViewportHeight;
            if (viewportWidth <= 1f || viewportHeight <= 1f)
            {
                return;
            }

            _layingOut = true;
            try
            {
                Canvas.ForceUpdateCanvases();
                var leftWidth = Mathf.Clamp(
                    viewportWidth * MenuMetrics.SettingsShare,
                    MenuMetrics.SettingsMinWidth,
                    MenuMetrics.SettingsWidth);
                var rightWidth = viewportWidth - leftWidth - MenuMetrics.ColumnGap;
                if (rightWidth < MenuMetrics.PreviewMinWidth)
                {
                    leftWidth = Mathf.Max(
                        MenuMetrics.SettingsMinWidth * 0.6f,
                        viewportWidth - MenuMetrics.PreviewMinWidth - MenuMetrics.ColumnGap);
                    rightWidth = viewportWidth - leftWidth - MenuMetrics.ColumnGap;
                }

                rightWidth = Mathf.Max(120f, rightWidth);

                _settings.sizeDelta = new Vector2(leftWidth, _settings.sizeDelta.y);
                LayoutRebuilder.ForceRebuildLayoutImmediate(_settings);
                var settingsHeight = _settings.rect.height;

                // Панель инвентаря обтягивает сетку: клетки считаются от ширины, высота — от сетки.
                var previewHeight = _preview.MeasureHeight(rightWidth);
                var contentHeight = Mathf.Max(
                    viewportHeight,
                    Mathf.Max(settingsHeight, previewHeight));
                _scroll.SetContentHeight(contentHeight);
                _preview.Resize(rightWidth, previewHeight);
            }
            finally
            {
                _layingOut = false;
            }
        }

        private void RefreshPreview()
        {
            if (string.IsNullOrEmpty(_selectedLoadout))
            {
                _preview.Show(null, "Нет доступных наборов", StartingMoney());
                return;
            }

            try
            {
                _preview.Show(_host.LoadoutPreviews.Build(_selectedLoadout), null, StartingMoney());
            }
            catch (Exception error)
            {
                _preview.Show(null, "Набор не удалось показать: " + error.Message, StartingMoney());
            }
        }

        private int StartingMoney() => _host.Rules.Rules?.StartingMoney ?? 0;

        private void UpdateButtons()
        {
            for (var index = 0; index < _loadoutButtons.Count && index < _loadoutIds.Count; index++)
            {
                var id = _loadoutIds[index];
                var label = _loadoutButtons[index].GetComponentInChildren<Text>();
                var name = _loadoutNames[index];
                label.text = (id == _selectedLoadout ? "> " : string.Empty) + name;
            }
        }

        private void StartGame()
        {
            if (string.IsNullOrEmpty(_selectedLoadout))
            {
                _status.text = "Нет доступных наборов";
                return;
            }

            var nickname = _nickname.Text.Trim();
            if (nickname.Length == 0)
            {
                _status.text = "Введите имя или нажмите «Случайное имя»";
                return;
            }

            if (nickname.Length > NewGameSettings.MaxNicknameLength)
            {
                _status.text = "Имя не длиннее " + NewGameSettings.MaxNicknameLength + " символов";
                return;
            }

            try
            {
                var seed = unchecked((int)DateTime.UtcNow.Ticks);
                var settings = new NewGameSettings(_lootLoss.Value, _tradeDifficulty.Value, nickname);
                _host.Session = _host.Sessions.StartNew(seed, _selectedLoadout, settings);
                _host.ShowGame();
            }
            catch (Exception error)
            {
                _status.text = "Ошибка: " + error.Message;
            }
        }

        private static List<LoadoutTemplate> SortedLoadouts(ILoadoutRepository repository)
        {
            var loadouts = new List<LoadoutTemplate>(repository.All);
            loadouts.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));
            return loadouts;
        }
    }
}
