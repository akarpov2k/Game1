#nullable enable
using System;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Talents;

namespace AnomalyPath.Core.Repositories
{
    /// <summary>Репозиторий единственного дерева талантов в памяти.</summary>
    public sealed class TalentRepository : ITalentRepository
    {
        /// <summary>Создаёт репозиторий дерева.</summary>
        /// <param name="tree">Дерево талантов.</param>
        /// <exception cref="ArgumentNullException">Дерево не задано.</exception>
        public TalentRepository(TalentTreeTemplate tree)
        {
            Tree = tree ?? throw new ArgumentNullException(nameof(tree));
        }

        /// <inheritdoc />
        public TalentTreeTemplate Tree { get; }

        /// <summary>Строит репозиторий из загруженных сущностей вида <c>talents</c>.</summary>
        /// <param name="data">Результат загрузки.</param>
        /// <returns>Заполненный репозиторий.</returns>
        /// <exception cref="DataException">Дерево отсутствует, задано дважды или разобрано с ошибкой.</exception>
        public static TalentRepository FromLoadedData(LoadedData data)
        {
            var records = data.All("talents");
            if (records.Count == 0)
            {
                throw new DataException("talents", CoreErrors.FileNotFound);
            }

            if (records.Count > 1)
            {
                throw new DataException("talents", CoreErrors.TalentTreeMultiple, records.Count);
            }

            return new TalentRepository(TalentTreeTemplate.FromJson(records[0], "talents"));
        }
    }
}
