#nullable enable
using System.IO;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Di;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.Services;
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
        public PreflightResult Preflight { get; private set; } = null!;

        /// <summary>Контейнер сервисов; <c>null</c>, если данные невалидны.</summary>
        public ServiceContainer? Services => Preflight?.Services;

        private void Awake()
        {
            var dataRoot = Path.Combine(Application.dataPath, "Data");
#if !UNITY_EDITOR
            dataRoot = Path.Combine(Application.streamingAssetsPath, "Data");
#endif
            Preflight = Core.Preflight.Run(dataRoot);

            if (!Preflight.CanStart)
            {
                Debug.LogError(CoreText.Format(CoreErrors.DataCheckFailed, Preflight.Report.Summary));
                foreach (var issue in Preflight.Report.Errors)
                {
                    Debug.LogError(CoreText.Format(CoreErrors.ReportLine, issue.Path, issue.Message));
                }

                enabled = false;
                return;
            }

            if (logReport)
            {
                Debug.Log($"AnomalyPath: {Preflight.Report.Summary}");
            }

            var saveRoot = Path.Combine(Application.persistentDataPath, "Saves");
            var items = Services!.Resolve<IItemRepository>();
            var saves = new SaveService(new FileSaveStorage(saveRoot), items);
            Services.RegisterSingleton<ISaveService>(saves);
            Services.RegisterSingleton<IGameSessionService>(new GameSessionService(
                Services.Resolve<ITravelService>(),
                Services.Resolve<ICombatService>(),
                saves,
                Services.Resolve<INewGameService>()));
        }
    }
}
