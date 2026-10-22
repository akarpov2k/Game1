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
    }
}
