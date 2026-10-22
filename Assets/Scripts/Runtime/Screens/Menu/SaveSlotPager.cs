#nullable enable
using System;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Menu
{
    /// <summary>Переключение страниц ячеек сохранения: «&lt;», номер страницы, «&gt;».</summary>
    internal sealed class SaveSlotPager
    {
        private const float ButtonSize = 54f;
        private const float LabelWidth = 120f;
        private const float Offset = 150f;

        private readonly GameObject _root;
        private readonly Button _previous;
        private readonly Button _next;
        private readonly Text _label;

        public SaveSlotPager(IUiHost host, Transform parent, Action onPrevious, Action onNext)
        {
            _root = new GameObject("SlotPager", typeof(RectTransform));
            _root.transform.SetParent(parent, false);
            var rect = (RectTransform)_root.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            _previous = host.Ui.CreateButton(_root.transform, "<", onPrevious);
            _previous.gameObject.name = "PrevPage";
            _next = host.Ui.CreateButton(_root.transform, ">", onNext);
            _next.gameObject.name = "NextPage";
            _label = host.Ui.CreateText(_root.transform, string.Empty, 22, TextAnchor.MiddleCenter);
            _label.gameObject.name = "PageLabel";
            _root.SetActive(false);
        }

        /// <summary>Ставит переключатель по центру на заданной высоте.</summary>
        /// <param name="centerX">Центр по горизонтали.</param>
        /// <param name="y">Отступ сверху.</param>
        public void Place(float centerX, float y)
        {
            UiWidgets.Place((RectTransform)_previous.transform, centerX - Offset, y, ButtonSize, ButtonSize);
            UiWidgets.Place((RectTransform)_label.transform, centerX - (LabelWidth / 2f), y, LabelWidth, ButtonSize);
            UiWidgets.Place((RectTransform)_next.transform, centerX + Offset - ButtonSize, y, ButtonSize, ButtonSize);
        }

        /// <summary>Обновляет номер страницы и видимость стрелок.</summary>
        /// <param name="page">Текущая страница с нуля.</param>
        /// <param name="pageCount">Всего страниц.</param>
        public void Refresh(int page, int pageCount)
        {
            var multiple = pageCount > 1;
            _root.SetActive(multiple);
            if (!multiple)
            {
                return;
            }

            _previous.gameObject.SetActive(page > 0);
            _next.gameObject.SetActive(page < pageCount - 1);
            _label.text = (page + 1) + " / " + pageCount;
        }
    }
}
