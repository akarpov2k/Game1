#nullable enable
using System;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Ui.Dialogs
{
    /// <summary>
    /// Модальное подтверждение действия: затемнение, вопрос и две кнопки — зелёная «Удалить»
    /// и «Отмена». Esc и клик по фону закрывают без действия.
    /// </summary>
    public sealed class ConfirmDialog
    {
        private static readonly Color ConfirmColor = new Color(0.24f, 0.48f, 0.30f, 1f);

        private readonly Text _title;
        private readonly Text _message;
        private readonly Button _confirm;
        private Action? _action;

        /// <summary>Создаёт диалог и прячет его.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="parent">Родитель, обычно панель экрана.</param>
        public ConfirmDialog(IUiHost host, Transform parent)
        {
            Panel = host.Ui.CreatePanel("ConfirmDialog", parent);
            var overlay = Panel.GetComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.7f);
            var backdrop = Panel.AddComponent<Button>();
            backdrop.targetGraphic = overlay;
            backdrop.onClick.AddListener(Cancel);

            var box = host.Ui.CreateColumn(Panel.transform, 700);
            var boxImage = box.gameObject.AddComponent<Image>();
            boxImage.color = new Color(0.12f, 0.16f, 0.15f, 1f);
            var blocker = box.gameObject.AddComponent<Button>();
            blocker.targetGraphic = boxImage;
            blocker.onClick.AddListener(() => { });
            _title = host.Ui.CreateTitle(box, "Подтверждение");
            _message = host.Ui.CreateText(box, string.Empty, 20, TextAnchor.MiddleCenter);
            _message.GetComponent<LayoutElement>().preferredHeight = 90;

            var row = new GameObject("ConfirmButtons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(box, false);
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            row.AddComponent<LayoutElement>().preferredHeight = 54;

            _confirm = host.Ui.CreateButton(row.transform, "Удалить", Confirm);
            _confirm.GetComponent<Image>().color = ConfirmColor;
            host.Ui.CreateButton(row.transform, "Отмена", Cancel);

            Panel.transform.SetAsLastSibling();
            Panel.SetActive(false);
        }

        /// <summary>Панель диалога.</summary>
        public GameObject Panel { get; }

        /// <summary><c>true</c>, если диалог открыт.</summary>
        public bool IsOpen => Panel.activeSelf;

        /// <summary>Открывает диалог с вопросом.</summary>
        /// <param name="title">Заголовок.</param>
        /// <param name="message">Текст вопроса.</param>
        /// <param name="onConfirm">Действие после подтверждения.</param>
        public void Show(string title, string message, Action onConfirm)
        {
            _title.text = title;
            _message.text = message;
            _action = onConfirm;
            Panel.SetActive(true);
            Panel.transform.SetAsLastSibling();
        }

        /// <summary>Esc закрывает подтверждение без действия.</summary>
        /// <returns><c>true</c>, если диалог был открыт и закрыт.</returns>
        public bool HandleEscape()
        {
            if (!Panel.activeSelf)
            {
                return false;
            }

            Cancel();
            return true;
        }

        private void Confirm()
        {
            var action = _action;
            Hide();
            action?.Invoke();
        }

        private void Cancel() => Hide();

        private void Hide()
        {
            _action = null;
            Panel.SetActive(false);
        }
    }
}
