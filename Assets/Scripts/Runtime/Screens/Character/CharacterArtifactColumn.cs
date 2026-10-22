#nullable enable
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Player;
using AnomalyPath.Runtime.Ui.Core;
using AnomalyPath.Runtime.Ui.Items;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Character
{
    /// <summary>Столбец слотов артефактов под поясом по правому краю фрейма снаряжения.</summary>
    public sealed class CharacterArtifactColumn
    {
        private readonly IUiHost _host;
        private readonly Transform _content;
        private readonly ICharacterFrameHost _frameHost;

        /// <summary>Создаёт столбец артефактов.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="content">Корень содержимого экрана.</param>
        /// <param name="frameHost">Экран персонажа.</param>
        public CharacterArtifactColumn(IUiHost host, Transform content, ICharacterFrameHost frameHost)
        {
            _host = host;
            _content = content;
            _frameHost = frameHost;
        }

        /// <summary>Рисует слоты артефактов по текущей ёмкости брони и пояса.</summary>
        /// <param name="player">Состояние игрока.</param>
        public void Build(PlayerState player)
        {
            var capacity = _host.Equipment.TotalArtifactSlots(player.Equipment);
            var count = capacity < 1 ? 1 : capacity;
            var artifacts = player.Equipment.Artifacts;
            var x = CharacterMetrics.FrameWidth - CharacterMetrics.Padding - CharacterMetrics.Cell;
            const float top = 354f;

            for (var index = 0; index < count; index++)
            {
                var item = index < artifacts.Count ? artifacts[index] : null;
                var y = top + (index * (CharacterMetrics.Cell + 8f));
                var frame = CharacterWidgets.Box(_host, _content, "ArtifactSlot_" + index, x, y,
                    CharacterMetrics.Cell, CharacterMetrics.Cell,
                    item == null ? CharacterPalette.Slot : ItemPalette.Artifact);

                var zone = frame.gameObject.AddComponent<ItemDropZone>();
                var capturedIndex = index;
                zone.Configure(_frameHost.Drag, _frameHost.DropOnArtifact);

                if (item != null)
                {
                    CharacterWidgets.AttachItem(_host, _frameHost, frame, item, null, capturedIndex, DragOrigin.Artifact);
                }

                var label = CharacterWidgets.Label(_host, _content, "ArtifactSlot_" + index + "Value",
                    item == null ? "—" : CharacterWidgets.Describe(_host, item), 12, TextAnchor.MiddleCenter,
                    item == null ? UiFactory.MutedTextColor : UiFactory.TextColor,
                    x + 4f, y + 4f, CharacterMetrics.Cell - 8f, CharacterMetrics.Cell - 8f);
                label.verticalOverflow = VerticalWrapMode.Truncate;
            }
        }
    }
}
