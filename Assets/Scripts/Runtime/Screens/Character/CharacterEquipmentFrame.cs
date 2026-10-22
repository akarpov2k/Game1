#nullable enable
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Player;
using AnomalyPath.Runtime.Ui.Core;
using AnomalyPath.Runtime.Ui.Items;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Character
{
    /// <summary>Фрейм снаряжения: слоты вокруг фигуры и столбец артефактов.</summary>
    public sealed class CharacterEquipmentFrame
    {
        private readonly IUiHost _host;
        private readonly Transform _content;
        private readonly ICharacterFrameHost _frameHost;

        /// <summary>Создаёт фрейм снаряжения.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="content">Корень содержимого экрана.</param>
        /// <param name="frameHost">Экран персонажа.</param>
        public CharacterEquipmentFrame(IUiHost host, Transform content, ICharacterFrameHost frameHost)
        {
            _host = host;
            _content = content;
            _frameHost = frameHost;
        }

        /// <summary>Рисует слоты, фигуру и столбец артефактов.</summary>
        /// <param name="player">Состояние игрока.</param>
        public void Build(PlayerState player)
        {
            CharacterWidgets.Box(_host, _content, "EquipmentFrame", 0f, 0f,
                CharacterMetrics.FrameWidth, CharacterMetrics.FrameHeight, CharacterPalette.Frame);

            var equipment = player.Equipment;
            CharacterWidgets.Label(_host, _content, "Nickname", player.Name, 16, TextAnchor.MiddleLeft,
                UiFactory.TextColor, 16f, 6f, 360f, 28f);
            CharacterWidgets.Label(_host, _content, "Level", $"Ур. {player.Level}", 14, TextAnchor.MiddleRight,
                UiFactory.MutedTextColor, 384f, 6f, 160f, 28f);

            Slot("SlotHelmet", 16f, 40f, 160f, 120f, "Шлем", EquipmentSlot.Helmet,
                equipment.Get(EquipmentSlot.Helmet));
            Slot("SlotArmor", 16f, 172f, 160f, 180f, "Броня", EquipmentSlot.Armor,
                equipment.Get(EquipmentSlot.Armor));
            Slot("SlotBackpack", 384f, 40f, 160f, 150f, "Рюкзак", EquipmentSlot.Backpack,
                equipment.Get(EquipmentSlot.Backpack));
            Slot("SlotBelt", 384f, 202f, 160f, 140f, "Пояс артефактов", EquipmentSlot.Belt,
                equipment.Get(EquipmentSlot.Belt));

            new CharacterArtifactColumn(_host, _content, _frameHost).Build(player);

            Slot("SlotSecondary", 130f, 455f, 300f, 90f, "Дополнительное оружие", EquipmentSlot.Secondary,
                equipment.Get(EquipmentSlot.Secondary));
            Slot("SlotMain", 100f, 552f, 360f, 128f, "Основное оружие", EquipmentSlot.Main,
                equipment.Get(EquipmentSlot.Main), 24);

            new CharacterFigure(_host, _content).Build();
        }

        private void Slot(
            string name,
            float x,
            float y,
            float width,
            float height,
            string caption,
            EquipmentSlot slot,
            ItemInstance? item,
            int valueSize = 18)
        {
            var frame = CharacterWidgets.Box(_host, _content, name, x, y, width, height, CharacterPalette.Slot);
            var zone = frame.gameObject.AddComponent<ItemDropZone>();
            zone.Configure(_frameHost.Drag, instanceId => _frameHost.DropOnSlot(instanceId, slot));

            if (item != null)
            {
                CharacterWidgets.AttachItem(_host, _frameHost, frame, item, slot, -1, DragOrigin.Equipment);
            }

            CharacterWidgets.Label(_host, _content, name + "Caption", caption, 14, TextAnchor.UpperLeft,
                UiFactory.MutedTextColor, x + 8f, y + 6f, width - 16f, 20f);
            CharacterWidgets.Label(_host, _content, name + "Value",
                item == null ? "—" : CharacterWidgets.Describe(_host, item), valueSize, TextAnchor.MiddleCenter,
                item == null ? UiFactory.MutedTextColor : UiFactory.TextColor,
                x + 8f, y + 28f, width - 16f, height - 36f);
        }
    }
}
