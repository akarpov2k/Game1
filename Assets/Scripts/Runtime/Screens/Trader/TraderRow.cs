#nullable enable
using System;
using AnomalyPath.Core.Templates;
using AnomalyPath.Core.Trade;
using AnomalyPath.Runtime.Ui.Core;
using AnomalyPath.Runtime.Ui.Items;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Trader
{
    /// <summary>Строка товара на экране торговца: иконка, название, цена, запас и кнопка сделки.</summary>
    public static class TraderRow
    {
        /// <summary>Создаёт строку покупки.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="content">Контент списка.</param>
        /// <param name="index">Порядковый номер строки.</param>
        /// <param name="offer">Товар торговца.</param>
        /// <param name="buy">Обработчик покупки.</param>
        /// <returns>Созданный объект строки.</returns>
        public static GameObject BuildBuy(IUiHost host, Transform content, int index, TraderOffer offer, Action buy)
        {
            var template = host.Items.Get(offer.ItemId);
            var row = CreateRow(host, content, "Buy_" + offer.ItemId, index);
            Icon(host, row.transform, template.Icon, template.Category);
            UiWidgets.Label(
                host,
                row.transform,
                "Name",
                host.Texts.Get(template.NameKey),
                22,
                TextAnchor.MiddleLeft,
                UiFactory.TextColor,
                TraderMetrics.TextLeft,
                6f,
                420f,
                28f);
            UiWidgets.Label(
                host,
                row.transform,
                "Stock",
                UiText.Format(host.Texts, UiTextKey.Trader_Stock, offer.Stock),
                18,
                TextAnchor.MiddleLeft,
                UiFactory.MutedTextColor,
                TraderMetrics.TextLeft,
                34f,
                420f,
                22f);
            RightLabel(
                host,
                row.transform,
                "Price",
                UiText.Format(host.Texts, UiTextKey.Trader_Price, offer.UnitPrice),
                -24f,
                8f,
                160f);
            var button = host.Ui.CreateButton(
                row.transform,
                UiText.Get(host.Texts, UiTextKey.Trader_Buy),
                buy);
            button.gameObject.name = "Buy";
            PlaceRight((RectTransform)button.transform, 8f, 10f, TraderMetrics.ButtonWidth, TraderMetrics.ButtonHeight);
            return row;
        }

        /// <summary>Создаёт строку продажи.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="content">Контент списка.</param>
        /// <param name="index">Порядковый номер строки.</param>
        /// <param name="offer">Предложение продажи.</param>
        /// <param name="sell">Обработчик продажи.</param>
        /// <returns>Созданный объект строки.</returns>
        public static GameObject BuildSell(
            IUiHost host,
            Transform content,
            int index,
            TraderSellOffer offer,
            Action sell)
        {
            var template = host.Items.Get(offer.ItemId);
            var row = CreateRow(host, content, "Sell_" + offer.InstanceId, index);
            Icon(host, row.transform, template.Icon, template.Category);
            var label = host.Texts.Get(template.NameKey);
            UiWidgets.Label(
                host,
                row.transform,
                "Name",
                label + " ×" + offer.Count,
                22,
                TextAnchor.MiddleLeft,
                UiFactory.TextColor,
                TraderMetrics.TextLeft,
                6f,
                420f,
                28f);
            UiWidgets.Label(
                host,
                row.transform,
                "Stock",
                UiText.Format(host.Texts, UiTextKey.Trader_Stock, offer.Count),
                18,
                TextAnchor.MiddleLeft,
                UiFactory.MutedTextColor,
                TraderMetrics.TextLeft,
                34f,
                420f,
                22f);
            RightLabel(
                host,
                row.transform,
                "Price",
                UiText.Format(host.Texts, UiTextKey.Trader_Price, offer.TotalPrice),
                -24f,
                8f,
                160f);
            var button = host.Ui.CreateButton(
                row.transform,
                UiText.Get(host.Texts, UiTextKey.Trader_Sell),
                sell);
            button.gameObject.name = "Sell";
            PlaceRight((RectTransform)button.transform, 8f, 10f, TraderMetrics.ButtonWidth, TraderMetrics.ButtonHeight);
            return row;
        }

        private static GameObject CreateRow(IUiHost host, Transform content, string name, int index)
        {
            var row = host.Ui.CreateImage(name, content, TraderPalette.Row);
            row.raycastTarget = false;
            var rect = row.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            var top = index * (TraderMetrics.RowHeight + TraderMetrics.RowGap);
            rect.offsetMin = new Vector2(0f, -(top + TraderMetrics.RowHeight));
            rect.offsetMax = new Vector2(0f, -top);
            return row.gameObject;
        }

        private static void Icon(IUiHost host, Transform row, string iconId, ItemCategory category)
        {
            if (host.Sprites.TryGet(iconId, out var sprite) && sprite != null)
            {
                UiWidgets.Icon(
                    host,
                    row,
                    "Icon",
                    sprite,
                    8f,
                    8f,
                    TraderMetrics.IconSize,
                    TraderMetrics.IconSize);
                return;
            }

            var color = category switch
            {
                ItemCategory.Weapon => ItemPalette.Weapon,
                ItemCategory.Ammo => ItemPalette.Ammo,
                ItemCategory.Armor => ItemPalette.Armor,
                ItemCategory.Helmet => ItemPalette.Armor,
                ItemCategory.Backpack => ItemPalette.Armor,
                ItemCategory.Belt => ItemPalette.Armor,
                ItemCategory.Artifact => ItemPalette.Artifact,
                _ => ItemPalette.Other
            };
            UiWidgets.Box(
                host,
                row,
                "Icon",
                8f,
                8f,
                TraderMetrics.IconSize,
                TraderMetrics.IconSize,
                color);
        }

        private static void RightLabel(
            IUiHost host,
            Transform row,
            string name,
            string text,
            float right,
            float top,
            float width)
        {
            var label = host.Ui.CreateText(row, text, 22, TextAnchor.MiddleRight);
            label.gameObject.name = name;
            label.raycastTarget = false;
            var rect = label.rectTransform;
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(right - TraderMetrics.ButtonWidth, -top);
            rect.sizeDelta = new Vector2(width, 48f);
        }

        private static void PlaceRight(RectTransform rect, float right, float top, float width, float height)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-right, -top);
            rect.sizeDelta = new Vector2(width, height);
        }
    }
}
