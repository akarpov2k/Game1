#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using EquipmentState = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Экипировка: надевание в слоты, быстрый доступ, суммарные слоты артефактов и строки рюкзака.</summary>
    public interface IEquipmentService
    {
        /// <summary>Проверяет, можно ли надеть предмет в слот.</summary>
        /// <param name="item">Экземпляр предмета.</param>
        /// <param name="slot">Целевой слот.</param>
        /// <returns><c>true</c>, если слот разрешён данными предмета.</returns>
        bool CanEquip(ItemInstance item, EquipmentSlot slot);

        /// <summary>Надевает предмет в слот.</summary>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="item">Экземпляр предмета.</param>
        /// <param name="slot">Целевой слот.</param>
        /// <param name="replaced">Предмет, который был в слоте.</param>
        /// <returns><c>true</c>, если предмет надет.</returns>
        bool TryEquip(EquipmentState equipment, ItemInstance item, EquipmentSlot slot, out ItemInstance? replaced);

        /// <summary>
        /// Снимает предмет из слота. Для пояса и брони артефакты сверх нового лимита
        /// перекладываются в инвентарь; если места нет — снятие блокируется.
        /// </summary>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="slot">Слот.</param>
        /// <param name="inventory">Контейнер, куда переложить артефакты.</param>
        /// <param name="removed">Снятый предмет.</param>
        /// <returns><c>true</c>, если предмет снят.</returns>
        bool TryUnequip(EquipmentState equipment, EquipmentSlot slot, GridContainer inventory, out ItemInstance? removed);

        /// <summary>Снимает предмет из слота.</summary>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="slot">Слот.</param>
        /// <returns>Снятый предмет или <c>null</c>.</returns>
        ItemInstance? Unequip(EquipmentState equipment, EquipmentSlot slot);

        /// <summary>Проверяет, можно ли положить предмет в быстрый слот (оружие или расходник).</summary>
        /// <param name="item">Экземпляр предмета.</param>
        /// <returns><c>true</c>, если предмет допустим.</returns>
        bool CanUseQuick(ItemInstance item);

        /// <summary>Кладёт предмет в быстрый слот.</summary>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="index">Индекс слота.</param>
        /// <param name="item">Экземпляр предмета.</param>
        /// <param name="replaced">Предмет, который был в слоте.</param>
        /// <returns><c>true</c>, если предмет положен.</returns>
        bool TryPutQuick(EquipmentState equipment, int index, ItemInstance item, out ItemInstance? replaced);

        /// <summary>Забирает предмет из быстрого слота.</summary>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="index">Индекс слота.</param>
        /// <returns>Предмет или <c>null</c>.</returns>
        ItemInstance? TakeQuick(EquipmentState equipment, int index);

        /// <summary>Сколько артефактов можно надеть: броня плюс пояс.</summary>
        /// <param name="equipment">Экипировка.</param>
        /// <returns>Число слотов под артефакты.</returns>
        int ArtifactCapacity(EquipmentState equipment);

        /// <summary>Надевает артефакт в свободный слот пояса.</summary>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="artifact">Экземпляр артефакта.</param>
        /// <param name="index">Индекс занятого слота.</param>
        /// <returns><c>true</c>, если артефакт надет.</returns>
        bool TryEquipArtifact(EquipmentState equipment, ItemInstance artifact, out int index);

        /// <summary>Снимает артефакт из слота.</summary>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="index">Индекс слота.</param>
        /// <returns>Снятый артефакт или <c>null</c>.</returns>
        ItemInstance? TakeArtifact(EquipmentState equipment, int index);

        /// <summary>Снимает артефакты, которым больше нет места после снятия брони или пояса.</summary>
        /// <param name="equipment">Экипировка.</param>
        /// <returns>Снятые артефакты.</returns>
        System.Collections.Generic.IReadOnlyList<ItemInstance> TrimArtifacts(EquipmentState equipment);

        /// <summary>Суммарные слоты артефактов: броня плюс пояс.</summary>
        /// <param name="equipment">Экипировка.</param>
        /// <returns>Число слотов.</returns>
        int TotalArtifactSlots(EquipmentState equipment);

        /// <summary>
        /// Суммарная защита надетых брони и шлема по типу урона.
        /// Сломанные предметы (прочность 0) в расчёт не входят.
        /// </summary>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="damageType">Тип урона, например <c>dmg_bullet</c>.</param>
        /// <returns>Сумма защиты; 0, если защиты нет.</returns>
        int GetProtection(EquipmentState equipment, string damageType);

        /// <summary>Строки инвентаря с учётом рюкзака.</summary>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="baseRows">Базовые строки из правил игры.</param>
        /// <returns>Итоговое число строк.</returns>
        int InventoryRows(EquipmentState equipment, int baseRows);
    }
}
