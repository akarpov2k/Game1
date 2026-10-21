#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Inventory;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Операции с контейнерами: добавление, стаки, автоподбор, перемещение, удаление.</summary>
    public interface IInventoryService
    {
        /// <summary>Добавляет предметы в контейнер: стакает, затем ищет места, остаток — в переполнение.</summary>
        /// <param name="container">Целевой контейнер.</param>
        /// <param name="itemId">Id шаблона предмета.</param>
        /// <param name="count">Сколько добавить.</param>
        /// <param name="overflow">Экземпляры, которым не хватило места.</param>
        /// <returns><c>true</c>, если предметы обработаны.</returns>
        bool TryAdd(GridContainer container, string itemId, int count, out IReadOnlyList<ItemInstance> overflow);

        /// <summary>Добавляет предметы с заданной прочностью и вариантом.</summary>
        /// <param name="container">Целевой контейнер.</param>
        /// <param name="itemId">Id шаблона предмета.</param>
        /// <param name="count">Сколько добавить.</param>
        /// <param name="durability">Прочность; <c>null</c> — взять максимум из шаблона.</param>
        /// <param name="variantKey">Ключ варианта (редкость, скиллы).</param>
        /// <param name="overflow">Экземпляры, которым не хватило места.</param>
        /// <returns><c>true</c>, если предметы обработаны.</returns>
        bool TryAdd(
            GridContainer container,
            string itemId,
            int count,
            int? durability,
            string? variantKey,
            out IReadOnlyList<ItemInstance> overflow);

        /// <summary>Проверяет, поместятся ли все экземпляры, не меняя контейнер.</summary>
        /// <param name="container">Контейнер.</param>
        /// <param name="items">Экземпляры для проверки.</param>
        /// <returns><c>true</c>, если места хватает всем.</returns>
        bool CanFit(GridContainer container, IReadOnlyList<ItemInstance> items);

        /// <summary>Убирает экземпляр из контейнера.</summary>
        /// <param name="container">Контейнер.</param>
        /// <param name="instanceId">Id экземпляра.</param>
        /// <returns><c>true</c>, если экземпляр был удалён.</returns>
        bool Remove(GridContainer container, string instanceId);

        /// <summary>Перемещает экземпляр в свободную позицию.</summary>
        /// <param name="container">Контейнер.</param>
        /// <param name="instanceId">Id экземпляра.</param>
        /// <param name="x">Колонка.</param>
        /// <param name="y">Строка.</param>
        /// <param name="rotated">Поворот.</param>
        /// <returns><c>true</c>, если перемещение удалось.</returns>
        bool TryMove(GridContainer container, string instanceId, int x, int y, bool rotated);

        /// <summary>Суммарное количество предметов по id, включая стопки и переполнение.</summary>
        /// <param name="container">Контейнер.</param>
        /// <param name="itemId">Id шаблона предмета.</param>
        /// <returns>Суммарное количество.</returns>
        int Count(GridContainer container, string itemId);

        /// <summary>Берёт патроны указанного калибра: сначала из неполных стаков, потом из полных.</summary>
        /// <param name="container">Контейнер.</param>
        /// <param name="caliber">Калибр.</param>
        /// <param name="count">Сколько патронов нужно.</param>
        /// <returns>Сколько удалось взять.</returns>
        int TakeAmmo(GridContainer container, string caliber, int count);
    }
}
