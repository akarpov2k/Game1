#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Validation
{
    /// <summary>Итог проверки данных перед инициализацией игры.</summary>
    public sealed class ValidationReport
    {
        private readonly List<ValidationIssue> _errors = new List<ValidationIssue>();
        private readonly List<ValidationIssue> _warnings = new List<ValidationIssue>();

        /// <summary>Создаёт отчёт из находок.</summary>
        /// <param name="issues">Находки валидатора.</param>
        public ValidationReport(IEnumerable<ValidationIssue> issues)
        {
            foreach (var issue in issues)
            {
                if (issue.IsWarning)
                {
                    _warnings.Add(issue);
                }
                else
                {
                    _errors.Add(issue);
                }
            }
        }

        /// <summary>Ошибки, из-за которых игра не должна стартовать.</summary>
        public IReadOnlyList<ValidationIssue> Errors => _errors;

        /// <summary>Предупреждения, не блокирующие старт.</summary>
        public IReadOnlyList<ValidationIssue> Warnings => _warnings;

        /// <summary><c>true</c>, если ошибок нет.</summary>
        public bool IsValid => _errors.Count == 0;

        /// <summary>Сводка для лога и экрана ошибки.</summary>
        public string Summary =>
            IsValid
                ? CoreText.Get(CoreErrors.ValidationOk)
                : CoreText.Format(CoreErrors.ValidationFailed, _errors.Count, _warnings.Count);
    }
}
