#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using InventoryEquipment = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Core.Loadouts
{
    /// <summary>
    /// Пробная раскладка набора снаряжения: сетка инвентаря, занятые слоты и всё, что не удалось
    /// применить. Показ набора не падает на битых данных — ошибки лежат в отчёте, а старт игры
    /// проверяет его и отказывается начинаться.
    /// </summary>
    public sealed class LoadoutPreview
    {
        /// <summary>Создаёт результат раскладки набора.</summary>
        /// <param name="loadout">Исходный набор снаряжения.</param>
        /// <param name="inventory">Сетка стартового инвентаря с разложенными предметами.</param>
        /// <param name="equipment">Экипировка с надетыми предметами.</param>
        /// <param name="overflow">Экземпляры, которым не хватило места в стартовом инвентаре.</param>
        /// <param name="rejected">Предметы, которые не встали в назначенный слот.</param>
        /// <param name="display">Сетка для показа: все предметы набора как обычные экземпляры.</param>
        /// <param name="displayOverflow">Экземпляры, которым не хватило места в сетке показа.</param>
        /// <exception cref="ArgumentNullException">Обязательное значение не задано.</exception>
        public LoadoutPreview(
            LoadoutTemplate loadout,
            GridContainer inventory,
            InventoryEquipment equipment,
            IReadOnlyList<ItemInstance> overflow,
            IReadOnlyList<KeyValuePair<EquipmentSlot, string>> rejected,
            GridContainer display,
            IReadOnlyList<ItemInstance> displayOverflow)
        {
            Loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
            Inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
            Overflow = overflow ?? throw new ArgumentNullException(nameof(overflow));
            Rejected = rejected ?? throw new ArgumentNullException(nameof(rejected));
            Display = display ?? throw new ArgumentNullException(nameof(display));
            DisplayOverflow = displayOverflow ?? throw new ArgumentNullException(nameof(displayOverflow));
        }

        /// <summary>Исходный набор снаряжения.</summary>
        public LoadoutTemplate Loadout { get; }

        /// <summary>Сетка стартового инвентаря: расходники набора уже разложены по клеткам.</summary>
        public GridContainer Inventory { get; }

        /// <summary>Экипировка с надетыми предметами набора.</summary>
        public InventoryEquipment Equipment { get; }

        /// <summary>Экземпляры, которым не хватило места в стартовом инвентаре.</summary>
        public IReadOnlyList<ItemInstance> Overflow { get; }

        /// <summary>Предметы набора, которые не встали в назначенный слот: слот и id предмета.</summary>
        public IReadOnlyList<KeyValuePair<EquipmentSlot, string>> Rejected { get; }

        /// <summary>
        /// Сетка для показа игроку: надетые предметы разложены как обычные экземпляры вместе
        /// с расходниками. Стартовый инвентарь и слоты она не заменяет.
        /// </summary>
        public GridContainer Display { get; }

        /// <summary>Экземпляры, которым не хватило места в сетке показа.</summary>
        public IReadOnlyList<ItemInstance> DisplayOverflow { get; }

        /// <summary><c>true</c>, если стартовый инвентарь и слоты собрались полностью.</summary>
        public bool IsComplete => Overflow.Count == 0 && Rejected.Count == 0;

        /// <summary><c>true</c>, если все предметы набора поместились в сетку показа.</summary>
        public bool IsDisplayComplete => DisplayOverflow.Count == 0;
    }
}
