#nullable enable
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AnomalyPath.Tests
{
    /// <summary>Unity EditMode-обёртка: запускает тот же набор проверок, что и консоль.</summary>
    public sealed class UnityEditModeTests
    {
        /// <summary>Все системные проверки Core на данных проекта.</summary>
        [Test]
        public void CoreSuite_ProjectData_AllChecksPass()
        {
            var dataRoot = Path.Combine(Application.dataPath, "Data");
            var result = TestSuite.Run(dataRoot, Debug.Log);

            Assert.IsTrue(result.Success, string.Join("\n", result.Failures));
        }
    }
}