#nullable enable
using System;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Session;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>Что экраны могут делать с игрой: переключать панели, читать сервисы, сохранять и выходить.</summary>
    public interface IUiHost
    {
        /// <summary>Фабрика элементов интерфейса.</summary>
        UiFactory Ui { get; }

        /// <summary>Переключатель экранов.</summary>
        ScreenRouter Router { get; }

        /// <summary>Текущая игровая сессия или <c>null</c>.</summary>
        GameSession? Session { get; set; }

        /// <summary>Действия сессии: новая игра, сохранение, загрузка.</summary>
        IGameSessionService Sessions { get; }

        /// <summary>Слоты сохранений.</summary>
        ISaveService Saves { get; }

        /// <summary>Наборы снаряжения.</summary>
        ILoadoutRepository Loadouts { get; }

        /// <summary>Пробная раскладка набора: что игрок получит на старте.</summary>
        ILoadoutPreviewService LoadoutPreviews { get; }

        /// <summary>Предметы: названия и категории для отображения.</summary>
        IItemRepository Items { get; }

        /// <summary>Шаблоны локаций: названия для основного экрана и меню.</summary>
        ILocationRepository Locations { get; }

        /// <summary>Экипировка: число слотов артефактов и строк инвентаря.</summary>
        IEquipmentService Equipment { get; }

        /// <summary>Действия с предметами: надеть, снять, разрядить, разобрать, выбросить.</summary>
        ICharacterActions Actions { get; }

        /// <summary>Каталог спрайтов: иконки предметов и фоны локаций по id из данных.</summary>
        ISpriteCatalog Sprites { get; }

        /// <summary>Актёры: максимум здоровья игрока для полосы.</summary>
        IActorRepository Actors { get; }

        /// <summary>Генератор ников для новой игры.</summary>
        INicknameGenerator Nicknames { get; }

        /// <summary>Правила прохождения.</summary>
        IGameRulesRepository Rules { get; }

        /// <summary>Тексты игры по ключу.</summary>
        IGameTexts Texts { get; }

        /// <summary>Показать стартовое меню.</summary>
        void ShowMainMenu();

        /// <summary>Показать выбор набора и настроек новой игры.</summary>
        void ShowNewGame();

        /// <summary>Показать список сохранений.</summary>
        /// <param name="fromGame">Пришли из игры, а не из стартового меню.</param>
        void ShowLoad(bool fromGame);

        /// <summary>Показать экран сохранения: ячейки, название и существующие сохранения.</summary>
        void ShowSave();

        /// <summary>Показать основной игровой экран.</summary>
        /// <param name="announce">Показать сообщение о начале игры.</param>
        void ShowGame(bool announce = true);

        /// <summary>Показать меню игры: продолжить, сохранить, загрузить, выйти.</summary>
        void ShowGameMenu();

        /// <summary>Показать экран персонажа с экипировкой и инвентарём.</summary>
        void ShowCharacter();

        /// <summary>Показать информационный экран.</summary>
        /// <param name="title">Заголовок.</param>
        /// <param name="body">Текст.</param>
        /// <param name="back">Куда вернуться по кнопке «Назад».</param>
        void ShowInfo(string title, string body, Action back);

        /// <summary>Показать модальный диалог выхода.</summary>
        void ShowExitDialog();

        /// <summary>Скрыть модальный диалог выхода.</summary>
        void HideExitDialog();

        /// <summary>Сохранить игру в слот выхода.</summary>
        void SaveExit();

        /// <summary>Показать всплывающее сообщение.</summary>
        /// <param name="text">Текст сообщения.</param>
        void Toast(string text);

        /// <summary>Выйти из игры.</summary>
        void Quit();
    }
}
