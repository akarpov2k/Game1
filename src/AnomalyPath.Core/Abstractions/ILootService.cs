#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Loot;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Бросок лута с побеждённого противника: детерминированно по seed.</summary>
    public interface ILootService
    {
        /// <summary>Считает лут по таблице противника.</summary>
        /// <param name="context">Кто погиб, где и с каким seed.</param>
        /// <returns>Выпавшие экземпляры; пустой список, если лута нет.</returns>
        IReadOnlyList<ItemInstance> Roll(LootContext context);
    }
}
