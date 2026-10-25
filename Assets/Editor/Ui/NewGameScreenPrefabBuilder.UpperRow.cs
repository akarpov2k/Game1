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
    /// <summary>Часть класса <see cref="NewGameScreenPrefabBuilder"/>: верхняя строка.</summary>
    public static partial class NewGameScreenPrefabBuilder
    {
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
    }
}
