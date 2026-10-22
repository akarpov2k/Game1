#nullable enable
using System.IO;
using AnomalyPath.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AnomalyPath.Editor
{
    /// <summary>Создаёт стартовую сцену с загрузчиком и экраном меню.</summary>
    public static class SetupMainScene
    {
        /// <summary>Создаёт сцену Main и добавляет её в список сборки.</summary>
        [MenuItem("Tools/AnomalyPath/Setup Main Scene")]
        public static void Setup()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var game = new GameObject("Game");
            game.AddComponent<GameBootstrap>();
            game.AddComponent<GameScreen>();
            CreateCamera();

            if (!Directory.Exists("Assets/Scenes"))
            {
                Directory.CreateDirectory("Assets/Scenes");
            }

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Main.unity");
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("AnomalyPath: сцена Assets/Scenes/Main.unity создана");
        }

        /// <summary>Добавляет в сцену Main камеру, чтобы Game view не показывал «No cameras rendering».</summary>
        [MenuItem("Tools/AnomalyPath/Добавить камеру в Main")]
        public static void EnsureCameraInMainScene()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Main.unity", OpenSceneMode.Single);
            if (Object.FindAnyObjectByType<Camera>() == null)
            {
                CreateCamera();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("AnomalyPath: в Assets/Scenes/Main.unity добавлена камера");
                return;
            }

            Debug.Log("AnomalyPath: в Assets/Scenes/Main.unity уже есть камера");
        }

        private static void CreateCamera()
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, 0f, -10f);
            var camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.04f, 0.05f, 0.06f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.cullingMask = 0;
            camera.depth = -1f;
            camera.useOcclusionCulling = false;
        }
    }
}
