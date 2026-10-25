#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Di;
using AnomalyPath.Core.Rules;
using AnomalyPath.Runtime;
using AnomalyPath.Runtime.Screens.Menu;
using AnomalyPath.Runtime.Ui.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Editor.Ui
{
    /// <summary>
    /// Собирает префаб экрана старта игры по прототипу и привязывает его к сцене Main.
    /// Префаб после сборки — источник истины: дальше разметка правится в инспекторе, а не здесь.
    /// </summary>
    public static partial class NewGameScreenPrefabBuilder
    {
        private const string PrefabPath = "Assets/Prefabs/Screens/NewGameScreen.prefab";
        private const string ScenePath = "Assets/Scenes/Main.unity";
        private const string DataRoot = "Assets/Data";

        /// <summary>Пункт меню: собрать префаб и привязать его к сцене.</summary>
        [MenuItem("Tools/AnomalyPath/UI/Собрать префаб «Старт игры»")]
        public static void BuildFromMenu() => Build();

        /// <summary>Точка входа batch-режима.</summary>
        public static void BuildFromBatch()
        {
            try
            {
                Build();
                EditorApplication.Exit(0);
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                EditorApplication.Exit(1);
            }
        }

        /// <summary>Собирает префаб экрана и назначает его в сцене Main.</summary>
        public static void Build()
        {
            var services = LoadServices();
            var texts = services.Resolve<IGameTexts>();
            var rules = services.Resolve<IGameRulesRepository>().Require();

            EnsureFolder("Assets/Prefabs");
            EnsureFolder("Assets/Prefabs/Screens");

            var root = BuildHierarchy(texts, rules);
            root.SetActive(false);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();

            if (prefab == null)
            {
                throw new InvalidOperationException($"Не удалось сохранить префаб {PrefabPath}");
            }

            AssignToScene(prefab);
            Debug.Log($"AnomalyPath: префаб экрана старта игры собран — {PrefabPath}");
        }

        private static ServiceContainer LoadServices()
        {
            var result = Preflight.Run(DataRoot);
            if (!result.CanStart || result.Services == null)
            {
                throw new InvalidOperationException(result.Report.Summary);
            }

            return result.Services;
        }

        private static GameObject BuildHierarchy(IGameTexts texts, GameRulesTemplate rules)
        {
            var ui = new UiFactory(UiFactory.FindFont());
            var root = new GameObject("NewGame", typeof(RectTransform), typeof(Image));
            Stretch((RectTransform)root.transform);
            root.GetComponent<Image>().color = UiFactory.BackgroundColor;
            var screen = root.AddComponent<NewGameScreen>();

            var backdropImage = ui.CreateImage("ScreenBackdrop", root.transform, Color.white);
            Stretch(backdropImage.rectTransform);
            var backdrop = backdropImage.gameObject.AddComponent<ScreenBackdrop>();
            SetObject(backdrop, "image", backdropImage);
            SetObject(screen, "backdrop", backdrop);

            var content = BuildContent(root.transform);
            var upper = BuildUpperRow(content);
            var left = BuildLeftColumn(upper);
            BuildNicknameRow(left, ui, texts, rules, screen);
            BuildLoadoutBlock(left, ui, texts, screen);
            BuildSettingsPanel(left, ui, texts, rules, screen);
            BuildPreviewPanel(upper, ui, texts, screen);
            BuildStatus(content, ui, screen);
            BuildButtons(content, ui, texts, screen);
            return root;
        }

        private static Transform BuildContent(Transform parent)
        {
            var go = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(MenuMetrics.SidePadding, MenuMetrics.BottomPadding);
            rect.offsetMax = new Vector2(-MenuMetrics.SidePadding, -MenuMetrics.TopPadding);

            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = MenuMetrics.RowSpacing;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            return go.transform;
        }
    }
}
