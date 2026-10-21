#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Di;

namespace AnomalyPath.Core.Validation
{
    /// <summary>Итог preflight-проверки: отчёт и собранные сервисы.</summary>
    public sealed class PreflightResult
    {
        /// <summary>Создаёт результат проверки.</summary>
        /// <param name="report">Отчёт проверки.</param>
        /// <param name="services">Контейнер сервисов или <c>null</c>, если данные невалидны.</param>
        public PreflightResult(ValidationReport report, ServiceContainer? services)
        {
            Report = report;
            Services = services;
        }

        /// <summary>Отчёт проверки.</summary>
        public ValidationReport Report { get; }

        /// <summary>Готовый контейнер сервисов; <c>null</c> при ошибках.</summary>
        public ServiceContainer? Services { get; }

        /// <summary><c>true</c>, если игра может стартовать.</summary>
        public bool CanStart => Report.IsValid && Services != null;
    }
}
