#nullable enable
using AnomalyPath.Runtime.Screens;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Game
{
    /// <summary>Меню игры: продолжить, сохранить, загрузить и выйти. Открывается шестерёнкой.</summary>
    public sealed class GamePanelScreen : ScreenBase
    {
        private readonly IUiHost _host;
        private readonly Text _status;

        /// <summary>Создаёт меню игры.</summary>
        /// <param name="host">Хост интерфейса: сервисы и переходы.</param>
        /// <param name="canvas">Родительский канвас.</param>
        public GamePanelScreen(IUiHost host, Transform canvas)
            : base(host, canvas, "GameMenu")
        {
            _host = host;
            var column = host.Ui.CreateColumn(Content, 760);
            host.Ui.CreateTitle(column, UiText.Get(host.Texts, UiTextKey.GameMenu_Title));
            _status = host.Ui.CreateText(column, string.Empty, 20, TextAnchor.UpperLeft);
            _status.GetComponent<LayoutElement>().preferredHeight = 160;
            host.Ui.CreateButton(
                column,
                UiText.Get(host.Texts, UiTextKey.GameMenu_Continue),
                () => host.ShowGame(announce: false));
            host.Ui.CreateButton(column, UiText.Get(host.Texts, UiTextKey.Game_Save), host.ShowSave);
            host.Ui.CreateButton(
                column,
                UiText.Get(host.Texts, UiTextKey.Game_Load),
                () => host.ShowLoad(fromGame: true));
            host.Ui.CreateButton(column, UiText.Get(host.Texts, UiTextKey.Game_Exit), host.ShowExitDialog);
        }

        /// <summary>Фон меню — текущая локация, если игра уже началась.</summary>
        protected override ScreenTheme Theme
        {
            get
            {
                var world = Host.Session?.State.World;
                return world == null ? ScreenTheme.Default : ScreenTheme.Location(world.CurrentLocation.Background);
            }
        }

        /// <summary>Показывает меню и обновляет состояние.</summary>
        public void Show()
        {
            Refresh();
            _host.Router.Show(Panel);
        }

        /// <summary>Esc возвращает в игру; без активной игры — в стартовое меню.</summary>
        /// <returns><c>true</c>, если Esc обработан.</returns>
        public override bool HandleEscape()
        {
            if (_host.Session == null)
            {
                _host.ShowMainMenu();
            }
            else
            {
                _host.ShowGame(announce: false);
            }

            return true;
        }

        /// <summary>Обновляет сводку состояния.</summary>
        public void Refresh()
        {
            ApplyTheme();
            var session = _host.Session;
            if (session == null)
            {
                _status.text = UiText.Get(_host.Texts, UiTextKey.Common_No_Game);
                return;
            }

            var world = session.State.World;
            var location = world.CurrentLocation;
            var player = session.State.Player;
            var health = player == null
                ? UiText.Get(_host.Texts, UiTextKey.Common_Dash)
                : player.Health.ToString();
            _status.text = UiText.Format(
                _host.Texts,
                UiTextKey.Game_Status,
                LocationText.Title(_host, location),
                world.Stamina,
                world.StaminaMax,
                world.Clock.Day,
                world.Clock.Hours,
                health);
        }
    }
}
