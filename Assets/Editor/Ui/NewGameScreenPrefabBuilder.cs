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
    public static class NewGameScreenPrefabBuilder
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

        private static Transform BuildUpperRow(Transform content)
        {
            var go = new GameObject("UpperRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            go.transform.SetParent(content, false);
            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = MenuMetrics.ColumnGap;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = true;
            var element = go.GetComponent<LayoutElement>();
            element.flexibleHeight = 1;
            element.minHeight = MenuMetrics.UpperMinHeight;
            return go.transform;
        }

        private static Transform BuildLeftColumn(Transform upper)
        {
            var go = new GameObject("LeftColumn", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            go.transform.SetParent(upper, false);
            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.spacing = MenuMetrics.RowSpacing;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            var element = go.GetComponent<LayoutElement>();
            element.flexibleWidth = 1.35f;
            element.minWidth = MenuMetrics.SettingsMinWidth;
            return go.transform;
        }

        private static void BuildNicknameRow(
            Transform content,
            UiFactory ui,
            IGameTexts texts,
            GameRulesTemplate rules,
            NewGameScreen screen)
        {
            var row = new GameObject("NicknameRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(content, false);
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = true;
            RowHeight(row, MenuMetrics.NicknameHeight);

            var input = ui.CreateInputField(
                row.transform,
                UiText.Get(texts, UiTextKey.NewGame_Name_Placeholder),
                string.Empty,
                rules.NicknameMaxLength,
                "NicknameInput");
            StyleField(input);
            var inputElement = input.GetComponent<LayoutElement>();
            inputElement.preferredWidth = MenuMetrics.NicknameWidth;
            inputElement.flexibleWidth = 0;
            inputElement.preferredHeight = MenuMetrics.NicknameHeight;
            SetObject(screen, "nickname", input);

            var diceGo = new GameObject(
                "DiceButton",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement),
                typeof(DiceButton));
            diceGo.transform.SetParent(row.transform, false);
            var face = diceGo.GetComponent<Image>();
            face.color = MenuPalette.DiceFace;
            var button = diceGo.GetComponent<Button>();
            button.targetGraphic = face;
            var diceElement = diceGo.GetComponent<LayoutElement>();
            diceElement.preferredWidth = MenuMetrics.DiceSize;
            diceElement.preferredHeight = MenuMetrics.DiceSize;
            diceElement.flexibleWidth = 0;
            diceElement.flexibleHeight = 0;

            var dice = diceGo.GetComponent<DiceButton>();
            var pips = new List<UnityEngine.Object>();
            for (var rowIndex = 0; rowIndex < 3; rowIndex++)
            {
                for (var column = 0; column < 3; column++)
                {
                    var pip = ui.CreateImage($"Pip_{rowIndex}{column}", diceGo.transform, MenuPalette.DicePip);
                    pip.raycastTarget = false;
                    UiWidgets.Place(
                        pip.rectTransform,
                        MenuMetrics.DicePipInset + (column * MenuMetrics.DicePipStep),
                        MenuMetrics.DicePipInset + (rowIndex * MenuMetrics.DicePipStep),
                        MenuMetrics.DicePipSize,
                        MenuMetrics.DicePipSize);
                    pips.Add(pip);
                }
            }

            SetArray(dice, "pips", pips.ToArray());
            dice.Show(5);
            SetObject(screen, "diceButton", button);
            SetObject(screen, "dice", dice);
        }

        private static void BuildLoadoutBlock(Transform content, UiFactory ui, IGameTexts texts, NewGameScreen screen)
        {
            var block = new GameObject("LoadoutBlock", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            block.transform.SetParent(content, false);
            block.GetComponent<Image>().color = MenuPalette.Panel;
            var layout = block.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(
                (int)MenuMetrics.LoadoutPadding,
                (int)MenuMetrics.LoadoutPadding,
                (int)MenuMetrics.LoadoutPadding,
                (int)MenuMetrics.LoadoutPadding);
            layout.spacing = MenuMetrics.LoadoutColumnsGap;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            var blockElement = block.GetComponent<LayoutElement>();
            blockElement.minHeight = MenuMetrics.LoadoutMinHeight;
            blockElement.flexibleHeight = 0;

            var left = BuildLoadoutColumn(block.transform, "LoadoutColumnLeft");
            var right = BuildLoadoutColumn(block.transform, "LoadoutColumnRight");

            var template = CreateLoadoutButton(
                block.transform,
                ui,
                UiText.Get(texts, UiTextKey.NewGame_Selected_Marker),
                string.Empty);
            template.gameObject.name = "LoadoutButtonTemplate";
            template.gameObject.SetActive(false);

            SetObject(screen, "loadoutLeft", left);
            SetObject(screen, "loadoutRight", right);
            SetObject(screen, "loadoutTemplate", template);
        }

        private static RectTransform BuildLoadoutColumn(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
            go.transform.SetParent(parent, false);
            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.spacing = MenuMetrics.LoadoutGap;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            return (RectTransform)go.transform;
        }

        private static Button CreateLoadoutButton(Transform parent, UiFactory ui, string markerText, string labelText)
        {
            var go = new GameObject("LoadoutButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = UiFactory.ButtonColor;
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

            var element = go.GetComponent<LayoutElement>();
            element.preferredHeight = MenuMetrics.LoadoutButtonHeight;
            element.flexibleHeight = 0;

            var label = ui.CreateText(go.transform, labelText, 20, TextAnchor.MiddleLeft);
            label.gameObject.name = "Label";
            var labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(30f, 4f);
            labelRect.offsetMax = new Vector2(-10f, -4f);

            var marker = ui.CreateText(go.transform, markerText, 18, TextAnchor.MiddleLeft);
            marker.gameObject.name = "SelectedMarker";
            var markerRect = marker.rectTransform;
            markerRect.anchorMin = new Vector2(0f, 0f);
            markerRect.anchorMax = new Vector2(0f, 1f);
            markerRect.pivot = new Vector2(0f, 0.5f);
            markerRect.anchoredPosition = new Vector2(8f, 0f);
            markerRect.sizeDelta = new Vector2(18f, 0f);
            marker.gameObject.SetActive(false);
            return button;
        }

        private static void BuildSettingsPanel(
            Transform parent,
            UiFactory ui,
            IGameTexts texts,
            GameRulesTemplate rules,
            NewGameScreen screen)
        {
            var panel = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = MenuPalette.Panel;
            var layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(
                (int)MenuMetrics.SettingsPadding,
                (int)MenuMetrics.SettingsPadding,
                (int)MenuMetrics.SettingsPadding,
                (int)MenuMetrics.SettingsPadding);
            layout.spacing = MenuMetrics.SettingsGap;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = true;
            var element = panel.GetComponent<LayoutElement>();
            element.flexibleHeight = 1;
            element.minHeight = 0;

            var grid = new GameObject("SettingsGrid", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            grid.transform.SetParent(panel.transform, false);
            var gridLayout = grid.GetComponent<HorizontalLayoutGroup>();
            gridLayout.spacing = MenuMetrics.SettingsGap;
            gridLayout.childAlignment = TextAnchor.UpperLeft;
            gridLayout.childControlWidth = true;
            gridLayout.childForceExpandWidth = true;
            gridLayout.childControlHeight = true;
            gridLayout.childForceExpandHeight = false;
            var gridElement = grid.GetComponent<LayoutElement>();
            gridElement.flexibleHeight = 1;

            var leftColumn = BuildSettingsColumn(grid.transform, "SettingsColumnLeft");
            var rightColumn = BuildSettingsColumn(grid.transform, "SettingsColumnRight");

            var template = BuildSettingRowTemplate(panel.transform, ui, texts, rules);
            SetObject(screen, "settingsLeft", leftColumn);
            SetObject(screen, "settingsRight", rightColumn);
            SetObject(screen, "settingRowTemplate", template);
        }

        private static RectTransform BuildSettingsColumn(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.spacing = MenuMetrics.SettingsGap;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            var element = go.GetComponent<LayoutElement>();
            element.flexibleWidth = 1;
            return (RectTransform)go.transform;
        }

        private static SettingRow BuildSettingRowTemplate(
            Transform parent,
            UiFactory ui,
            IGameTexts texts,
            GameRulesTemplate rules)
        {
            var go = new GameObject("SettingRowTemplate", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement), typeof(SettingRow));
            go.transform.SetParent(parent, false);
            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 4;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            var element = go.GetComponent<LayoutElement>();
            element.preferredHeight = MenuMetrics.SettingRowHeight;
            element.flexibleHeight = 0;

            var label = ui.CreateText(go.transform, UiText.Get(texts, UiTextKey.NewGame_Insurance_Label), 20, TextAnchor.MiddleLeft);
            label.gameObject.name = "SettingLabel";
            var labelElement = label.GetComponent<LayoutElement>();
            labelElement.preferredHeight = MenuMetrics.SettingLabelHeight;

            var field = ui.CreateNumberField(go.transform, "SettingField", 0, 1, rules.InsuranceChance, rules.InsuranceStep);
            var fieldRow = field.GetComponent<HorizontalLayoutGroup>();
            if (fieldRow != null)
            {
                fieldRow.childAlignment = TextAnchor.MiddleLeft;
            }

            var fieldElement = field.GetComponent<LayoutElement>();
            if (fieldElement != null)
            {
                fieldElement.preferredHeight = MenuMetrics.NumberFieldHeight;
                fieldElement.flexibleHeight = 0;
            }

            var input = field.transform.Find("SettingFieldInput");
            if (input != null)
            {
                var inputElement = input.GetComponent<LayoutElement>();
                if (inputElement != null)
                {
                    inputElement.preferredWidth = MenuMetrics.NumberFieldWidth;
                    inputElement.flexibleWidth = 0;
                    inputElement.preferredHeight = MenuMetrics.NumberFieldHeight;
                }

                var inputField = input.GetComponent<TextInputField>();
                if (inputField != null)
                {
                    StyleField(inputField);
                }
            }

            foreach (var name in new[] { "SettingFieldIncrease", "SettingFieldDecrease" })
            {
                var step = field.transform.Find(name);
                if (step == null)
                {
                    continue;
                }

                var stepElement = step.GetComponent<LayoutElement>();
                if (stepElement != null)
                {
                    stepElement.preferredWidth = MenuMetrics.StepButtonSize;
                    stepElement.preferredHeight = MenuMetrics.StepButtonSize;
                }
            }

            var row = go.GetComponent<SettingRow>();
            SetObject(row, "label", label);
            SetObject(row, "field", field);
            go.SetActive(false);
            return row;
        }

        private static void BuildPreviewPanel(Transform parent, UiFactory ui, IGameTexts texts, NewGameScreen screen)
        {
            var panel = new GameObject(
                "NewGameInventoryFrame",
                typeof(RectTransform),
                typeof(Image),
                typeof(LayoutElement),
                typeof(NewGameInventoryFrame));
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = MenuPalette.Frame;
            var element = panel.GetComponent<LayoutElement>();
            element.flexibleWidth = 1;
            element.flexibleHeight = 1;
            element.minWidth = MenuMetrics.PreviewMinWidth;
            element.minHeight = 0;

            var scrollGo = new GameObject("PreviewScroll", typeof(RectTransform), typeof(ScrollArea));
            scrollGo.transform.SetParent(panel.transform, false);
            var scrollRect = (RectTransform)scrollGo.transform;
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = new Vector2(1f, MenuMetrics.PreviewPanelTop);
            scrollRect.offsetMin = new Vector2(MenuMetrics.PreviewPadding, MenuMetrics.PreviewPadding);
            scrollRect.offsetMax = new Vector2(-MenuMetrics.PreviewPadding, -MenuMetrics.PreviewPadding);
            var scroll = scrollGo.GetComponent<ScrollArea>();
            scroll.Configure(MenuMetrics.PreviewScrollbarWidth);

            var money = ui.CreateText(panel.transform, string.Empty, 24, TextAnchor.MiddleCenter, UiFactory.TextColor);
            money.gameObject.name = "PreviewMoney";
            money.raycastTarget = false;
            var moneyRect = money.rectTransform;
            var plate = InventoryPanelArt.CurrencyPlate;
            moneyRect.anchorMin = new Vector2(plate.xMin, plate.yMin);
            moneyRect.anchorMax = new Vector2(plate.xMax, plate.yMax);
            moneyRect.pivot = new Vector2(0.5f, 0.5f);
            moneyRect.offsetMin = Vector2.zero;
            moneyRect.offsetMax = Vector2.zero;
            money.gameObject.SetActive(false);

            var empty = ui.CreateText(
                panel.transform,
                UiText.Get(texts, UiTextKey.NewGame_Preview_Empty),
                18,
                TextAnchor.MiddleCenter,
                UiFactory.ToastColor);
            empty.gameObject.name = "PreviewEmpty";
            empty.raycastTarget = false;
            var emptyRect = empty.rectTransform;
            emptyRect.anchorMin = Vector2.zero;
            emptyRect.anchorMax = Vector2.one;
            emptyRect.offsetMin = new Vector2(MenuMetrics.PreviewPadding, MenuMetrics.PreviewPadding);
            emptyRect.offsetMax = new Vector2(-MenuMetrics.PreviewPadding, -MenuMetrics.PreviewPadding);
            AddLocalized(empty, UiTextKey.NewGame_Preview_Empty);
            empty.gameObject.SetActive(false);

            var frame = panel.GetComponent<NewGameInventoryFrame>();
            SetObject(frame, "scroll", scroll);
            SetObject(frame, "gridRoot", scroll.Content);
            SetObject(frame, "moneyLabel", money);
            SetObject(frame, "emptyLabel", empty);
            SetObject(frame, "panelTexture", AssetDatabase.LoadAssetAtPath<Texture2D>(InventoryPanelArt.TexturePath));
            SetFloat(frame, "gridGap", MenuMetrics.PreviewGridGap);
            SetFloat(frame, "minCell", MenuMetrics.PreviewMinCell);
            SetFloat(frame, "padding", MenuMetrics.PreviewPadding);
            SetFloat(frame, "gridLine", MenuMetrics.PreviewGridLine);
            SetObject(screen, "preview", frame);
        }

        private static void BuildStatus(Transform content, UiFactory ui, NewGameScreen screen)
        {
            var status = ui.CreateText(content, string.Empty, 16, TextAnchor.MiddleLeft, UiFactory.ToastColor);
            status.gameObject.name = "Status";
            status.raycastTarget = false;
            var element = status.GetComponent<LayoutElement>();
            element.preferredHeight = MenuMetrics.StatusHeight;
            element.flexibleHeight = 0;
            status.gameObject.SetActive(false);
            SetObject(screen, "status", status);
        }

        private static void BuildButtons(Transform content, UiFactory ui, IGameTexts texts, NewGameScreen screen)
        {
            var row = new GameObject("ButtonsRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(content, false);
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = MenuMetrics.ColumnGap;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = true;
            RowHeight(row, MenuMetrics.ButtonsHeight);

            var start = ui.CreateButton(row.transform, UiText.Get(texts, UiTextKey.NewGame_Start));
            start.gameObject.name = "Начать";
            var startElement = start.GetComponent<LayoutElement>();
            startElement.preferredHeight = MenuMetrics.ButtonsHeight;
            startElement.flexibleWidth = 1.35f;
            AddLocalized(GetButtonLabel(start), UiTextKey.NewGame_Start);
            SetObject(screen, "start", start);

            var back = ui.CreateButton(row.transform, UiText.Get(texts, UiTextKey.NewGame_Back));
            back.gameObject.name = "Назад";
            var backElement = back.GetComponent<LayoutElement>();
            backElement.preferredHeight = MenuMetrics.ButtonsHeight;
            backElement.flexibleWidth = 1;
            AddLocalized(GetButtonLabel(back), UiTextKey.NewGame_Back);
            SetObject(screen, "back", back);
        }

        private static void StyleField(TextInputField input)
        {
            var image = input.GetComponent<Image>();
            if (image != null)
            {
                image.color = MenuPalette.Field;
            }

            if (input.transform.childCount > 0)
            {
                var text = input.transform.GetChild(0).GetComponent<Text>();
                if (text != null)
                {
                    text.color = MenuPalette.FieldText;
                }
            }

            if (input.transform.childCount > 1)
            {
                var hint = input.transform.GetChild(1).GetComponent<Text>();
                if (hint != null)
                {
                    hint.color = MenuPalette.FieldHint;
                }
            }
        }

        private static Text GetButtonLabel(Button button)        {
            var label = button.GetComponentInChildren<Text>(true);
            if (label == null)
            {
                throw new InvalidOperationException("У кнопки нет подписи");
            }

            return label;
        }

        private static void AddLocalized(Text label, UiTextKey key)
        {
            var localized = label.gameObject.AddComponent<LocalizedText>();
            SetObject(localized, "label", label);
            var serialized = new SerializedObject(localized);
            serialized.FindProperty("key").intValue = (int)key;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void RowHeight(GameObject row, float height)
        {
            var element = row.GetComponent<LayoutElement>();
            element.preferredHeight = height;
            element.minHeight = height;
            element.flexibleHeight = 0;
        }

        private static void AssignToScene(GameObject prefab)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var screen = UnityEngine.Object.FindAnyObjectByType<GameScreen>();
            if (screen == null)
            {
                throw new InvalidOperationException($"В сцене {ScenePath} нет GameScreen");
            }

            var serialized = new SerializedObject(screen);
            var property = serialized.FindProperty("newGameScreenPrefab");
            if (property == null)
            {
                throw new InvalidOperationException("У GameScreen нет поля newGameScreenPrefab");
            }

            property.objectReferenceValue = prefab;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static void SetObject(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(field);
            if (property == null)
            {
                throw new InvalidOperationException($"Нет поля {field} у {target.GetType().Name}");
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(UnityEngine.Object target, string field, float value)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(field);
            if (property == null)
            {
                throw new InvalidOperationException($"Нет поля {field} у {target.GetType().Name}");
            }

            property.floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray(UnityEngine.Object target, string field, UnityEngine.Object[] values)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(field);
            if (property == null)
            {
                throw new InvalidOperationException($"Нет поля {field} у {target.GetType().Name}");
            }

            property.arraySize = values.Length;
            for (var index = 0; index < values.Length; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parent = System.IO.Path.GetDirectoryName(path)!.Replace('\\', '/');
                var name = System.IO.Path.GetFileName(path)!;
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }
}
