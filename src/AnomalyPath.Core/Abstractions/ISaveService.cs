#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Сохранение и загрузка состояния прохождения.</summary>
    public interface ISaveService
    {
        /// <summary>Сохраняет состояние мира и игрока в слот.</summary>
        /// <param name="state">Состояние прохождения.</param>
        /// <param name="slot">Имя слота.</param>
        /// <param name="playtime">Наигранное время в секундах.</param>
        /// <param name="createdAt">Дата создания; по умолчанию текущая.</param>
        /// <param name="title">Название сохранения, заданное игроком.</param>
        void Save(
            GameState state,
            string slot,
            double playtime = 0,
            DateTimeOffset? createdAt = null,
            string? title = null);

        /// <summary>Загружает состояние из слота.</summary>
        /// <param name="slot">Имя слота.</param>
        /// <returns>Восстановленное состояние; игрок может быть <c>null</c> в старом сейве.</returns>
        GameState Load(string slot);

        /// <summary>Проверяет существование слота.</summary>
        /// <param name="slot">Имя слота.</param>
        /// <returns><c>true</c>, если сохранение есть.</returns>
        bool Exists(string slot);

        /// <summary>Список слотов для меню.</summary>
        /// <returns>Описания слотов, включая повреждённые.</returns>
        IReadOnlyList<SaveSlotInfo> ListSlots();

        /// <summary>Удаляет слот.</summary>
        /// <param name="slot">Имя слота.</param>
        void Delete(string slot);
    }
}
