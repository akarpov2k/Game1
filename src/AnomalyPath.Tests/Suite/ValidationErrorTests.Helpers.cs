#nullable enable
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Core;
using AnomalyPath.Core.Validation;

namespace AnomalyPath.Tests
{
    /// <summary>Часть класса <see cref="ValidationErrorTests"/>: служебные помощники.</summary>
    public sealed partial class ValidationErrorTests
    {
        private static bool HasFragment(ValidationReport report, string fragment)
        {
            foreach (var issue in report.Errors)
            {
                if (issue.Message.IndexOf(fragment, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
