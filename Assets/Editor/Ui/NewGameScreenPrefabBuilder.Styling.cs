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
    /// <summary>Часть класса <see cref="NewGameScreenPrefabBuilder"/>: стили полей.</summary>
    public static partial class NewGameScreenPrefabBuilder
    {
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
            serialized.FindProperty("key").stringValue = UiText.Code(key);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void RowHeight(GameObject row, float height)
        {
            var element = row.GetComponent<LayoutElement>();
            element.preferredHeight = height;
            element.minHeight = height;
            element.flexibleHeight = 0;
        }
    }
}
