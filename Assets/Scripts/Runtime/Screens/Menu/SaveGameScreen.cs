#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Save;
using AnomalyPath.Runtime.Ui.Core;
using AnomalyPath.Runtime.Ui.Dialogs;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Menu
{
    /// <summary>
    /// Экран сохранения: существующие сохранения и один слот «Новое сохранение» квадратными
    /// плитками. Клик по плитке открывает диалог названия; отдельного поля и кнопки на экране нет.
    /// </summary>
    public sealed class SaveGameScreen : ScreenBase
    {
        private const float GridTop = 150f;
        private const float FooterHeight = 170f;

        private readonly IUiHost _host;
        private readonly int _pageSize;
        private readonly Text _title;
        private readonly Text _section;
        private readonly Transform _tilesRoot;
        private readonly Text _status;
        private readonly Button _backButton;
        private readonly SaveSlotPager _pager;
        private readonly SaveNameDialog _dialog;
        private readonly List<KeyValuePair<string, SaveSlotInfo?>> _entries =
            new List<KeyValuePair<string, SaveSlotInfo?>>();
        private readonly Dictionary<string, SaveSlotInfo> _slots =
            new Dictionary<string, SaveSlotInfo>(StringComparer.Ordinal);
        private string _newSlot = string.Empty;
        private int _page;
        private int _pages = 1;

        /// <summary>Создаёт экран сохранения.</summary>
        /// <param name="host">Хост интерфейса: сервисы и переходы.</param>
        /// <param name="canvas">Родительский канвас.</param>
        public SaveGameScreen(IUiHost host, Transform canvas)
            : base(host, canvas, "SaveGame")
        {
            _host = host;
            _pageSize = host.Rules.Require().SavePageSize;
            _title = host.Ui.CreateTitle(Content, UiText.Get(host.Texts, UiTextKey.SaveGame_Title));
            _section = host.Ui.CreateText(Content, UiText.Get(host.Texts, UiTextKey.SaveGame_Section), 18, TextAnchor.MiddleLeft);

            var tilesGo = new GameObject("SaveSlots", typeof(RectTransform));
            tilesGo.transform.SetParent(Content, false);
            var tilesRect = (RectTransform)tilesGo.transform;
            tilesRect.anchorMin = Vector2.zero;
            tilesRect.anchorMax = Vector2.one;
            tilesRect.offsetMin = Vector2.zero;
            tilesRect.offsetMax = Vector2.zero;
            _tilesRoot = tilesGo.transform;

            _status = host.Ui.CreateText(Content, string.Empty, 18, TextAnchor.MiddleCenter, UiFactory.ToastColor);
            _backButton = host.Ui.CreateButton(Content, UiText.Get(host.Texts, UiTextKey.Common_Back), Back);
            _pager = new SaveSlotPager(host, Content, PreviousPage, NextPage);
            _dialog = new SaveNameDialog(host, Panel.transform);
            Panel.AddComponent<RectResizeWatcher>().Resized += Layout;
        }

        /// <summary>Показывает сохранения: сначала существующие, затем «Новое сохранение».</summary>
        public void Show()
        {
            BuildEntries();
            _page = 0;
            _status.text = string.Empty;
            _host.Router.Show(Panel);
            Canvas.ForceUpdateCanvases();
            Layout();
        }

        /// <summary>Esc закрывает диалог названия, иначе возвращает в игру.</summary>
        /// <returns><c>true</c>, если Esc обработан.</returns>
        public override bool HandleEscape()
        {
            if (_dialog.HandleEscape())
            {
                return true;
            }

            Back();
            return true;
        }

        private void BuildEntries()
        {
            _slots.Clear();
            foreach (var slot in _host.Saves.ListSlots())
            {
                _slots[slot.Slot] = slot;
            }

            var numeric = new List<SaveSlotInfo>();
            var extra = new List<string>();
            foreach (var pair in _slots)
            {
                if (SlotNumber(pair.Key) > 0)
                {
                    numeric.Add(pair.Value);
                }
                else
                {
                    extra.Add(pair.Key);
                }
            }

            numeric.Sort((left, right) => SlotNumber(left.Slot).CompareTo(SlotNumber(right.Slot)));
            extra.Sort(StringComparer.Ordinal);

            _entries.Clear();
            foreach (var slot in numeric)
            {
                _entries.Add(new KeyValuePair<string, SaveSlotInfo?>(slot.Slot, slot));
            }

            foreach (var slot in extra)
            {
                _entries.Add(new KeyValuePair<string, SaveSlotInfo?>(slot, _slots[slot]));
            }

            var next = 1;
            while (_slots.ContainsKey(SaveSlots.Slot(next)))
            {
                next++;
            }

            _newSlot = SaveSlots.Slot(next);
            _entries.Add(new KeyValuePair<string, SaveSlotInfo?>(_newSlot, null));
            _pages = Mathf.Max(1, Mathf.CeilToInt(_entries.Count / (float)_pageSize));
        }

        private void Layout()
        {
            if (!Panel.activeInHierarchy)
            {
                return;
            }

            var panel = (RectTransform)Panel.transform;
            var width = panel.rect.width;
            var height = panel.rect.height;
            if (width <= 1f || height <= 1f)
            {
                return;
            }

            var availableWidth = Mathf.Min(SaveSlotLayout.MaxGridWidth, width - 160f);
            if (availableWidth < SaveSlotLayout.MinTile)
            {
                availableWidth = Mathf.Max(SaveSlotLayout.MinTile, width - 40f);
            }

            var columns = SaveSlotLayout.ColumnsForWidth(availableWidth);
            var pageStart = _page * _pageSize;
            var count = Mathf.Clamp(_entries.Count - pageStart, 0, _pageSize);
            var rows = SaveSlotLayout.Rows(count, columns);
            var gridHeight = Mathf.Max(SaveSlotLayout.MinTile, height - GridTop - FooterHeight);
            var tile = SaveSlotLayout.TileSize(availableWidth, gridHeight, count, columns);
            var gridWidth = (columns * tile) + ((columns - 1) * SaveSlotLayout.Gap);
            var x0 = (width - gridWidth) / 2f;

            var fieldWidth = Mathf.Min(700f, availableWidth);
            UiWidgets.Place((RectTransform)_title.transform, (width - 600f) / 2f, 24f, 600f, 70f);
            UiWidgets.Place((RectTransform)_section.transform, (width - fieldWidth) / 2f, 110f, fieldWidth, 28f);

            BuildTiles(x0, tile, columns, pageStart, count);
            var gridBottom = GridTop + (rows * tile) + ((rows - 1) * SaveSlotLayout.Gap);
            _pager.Place(width / 2f, gridBottom + 6f);
            _pager.Refresh(_page, _pages);
            UiWidgets.Place(
                (RectTransform)_status.transform,
                (width - availableWidth) / 2f,
                gridBottom + 58f,
                availableWidth,
                30f);
            UiWidgets.Place(
                (RectTransform)_backButton.transform,
                (width / 2f) - 130f,
                gridBottom + 94f,
                260f,
                54f);
        }

        private void BuildTiles(float x0, float tile, int columns, int pageStart, int count)
        {
            foreach (Transform child in _tilesRoot)
            {
                child.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(child.gameObject);
            }

            for (var index = 0; index < count; index++)
            {
                var entry = _entries[pageStart + index];
                var column = index % columns;
                var row = index / columns;
                var x = x0 + (column * (tile + SaveSlotLayout.Gap));
                var y = GridTop + (row * (tile + SaveSlotLayout.Gap));
                var slot = entry.Key;
                var info = entry.Value;
                SaveSlotTile.Create(
                    _host,
                    _tilesRoot,
                    "SaveSlot_" + slot,
                    slot,
                    info,
                    SlotNumber(slot),
                    x,
                    y,
                    tile,
                    selected: false,
                    markSelected: false,
                    onClick: () => OpenSaveDialog(slot, info),
                    isNew: slot == _newSlot);
            }
        }

        private void OpenSaveDialog(string slot, SaveSlotInfo? info)
        {
            var defaultName = info != null && info.IsReadable
                ? string.IsNullOrWhiteSpace(info.Title)
                    ? slot
                    : info.Title
                : NextDefaultName();
            _dialog.Show(defaultName, name => SaveTo(slot, name));
        }

        private string NextDefaultName()
        {
            var count = 0;
            foreach (var info in _slots.Values)
            {
                if (info.Title.StartsWith(UiText.Get(_host.Texts, UiTextKey.SaveSlot_New), StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return UiText.Format(_host.Texts, UiTextKey.SaveSlot_New_Name, count + 1);
        }

        private void SaveTo(string slot, string name)
        {
            var session = _host.Session;
            if (session == null)
            {
                _status.text = UiText.Get(_host.Texts, UiTextKey.Common_No_Game);
                return;
            }

            try
            {
                _host.Sessions.Save(session, slot, title: name);
                _host.ShowGame(announce: false);
                _host.Toast(UiText.Format(_host.Texts, UiTextKey.SaveGame_Toast_Saved, name));
            }
            catch (Exception error)
            {
                _status.text = UiText.Format(_host.Texts, UiTextKey.SaveGame_Status_Error, error.Message);
            }
        }

        private void PreviousPage()
        {
            if (_page <= 0)
            {
                return;
            }

            _page--;
            Layout();
        }

        private void NextPage()
        {
            if (_page >= _pages - 1)
            {
                return;
            }

            _page++;
            Layout();
        }

        private static int SlotNumber(string slot)
        {
            if (!slot.StartsWith("save_", StringComparison.Ordinal))
            {
                return 0;
            }

            return int.TryParse(slot.Substring(5), out var number) && number > 0 ? number : 0;
        }

        private void Back()
        {
            if (_host.Session != null)
            {
                _host.ShowGame(announce: false);
            }
            else
            {
                _host.ShowMainMenu();
            }
        }
    }
}
