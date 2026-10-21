#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Templates;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Репозиторий типов урона: только чтение, строится при загрузке данных.</summary>
    public interface IDamageTypeRepository
    {
        /// <summary>Все типы урона.</summary>
        IReadOnlyList<DamageTypeTemplate> All { get; }

        /// <summary>Ищет тип урона по id.</summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="template">Найденный тип.</param>
        /// <returns><c>true</c>, если тип найден.</returns>
        bool TryGet(string id, out DamageTypeTemplate template);

        /// <summary>Возвращает тип урона по id.</summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Тип урона.</returns>
        /// <exception cref="DataException">Тип урона не найден.</exception>
        DamageTypeTemplate Get(string id);

        /// <summary>Проверяет, существует ли тип урона.</summary>
        /// <param name="id">Идентификатор.</param>
        /// <returns><c>true</c>, если тип существует.</returns>
        bool Contains(string id);
    }
}
