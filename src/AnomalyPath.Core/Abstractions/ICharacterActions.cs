#nullable enable
using AnomalyPath.Core.Character;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using EquipmentState = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>
    /// Действия с предметами игрока: надеть, снять, надеть и снять артефакт, разрядить оружие,
    /// разобрать (тестовый режим) и выбросить. Меняют переданные состояния и не трогают UI.
    /// </summary>
    public interface ICharacterActions
    {
        /// <summary>Надевает предмет из инвентаря в слот; ранее надетый предмет возвращается в инвентарь.</summary>
        /// <param name="inventory">Сетка инвентаря.</param>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="instanceId">Id экземпляра в инвентаре.</param>
        /// <param name="slot">Целевой слот.</param>
        /// <returns>Итог действия; при ошибке состояние не меняется.</returns>
        CharacterActionResult Equip(GridContainer inventory, EquipmentState equipment, string instanceId, EquipmentSlot slot);

        /// <summary>Снимает предмет из слота в инвентарь; лишние артефакты тоже перекладываются.</summary>
        /// <param name="inventory">Сетка инвентаря.</param>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="slot">Слот.</param>
        /// <returns>Итог действия; при ошибке состояние не меняется.</returns>
        CharacterActionResult Unequip(GridContainer inventory, EquipmentState equipment, EquipmentSlot slot);

        /// <summary>Надевает артефакт из инвентаря в свободный слот пояса.</summary>
        /// <param name="inventory">Сетка инвентаря.</param>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="instanceId">Id экземпляра в инвентаре.</param>
        /// <returns>Итог действия; при ошибке состояние не меняется.</returns>
        CharacterActionResult EquipArtifact(GridContainer inventory, EquipmentState equipment, string instanceId);

        /// <summary>Снимает артефакт из слота в инвентарь.</summary>
        /// <param name="inventory">Сетка инвентаря.</param>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="index">Индекс слота артефакта.</param>
        /// <returns>Итог действия; при ошибке состояние не меняется.</returns>
        CharacterActionResult UnequipArtifact(GridContainer inventory, EquipmentState equipment, int index);

        /// <summary>Разряжает оружие: патроны из магазина возвращаются в инвентарь.</summary>
        /// <param name="inventory">Сетка инвентаря.</param>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="instanceId">Id экземпляра оружия (в инвентаре или в экипировке).</param>
        /// <param name="rounds">Сколько патронов удалось вернуть.</param>
        /// <returns>Итог действия; при ошибке состояние не меняется.</returns>
        CharacterActionResult Unload(GridContainer inventory, EquipmentState equipment, string instanceId, out int rounds);

        /// <summary>Разбирает предмет (тестовый режим): предмет исчезает, ресурсы пока не выдаются.</summary>
        /// <param name="inventory">Сетка инвентаря.</param>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="instanceId">Id экземпляра.</param>
        /// <returns>Итог действия.</returns>
        CharacterActionResult Disassemble(GridContainer inventory, EquipmentState equipment, string instanceId);

        /// <summary>Выбрасывает предмет: он исчезает из инвентаря или экипировки.</summary>
        /// <param name="inventory">Сетка инвентаря.</param>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="instanceId">Id экземпляра.</param>
        /// <returns>Итог действия.</returns>
        CharacterActionResult Discard(GridContainer inventory, EquipmentState equipment, string instanceId);

        /// <summary>Подбирает слот для кнопки «Одеть»: первый свободный из разрешённых, иначе первый разрешённый.</summary>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="item">Экземпляр предмета.</param>
        /// <returns>Слот или <c>null</c>, если предмет не надевается.</returns>
        EquipmentSlot? DefaultSlot(EquipmentState equipment, ItemInstance item);
    }
}
