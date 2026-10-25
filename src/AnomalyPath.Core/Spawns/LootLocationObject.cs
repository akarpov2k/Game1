#nullable enable
using System;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Spawns
{
    /// <summary>Объект лута: при осмотре открывает лут-диалог по своей таблице.</summary>
    public sealed class LootLocationObject : LocationObject
    {
        /// <summary>Создаёт объект лута.</summary>
        /// <param name="spawnId">Id записи спавна.</param>
        /// <param name="titleKey">Ключ названия в каталоге строк.</param>
        /// <param name="lootTableId">Id таблицы лута.</param>
        /// <exception cref="ArgumentException">Id таблицы пуст.</exception>
        public LootLocationObject(string spawnId, string titleKey, string lootTableId)
            : base(spawnId, titleKey, LocationObjectKind.Loot)
        {
            if (string.IsNullOrWhiteSpace(lootTableId))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(lootTableId)),
                    nameof(lootTableId));
            }

            LootTableId = lootTableId;
        }

        /// <summary>Id таблицы лута.</summary>
        public string LootTableId { get; }
    }
}
