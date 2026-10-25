#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.World;
using AnomalyPath.Runtime;
using AnomalyPath.Runtime.Screens.Game;
using AnomalyPath.Runtime.Screens.Menu;
using AnomalyPath.Runtime.Ui.Core;
using AnomalyPath.Runtime.Ui.Items;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AnomalyPath.Tests.PlayMode
{
    /// <summary>Часть класса <see cref="MainMenuPlayModeTests"/>: служебные помощники.</summary>
    public sealed partial class MainMenuPlayModeTests
    {
        private static int CountSaveTiles()
        {
            var root = GameObject.Find("SaveSlots");
            if (root == null)
            {
                return 0;
            }

            var count = 0;
            foreach (Transform child in root.transform)
            {
                if (child.gameObject.activeSelf && child.name.StartsWith("SaveSlot_"))
                {
                    count++;
                }
            }

            return count;
        }

        private static GameObject? FindNewSlotTile()
        {
            var root = GameObject.Find("SaveSlots");
            if (root == null)
            {
                return null;
            }

            foreach (Transform child in root.transform)
            {
                if (!child.gameObject.activeSelf || !child.name.StartsWith("SaveSlot_"))
                {
                    continue;
                }

                var label = child.GetComponentInChildren<Text>();
                if (label != null && label.text.Contains("Новое сохранение"))
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        private static Image BackdropOf(string panelName)
        {
            var panel = GameObject.Find(panelName);
            Assert.IsNotNull(panel, $"Панель «{panelName}» не найдена");
            var backdrop = panel.transform.Find("ScreenBackdrop");
            Assert.IsNotNull(backdrop, $"У окна «{panelName}» нет задника");
            var image = backdrop.GetComponent<Image>();
            Assert.IsNotNull(image, $"У задника «{panelName}» нет Image");
            return image;
        }

        private static void RightClick(GameObject target, Vector2 position)
        {
            var data = new PointerEventData(EventSystem.current)
            {
                position = position,
                button = PointerEventData.InputButton.Right
            };
            ExecuteEvents.Execute(target, data, ExecuteEvents.pointerClickHandler);
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
