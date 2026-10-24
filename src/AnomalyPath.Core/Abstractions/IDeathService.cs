#nullable enable
using AnomalyPath.Core.Death;
using AnomalyPath.Core.Save;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>
    /// Смерть персонажа: инвентарь теряется полностью, кроме помеченных неутрачиваемыми,
    /// а каждый надетый предмет страхуется отдельным броском. В режиме постоянной смерти
    /// страховка не применяется и теряется всё снаряжение.
    /// </summary>
    public interface IDeathService
    {
        /// <summary>Применяет последствия смерти к игроку: убирает потерянные вещи из инвентаря и слотов.</summary>
        /// <param name="state">Состояние прохождения; без игрока результат пуст.</param>
        /// <returns>Что потеряно, что осталось и что спасла страховка.</returns>
        /// <exception cref="System.ArgumentNullException">Состояние не задано.</exception>
        DeathResult Apply(GameState state);
    }
}
