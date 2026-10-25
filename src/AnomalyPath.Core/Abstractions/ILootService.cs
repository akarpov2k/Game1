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

        /// <summary>
        /// Считает лут по произвольной таблице: тайник, джекпот или другой пул без противника.
        /// Множителя редкости нет, броски детерминированы seed.
        /// </summary>
        /// <param name="lootTableId">Id таблицы лута.</param>
        /// <param name="locationTier">Тир локации для полосы тира.</param>
        /// <param name="seed">Seed броска.</param>
        /// <param name="ignoreTierBand"><c>true</c> — полоса тира не проверяется.</param>
        /// <returns>Выпавшие экземпляры; пустой список, если таблицы нет.</returns>
        IReadOnlyList<ItemInstance> RollTable(
            string lootTableId,
            int locationTier,
            int seed,
            bool ignoreTierBand);
    }
}
