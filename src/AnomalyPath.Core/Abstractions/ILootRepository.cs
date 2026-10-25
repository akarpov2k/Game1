#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Loot;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Таблицы лута и настройка полос тира; только чтение.</summary>
    public interface ILootRepository
    {
        /// <summary>Настройка полос тира.</summary>
        LootSettings Settings { get; }

        /// <summary>Все таблицы лута.</summary>
        IReadOnlyList<LootTable> All { get; }

        /// <summary>Пробует найти таблицу лута по id.</summary>
        /// <param name="id">Идентификатор таблицы.</param>
        /// <param name="table">Найденная таблица.</param>
        /// <returns><c>true</c>, если таблица существует.</returns>
        bool TryGet(string id, out LootTable table);

        /// <summary>Возвращает таблицу лута по id.</summary>
        /// <param name="id">Идентификатор таблицы.</param>
        /// <returns>Таблица лута.</returns>
        /// <exception cref="Data.DataException">Таблица не найдена.</exception>
        LootTable Get(string id);
    }
}
