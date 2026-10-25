#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Inventory;

namespace AnomalyPath.Core.Loot
{
    /// <summary>Результат вскрытия тайника: статус, лут и сработал ли джекпот.</summary>
    public sealed class StashOpenResult
    {
        private StashOpenResult(
            StashOpenStatus status,
            bool jackpot,
            string? lootTableId,
            IReadOnlyList<ItemInstance> loot)
        {
            Status = status;
            Jackpot = jackpot;
            LootTableId = lootTableId;
            Loot = loot;
        }

        /// <summary>Чем закончилась попытка.</summary>
        public StashOpenStatus Status { get; }

        /// <summary><c>true</c> — сработал джекпот и лут взят из его пула.</summary>
        public bool Jackpot { get; }

        /// <summary>Какая таблица дала лут; <c>null</c>, если лута нет.</summary>
        public string? LootTableId { get; }

        /// <summary>Выпавшие экземпляры; пустой список, если тайника нет или пул пуст.</summary>
        public IReadOnlyList<ItemInstance> Loot { get; }

        /// <summary>На локации нет тайника.</summary>
        /// <returns>Результат без лута.</returns>
        public static StashOpenResult NoStash() =>
            new StashOpenResult(StashOpenStatus.NoStash, false, null, Array.Empty<ItemInstance>());

        /// <summary>Тайник уже вскрыт.</summary>
        /// <returns>Результат без лута.</returns>
        public static StashOpenResult AlreadyOpened() =>
            new StashOpenResult(StashOpenStatus.AlreadyOpened, false, null, Array.Empty<ItemInstance>());

        /// <summary>Тайник вскрыт.</summary>
        /// <param name="jackpot">Сработал ли джекпот.</param>
        /// <param name="lootTableId">Таблица, из которой взят лут.</param>
        /// <param name="loot">Выпавшие экземпляры.</param>
        /// <returns>Результат вскрытия.</returns>
        /// <exception cref="ArgumentNullException">Лут не задан.</exception>
        public static StashOpenResult Opened(
            bool jackpot,
            string lootTableId,
            IReadOnlyList<ItemInstance> loot)
        {
            if (loot == null)
            {
                throw new ArgumentNullException(nameof(loot));
            }

            return new StashOpenResult(StashOpenStatus.Opened, jackpot, lootTableId, loot);
        }
    }
}
