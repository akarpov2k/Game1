#nullable enable
using System.Collections.Generic;

namespace AnomalyPath.Tests
{
    /// <summary>Итог прогона системных тестов: счётчики и тексты упавших проверок.</summary>
    public sealed class TestRunResult
    {
        /// <summary>Создаёт результат прогона.</summary>
        /// <param name="passed">Сколько проверок прошло.</param>
        /// <param name="failed">Сколько проверок упало.</param>
        /// <param name="failures">Описания упавших проверок.</param>
        public TestRunResult(int passed, int failed, IReadOnlyList<string> failures)
        {
            Passed = passed;
            Failed = failed;
            Failures = failures;
        }

        /// <summary>Сколько проверок прошло.</summary>
        public int Passed { get; }

        /// <summary>Сколько проверок упало.</summary>
        public int Failed { get; }

        /// <summary>Описания упавших проверок.</summary>
        public IReadOnlyList<string> Failures { get; }

        /// <summary><c>true</c>, если все проверки прошли.</summary>
        public bool Success => Failed == 0;
    }
}