#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Data;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Репозиторий формул: только чтение, строится при загрузке данных.</summary>
    public interface IFormulaRepository
    {
        /// <summary>Все формулы.</summary>
        IReadOnlyList<FormulaTemplate> All { get; }

        /// <summary>Возвращает формулу по id.</summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Формула.</returns>
        /// <exception cref="DataException">Формула не найдена.</exception>
        FormulaTemplate Get(string id);

        /// <summary>Проверяет, существует ли формула.</summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns><c>true</c>, если формула существует.</returns>
        bool Contains(string id);
    }
}
