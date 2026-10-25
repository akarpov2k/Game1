#nullable enable
using AnomalyPath.Core.Death;
using AnomalyPath.Core.Save;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>
    /// Смерть персонажа: инвентарь теряется полностью, кроме помеченных неутрачиваемыми,
    /// а каждый надетый предмет страхуется отдельным броском. В режиме постоянной смерти
    /// страховка не применяется и теряется всё снаряжение. Доля опыта текущего уровня
    /// теряется по настройке старта, но уровень не понижается.
    /// </summary>
    public interface IDeathService
    {
        /// <summary>
        /// Применяет последствия смерти к игроку: убирает потерянные вещи из инвентаря и слотов
        /// и списывает долю опыта текущего уровня.
        /// </summary>
        /// <param name="state">Состояние прохождения; без игрока результат пуст.</param>
        /// <returns>Что потеряно, что осталось, что спасла страховка и сколько опыта потеряно.</returns>
        /// <exception cref="System.ArgumentNullException">Состояние не задано.</exception>
        DeathResult Apply(GameState state);
    }
}
