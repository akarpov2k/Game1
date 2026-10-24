#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Inventory;

namespace AnomalyPath.Core.Death
{
    /// <summary>Итог смерти: потерянные вещи, оставшиеся и спасённые страховкой надетые предметы.</summary>
    public sealed class DeathResult
    {
        internal DeathResult(
            IReadOnlyList<ItemInstance> lost,
            IReadOnlyList<ItemInstance> kept,
            IReadOnlyList<ItemInstance> insured,
            bool permadeath)
        {
            Lost = lost;
            Kept = kept;
            Insured = insured;
            Permadeath = permadeath;
        }

        /// <summary>Предметы, убранные из инвентаря и слотов.</summary>
        public IReadOnlyList<ItemInstance> Lost { get; }

        /// <summary>Предметы, оставшиеся у игрока: неутрачиваемые и спасённые страховкой.</summary>
        public IReadOnlyList<ItemInstance> Kept { get; }

        /// <summary>Надетые предметы, которые остались именно благодаря успешному броску страховки.</summary>
        public IReadOnlyList<ItemInstance> Insured { get; }

        /// <summary><c>true</c>, если действовал режим полной потери.</summary>
        public bool Permadeath { get; }
    }
}
