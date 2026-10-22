#nullable enable
using AnomalyPath.Runtime.Screens;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;

namespace AnomalyPath.Runtime.Screens.Menu
{
    /// <summary>Стартовое меню: новая игра, загрузка, сведения, выход.</summary>
    public sealed class MainMenuScreen : ScreenBase
    {
        /// <summary>Заголовок игры.</summary>
        public const string GameTitle = "Anomaly Path";

        /// <summary>Текст пункта «Об игре».</summary>
        public const string AboutText =
            "Anomaly Path — постапокалиптическое путешествие по аномальным зонам. " +
            "Переходите между локациями, собирайте снаряжение, выживайте в пошаговых боях.";

        /// <summary>Текст пункта «Авторы».</summary>
        public const string AuthorsText = "VlomInc\nAnomaly Path";

        /// <summary>Создаёт панель стартового меню.</summary>
        /// <param name="host">Хост интерфейса: сервисы и переходы.</param>
        /// <param name="canvas">Родительский канвас.</param>
        public MainMenuScreen(IUiHost host, Transform canvas)
            : base(host, canvas, "MainMenu")
        {
            var column = host.Ui.CreateColumn(Content, 620);
            host.Ui.CreateTitle(column, GameTitle);
            host.Ui.CreateText(column, "Аномальное путешествие", 20, TextAnchor.MiddleCenter, UiFactory.MutedTextColor);
            host.Ui.CreateButton(column, "Новая игра", host.ShowNewGame);
            host.Ui.CreateButton(column, "Загрузить", () => host.ShowLoad(fromGame: false));
            host.Ui.CreateButton(column, "Об игре", () => host.ShowInfo("Об игре", AboutText, host.ShowMainMenu));
            host.Ui.CreateButton(column, "Авторы", () => host.ShowInfo("Авторы", AuthorsText, host.ShowMainMenu));
            host.Ui.CreateButton(column, "Выход", host.Quit);
        }
    }
}
