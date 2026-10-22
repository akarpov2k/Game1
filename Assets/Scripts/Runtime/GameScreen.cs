#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Loadouts;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.Session;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace AnomalyPath.Runtime
{
    /// <summary>Стартовое меню и меню игры: новая игра, сохранения, выход.</summary>
    public sealed class GameScreen : MonoBehaviour
    {
        private const string GameTitle = "Anomaly Path";
        private const string AboutText =
            "Anomaly Path — постапокалиптическое путешествие по аномальным зонам. " +
            "Переходите между локациями, собирайте снаряжение, выживайте в пошаговых боях.";
        private const string AuthorsText = "VlomInc\nAnomaly Path";

        private GameBootstrap _bootstrap = null!;
        private IGameSessionService _sessions = null!;
        private ISaveService _saves = null!;
        private ILoadoutRepository _loadouts = null!;
        private IGameRulesRepository _rules = null!;
        private GameSession? _session;
        private Font _font = null!;

        private GameObject _mainMenu = null!;
        private GameObject _newGamePanel = null!;
        private GameObject _loadPanel = null!;
        private GameObject _infoPanel = null!;
        private GameObject _gamePanel = null!;
        private GameObject _exitDialog = null!;
        private Transform _loadList = null!;
        private Text _infoTitle = null!;
        private Text _infoBody = null!;
        private Text _gameStatus = null!;
        private Text _toast = null!;
        private Text _newGameStatus = null!;
        private Slider _lootSlider = null!;
        private Slider _tradeSlider = null!;
        private Text _lootValue = null!;
        private Text _tradeValue = null!;
        private string _selectedLoadout = string.Empty;
        private bool _loadFromGame;
        private Action _infoBack = null!;
        private readonly List<Button> _loadoutButtons = new List<Button>();

        private void Start()
        {
            _bootstrap = GetComponent<GameBootstrap>();
            _font = FindFont();
            EnsureEventSystem();
            var canvas = BuildCanvas();

            if (_bootstrap.Preflight == null || !_bootstrap.Preflight.CanStart)
            {
                BuildDataError(canvas);
                return;
            }

            _sessions = _bootstrap.Services!.Resolve<IGameSessionService>();
            _saves = _bootstrap.Services.Resolve<ISaveService>();
            _loadouts = _bootstrap.Services.Resolve<ILoadoutRepository>();
            _rules = _bootstrap.Services.Resolve<IGameRulesRepository>();

            BuildPanels(canvas);
            ShowMainMenu();
        }

        private Font FindFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return font != null ? font : Font.CreateDynamicFontFromOSFont("Arial", 16);
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

        private Transform BuildCanvas()
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            return go.transform;
        }

        private void BuildDataError(Transform canvas)
        {
            var panel = CreatePanel("DataError", canvas);
            var column = CreateColumn(panel.transform, 900);
            CreateTitle(column, "Ошибка данных");
            var text = CreateText(column, _bootstrap.Preflight?.Report.Summary ?? "Данные не загружены", 20, TextAnchor.UpperLeft);
            text.GetComponent<LayoutElement>().preferredHeight = 400;
            CreateButton(column, "Выход", Quit);
            panel.SetActive(true);
        }

        private void BuildPanels(Transform canvas)
        {
            _mainMenu = CreatePanel("MainMenu", canvas);
            _newGamePanel = CreatePanel("NewGame", canvas);
            _loadPanel = CreatePanel("LoadGame", canvas);
            _infoPanel = CreatePanel("Info", canvas);
            _gamePanel = CreatePanel("InGame", canvas);

            BuildMainMenu();
            BuildNewGame();
            BuildLoadPanel();
            BuildInfoPanel();
            BuildGamePanel();
            BuildExitDialog(canvas);
        }

        private void BuildMainMenu()
        {
            var column = CreateColumn(_mainMenu.transform, 620);
            CreateTitle(column, GameTitle);
            CreateText(column, "Аномальное путешествие", 20, TextAnchor.MiddleCenter, new Color(0.7f, 0.75f, 0.72f));
            CreateButton(column, "Новая игра", ShowNewGame);
            CreateButton(column, "Загрузить", () => ShowLoad(fromGame: false));
            CreateButton(column, "Об игре", () => ShowInfo("Об игре", AboutText, ShowMainMenu));
            CreateButton(column, "Авторы", () => ShowInfo("Авторы", AuthorsText, ShowMainMenu));
            CreateButton(column, "Выход", Quit);
        }

        private void BuildNewGame()
        {
            var column = CreateColumn(_newGamePanel.transform, 760);
            CreateTitle(column, "Новая игра");
            CreateText(column, "Выберите набор снаряжения", 22, TextAnchor.MiddleCenter);

            var loadouts = new List<LoadoutTemplate>(_loadouts.All);
            loadouts.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));
            foreach (var loadout in loadouts)
            {
                var id = loadout.Id;
                var button = CreateButton(column, loadout.NameKey, () => SelectLoadout(id));
                _loadoutButtons.Add(button);
            }

            _newGameStatus = CreateText(column, string.Empty, 18, TextAnchor.MiddleCenter);
            CreateText(column, "Потеря лута при смерти", 18, TextAnchor.MiddleLeft);
            _lootSlider = CreateSlider(column, 0, 1, 0.5f, out _lootValue);
            CreateText(column, "Усложнение торговли", 18, TextAnchor.MiddleLeft);
            _tradeSlider = CreateSlider(column, 0, 3, 1f, out _tradeValue);

            CreateButton(column, "Начать", StartNewGame);
            CreateButton(column, "Назад", ShowMainMenu);
        }

        private void BuildLoadPanel()
        {
            var column = CreateColumn(_loadPanel.transform, 900);
            CreateTitle(column, "Загрузить игру");
            var list = new GameObject("Slots", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            list.transform.SetParent(column, false);
            var layout = list.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            list.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            _loadList = list.transform;
            CreateButton(column, "Назад", BackFromLoad);
        }

        private void BuildInfoPanel()
        {
            var column = CreateColumn(_infoPanel.transform, 1200);
            _infoTitle = CreateTitle(column, string.Empty);
            _infoBody = CreateText(column, string.Empty, 22, TextAnchor.UpperLeft);
            _infoBody.GetComponent<LayoutElement>().preferredHeight = 500;
            CreateButton(column, "Назад", () => _infoBack());
        }

        private void BuildGamePanel()
        {
            var column = CreateColumn(_gamePanel.transform, 760);
            CreateTitle(column, GameTitle);
            _gameStatus = CreateText(column, string.Empty, 20, TextAnchor.UpperLeft);
            _gameStatus.GetComponent<LayoutElement>().preferredHeight = 160;
            CreateButton(column, "Сохранить игру", SaveManual);
            CreateButton(column, "Загрузить игру", () => ShowLoad(fromGame: true));
            CreateButton(column, "Выйти", ShowExitDialog);

            _toast = CreateText(_gamePanel.transform, string.Empty, 20, TextAnchor.MiddleCenter, new Color(0.95f, 0.85f, 0.55f));
            var toastRect = _toast.rectTransform;
            toastRect.anchorMin = new Vector2(0.1f, 0.03f);
            toastRect.anchorMax = new Vector2(0.9f, 0.1f);
            toastRect.offsetMin = Vector2.zero;
            toastRect.offsetMax = Vector2.zero;
        }

        private void BuildExitDialog(Transform canvas)
        {
            _exitDialog = CreatePanel("ExitDialog", canvas);
            var overlay = _exitDialog.GetComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.7f);
            var overlayButton = _exitDialog.AddComponent<Button>();
            overlayButton.targetGraphic = overlay;
            overlayButton.onClick.AddListener(HideExitDialog);

            var box = CreateColumn(_exitDialog.transform, 700);
            var boxImage = box.gameObject.AddComponent<Image>();
            boxImage.color = new Color(0.12f, 0.16f, 0.15f, 1f);
            CreateTitle(box, "Выйти из игры?");
            CreateText(box, "Игра будет сохранена в exit_save.", 20, TextAnchor.MiddleCenter);
            CreateButton(box, "В меню", ExitToMenu);
            CreateButton(box, "На рабочий стол", ExitToDesktop);

            _exitDialog.transform.SetAsLastSibling();
            _exitDialog.SetActive(false);
        }

        private void ShowMainMenu()
        {
            _session = null;
            ShowOnly(_mainMenu);
        }

        private void ShowNewGame()
        {
            _selectedLoadout = string.Empty;
            if (_loadouts.All.Count > 0)
            {
                var templates = new List<LoadoutTemplate>(_loadouts.All);
                templates.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));
                _selectedLoadout = templates[0].Id;
            }

            var rules = _rules.Rules;
            if (rules != null)
            {
                foreach (var option in rules.LootLossOptions)
                {
                    if (option.Id == rules.DefaultLootLoss)
                    {
                        _lootSlider.value = (float)option.Share;
                        break;
                    }
                }
            }

            _tradeSlider.value = (float)NewGameSettings.DefaultTradeDifficulty;
            UpdateLoadoutButtons();
            _newGameStatus.text = "Набор: " + _selectedLoadout;
            ShowOnly(_newGamePanel);
        }

        private void SelectLoadout(string id)
        {
            _selectedLoadout = id;
            _newGameStatus.text = "Набор: " + id;
            UpdateLoadoutButtons();
        }

        private void UpdateLoadoutButtons()
        {
            var index = 0;
            var templates = new List<LoadoutTemplate>(_loadouts.All);
            templates.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));
            foreach (var button in _loadoutButtons)
            {
                var id = templates[index].Id;
                var label = button.GetComponentInChildren<Text>();
                label.text = (id == _selectedLoadout ? "> " : string.Empty) + templates[index].NameKey;
                index++;
            }
        }

        private void StartNewGame()
        {
            if (string.IsNullOrEmpty(_selectedLoadout))
            {
                _newGameStatus.text = "Нет доступных наборов";
                return;
            }

            try
            {
                var seed = unchecked((int)DateTime.UtcNow.Ticks);
                var settings = new NewGameSettings(_lootSlider.value, _tradeSlider.value);
                _session = _sessions.StartNew(seed, _selectedLoadout, settings);
                ShowGame();
            }
            catch (Exception error)
            {
                _newGameStatus.text = "Ошибка: " + error.Message;
            }
        }

        private void ShowGame()
        {
            _exitDialog.SetActive(false);
            ShowOnly(_gamePanel);
            RefreshGame();
            ShowToast("Игра начата");
        }

        private void RefreshGame()
        {
            if (_session == null)
            {
                _gameStatus.text = "Нет активной игры";
                return;
            }

            var world = _session.State.World;
            var player = _session.State.Player;
            var health = player == null ? "—" : player.Health.ToString();
            _gameStatus.text =
                $"Локация: {world.CurrentLocationId}\n" +
                $"Выносливость: {world.Stamina}/{world.StaminaMax}\n" +
                $"День {world.Clock.Day}, {world.Clock.Hours:0.0} ч\n" +
                $"Здоровье: {health}";
        }

        private void SaveManual()
        {
            if (_session == null)
            {
                ShowToast("Нет активной игры");
                return;
            }

            try
            {
                _sessions.Save(_session, SaveSlots.Manual);
                ShowToast("Сохранено в " + SaveSlots.Manual);
            }
            catch (Exception error)
            {
                ShowToast("Ошибка сохранения: " + error.Message);
            }
        }

        private void SaveExit()
        {
            if (_session == null)
            {
                return;
            }

            try
            {
                _sessions.Save(_session, SaveSlots.Exit);
            }
            catch (Exception error)
            {
                Debug.LogError("AnomalyPath: не удалось сохранить exit_save: " + error.Message);
            }
        }

        private void ShowLoad(bool fromGame)
        {
            _loadFromGame = fromGame;
            BuildLoadList();
            ShowOnly(_loadPanel);
        }

        private void BuildLoadList()
        {
            foreach (Transform child in _loadList)
            {
                Destroy(child.gameObject);
            }

            var slots = _saves.ListSlots();
            if (slots.Count == 0)
            {
                CreateText(_loadList, "Сохранений нет", 20, TextAnchor.MiddleCenter);
                return;
            }

            foreach (var slot in slots)
            {
                var name = slot.IsReadable
                    ? $"{slot.Slot}   день {slot.Day}, {slot.Hours:0.0} ч"
                    : slot.Slot + "   (повреждён)";
                var slotId = slot.Slot;
                CreateButton(_loadList, name, () => LoadSlot(slotId));
            }
        }

        private void LoadSlot(string slot)
        {
            try
            {
                _session = _sessions.Load(slot);
                ShowGame();
            }
            catch (Exception error)
            {
                ShowToast("Ошибка загрузки: " + error.Message);
            }
        }

        private void BackFromLoad()
        {
            if (_loadFromGame && _session != null)
            {
                ShowOnly(_gamePanel);
                RefreshGame();
            }
            else
            {
                ShowMainMenu();
            }
        }

        private void ShowInfo(string title, string body, Action back)
        {
            _infoTitle.text = title;
            _infoBody.text = body;
            _infoBack = back;
            ShowOnly(_infoPanel);
        }

        private void ShowExitDialog()
        {
            _exitDialog.SetActive(true);
            _exitDialog.transform.SetAsLastSibling();
        }

        private void HideExitDialog() => _exitDialog.SetActive(false);

        private void ExitToMenu()
        {
            SaveExit();
            HideExitDialog();
            _session = null;
            ShowMainMenu();
        }

        private void ExitToDesktop()
        {
            SaveExit();
            HideExitDialog();
            Application.Quit();
        }

        private void Quit() => Application.Quit();

        private void ShowToast(string text)
        {
            if (_toast != null)
            {
                _toast.text = text;
            }
        }

        private void ShowOnly(GameObject panel)
        {
            _mainMenu.SetActive(panel == _mainMenu);
            _newGamePanel.SetActive(panel == _newGamePanel);
            _loadPanel.SetActive(panel == _loadPanel);
            _infoPanel.SetActive(panel == _infoPanel);
            _gamePanel.SetActive(panel == _gamePanel);
            _exitDialog.SetActive(false);
        }

        private GameObject CreatePanel(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.09f, 1f);
            go.SetActive(false);
            return go;
        }

        private static Transform CreateColumn(Transform parent, float width)
        {
            var go = new GameObject("Column", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, 0);
            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            var fitter = go.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return go.transform;
        }

        private Text CreateTitle(Transform parent, string text)
        {
            var label = CreateText(parent, text, 40, TextAnchor.MiddleCenter);
            label.gameObject.AddComponent<LayoutElement>().preferredHeight = 70;
            return label;
        }

        private Text CreateText(Transform parent, string text, int size, TextAnchor anchor)
        {
            return CreateText(parent, text, size, anchor, new Color(0.9f, 0.92f, 0.9f));
        }

        private Text CreateText(Transform parent, string text, int size, TextAnchor anchor, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<Text>();
            label.font = _font;
            label.fontSize = size;
            label.text = text;
            label.alignment = anchor;
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            go.AddComponent<LayoutElement>().preferredHeight = size + 12;
            return label;
        }

        private Button CreateButton(Transform parent, string text, Action action)
        {
            var go = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = new Color(0.18f, 0.24f, 0.23f, 1f);
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => action());
            go.AddComponent<LayoutElement>().preferredHeight = 54;

            var label = CreateText(go.transform, text, 22, TextAnchor.MiddleCenter);
            var rect = label.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return button;
        }

        private Slider CreateSlider(Transform parent, float min, float max, float value, out Text valueLabel)
        {
            var go = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent, false);
            go.AddComponent<LayoutElement>().preferredHeight = 40;

            var slider = go.GetComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;

            var background = CreateImage("Background", go.transform, new Color(0.16f, 0.18f, 0.18f, 1f));
            Stretch(background.rectTransform, 0.72f, 0.88f);
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            Stretch(fillArea.GetComponent<RectTransform>(), 0.72f, 0.88f);
            var fill = CreateImage("Fill", fillArea.transform, new Color(0.35f, 0.65f, 0.45f, 1f));
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = Vector2.one;
            fill.rectTransform.offsetMin = Vector2.zero;
            fill.rectTransform.offsetMax = Vector2.zero;
            var handleArea = new GameObject("Handle Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            Stretch(handleArea.GetComponent<RectTransform>(), 0.72f, 0.88f);
            var handle = CreateImage("Handle", handleArea.transform, Color.white);
            handle.rectTransform.sizeDelta = new Vector2(24, 0);

            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;

            valueLabel = CreateText(parent, value.ToString("0.00"), 16, TextAnchor.MiddleCenter);
            var capturedLabel = valueLabel;
            slider.onValueChanged.AddListener(current => capturedLabel.text = current.ToString("0.00"));
            return slider;
        }

        private Image CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static void Stretch(RectTransform rect, float anchorMinY, float anchorMaxY)
        {
            rect.anchorMin = new Vector2(0, anchorMinY);
            rect.anchorMax = new Vector2(1, anchorMaxY);
            rect.offsetMin = new Vector2(0, 0);
            rect.offsetMax = new Vector2(0, 0);
        }
    }
}
