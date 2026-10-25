#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AnomalyPath.Runtime;
using AnomalyPath.Runtime.Screens.Menu;
using AnomalyPath.Runtime.Ui.Core;
using AnomalyPath.Runtime.Ui.Items;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AnomalyPath.Tests.PlayMode
{
    /// <summary>Часть класса <see cref="NewGameScreenPlayModeTests"/>: служебные помощники.</summary>
    public sealed partial class NewGameScreenPlayModeTests
    {
        private static void AssertField(GameObject input, string label)
        {
            Assert.IsNotNull(input, "Нет поля ввода: " + label);
            var background = input.GetComponent<Image>();
            Assert.IsNotNull(background, "У поля нет фона: " + label);
            Assert.Greater(background.color.r, 0.8f, "Фон поля должен быть светлым: " + label);

            var text = input.transform.GetChild(0).GetComponent<Text>();
            Assert.IsNotNull(text, "У поля нет текста: " + label);
            Assert.Less(text.color.r, 0.2f, "Текст поля должен быть чёрным: " + label);
            Assert.Less(text.color.g, 0.2f, "Текст поля должен быть чёрным: " + label);
            Assert.Less(text.color.b, 0.2f, "Текст поля должен быть чёрным: " + label);
            Assert.Greater(text.color.a, 0.9f, "Текст поля должен быть непрозрачным: " + label);
        }

        private static IEnumerator OpenNewGame()
        {
            yield return LoadMain();
            Click("Новая игра");
            yield return null;
            Canvas.ForceUpdateCanvases();
            yield return null;
        }

        private static int CountLoadoutButtons()
        {
            var count = 0;
            foreach (var columnName in new[] { "LoadoutColumnLeft", "LoadoutColumnRight" })
            {
                var column = FindInPanel(columnName);
                if (column == null)
                {
                    continue;
                }

                foreach (Transform child in column.transform)
                {
                    if (child.gameObject.activeSelf && child.GetComponent<Button>() != null)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static int CountMarkers()
        {
            var count = 0;
            foreach (var marker in FindMarkers())
            {
                if (marker.gameObject.activeSelf)
                {
                    count++;
                }
            }

            return count;
        }

        private static string? MarkerParent()
        {
            foreach (var marker in FindMarkers())
            {
                if (marker.gameObject.activeSelf)
                {
                    return marker.parent.name;
                }
            }

            return null;
        }

        private static List<Transform> FindMarkers()
        {
            var markers = new List<Transform>();
            var panel = GameObject.Find("NewGame");
            foreach (var child in panel.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "SelectedMarker")
                {
                    markers.Add(child);
                }
            }

            return markers;
        }

        private static void ClickStep(NumberField field, string name)
        {
            var button = field.transform.Find(name);
            Assert.IsNotNull(button, $"Нет кнопки {name} у поля {field.name}");
            button.GetComponent<Button>().onClick.Invoke();
        }

        private static GameObject FindInPanel(string name)
        {
            var panel = GameObject.Find("NewGame");
            Assert.IsNotNull(panel, "Панель старта игры не создана");
            foreach (var child in panel.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                {
                    return child.gameObject;
                }
            }

            return null!;
        }

        private static Transform? FindChild(GameObject root, string prefix)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name.StartsWith(prefix, System.StringComparison.Ordinal))
                {
                    return child;
                }
            }

            return null;
        }

        private static Rect WorldRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            var min = corners[0];
            var max = corners[0];
            for (var i = 1; i < corners.Length; i++)
            {
                min = Vector3.Min(min, corners[i]);
                max = Vector3.Max(max, corners[i]);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static bool IsItemBox(string name) =>
            name.StartsWith("PreviewItem_", System.StringComparison.Ordinal) &&
            !name.StartsWith("PreviewItemIcon_", System.StringComparison.Ordinal);

        private static int CountItemBoxes(GameObject root)
        {
            var count = 0;
            foreach (var child in root.GetComponentsInChildren<Transform>(includeInactive: false))
            {
                if (IsItemBox(child.name))
                {
                    count++;
                }
            }

            return count;
        }

        private static bool HasItem(GameObject root, string itemId)
        {
            var prefix = "PreviewItem_" + itemId + "_";
            foreach (var child in root.GetComponentsInChildren<Transform>(includeInactive: false))
            {
                if (child.name.StartsWith(prefix, System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static int CountChildren(GameObject root, string prefix)
        {
            var count = 0;
            foreach (var child in root.GetComponentsInChildren<Transform>(includeInactive: false))
            {
                if (child.name.StartsWith(prefix, System.StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        private static IEnumerator LoadMain()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;
        }

        private static void Click(string name)
        {
            var go = GameObject.Find(name);
            Assert.IsNotNull(go, $"Кнопка «{name}» не найдена");
            var button = go.GetComponent<Button>();
            Assert.IsNotNull(button, $"У «{name}» нет Button");
            button.onClick.Invoke();
        }
    }
}
