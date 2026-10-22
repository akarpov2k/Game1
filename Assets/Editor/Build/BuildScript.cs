#nullable enable
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AnomalyPath.Editor
{
    /// <summary>
    /// Сборка игры: Windows x64, сцена <c>Main</c>, данные копируются в <c>StreamingAssets</c> билда.
    /// Вызывается из меню и из командной строки (<c>-executeMethod AnomalyPath.Editor.BuildScript.BuildWindows</c>).
    /// </summary>
    public static class BuildScript
    {
        /// <summary>Сцена, с которой стартует игра.</summary>
        public const string MainScene = "Assets/Scenes/Main.unity";

        /// <summary>Папка сборки относительно корня проекта.</summary>
        public const string OutputDirectory = "Build/Windows";

        /// <summary>Имя исполняемого файла.</summary>
        public const string ExecutableName = "AnomalyPath.exe";

        /// <summary>Папка данных проекта.</summary>
        public const string DataRoot = "Assets/Data";

        /// <summary>
        /// Собирает Windows-билд и проверяет, что данные попали в сборку.
        /// В batch-mode при ошибке завершает Unity с ненулевым кодом.
        /// </summary>
        public static void BuildWindows()
        {
            var executable = Path.GetFullPath(Path.Combine(OutputDirectory, ExecutableName));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { MainScene },
                locationPathName = executable,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });

            var summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                Fail($"сборка не удалась ({summary.result}), ошибок: {summary.totalErrors}");
                return;
            }

            var dataRoot = GameDataCopier.CopyToBuild(executable, DataRoot);
            if (!GameDataCopier.HasCatalog(dataRoot))
            {
                Fail($"данные не попали в сборку: {dataRoot}");
                return;
            }

            Debug.Log($"AnomalyPath: сборка готова — {executable}; данные: {dataRoot}");
        }

        /// <summary>Собирает Windows-билд из меню редактора.</summary>
        [MenuItem("Tools/AnomalyPath/Собрать Windows")]
        public static void BuildWindowsFromMenu()
        {
            BuildWindows();
            var executable = Path.Combine(OutputDirectory, ExecutableName);
            if (File.Exists(executable))
            {
                EditorUtility.RevealInFinder(executable);
            }
        }

        private static void Fail(string reason)
        {
            Debug.LogError("AnomalyPath: " + reason);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }
        }
    }
}
