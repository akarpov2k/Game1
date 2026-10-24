#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Quests;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Доступ к шаблонам квестов: единственный путь к данным квестов из игрового кода.</summary>
    public interface IQuestRepository
    {
        /// <summary>Все квесты, загруженные при старте.</summary>
        IReadOnlyList<QuestTemplate> All { get; }

        /// <summary>Ищет квест по id.</summary>
        /// <param name="id">Идентификатор квеста.</param>
        /// <param name="template">Найденный шаблон или <c>null</c>.</param>
        /// <returns><c>true</c>, если квест найден.</returns>
        bool TryGet(string id, out QuestTemplate template);

        /// <summary>Проверяет, существует ли квест.</summary>
        /// <param name="id">Идентификатор квеста.</param>
        /// <returns><c>true</c>, если квест существует.</returns>
        bool Contains(string id);

        /// <summary>Возвращает квест по id.</summary>
        /// <param name="id">Идентификатор квеста.</param>
        /// <returns>Шаблон квеста.</returns>
        /// <exception cref="Data.DataException">Квест не найден.</exception>
        QuestTemplate Get(string id);
    }
}
