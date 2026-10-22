#nullable enable
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Runtime.Ui.Items;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Character
{
    /// <summary>Что фреймы экрана персонажа могут попросить у самого экрана.</summary>
    public interface ICharacterFrameHost
    {
        /// <summary>Общий контекст перетаскивания.</summary>
        ItemDragContext Drag { get; }

        /// <summary>Подпись, которая летит за курсором при перетаскивании.</summary>
        Text Ghost { get; }

        /// <summary>Проверяет, есть ли у предмета подходящий слот для кнопки «Одеть».</summary>
        /// <param name="item">Экземпляр предмета.</param>
        /// <returns><c>true</c>, если предмет надевается.</returns>
        bool CanEquip(ItemInstance item);

        /// <summary>Открывает контекстное меню предмета.</summary>
        /// <param name="position">Позиция курсора.</param>
        /// <param name="item">Предмет.</param>
        /// <param name="slot">Слот снаряжения, если предмет надет.</param>
        /// <param name="artifactIndex">Индекс слота артефакта или −1.</param>
        void OpenItemMenu(Vector2 position, ItemInstance item, EquipmentSlot? slot, int artifactIndex);

        /// <summary>Обрабатывает сброс предмета на слот снаряжения.</summary>
        /// <param name="instanceId">Id экземпляра.</param>
        /// <param name="slot">Слот.</param>
        void DropOnSlot(string instanceId, EquipmentSlot slot);

        /// <summary>Обрабатывает сброс предмета в столбец артефактов.</summary>
        /// <param name="instanceId">Id экземпляра.</param>
        void DropOnArtifact(string instanceId);

        /// <summary>Обрабатывает сброс предмета в область инвентаря.</summary>
        /// <param name="instanceId">Id экземпляра.</param>
        /// <param name="x">Колонка цели или −1, если место подбирается автоматически.</param>
        /// <param name="y">Строка цели или −1, если место подбирается автоматически.</param>
        /// <param name="rotated">Нужно ли повернуть предмет при сбросе.</param>
        void DropOnInventory(string instanceId, int x, int y, bool rotated);
    }
}
