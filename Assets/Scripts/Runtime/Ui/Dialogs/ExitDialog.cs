#nullable enable
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Ui.Dialogs
{
    /// <summary>Модальный диалог выхода поверх текущего экрана.</summary>
    public sealed class ExitDialog
    {
        /// <summary>Создаёт диалог и прячет его.</summary>
        /// <param name="host">Хост интерфейса: сервисы и переходы.</param>
        /// <param name="canvas">Родительский канвас.</param>
        public ExitDialog(IUiHost host, Transform canvas)
        {
            Panel = host.Ui.CreatePanel("ExitDialog", canvas);
            var overlay = Panel.GetComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.7f);
            var overlayButton = Panel.AddComponent<Button>();
            overlayButton.targetGraphic = overlay;
            overlayButton.onClick.AddListener(host.HideExitDialog);

            var box = host.Ui.CreateColumn(Panel.transform, 700);
            var boxImage = box.gameObject.AddComponent<Image>();
            boxImage.color = new Color(0.12f, 0.16f, 0.15f, 1f);
            host.Ui.CreateTitle(box, UiText.Get(host.Texts, UiTextKey.Exit_Title));
            host.Ui.CreateText(
                box,
                UiText.Get(host.Texts, UiTextKey.Exit_Body),
                20,
                TextAnchor.MiddleCenter);
            host.Ui.CreateButton(box, UiText.Get(host.Texts, UiTextKey.Exit_To_Menu), () =>
            {
                host.SaveExit();
                host.HideExitDialog();
                host.ShowMainMenu();
            });
            host.Ui.CreateButton(box, UiText.Get(host.Texts, UiTextKey.Exit_To_Desktop), () =>
            {
                host.SaveExit();
                host.HideExitDialog();
                host.Quit();
            });

            Panel.transform.SetAsLastSibling();
            Panel.SetActive(false);
        }

        /// <summary>Панель диалога.</summary>
        public GameObject Panel { get; }

        /// <summary>Показывает диалог поверх остальных панелей.</summary>
        public void Show()
        {
            Panel.SetActive(true);
            Panel.transform.SetAsLastSibling();
        }

        /// <summary>Скрывает диалог.</summary>
        public void Hide() => Panel.SetActive(false);

        /// <summary>Esc закрывает диалог, как клик по фону.</summary>
        /// <returns><c>true</c>, если диалог был открыт и закрыт.</returns>
        public bool HandleEscape()
        {
            if (!Panel.activeSelf)
            {
                return false;
            }

            Hide();
            return true;
        }
    }
}
