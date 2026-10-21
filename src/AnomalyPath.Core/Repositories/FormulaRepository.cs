#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Repositories
{
    /// <summary>Репозиторий формул в памяти.</summary>
    public sealed class FormulaRepository : IFormulaRepository
    {
        private readonly Dictionary<string, FormulaTemplate> _byId;

        /// <summary>Создаёт репозиторий из готовых формул.</summary>
        /// <param name="templates">Формулы.</param>
        /// <exception cref="DataException">Дублирующийся id.</exception>
        public FormulaRepository(IEnumerable<FormulaTemplate> templates)
        {
            _byId = new Dictionary<string, FormulaTemplate>(StringComparer.Ordinal);
            foreach (var template in templates)
            {
                if (_byId.ContainsKey(template.Id))
                {
                    throw new DataException(template.Id, CoreErrors.DuplicateRepositoryId);
                }

                _byId[template.Id] = template;
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<FormulaTemplate> All => new List<FormulaTemplate>(_byId.Values);

        /// <inheritdoc />
        public FormulaTemplate Get(string id) =>
            _byId.TryGetValue(id, out var template)
                ? template
                : throw new DataException(id, CoreErrors.FormulaNotFound);

        /// <inheritdoc />
        public bool Contains(string id) => _byId.ContainsKey(id);

        /// <summary>Строит репозиторий из загруженных сущностей вида <c>formulas</c>.</summary>
        /// <param name="data">Результат загрузки.</param>
        /// <returns>Заполненный репозиторий.</returns>
        public static FormulaRepository FromLoadedData(LoadedData data)
        {
            var templates = new List<FormulaTemplate>();
            foreach (var entity in data.All("formulas"))
            {
                templates.Add(FormulaTemplate.FromJson(entity, entity.TryGet("id", out var id) ? id.Text : "formulas"));
            }

            return new FormulaRepository(templates);
        }
    }
}
