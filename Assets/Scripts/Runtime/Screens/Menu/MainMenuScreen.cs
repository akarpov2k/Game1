#nullable enable
using AnomalyPath.Runtime.Screens;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;

namespace AnomalyPath.Runtime.Screens.Menu
{
    /// <summary>Стартовое меню: новая игра, загрузка, сведения, выход.</summary>
    public sealed class MainMenuScreen : ScreenBase
    {
        /// <summary>Создаёт панель стартового меню.</summary>
        /// <param name="host">Хост интерфейса: сервисы и переходы.</param>
        /// <param name="canvas">Родительский канвас.</param>
        public MainMenuScreen(IUiHost host, Transform canvas)
            : base(host, canvas, "MainMenu")
        {
            var column = host.Ui.CreateColumn(Content, 620);
            host.Ui.CreateTitle(column, UiText.Get(host.Texts, UiTextKey.MainMenu_Game_Title));
            host.Ui.CreateText(
                column,
                UiText.Get(host.Texts, UiTextKey.MainMenu_Subtitle),
                20,
                TextAnchor.MiddleCenter,
                UiFactory.MutedTextColor);
            host.Ui.CreateButton(
                column,
                UiText.Get(host.Texts, UiTextKey.MainMenu_New_Game),
                host.ShowNewGame);
            host.Ui.CreateButton(
                column,
                UiText.Get(host.Texts, UiTextKey.MainMenu_Load),
                () => host.ShowLoad(fromGame: false));
            host.Ui.CreateButton(
                column,
                UiText.Get(host.Texts, UiTextKey.MainMenu_About),
                () => host.ShowInfo(
                    UiText.Get(host.Texts, UiTextKey.MainMenu_About),
                    UiText.Get(host.Texts, UiTextKey.MainMenu_About_Text),
                    host.ShowMainMenu));
            host.Ui.CreateButton(
                column,
                UiText.Get(host.Texts, UiTextKey.MainMenu_Authors),
                () => host.ShowInfo(
                    UiText.Get(host.Texts, UiTextKey.MainMenu_Authors),
                    UiText.Get(host.Texts, UiTextKey.MainMenu_Authors_Text),
                    host.ShowMainMenu));
            host.Ui.CreateButton(
                column,
                UiText.Get(host.Texts, UiTextKey.Common_Quit),
                host.Quit);
        }
    }
}
