#nullable enable
using AnomalyPath.Runtime.Screens;
using AnomalyPath.Runtime.Screens.Menu;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Game
{
    /// <summary>Игровая панель: состояние сессии, сохранение, загрузка, выход.</summary>
    public sealed class GamePanelScreen : ScreenBase
    {
        private readonly IUiHost _host;
        private readonly Text _status;
        private readonly Text _toast;

        /// <summary>Создаёт игровую панель.</summary>
        /// <param name="host">Хост интерфейса: сервисы и переходы.</param>
        /// <param name="canvas">Родительский канвас.</param>
        public GamePanelScreen(IUiHost host, Transform canvas)
            : base(host, canvas, "InGame")
        {
            _host = host;
            var column = host.Ui.CreateColumn(Content, 760);
            host.Ui.CreateTitle(column, MainMenuScreen.GameTitle);
            _status = host.Ui.CreateText(column, string.Empty, 20, TextAnchor.UpperLeft);
            _status.GetComponent<LayoutElement>().preferredHeight = 160;
            host.Ui.CreateButton(column, "Персонаж", host.ShowCharacter);
            host.Ui.CreateButton(column, "Сохранить игру", host.ShowSave);
            host.Ui.CreateButton(column, "Загрузить игру", () => host.ShowLoad(fromGame: true));
            host.Ui.CreateButton(column, "Выйти", host.ShowExitDialog);

            _toast = host.Ui.CreateText(Content, string.Empty, 20, TextAnchor.MiddleCenter, UiFactory.ToastColor);
            var toastRect = _toast.rectTransform;
            toastRect.anchorMin = new Vector2(0.1f, 0.03f);
            toastRect.anchorMax = new Vector2(0.9f, 0.1f);
            toastRect.offsetMin = Vector2.zero;
            toastRect.offsetMax = Vector2.zero;
        }

        /// <summary>Фон игровой панели — текущая локация, если игра уже началась.</summary>
        protected override ScreenTheme Theme
        {
            get
            {
                var world = Host.Session?.State.World;
                return world == null ? ScreenTheme.Default : ScreenTheme.Location(world.CurrentLocation.Background);
            }
        }

        /// <summary>Обновляет текст состояния.</summary>
        public void Refresh()
        {
            ApplyTheme();
            var session = _host.Session;
            if (session == null)
            {
                _status.text = "Нет активной игры";
                return;
            }

            var world = session.State.World;
            var player = session.State.Player;
            var health = player == null ? "—" : player.Health.ToString();
            _status.text =
                $"Локация: {world.CurrentLocationId}\n" +
                $"Выносливость: {world.Stamina}/{world.StaminaMax}\n" +
                $"День {world.Clock.Day}, {world.Clock.Hours:0.0} ч\n" +
                $"Здоровье: {health}";
        }

        /// <summary>Показывает всплывающее сообщение.</summary>
        /// <param name="text">Текст сообщения.</param>
        public void ShowToast(string text) => _toast.text = text;
    }
}
