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
    /// <summary>Часть класса <see cref="NewGameScreenPrefabBuilder"/>: панель настроек.</summary>
    public static partial class NewGameScreenPrefabBuilder
    {
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
    }
}
