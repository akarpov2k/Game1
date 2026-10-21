#nullable enable
using AnomalyPath.Core.Inventory;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Износ брони и шлема: сколько прочности снимает попадание.</summary>
    public interface IArmorWearService
    {
        /// <summary>Считает износ от попадания по формуле: урон × материал × качество, минимум 1.</summary>
        /// <param name="damage">Урон попадания.</param>
        /// <param name="armor">Экземпляр брони или шлема.</param>
        /// <returns>Сколько прочности снять; 0, если урона нет.</returns>
        int CalculateWear(double damage, ItemInstance armor);

        /// <summary>Применяет износ: уменьшает прочность, но не ниже нуля.</summary>
        /// <param name="armor">Экземпляр брони или шлема.</param>
        /// <param name="damage">Урон попадания.</param>
        /// <returns>Фактически снятая прочность.</returns>
        int ApplyWear(ItemInstance armor, double damage);
    }
}
