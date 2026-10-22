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
    /// Экран загрузки: квадратные ячейки сохранений по 2–3 в строке с названием, уровнем,
    /// деньгами и днём мира — так же, как на экране сохранения. Клик по ячейке загружает игру.
    /// </summary>
    public sealed class LoadGameScreen : ScreenBase
    {
        private const float GridTop = 150f;
        private const float FooterHeight = 170f;

        private readonly IUiHost _host;
        private readonly int _pageSize;
        private readonly Text _title;
        private readonly Transform _tilesRoot;
        private readonly Text _empty;
        private readonly Text _status;
        private readonly Button _backButton;
        private readonly SaveSlotPager _pager;
        private readonly ConfirmDialog _confirm;
        private readonly List<SaveSlotInfo> _slots = new List<SaveSlotInfo>();
        private bool _fromGame;
        private int _page;
        private int _pages = 1;

        /// <summary>Создаёт панель загрузки.</summary>
        /// <param name="host">Хост интерфейса: сервисы и переходы.</param>
        /// <param name="canvas">Родительский канвас.</param>
        public LoadGameScreen(IUiHost host, Transform canvas)
            : base(host, canvas, "LoadGame")
        {
            _host = host;
            _pageSize = host.Rules.Require().SavePageSize;
            _title = host.Ui.CreateTitle(Content, UiText.Get(host.Texts, UiTextKey.Load_Title));

            var tilesGo = new GameObject("LoadSlots", typeof(RectTransform));
            tilesGo.transform.SetParent(Content, false);
            var tilesRect = (RectTransform)tilesGo.transform;
            tilesRect.anchorMin = Vector2.zero;
            tilesRect.anchorMax = Vector2.one;
            tilesRect.offsetMin = Vector2.zero;
            tilesRect.offsetMax = Vector2.zero;
            _tilesRoot = tilesGo.transform;

            _empty = host.Ui.CreateText(Content, UiText.Get(host.Texts, UiTextKey.Load_Empty), 24, TextAnchor.MiddleCenter,
                UiFactory.MutedTextColor);
            _status = host.Ui.CreateText(Content, string.Empty, 18, TextAnchor.MiddleCenter, UiFactory.ToastColor);
            _backButton = host.Ui.CreateButton(Content, UiText.Get(host.Texts, UiTextKey.Common_Back), Back);
            _pager = new SaveSlotPager(host, Content, PreviousPage, NextPage);
            _confirm = new ConfirmDialog(host, Panel.transform);
            Panel.AddComponent<RectResizeWatcher>().Resized += Layout;
        }

        /// <summary>Показывает список сохранений.</summary>
        /// <param name="fromGame">Пришли из игры, а не из стартового меню.</param>
        public void Show(bool fromGame)
        {
            _fromGame = fromGame;
            _page = 0;
            _slots.Clear();
            _slots.AddRange(_host.Saves.ListSlots());
            _pages = PageCount();
            _status.text = string.Empty;
            _host.Router.Show(Panel);
            Canvas.ForceUpdateCanvases();
            Layout();
        }

        /// <summary>Esc закрывает подтверждение удаления, иначе возвращает назад.</summary>
        /// <returns><c>true</c>, если Esc обработан.</returns>
        public override bool HandleEscape()
        {
            if (_confirm.HandleEscape())
            {
                return true;
            }

            Back();
            return true;
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
            var count = Mathf.Clamp(_slots.Count - pageStart, 0, _pageSize);
            var rows = SaveSlotLayout.Rows(count, columns);
            var gridHeight = Mathf.Max(SaveSlotLayout.MinTile, height - GridTop - FooterHeight);
            var tile = SaveSlotLayout.TileSize(availableWidth, gridHeight, count, columns);
            var gridWidth = (columns * tile) + ((columns - 1) * SaveSlotLayout.Gap);
            var x0 = (width - gridWidth) / 2f;

            UiWidgets.Place((RectTransform)_title.transform, (width - 600f) / 2f, 24f, 600f, 70f);
            BuildTiles(x0, tile, columns, pageStart, count);

            var gridBottom = count > 0
                ? GridTop + (rows * tile) + ((rows - 1) * SaveSlotLayout.Gap)
                : GridTop + 80f;
            _empty.gameObject.SetActive(_slots.Count == 0);
            UiWidgets.Place(
                (RectTransform)_empty.transform,
                (width - availableWidth) / 2f,
                GridTop + ((gridHeight - 40f) / 2f),
                availableWidth,
                40f);
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

        private int PageCount() =>
            Mathf.Max(1, Mathf.CeilToInt(_slots.Count / (float)_pageSize));

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

        private void BuildTiles(float x0, float tile, int columns, int pageStart, int count)
        {
            foreach (Transform child in _tilesRoot)
            {
                child.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(child.gameObject);
            }

            for (var index = 0; index < count; index++)
            {
                var slot = _slots[pageStart + index];
                var column = index % columns;
                var row = index / columns;
                var x = x0 + (column * (tile + SaveSlotLayout.Gap));
                var y = GridTop + (row * (tile + SaveSlotLayout.Gap));
                var slotId = slot.Slot;
                SaveSlotTile.Create(
                    _host,
                    _tilesRoot,
                    "LoadSlot_" + slotId,
                    slotId,
                    slot,
                    0,
                    x,
                    y,
                    tile,
                    selected: false,
                    markSelected: false,
                    onClick: () => LoadSlot(slotId),
                    onDelete: () => ConfirmDelete(slotId));
            }
        }

        private void ConfirmDelete(string slot)
        {
            var title = slot;
            foreach (var item in _slots)
            {
                if (item.Slot == slot)
                {
                    title = string.IsNullOrWhiteSpace(item.Title) ? slot : item.Title;
                    break;
                }
            }

            _confirm.Show(
                UiText.Get(_host.Texts, UiTextKey.Load_Delete_Title),
                UiText.Format(_host.Texts, UiTextKey.Load_Delete_Question, title),
                () => DeleteSave(slot));
        }

        private void DeleteSave(string slot)
        {
            try
            {
                _host.Saves.Delete(slot);
            }
            catch (Exception error)
            {
                _status.text = UiText.Format(_host.Texts, UiTextKey.Load_Delete_Error, error.Message);
                return;
            }

            _slots.Clear();
            _slots.AddRange(_host.Saves.ListSlots());
            _pages = PageCount();
            if (_page >= _pages)
            {
                _page = _pages - 1;
            }

            Layout();
            _status.text = UiText.Get(_host.Texts, UiTextKey.Load_Deleted);
        }

        private void LoadSlot(string slot)
        {
            try
            {
                _host.Session = _host.Sessions.Load(slot);
                _host.ShowGame();
            }
            catch (Exception error)
            {
                _status.text = UiText.Format(_host.Texts, UiTextKey.Load_Load_Error, error.Message);
            }
        }

        private void Back()
        {
            if (_fromGame && _host.Session != null)
            {
                _host.ShowGame();
            }
            else
            {
                _host.ShowMainMenu();
            }
        }
    }
}
