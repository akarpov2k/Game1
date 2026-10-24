#nullable enable
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Talents
{
    /// <summary>
    /// Всплывающая подсказка таланта у курсора: название, ранг, описание, эффекты и статус.
    /// Панель не перехватывает мышь и сама подстраивает высоту под текст.
    /// </summary>
    public sealed class TalentTooltip
    {
        private readonly RectTransform _panel;
        private readonly Text _title;
        private readonly Text _rank;
        private readonly Text _description;
        private readonly Text _effects;
        private readonly Text _status;

        /// <summary>Создаёт скрытую подсказку на экране.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="parent">Родитель, обычно корень экрана.</param>
        public TalentTooltip(IUiHost host, Transform parent)
        {
            var background = host.Ui.CreateImage("TalentTooltip", parent, TalentPalette.Tooltip);
            background.raycastTarget = false;
            var outline = background.gameObject.AddComponent<Outline>();
            outline.effectColor = TalentPalette.TooltipBorder;
            outline.effectDistance = new Vector2(1f, -1f);

            _panel = background.rectTransform;
            _panel.anchorMin = new Vector2(0.5f, 0.5f);
            _panel.anchorMax = new Vector2(0.5f, 0.5f);
            _panel.pivot = new Vector2(0f, 1f);
            _panel.sizeDelta = new Vector2(TalentMetrics.TooltipWidth, 0f);

            var layout = background.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 12, 12);
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = background.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _title = CreateLabel(host, _panel, "Title", 21, UiFactory.TextColor, false);
            _title.fontStyle = FontStyle.Bold;
            _rank = CreateLabel(host, _panel, "Rank", 17, UiFactory.MutedTextColor, false);
            _description = CreateLabel(host, _panel, "Description", 17, UiFactory.TextColor, true);
            _effects = CreateLabel(host, _panel, "Effects", 16, UiFactory.ToastColor, true);
            _status = CreateLabel(host, _panel, "Status", 16, UiFactory.MutedTextColor, true);

            _panel.gameObject.SetActive(false);
        }

        /// <summary>Показывает подсказку и ставит её у курсора.</summary>
        /// <param name="title">Название таланта.</param>
        /// <param name="rank">Текущий и предельный ранг.</param>
        /// <param name="description">Описание.</param>
        /// <param name="effects">Строки эффектов.</param>
        /// <param name="status">Статус: изучить, закрыт или максимум.</param>
        /// <param name="statusColor">Цвет статуса.</param>
        /// <param name="screenPosition">Позиция курсора в координатах экрана.</param>
        /// <param name="camera">Камера события указателя или <c>null</c> для оверлея.</param>
        public void Show(
            string title,
            string rank,
            string description,
            string effects,
            string status,
            Color statusColor,
            Vector2 screenPosition,
            Camera? camera)
        {
            _title.text = title;
            _rank.text = rank;
            _description.text = description;
            _effects.text = effects;
            _status.text = status;
            _status.color = statusColor;

            _panel.gameObject.SetActive(true);
            _panel.SetAsLastSibling();
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_panel);
            Canvas.ForceUpdateCanvases();
            Move(screenPosition, camera);
        }

        /// <summary>Перемещает подсказку к курсору, не выходя за края экрана.</summary>
        /// <param name="screenPosition">Позиция курсора в координатах экрана.</param>
        /// <param name="camera">Камера события указателя или <c>null</c> для оверлея.</param>
        public void Move(Vector2 screenPosition, Camera? camera)
        {
            if (!_panel.gameObject.activeInHierarchy || !(_panel.parent is RectTransform area))
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(area, screenPosition, camera, out var local))
            {
                return;
            }

            var size = _panel.rect.size;
            if (size.x <= 0f || size.y <= 0f)
            {
                return;
            }

            var half = area.rect.size * 0.5f;
            var margin = TalentMetrics.TooltipMargin;
            var offset = TalentMetrics.TooltipOffset;

            var x = local.x + offset;
            if (x + size.x > half.x - margin)
            {
                x = local.x - size.x - offset;
            }

            var y = local.y - offset;
            if (y - size.y < -half.y + margin)
            {
                y = local.y + size.y + offset;
            }

            var minX = -half.x + margin;
            var maxX = half.x - size.x - margin;
            if (minX > maxX)
            {
                minX = maxX;
            }

            var minY = -half.y + size.y + margin;
            var maxY = half.y - margin;
            if (minY > maxY)
            {
                minY = maxY;
            }

            _panel.anchoredPosition = new Vector2(
                Mathf.Clamp(x, minX, maxX),
                Mathf.Clamp(y, minY, maxY));
        }

        /// <summary>Скрывает подсказку.</summary>
        public void Hide()
        {
            if (_panel != null)
            {
                _panel.gameObject.SetActive(false);
            }
        }

        private static Text CreateLabel(
            IUiHost host,
            Transform parent,
            string name,
            int size,
            Color color,
            bool flexibleHeight)
        {
            var label = host.Ui.CreateText(parent, string.Empty, size, TextAnchor.UpperLeft, color);
            label.gameObject.name = name;
            label.raycastTarget = false;
            if (flexibleHeight)
            {
                var element = label.GetComponent<LayoutElement>();
                if (element != null)
                {
                    element.preferredHeight = -1f;
                }
            }

            return label;
        }
    }
}
