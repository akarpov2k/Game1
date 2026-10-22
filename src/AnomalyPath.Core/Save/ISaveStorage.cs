#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Data;

namespace AnomalyPath.Core.Save
{
    /// <summary>Хранилище слотов сохранения: чтение, запись и список.</summary>
    public interface ISaveStorage
    {
        /// <summary>Проверяет существование слота.</summary>
        /// <param name="slot">Имя слота.</param>
        /// <returns><c>true</c>, если сохранение есть.</returns>
        bool Exists(string slot);

        /// <summary>Читает текст сохранения.</summary>
        /// <param name="slot">Имя слота.</param>
        /// <returns>JSON-текст.</returns>
        /// <exception cref="DataException">Слот не найден.</exception>
        string Read(string slot);

        /// <summary>Записывает сохранение атомарно.</summary>
        /// <param name="slot">Имя слота.</param>
        /// <param name="text">JSON-текст.</param>
        /// <exception cref="DataException">Запись не удалась.</exception>
        void Write(string slot, string text);

        /// <summary>Удаляет слот и его бэкап.</summary>
        /// <param name="slot">Имя слота.</param>
        void Delete(string slot);

        /// <summary>Список существующих слотов.</summary>
        /// <returns>Имена слотов без расширения.</returns>
        IReadOnlyList<string> List();
    }
}
