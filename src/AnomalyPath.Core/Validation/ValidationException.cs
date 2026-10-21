#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace AnomalyPath.Core.Validation
{
    /// <summary>Данные не прошли preflight-проверку: несёт полный отчёт для показа игроку.</summary>
    public sealed class ValidationException : Exception
    {
        private const int MaxMessages = 3;

        /// <summary>Создаёт исключение по отчёту проверки.</summary>
        /// <param name="report">Отчёт с ошибками.</param>
        public ValidationException(ValidationReport report)
            : base(BuildMessage(report))
        {
            Report = report;
        }

        /// <summary>Полный отчёт проверки.</summary>
        public ValidationReport Report { get; }

        private static string BuildMessage(ValidationReport report)
        {
            var builder = new StringBuilder(report.Summary);
            var shown = 0;
            foreach (var issue in report.Errors)
            {
                if (shown == MaxMessages)
                {
                    break;
                }

                builder.Append(" | ").Append(issue.Path).Append(": ").Append(issue.Message);
                shown++;
            }

            return builder.ToString();
        }
    }
}
