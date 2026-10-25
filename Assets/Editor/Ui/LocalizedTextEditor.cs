#nullable enable
using System;
using AnomalyPath.Runtime.Ui.Core;
using UnityEditor;

namespace AnomalyPath.Editor.Ui
{
    /// <summary>
    /// Инспектор <see cref="LocalizedText"/>: ключ текста хранится кодом каталога,
    /// а выбирается из списка <see cref="UiTextKey"/>, чтобы не держать код в голове.
    /// </summary>
    [CustomEditor(typeof(LocalizedText))]
    public sealed class LocalizedTextEditor : UnityEditor.Editor
    {
        private static readonly UiTextKey[] Keys = (UiTextKey[])Enum.GetValues(typeof(UiTextKey));

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            var serialized = serializedObject;
            serialized.Update();

            EditorGUILayout.PropertyField(serialized.FindProperty("label"));
            EditorGUILayout.PropertyField(serialized.FindProperty("key"));

            var current = serialized.FindProperty("key").stringValue;
            var names = new string[Keys.Length + 1];
            names[0] = "— не задан —";
            var index = 0;
            for (var i = 0; i < Keys.Length; i++)
            {
                names[i + 1] = UiText.Code(Keys[i]);
                if (string.Equals(current, names[i + 1], StringComparison.Ordinal))
                {
                    index = i + 1;
                }
            }

            var picked = EditorGUILayout.Popup("Ключ из каталога", index, names);
            if (picked != index)
            {
                serialized.FindProperty("key").stringValue = picked == 0 ? string.Empty : names[picked];
            }

            serialized.ApplyModifiedProperties();
        }
    }
}
