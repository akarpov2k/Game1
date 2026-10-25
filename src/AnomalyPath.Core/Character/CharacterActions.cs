#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Templates;
using EquipmentState = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Core.Character
{
    /// <summary>
    /// Действия с предметами игрока: надеть и снять предмет, надеть и снять артефакт, зарядить и разрядить оружие,
    /// разобрать и выбросить. Все проверки делаются до изменения состояния, чтобы ошибка ничего не ломала.
    /// </summary>
    public sealed partial class CharacterActions : ICharacterActions
    {
        private readonly IItemRepository _items;
        private readonly IInventoryService _inventory;
        private readonly IEquipmentService _equipment;

        /// <summary>Создаёт действия поверх репозитория предметов, инвентаря и экипировки.</summary>
        /// <param name="items">Репозиторий шаблонов.</param>
        /// <param name="inventory">Сервис инвентаря.</param>
        /// <param name="equipment">Сервис экипировки.</param>
        /// <exception cref="ArgumentNullException">Зависимость не задана.</exception>
        public CharacterActions(IItemRepository items, IInventoryService inventory, IEquipmentService equipment)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
        }
    }
}
