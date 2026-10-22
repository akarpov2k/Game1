#nullable enable
using System;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Rules;

namespace AnomalyPath.Core.Repositories
{
    /// <summary>Правила прохождения в памяти; допускают отсутствие файла до старта новой игры.</summary>
    public sealed class GameRulesRepository : IGameRulesRepository
    {
        private readonly GameRulesTemplate? _rules;

        /// <summary>Создаёт репозиторий из готового шаблона.</summary>
        /// <param name="rules">Правила или <c>null</c>, если файла нет.</param>
        public GameRulesRepository(GameRulesTemplate? rules)
        {
            _rules = rules;
        }

        /// <inheritdoc />
        public GameRulesTemplate? Rules => _rules;

        /// <inheritdoc />
        public GameRulesTemplate Require() =>
            _rules ?? throw new DataException("rules", CoreErrors.RulesMissing, "rules");

        /// <summary>Строит репозиторий из загруженных сущностей вида <c>rules</c>.</summary>
        /// <param name="data">Результат загрузки.</param>
        /// <returns>Репозиторий с правилами или без них.</returns>
        /// <exception cref="DataException">Правил больше одной записи.</exception>
        public static GameRulesRepository FromLoadedData(LoadedData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var records = data.All("rules");
            if (records.Count == 0)
            {
                return new GameRulesRepository(null);
            }

            if (records.Count > 1)
            {
                throw new DataException("rules", CoreErrors.RulesSingleRecord, records.Count);
            }

            return new GameRulesRepository(GameRulesTemplate.FromJson(records[0], "rules"));
        }
    }
}
