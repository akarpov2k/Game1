#nullable enable
using System;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Ui.Dialogs
{
    /// <summary>
    /// Диалог имени сохранения: поле названия, зелёная кнопка «Сохранить» и «Отмена».
    /// Esc и клик по фону закрывают диалог без записи.
    /// </summary>
    public sealed class SaveNameDialog
    {
        private static readonly Color SaveColor = new Color(0.24f, 0.48f, 0.30f, 1f);

        private readonly TextInputField _name;
        private readonly Text _error;
        private Action<string>? _onSave;

        /// <summary>Создаёт диалог и прячет его.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="parent">Родитель, обычно панель экрана сохранения.</param>
        public SaveNameDialog(IUiHost host, Transform parent)
        {
            Panel = host.Ui.CreatePanel("SaveNameDialog", parent);
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
            host.Ui.CreateTitle(box, "Сохранить игру");
            host.Ui.CreateText(box, "Название сохранения", 18, TextAnchor.MiddleLeft);
            _name = host.Ui.CreateInputField(box, "Введите название", string.Empty, 32, "SaveDialogName");
            _name.ConsumeEscape = false;
            _error = host.Ui.CreateText(box, string.Empty, 16, TextAnchor.MiddleCenter, UiFactory.ToastColor);

            var row = new GameObject("SaveDialogButtons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(box, false);
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            row.AddComponent<LayoutElement>().preferredHeight = 54;

            var save = host.Ui.CreateButton(row.transform, "Сохранить", Confirm);
            save.GetComponent<Image>().color = SaveColor;
            host.Ui.CreateButton(row.transform, "Отмена", Cancel);

            Panel.transform.SetAsLastSibling();
            Panel.SetActive(false);
        }

        /// <summary>Панель диалога.</summary>
        public GameObject Panel { get; }

        /// <summary><c>true</c>, если диалог открыт.</summary>
        public bool IsOpen => Panel.activeSelf;

        /// <summary>Открывает диалог с подставленным названием.</summary>
        /// <param name="defaultName">Название по умолчанию.</param>
        /// <param name="onSave">Действие с введённым названием.</param>
        public void Show(string defaultName, Action<string> onSave)
        {
            _onSave = onSave;
            _error.text = string.Empty;
            _name.SetText(defaultName);
            Panel.SetActive(true);
            Panel.transform.SetAsLastSibling();
            _name.Focus();
        }

        /// <summary>Esc закрывает диалог без сохранения.</summary>
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
            var name = _name.Text.Trim();
            if (name.Length == 0)
            {
                _error.text = "Введите название сохранения";
                return;
            }

            var action = _onSave;
            Hide();
            action?.Invoke(name);
        }

        private void Cancel() => Hide();

        private void Hide()
        {
            _onSave = null;
            Panel.SetActive(false);
        }
    }
}
