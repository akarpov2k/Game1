#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Talents;
using AnomalyPath.Runtime.Screens;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Talents
{
    /// <summary>
    /// Дерево талантов с ветками: карточки-картинки в координатах row/column, связи между
    /// соседними талантами, подсказка по наведению с задержкой и изучение по нажатию.
    /// Разметка строится кодом; все подписи берутся из каталога строк.
    /// </summary>
    public sealed class TalentScreen : ScreenBase
    {
        private const string PanelName = "Talents";

        private readonly IUiHost _host;
        private readonly Text _title;
        private readonly Text _points;
        private readonly Text _spent;
        private readonly Text _description;
        private readonly Text _toast;
        private readonly ScrollArea _scroll;
        private readonly TalentTooltip _tooltip;

        /// <summary>Создаёт экран талантов.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="canvas">Родительский канвас.</param>
        public TalentScreen(IUiHost host, Transform canvas)
            : base(host, canvas, PanelName)
        {
            _host = host;

            _title = host.Ui.CreateTitle(Content, string.Empty);
            _title.gameObject.name = "TalentTitle";
            var titleRect = _title.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -TalentMetrics.TitleTop);
            titleRect.sizeDelta = new Vector2(600f, TalentMetrics.TitleHeight);

            _points = host.Ui.CreateText(Content, string.Empty, 24, TextAnchor.MiddleRight, UiFactory.TextColor);
            _points.gameObject.name = "TalentPoints";
            PlaceRight(_points.rectTransform, TalentMetrics.PointsTop, 360f, 28f);

            _spent = host.Ui.CreateText(Content, string.Empty, 20, TextAnchor.MiddleRight, UiFactory.MutedTextColor);
            _spent.gameObject.name = "TalentSpent";
            PlaceRight(_spent.rectTransform, TalentMetrics.PointsTop + 30f, 360f, 24f);

            _description = host.Ui.CreateText(Content, string.Empty, 18, TextAnchor.UpperLeft, UiFactory.MutedTextColor);
            _description.gameObject.name = "TalentDescription";
            _description.raycastTarget = false;
            var descriptionRect = _description.rectTransform;
            descriptionRect.anchorMin = new Vector2(0f, 1f);
            descriptionRect.anchorMax = new Vector2(1f, 1f);
            descriptionRect.pivot = new Vector2(0.5f, 1f);
            descriptionRect.offsetMin = new Vector2(TalentMetrics.Side, -(TalentMetrics.DescriptionTop + TalentMetrics.DescriptionHeight));
            descriptionRect.offsetMax = new Vector2(-TalentMetrics.Side, -TalentMetrics.DescriptionTop);

            _scroll = CreateScroll();

            _toast = host.Ui.CreateText(Content, string.Empty, 22, TextAnchor.MiddleCenter, UiFactory.ToastColor);
            _toast.gameObject.name = "TalentToast";
            var toastRect = _toast.rectTransform;
            toastRect.anchorMin = new Vector2(0f, 0f);
            toastRect.anchorMax = new Vector2(1f, 0f);
            toastRect.pivot = new Vector2(0.5f, 0f);
            toastRect.offsetMin = new Vector2(TalentMetrics.Side, 96f);
            toastRect.offsetMax = new Vector2(-TalentMetrics.Side, 136f);

            var back = host.Ui.CreateButton(Content, UiText.Get(host.Texts, UiTextKey.Common_Back), Back);
            back.gameObject.name = "TalentBack";
            var backRect = (RectTransform)back.transform;
            backRect.anchorMin = new Vector2(1f, 0f);
            backRect.anchorMax = new Vector2(1f, 0f);
            backRect.pivot = new Vector2(1f, 0f);
            backRect.anchoredPosition = new Vector2(-TalentMetrics.Side, 32f);
            backRect.sizeDelta = new Vector2(180f, TalentMetrics.BottomButtonHeight);

            _tooltip = new TalentTooltip(host, Content);
        }

        /// <summary>Открывает дерево талантов и перестраивает его по текущему состоянию игрока.</summary>
        public void Show()
        {
            ApplyTheme();
            _host.Router.Show(Panel);
            Canvas.ForceUpdateCanvases();
            Refresh();
        }

        /// <summary>Esc возвращает на экран персонажа.</summary>
        /// <returns><c>true</c>, если Esc обработан.</returns>
        public override bool HandleEscape()
        {
            Back();
            return true;
        }

        private void Back() => _host.ShowCharacter();

        private void Refresh()
        {
            var player = _host.Session?.State.Player;
            var texts = _host.Texts;
            _tooltip.Hide();
            _title.text = UiText.Get(texts, UiTextKey.Talent_Title);
            Clear(_scroll.Content);

            if (player == null)
            {
                _points.text = string.Empty;
                _spent.text = string.Empty;
                _description.text = UiText.Get(texts, UiTextKey.Common_No_Game);
                _scroll.SetContentHeight(TalentMetrics.ListHeight);
                return;
            }

            var tree = _host.Talents.Tree;
            _points.text = UiText.Format(texts, UiTextKey.Talent_Points, _host.Talents.AvailablePoints(player));
            _spent.text = UiText.Format(texts, UiTextKey.Talent_Spent, _host.Talents.SpentPoints(player));
            _description.text = texts.Get(tree.TitleKey) + "\n" + texts.Get(tree.DescriptionKey);

            if (tree.Nodes.Count == 0)
            {
                UiWidgets.Label(
                    _host,
                    _scroll.Content,
                    "TalentEmpty",
                    UiText.Get(texts, UiTextKey.Talent_Empty),
                    20,
                    TextAnchor.MiddleCenter,
                    UiFactory.MutedTextColor,
                    8f,
                    8f,
                    TalentMetrics.NodeWidth,
                    40f);
                _scroll.SetContentHeight(TalentMetrics.ListHeight);
                return;
            }

            var maxColumn = 0;
            foreach (var node in tree.Nodes)
            {
                maxColumn = Mathf.Max(maxColumn, node.Column);
            }

            var columns = maxColumn + 1;
            var gridWidth = (columns * TalentMetrics.NodeWidth) + ((columns - 1) * TalentMetrics.ColumnGap);
            var gridLeft = Mathf.Max(
                TalentMetrics.PaddingX,
                (TalentMetrics.ListWidth - gridWidth) / 2f);

            foreach (var node in tree.Nodes)
            {
                foreach (var parent in FindParents(tree.Nodes, node))
                {
                    CreateLink(_scroll.Content, parent, node, gridLeft);
                }
            }

            var bottom = TalentMetrics.PaddingTop;
            foreach (var node in tree.Nodes)
            {
                var top = TalentMetrics.PaddingTop + (node.Row * TalentMetrics.RowStep);
                CreateNode(_scroll.Content, node, player, top, gridLeft);
                bottom = Mathf.Max(bottom, top + TalentMetrics.NodeHeight);
            }

            _scroll.SetContentHeight(Mathf.Max(
                TalentMetrics.ListHeight,
                bottom + TalentMetrics.PaddingBottom));
        }

        private void CreateNode(
            Transform content,
            TalentNodeTemplate node,
            PlayerState player,
            float top,
            float gridLeft)
        {
            var rank = _host.Talents.RankOf(player, node.Id);
            var unlocked = _host.Talents.IsUnlocked(player, node);
            var maxed = rank >= node.MaxRank;
            var frame = _host.Ui.CreateImage(
                "TalentNode_" + node.Id,
                content,
                maxed
                    ? TalentPalette.MaxNode
                    : unlocked ? TalentPalette.Node : TalentPalette.LockedNode);
            frame.raycastTarget = true;
            var rect = frame.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(
                gridLeft + (node.Column * TalentMetrics.ColumnStep),
                -top);
            rect.sizeDelta = new Vector2(TalentMetrics.NodeWidth, TalentMetrics.NodeHeight);

            var icon = _host.Ui.CreateImage(
                "Icon",
                frame.transform,
                maxed
                    ? TalentPalette.MaxIcon
                    : unlocked ? TalentPalette.NodeIcon : TalentPalette.LockedIcon);
            icon.raycastTarget = false;
            var iconRect = icon.rectTransform;
            iconRect.anchorMin = new Vector2(0f, 1f);
            iconRect.anchorMax = new Vector2(0f, 1f);
            iconRect.pivot = new Vector2(0f, 1f);
            iconRect.anchoredPosition = new Vector2(TalentMetrics.IconInset, -TalentMetrics.IconInset);
            iconRect.sizeDelta = new Vector2(TalentMetrics.IconSize, TalentMetrics.IconSize);
            if (!string.IsNullOrWhiteSpace(node.Icon) &&
                _host.Sprites.TryGet(node.Icon, out var sprite) &&
                sprite != null)
            {
                icon.sprite = sprite;
                icon.preserveAspect = true;
            }
            else
            {
                icon.color = TalentPalette.MissingIcon;
            }

            var badge = UiWidgets.Box(
                _host,
                frame.transform,
                "RankBadge",
                TalentMetrics.NodeWidth - TalentMetrics.IconInset - TalentMetrics.RankWidth,
                TalentMetrics.IconInset + TalentMetrics.IconSize - TalentMetrics.RankHeight,
                TalentMetrics.RankWidth,
                TalentMetrics.RankHeight,
                maxed ? TalentPalette.MaxBadge : TalentPalette.RankBadge);
            badge.raycastTarget = false;
            UiWidgets.Label(
                _host,
                badge.transform,
                "Rank",
                UiText.Format(_host.Texts, UiTextKey.Talent_Rank, rank, node.MaxRank),
                16,
                TextAnchor.MiddleCenter,
                UiFactory.TextColor,
                0f,
                0f,
                TalentMetrics.RankWidth,
                TalentMetrics.RankHeight);

            UiWidgets.Label(
                _host,
                frame.transform,
                "Title",
                _host.Texts.Get(node.TitleKey),
                16,
                TextAnchor.MiddleCenter,
                unlocked ? UiFactory.TextColor : UiFactory.MutedTextColor,
                0f,
                TalentMetrics.NodeTitleTop,
                TalentMetrics.NodeWidth,
                TalentMetrics.NodeTitleHeight);

            var button = frame.gameObject.AddComponent<Button>();
            button.targetGraphic = frame;
            button.onClick.AddListener(() => Learn(node));

            var hover = frame.gameObject.AddComponent<TalentHover>();
            hover.Configure(
                TalentMetrics.TooltipDelay,
                (position, camera) => ShowTooltip(node, rank, unlocked, maxed, position, camera),
                (position, camera) => _tooltip.Move(position, camera),
                _tooltip.Hide);
        }

        private void ShowTooltip(
            TalentNodeTemplate node,
            int rank,
            bool unlocked,
            bool maxed,
            Vector2 position,
            Camera? camera)
        {
            var texts = _host.Texts;
            var player = _host.Session?.State.Player;
            var available = player != null ? _host.Talents.AvailablePoints(player) : 0;
            var status = maxed
                ? UiText.Get(texts, UiTextKey.Talent_Max)
                : !unlocked
                    ? UiText.Format(texts, UiTextKey.Talent_Locked, node.RequiredPoints)
                    : available > 0
                        ? UiText.Get(texts, UiTextKey.Talent_Tooltip_Click)
                        : UiText.Get(texts, UiTextKey.Talent_Result_No_Points);
            var statusColor = maxed
                ? TalentPalette.MaxBadge
                : unlocked && available > 0 ? UiFactory.ToastColor : UiFactory.MutedTextColor;
            _tooltip.Show(
                texts.Get(node.TitleKey),
                UiText.Format(texts, UiTextKey.Talent_Rank, rank, node.MaxRank),
                texts.Get(node.DescriptionKey),
                EffectsLine(node),
                status,
                statusColor,
                position,
                camera);
        }

        private void Learn(TalentNodeTemplate node)
        {
            var player = _host.Session?.State.Player;
            if (player == null)
            {
                return;
            }

            var result = _host.Talents.Learn(player, node.Id);
            if (result.Succeeded)
            {
                ShowToast(UiText.Format(
                    _host.Texts,
                    UiTextKey.Talent_Learned,
                    _host.Texts.Get(node.TitleKey)));
                Refresh();
                return;
            }

            ShowToast(UiText.Get(_host.Texts, KeyFor(result.Status)), error: true);
        }

        private string EffectsLine(TalentNodeTemplate node)
        {
            var parts = new List<string>();
            foreach (var effect in node.Effects)
            {
                var value = effect.PerRank.ToString("0.###", CultureInfo.InvariantCulture);
                var key = effect.Stat switch
                {
                    TalentStat.ApRestore => UiTextKey.Talent_Effect_Ap_Restore,
                    TalentStat.ApMax => UiTextKey.Talent_Effect_Ap_Max,
                    TalentStat.HealthMax => UiTextKey.Talent_Effect_Health,
                    TalentStat.Accuracy => UiTextKey.Talent_Effect_Accuracy,
                    TalentStat.StaminaMax => UiTextKey.Talent_Effect_Stamina,
                    TalentStat.Luck => UiTextKey.Talent_Effect_Luck,
                    _ => UiTextKey.Talent_Effect_Rows
                };
                parts.Add(UiText.Format(
                    _host.Texts,
                    UiTextKey.Talent_Effect_Per_Rank,
                    UiText.Format(_host.Texts, key, value)));
            }

            return string.Join("\n", parts);
        }

        private void ShowToast(string text, bool error = false)
        {
            _toast.text = text;
            _toast.color = error ? TalentPalette.Error : UiFactory.ToastColor;
        }

        private ScrollArea CreateScroll()
        {
            var go = new GameObject("TalentList", typeof(RectTransform), typeof(ScrollArea));
            go.transform.SetParent(Content, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -TalentMetrics.ListTop);
            rect.sizeDelta = new Vector2(TalentMetrics.ListWidth, TalentMetrics.ListHeight);
            var scroll = go.GetComponent<ScrollArea>();
            scroll.Configure(8f);
            return scroll;
        }

        private void CreateLink(
            Transform content,
            TalentNodeTemplate parent,
            TalentNodeTemplate node,
            float gridLeft)
        {
            var start = NodeCenter(parent, gridLeft);
            var end = NodeCenter(node, gridLeft);
            var delta = end - start;
            var length = delta.magnitude;
            if (length <= 0.01f)
            {
                return;
            }

            var link = _host.Ui.CreateImage("TalentLink", content, TalentPalette.Link);
            link.raycastTarget = false;
            var rect = link.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(
                start.x + (delta.x / 2f),
                -(start.y + (delta.y / 2f)));
            rect.sizeDelta = new Vector2(length, TalentMetrics.LinkWidth);
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(-delta.y, delta.x) * Mathf.Rad2Deg);
        }

        private static List<TalentNodeTemplate> FindParents(
            IReadOnlyList<TalentNodeTemplate> nodes,
            TalentNodeTemplate node)
        {
            var parents = new List<TalentNodeTemplate>();
            var previousRow = -1;
            foreach (var candidate in nodes)
            {
                if (candidate.Row >= node.Row)
                {
                    continue;
                }

                if (candidate.Row > previousRow)
                {
                    previousRow = candidate.Row;
                    parents.Clear();
                }

                if (candidate.Row == previousRow && Math.Abs(candidate.Column - node.Column) <= 1)
                {
                    parents.Add(candidate);
                }
            }

            if (parents.Count == 0 && previousRow >= 0)
            {
                TalentNodeTemplate? nearest = null;
                var best = int.MaxValue;
                foreach (var candidate in nodes)
                {
                    if (candidate.Row != previousRow)
                    {
                        continue;
                    }

                    var distance = Math.Abs(candidate.Column - node.Column);
                    if (distance < best)
                    {
                        best = distance;
                        nearest = candidate;
                    }
                }

                if (nearest != null)
                {
                    parents.Add(nearest);
                }
            }

            return parents;
        }

        private static Vector2 NodeCenter(TalentNodeTemplate node, float gridLeft) =>
            new Vector2(
                gridLeft + (node.Column * TalentMetrics.ColumnStep) + (TalentMetrics.NodeWidth / 2f),
                TalentMetrics.PaddingTop + (node.Row * TalentMetrics.RowStep) +
                TalentMetrics.IconInset + (TalentMetrics.IconSize / 2f));

        private void PlaceRight(RectTransform rect, float top, float width, float height)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-TalentMetrics.Side, -top);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void Clear(RectTransform content)
        {
            for (var index = content.childCount - 1; index >= 0; index--)
            {
                var child = content.GetChild(index).gameObject;
                child.SetActive(false);
                UnityEngine.Object.Destroy(child);
            }
        }

        private static UiTextKey KeyFor(TalentResultStatus status) => status switch
        {
            TalentResultStatus.UnknownTalent => UiTextKey.Talent_Result_Unknown,
            TalentResultStatus.MaxRank => UiTextKey.Talent_Result_Max,
            TalentResultStatus.Locked => UiTextKey.Talent_Result_Locked,
            TalentResultStatus.NoPoints => UiTextKey.Talent_Result_No_Points,
            _ => UiTextKey.Talent_Result_Invalid
        };
    }
}
