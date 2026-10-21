namespace AnomalyPath.Core.Validation
{
    /// <summary>Одна находка валидатора: ошибка или предупреждение.</summary>
    public sealed class ValidationIssue
    {
        /// <summary>Создаёт находку.</summary>
        /// <param name="path">Файл или id, к которому относится находка.</param>
        /// <param name="code">Код текста из <see cref="Localization.CoreErrors"/>.</param>
        /// <param name="message">Готовое локализованное сообщение.</param>
        /// <param name="isWarning">Предупреждение, а не ошибка.</param>
        public ValidationIssue(string path, string code, string message, bool isWarning = false)
        {
            Path = path;
            Code = code;
            Message = message;
            IsWarning = isWarning;
        }

        /// <summary>Файл или id, к которому относится находка.</summary>
        public string Path { get; }

        /// <summary>Код текста.</summary>
        public string Code { get; }

        /// <summary>Готовое сообщение.</summary>
        public string Message { get; }

        /// <summary>Признак предупреждения.</summary>
        public bool IsWarning { get; }
    }
}
