#nullable enable
using System;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Inventory;

namespace AnomalyPath.Core.Equipment
{
    /// <summary>
    /// Износ брони и шлема: <c>ceil(урон / (материал × качество × 10))</c>, но не меньше 1 за попадание.
    /// Чем прочнее материал и выше качество, тем меньше износ; пулестойкость влияет на здоровье, а не на износ.
    /// </summary>
    public sealed class ArmorWearService : IArmorWearService
    {
        private readonly IItemRepository _items;

        /// <summary>Создаёт сервис поверх репозитория предметов.</summary>
        /// <param name="items">Репозиторий шаблонов.</param>
        public ArmorWearService(IItemRepository items)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
        }

        /// <inheritdoc />
        public int CalculateWear(double damage, ItemInstance armor)
        {
            if (armor == null)
            {
                throw new ArgumentNullException(nameof(armor));
            }

            if (damage <= 0 || armor.IsBroken)
            {
                return 0;
            }

            var template = _items.Get(armor.ItemId);
            var wear = Math.Ceiling(damage / (template.DurabilityFactor * template.Quality * 10.0));
            return wear < 1 ? 1 : (int)wear;
        }

        /// <inheritdoc />
        public int ApplyWear(ItemInstance armor, double damage)
        {
            if (armor == null)
            {
                throw new ArgumentNullException(nameof(armor));
            }

            var wear = CalculateWear(damage, armor);
            if (wear <= 0)
            {
                return 0;
            }

            var current = armor.Durability ?? armor.DurabilityMax ?? 0;
            var next = Math.Max(0, current - wear);
            armor.Durability = next;
            return current - next;
        }
    }
}
