using System.IO;
using AnomalyPath.Core;
using AnomalyPath.Core.Di;
using AnomalyPath.Core.Validation;
using UnityEngine;

namespace AnomalyPath.Runtime
{
    /// <summary>
    /// Точка входа в игру: до инициализации проверяет данные и отдаёт готовые сервисы.
    /// Вешается на объект в загрузочной сцене; при ошибках игра не стартует.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Tooltip("Писать сводку проверки данных в консоль")]
        [SerializeField]
        private bool logReport = true;

        /// <summary>Результат проверки данных: отчёт и сервисы.</summary>
        public PreflightResult Preflight { get; private set; }

        /// <summary>Контейнер сервисов; <c>null</c>, если данные невалидны.</summary>
        public ServiceContainer Services => Preflight?.Services;

        private void Awake()
        {
            var dataRoot = Path.Combine(Application.dataPath, "Data");
            Preflight = Core.Preflight.Run(dataRoot);

            if (!Preflight.CanStart)
            {
                Debug.LogError($"AnomalyPath: данные не прошли проверку. {Preflight.Report.Summary}");
                foreach (var issue in Preflight.Report.Errors)
                {
                    Debug.LogError($"{issue.Path}: {issue.Message}");
                }

                enabled = false;
                return;
            }

            if (logReport)
            {
                Debug.Log($"AnomalyPath: {Preflight.Report.Summary}");
            }
        }
    }
}
