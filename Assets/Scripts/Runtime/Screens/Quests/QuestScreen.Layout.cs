#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Quests;
using AnomalyPath.Core.Session;
using AnomalyPath.Runtime.Screens;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Quests
{
    /// <summary>Часть класса <see cref="QuestScreen"/>: раскладка.</summary>
    public sealed partial class QuestScreen
    {
        private void ShowToast(string text, bool error = false)
        {
            _toast.text = text;
            _toast.color = error ? QuestPalette.Error : UiFactory.ToastColor;
        }

        private Transform Row(Transform content, string name, int index, float height, Color color)
        {
            var row = _host.Ui.CreateImage(name, content, color);
            row.raycastTarget = false;
            var rect = row.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            var top = index * (height + QuestMetrics.RowGap);
            rect.offsetMin = new Vector2(0f, -(top + height));
            rect.offsetMax = new Vector2(0f, -top);
            return row.transform;
        }

        private ScrollArea CreateScroll(string name, float left, float width)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(ScrollArea));
            go.transform.SetParent(Content, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(left, -QuestMetrics.ListTop);
            rect.sizeDelta = new Vector2(width, QuestMetrics.ListHeight);
            var scroll = go.GetComponent<ScrollArea>();
            scroll.Configure(QuestMetrics.RowGap);
            return scroll;
        }

        private void AddEmpty(RectTransform content, UiTextKey key)
        {
            var label = UiWidgets.Label(
                _host,
                content,
                "Empty",
                UiText.Get(_host.Texts, key),
                20,
                TextAnchor.MiddleCenter,
                UiFactory.MutedTextColor,
                8f,
                8f,
                600f,
                32f);
            label.raycastTarget = false;
        }

        private static float ContentHeight(int count, float rowHeight) =>
            Mathf.Max(QuestMetrics.ListHeight, 16f + (count * (rowHeight + QuestMetrics.RowGap)));

        private static void Clear(RectTransform content)
        {
            for (var index = content.childCount - 1; index >= 0; index--)
            {
                var child = content.GetChild(index).gameObject;
                child.SetActive(false);
                UnityEngine.Object.Destroy(child);
            }
        }

        private static void PlaceTop(RectTransform rect, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(QuestMetrics.Side, -(top + height));
            rect.offsetMax = new Vector2(-QuestMetrics.Side, -top);
        }

        private static void PlaceSection(RectTransform rect, float left, float top)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(left, -top);
            rect.sizeDelta = new Vector2(600f, QuestMetrics.SectionHeight);
        }

        private static void PlaceButton(RectTransform rect, float right, float width)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(right, -48f);
            rect.sizeDelta = new Vector2(width, QuestMetrics.ButtonHeight);
        }

        private static UiTextKey KeyFor(QuestResultStatus status) => status switch
        {
            QuestResultStatus.QuestNotFound => UiTextKey.Quest_Result_Not_Found,
            QuestResultStatus.NotRepeatable => UiTextKey.Quest_Result_Not_Repeatable,
            QuestResultStatus.NotOffered => UiTextKey.Quest_Result_Not_Offered,
            QuestResultStatus.TraderNotFound => UiTextKey.Quest_Result_Trader_Not_Found,
            QuestResultStatus.NotTrader => UiTextKey.Quest_Result_Not_Trader,
            QuestResultStatus.WrongLocation => UiTextKey.Quest_Result_Wrong_Location,
            QuestResultStatus.Unavailable => UiTextKey.Quest_Result_Unavailable,
            QuestResultStatus.RequirementsNotMet => UiTextKey.Quest_Result_Requirements,
            QuestResultStatus.AlreadyActive => UiTextKey.Quest_Result_Already_Active,
            QuestResultStatus.ActiveLimit => UiTextKey.Quest_Result_Active_Limit,
            QuestResultStatus.Cooldown => UiTextKey.Quest_Result_Cooldown,
            QuestResultStatus.NoTargets => UiTextKey.Quest_Result_No_Targets,
            QuestResultStatus.NotReady => UiTextKey.Quest_Result_Not_Ready,
            QuestResultStatus.WrongGiver => UiTextKey.Quest_Result_Wrong_Giver,
            QuestResultStatus.NoRoom => UiTextKey.Quest_Result_No_Room,
            _ => UiTextKey.Quest_Result_Invalid
        };
    }
}
