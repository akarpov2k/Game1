#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.Session;
using AnomalyPath.Runtime.Screens;
using AnomalyPath.Runtime.Screens.Character;
using AnomalyPath.Runtime.Screens.Game;
using AnomalyPath.Runtime.Screens.Menu;
using AnomalyPath.Runtime.Ui.Art;
using AnomalyPath.Runtime.Ui.Core;
using AnomalyPath.Runtime.Ui.Dialogs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace AnomalyPath.Runtime
{
    /// <summary>
    /// Точка входа интерфейса: создаёт канвас и экраны, переключает их и связывает с сервисами.
    /// Сами панели живут в отдельных классах <c>Screens</c>; здесь только навигация.
    /// </summary>
    public sealed class GameScreen : MonoBehaviour, IUiHost
    {
        [Tooltip("Каталог спрайтов: иконки предметов и фоны локаций. Пусто — цветные заглушки")]
        [SerializeField]
        private SpriteCatalog? spriteCatalog = null;

        private GameBootstrap _bootstrap = null!;
        private readonly List<ScreenBase> _screens = new List<ScreenBase>();
        private MainMenuScreen _mainMenu = null!;
        private NewGameScreen _newGame = null!;
        private LoadGameScreen _load = null!;
        private SaveGameScreen _saveGame = null!;
        private InfoScreen _info = null!;
        private PlayScreen _play = null!;
        private GamePanelScreen _gamePanel = null!;
        private CharacterScreen _character = null!;
        private ExitDialog _exitDialog = null!;

        /// <inheritdoc />
        public UiFactory Ui { get; private set; } = null!;

        /// <inheritdoc />
        public ScreenRouter Router { get; } = new ScreenRouter();

        /// <inheritdoc />
        public GameSession? Session { get; set; }

        /// <inheritdoc />
        public IGameSessionService Sessions { get; private set; } = null!;

        /// <inheritdoc />
        public ISaveService Saves { get; private set; } = null!;

        /// <inheritdoc />
        public ILoadoutRepository Loadouts { get; private set; } = null!;

        /// <inheritdoc />
        public ILoadoutPreviewService LoadoutPreviews { get; private set; } = null!;

        /// <inheritdoc />
        public IItemRepository Items { get; private set; } = null!;

        /// <inheritdoc />
        public IEquipmentService Equipment { get; private set; } = null!;

        /// <inheritdoc />
        public ILocationRepository Locations { get; private set; } = null!;

        /// <inheritdoc />
        public ICharacterActions Actions { get; private set; } = null!;

        /// <inheritdoc />
        public ISpriteCatalog Sprites => spriteCatalog != null ? spriteCatalog : NullSpriteCatalog.Instance;

        /// <inheritdoc />
        public IActorRepository Actors { get; private set; } = null!;

        /// <inheritdoc />
        public INicknameGenerator Nicknames { get; private set; } = null!;

        /// <inheritdoc />
        public IGameRulesRepository Rules { get; private set; } = null!;

        /// <inheritdoc />
        public IGameTexts Texts { get; private set; } = null!;

        private void Start()
        {
            _bootstrap = GetComponent<GameBootstrap>();
            Ui = new UiFactory(UiFactory.FindFont());
            EnsureEventSystem();
            EnsureCamera();
            var canvas = BuildCanvas();

            if (_bootstrap.Preflight == null || !_bootstrap.Preflight.CanStart)
            {
                BuildDataError(canvas);
                return;
            }

            var services = _bootstrap.Services!;
            Sessions = services.Resolve<IGameSessionService>();
            Saves = services.Resolve<ISaveService>();
            Loadouts = services.Resolve<ILoadoutRepository>();
            LoadoutPreviews = services.Resolve<ILoadoutPreviewService>();
            Items = services.Resolve<IItemRepository>();
            Equipment = services.Resolve<IEquipmentService>();
            Locations = services.Resolve<ILocationRepository>();
            Actions = services.Resolve<ICharacterActions>();
            Actors = services.Resolve<IActorRepository>();
            Nicknames = services.Resolve<INicknameGenerator>();
            Rules = services.Resolve<IGameRulesRepository>();
            Texts = services.Resolve<IGameTexts>();

            _mainMenu = new MainMenuScreen(this, canvas);
            _newGame = new NewGameScreen(this, canvas);
            _load = new LoadGameScreen(this, canvas);
            _saveGame = new SaveGameScreen(this, canvas);
            _info = new InfoScreen(this, canvas);
            _play = new PlayScreen(this, canvas);
            _gamePanel = new GamePanelScreen(this, canvas);
            _character = new CharacterScreen(this, canvas);
            Router.Add(_mainMenu.Panel);
            Router.Add(_newGame.Panel);
            Router.Add(_load.Panel);
            Router.Add(_saveGame.Panel);
            Router.Add(_info.Panel);
            Router.Add(_play.Panel);
            Router.Add(_gamePanel.Panel);
            Router.Add(_character.Panel);
            _screens.Add(_mainMenu);
            _screens.Add(_newGame);
            _screens.Add(_load);
            _screens.Add(_saveGame);
            _screens.Add(_info);
            _screens.Add(_play);
            _screens.Add(_gamePanel);
            _screens.Add(_character);

            _exitDialog = new ExitDialog(this, canvas);
            ShowMainMenu();
        }

        /// <summary>Esc: диалог выхода закрывается, меню игры возвращает в игру, игровой экран открывает меню, персонаж — обратно в игру.</summary>
        /// <returns><c>true</c>, если Esc обработан.</returns>
        public bool HandleEscape()
        {
            if (_exitDialog != null && _exitDialog.HandleEscape())
            {
                return true;
            }

            var current = Router.Current;
            if (current == null)
            {
                return false;
            }

            foreach (var screen in _screens)
            {
                if (screen.Panel == current)
                {
                    return screen.HandleEscape();
                }
            }

            return false;
        }

        private void Update()
        {
            if (TextInputField.EscapeConsumedFrame == Time.frameCount)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                HandleEscape();
            }
        }

        /// <inheritdoc />
        public void ShowMainMenu()
        {
            Session = null;
            _exitDialog.Hide();
            Router.Show(_mainMenu.Panel);
        }

        /// <inheritdoc />
        public void ShowNewGame()
        {
            _exitDialog.Hide();
            _newGame.Show();
        }

        /// <inheritdoc />
        public void ShowLoad(bool fromGame)
        {
            _exitDialog.Hide();
            _load.Show(fromGame);
        }

        /// <inheritdoc />
        public void ShowSave()
        {
            _exitDialog.Hide();
            _saveGame.Show();
        }

        /// <inheritdoc />
        public void ShowGame(bool announce = true)
        {
            _exitDialog.Hide();
            _play.Show();
            if (announce)
            {
                Toast(UiText.Get(Texts, UiTextKey.Game_Started));
            }
        }

        /// <inheritdoc />
        public void ShowGameMenu()
        {
            _exitDialog.Hide();
            _gamePanel.Show();
        }

        /// <inheritdoc />
        public void ShowCharacter()
        {
            _exitDialog.Hide();
            _character.Show();
        }

        /// <inheritdoc />
        public void ShowInfo(string title, string body, Action back)
        {
            _exitDialog.Hide();
            _info.Show(title, body, back);
        }

        /// <inheritdoc />
        public void ShowExitDialog() => _exitDialog.Show();

        /// <inheritdoc />
        public void HideExitDialog() => _exitDialog.Hide();

        /// <inheritdoc />
        public void Toast(string text) => _play?.ShowToast(text);

        /// <inheritdoc />
        public void SaveExit()
        {
            if (Session == null)
            {
                return;
            }

            try
            {
                Sessions.Save(Session, SaveSlots.Exit);
            }
            catch (Exception error)
            {
                Debug.LogError(CoreText.Format(CoreErrors.ExitSaveFailed, error.Message));
            }
        }

        /// <inheritdoc />
        public void Quit() => Application.Quit();

        private static void EnsureCamera()
        {
            if (UnityEngine.Object.FindAnyObjectByType<Camera>() != null)
            {
                return;
            }

            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, 0f, -10f);
            var camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.04f, 0.05f, 0.06f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.cullingMask = 0;
            camera.depth = -1f;
            camera.useOcclusionCulling = false;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var module = go.GetComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }

        private static Transform BuildCanvas()
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            return go.transform;
        }

        private void BuildDataError(Transform canvas)
        {
            var panel = Ui.CreatePanel("DataError", canvas);
            var column = Ui.CreateColumn(panel.transform, 900);
            Ui.CreateTitle(column, CoreText.Get(CoreErrors.DataErrorTitle));
            var text = Ui.CreateText(column, _bootstrap.Preflight?.Report.Summary ?? CoreText.Get(CoreErrors.DataErrorMissing), 20, TextAnchor.UpperLeft);
            text.GetComponent<LayoutElement>().preferredHeight = 400;
            Ui.CreateButton(column, CoreText.Get(CoreErrors.DataErrorQuit), Quit);
            panel.SetActive(true);
        }
    }
}
