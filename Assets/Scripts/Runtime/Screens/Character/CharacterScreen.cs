#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Player;
using AnomalyPath.Runtime.Screens;
using AnomalyPath.Runtime.Ui.Core;
using AnomalyPath.Runtime.Ui.Items;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Character
{
    /// <summary>
    /// Экран персонажа: связывает фреймы снаряжения и инвентаря, перетаскивание и контекстное меню.
    /// Вся отрисовка живёт в отдельных фреймах, здесь только панель, навигация и действия.
    /// </summary>
    public sealed class CharacterScreen : ScreenBase, ICharacterFrameHost
    {
        private readonly IUiHost _host;
        private readonly Transform _content;
        private readonly ItemContextMenu _menu;
        private readonly Text _ghost;
        private readonly Text _message;
        private readonly CharacterActionsPresenter _presenter;
        private readonly ItemDragContext _drag = new ItemDragContext();

        /// <summary>Создаёт панель персонажа.</summary>
        /// <param name="host">Хост интерфейса: сервисы и переходы.</param>
        /// <param name="canvas">Родительский канвас.</param>
        public CharacterScreen(IUiHost host, Transform canvas)
            : base(host, canvas, "Character")
        {
            _host = host;

            var title = host.Ui.CreateTitle(Content, UiText.Get(host.Texts, UiTextKey.Character_Title));
            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -30f);
            titleRect.sizeDelta = new Vector2(600f, 70f);

            _content = CreateContent(Content);

            var back = host.Ui.CreateButton(Content, UiText.Get(host.Texts, UiTextKey.Common_Back), () => host.ShowGame(announce: false));
            var backRect = (RectTransform)back.transform;
            backRect.anchorMin = new Vector2(0.5f, 0f);
            backRect.anchorMax = new Vector2(0.5f, 0f);
            backRect.pivot = new Vector2(0.5f, 0f);
            backRect.anchoredPosition = new Vector2(0f, 40f);
            backRect.sizeDelta = new Vector2(320f, 54f);

            _message = host.Ui.CreateText(Content, string.Empty, 18, TextAnchor.MiddleCenter, UiFactory.ToastColor);
            var messageRect = _message.rectTransform;
            messageRect.anchorMin = new Vector2(0.5f, 0f);
            messageRect.anchorMax = new Vector2(0.5f, 0f);
            messageRect.pivot = new Vector2(0.5f, 0f);
            messageRect.anchoredPosition = new Vector2(0f, 104f);
            messageRect.sizeDelta = new Vector2(1200f, 28f);

            _ghost = host.Ui.CreateText(Content, string.Empty, 18, TextAnchor.MiddleCenter, UiFactory.ToastColor);
            _ghost.raycastTarget = false;
            _ghost.rectTransform.sizeDelta = new Vector2(360f, 40f);
            _ghost.gameObject.SetActive(false);

            _menu = new ItemContextMenu(Content, host.Ui);
            _presenter = new CharacterActionsPresenter(host, SetMessage, Show);
        }

        /// <inheritdoc />
        public ItemDragContext Drag => _drag;

        /// <inheritdoc />
        public Text Ghost => _ghost;

        /// <summary>Esc возвращает из персонажа в игровое поле.</summary>
        /// <returns><c>true</c>, если Esc обработан.</returns>
        public override bool HandleEscape()
        {
            _host.ShowGame(announce: false);
            return true;
        }

        /// <summary>Перестраивает содержимое по текущему состоянию игрока.</summary>
        public void Show()
        {
            _menu.Close();
            _drag.End();
            _ghost.gameObject.SetActive(false);
            ApplyTheme();
            foreach (Transform child in _content)
            {
                child.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(child.gameObject);
            }

            var player = Player;
            if (player == null)
            {
                CharacterWidgets.Box(_host, _content, "EquipmentFrame", 0f, 0f,
                    CharacterMetrics.FrameWidth, CharacterMetrics.FrameHeight, CharacterPalette.Frame);
                var inventory = CharacterWidgets.Box(_host, _content, "InventoryFrame",
                    CharacterMetrics.FrameWidth + CharacterMetrics.Gap, 0f,
                    CharacterMetrics.InventoryWidth, CharacterMetrics.FrameHeight, CharacterPalette.Frame);
                InventoryPanelArt.ApplyBackground(_host, inventory, CharacterPalette.Frame);
                CharacterWidgets.Label(_host, _content, "NoGame", UiText.Get(_host.Texts, UiTextKey.Common_No_Game), 20, TextAnchor.MiddleCenter,
                    UiFactory.MutedTextColor, CharacterMetrics.FrameWidth + CharacterMetrics.Gap + 20f, 20f,
                    CharacterMetrics.InventoryWidth - 40f, 40f);
                _host.Router.Show(Panel);
                return;
            }

            new CharacterEquipmentFrame(_host, _content, this).Build(player);
            new CharacterInventoryFrame(_host, _content, this).Build(player);
            new CharacterStatsStrip(_host, _content).Build(player);
            _host.Router.Show(Panel);
        }

        /// <inheritdoc />
        public bool CanEquip(ItemInstance item)
        {
            var player = Player;
            if (player == null)
            {
                return false;
            }

            var template = CharacterWidgets.Template(_host, item);
            if (template == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(template.ArtifactType))
            {
                return true;
            }

            return _host.Actions.DefaultSlot(player.Equipment, item) != null;
        }

        /// <inheritdoc />
        public void OpenItemMenu(Vector2 position, ItemInstance item, EquipmentSlot? slot, int artifactIndex)
        {
            var entries = new List<KeyValuePair<string, Action>>();

            if (slot.HasValue)
            {
                var captured = slot.Value;
                entries.Add(new KeyValuePair<string, Action>(UiText.Get(_host.Texts, UiTextKey.Character_Menu_Unequip), () => _presenter.Unequip(captured)));
            }
            else if (artifactIndex >= 0)
            {
                var capturedIndex = artifactIndex;
                entries.Add(new KeyValuePair<string, Action>(UiText.Get(_host.Texts, UiTextKey.Character_Menu_Unequip), () => _presenter.UnequipArtifact(capturedIndex)));
            }
            else if (CanEquip(item))
            {
                var id = item.InstanceId;
                entries.Add(new KeyValuePair<string, Action>(UiText.Get(_host.Texts, UiTextKey.Character_Menu_Equip), () => _presenter.EquipAuto(id)));
            }

            if (CharacterWidgets.Template(_host, item)?.Weapon != null)
            {
                var id = item.InstanceId;
                entries.Add(new KeyValuePair<string, Action>(UiText.Get(_host.Texts, UiTextKey.Character_Menu_Unload), () => _presenter.Unload(id)));
            }

            entries.Add(new KeyValuePair<string, Action>(UiText.Get(_host.Texts, UiTextKey.Character_Menu_Disassemble), () => _presenter.Disassemble(item.InstanceId)));
            entries.Add(new KeyValuePair<string, Action>(UiText.Get(_host.Texts, UiTextKey.Character_Menu_Discard), () => _presenter.Discard(item.InstanceId)));

            _menu.Open(position, entries);
        }

        /// <inheritdoc />
        public void DropOnSlot(string instanceId, EquipmentSlot slot)
        {
            if (_drag.Origin == DragOrigin.Inventory)
            {
                _presenter.Equip(instanceId, slot);
            }
        }

        /// <inheritdoc />
        public void DropOnArtifact(string instanceId)
        {
            if (_drag.Origin == DragOrigin.Inventory)
            {
                _presenter.EquipArtifact(instanceId);
            }
        }

        /// <inheritdoc />
        public void DropOnInventory(string instanceId, int x, int y, bool rotated)
        {
            var player = Player;
            if (player == null)
            {
                return;
            }

            if (_drag.Origin == DragOrigin.Inventory)
            {
                if (x >= 0 && y >= 0)
                {
                    _presenter.Move(instanceId, x, y, rotated);
                }

                return;
            }

            if (_drag.Origin == DragOrigin.Equipment)
            {
                foreach (var pair in player.Equipment.Slots)
                {
                    if (pair.Value.InstanceId == instanceId)
                    {
                        _presenter.Unequip(pair.Key);
                        return;
                    }
                }

                return;
            }

            for (var index = 0; index < player.Equipment.Artifacts.Count; index++)
            {
                var artifact = player.Equipment.Artifacts[index];
                if (artifact != null && artifact.InstanceId == instanceId)
                {
                    _presenter.UnequipArtifact(index);
                    return;
                }
            }
        }

        private PlayerState? Player => _host.Session?.State.Player;

        /// <summary>Фон экрана персонажа — текущая локация, если игра уже началась.</summary>
        protected override ScreenTheme Theme
        {
            get
            {
                var world = Host.Session?.State.World;
                return world == null ? ScreenTheme.Default : ScreenTheme.Location(world.CurrentLocation.Background);
            }
        }

        private void SetMessage(string text, bool ok)
        {
            _message.text = text;
            _message.color = ok ? UiFactory.ToastColor : CharacterPalette.Error;
        }

        private static Transform CreateContent(Transform parent)
        {
            var root = new GameObject("CharacterContent", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(
                CharacterMetrics.FrameWidth + CharacterMetrics.Gap + CharacterMetrics.InventoryWidth,
                CharacterMetrics.FrameHeight);
            rect.anchoredPosition = Vector2.zero;
            return root.transform;
        }
    }
}
