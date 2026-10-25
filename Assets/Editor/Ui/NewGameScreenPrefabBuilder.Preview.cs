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
    /// <summary>Часть класса <see cref="NewGameScreenPrefabBuilder"/>: панель превью.</summary>
    public static partial class NewGameScreenPrefabBuilder
    {
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
    }
}
