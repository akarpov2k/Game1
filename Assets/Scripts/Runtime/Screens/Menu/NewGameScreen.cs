#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Loadouts;
using AnomalyPath.Core.Session;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Menu
{
    /// <summary>
    /// Экран старта игры: ник с кубиком, выбор набора из данных, настройки сложности
    /// из общей заготовки строки и набор экипировки в собственной прокрутке.
    /// Разметка, размеры и отступы заданы в префабе; код связывает данные и обновляет превью.
    /// </summary>
    public sealed class NewGameScreen : PrefabScreenBase
    {
        [Tooltip("Поле ввода ника")]
        [SerializeField]
        private TextInputField nickname = null!;

        [Tooltip("Кнопка-кубик: случайное имя")]
        [SerializeField]
        private Button diceButton = null!;

        [Tooltip("Грань кубика")]
        [SerializeField]
        private DiceButton dice = null!;

        [Tooltip("Левая колонка кнопок наборов")]
        [SerializeField]
        private RectTransform loadoutLeft = null!;

        [Tooltip("Правая колонка кнопок наборов")]
        [SerializeField]
        private RectTransform loadoutRight = null!;

        [Tooltip("Неактивный образец кнопки набора")]
        [SerializeField]
        private Button loadoutTemplate = null!;

        [Tooltip("Левая колонка строк настроек")]
        [SerializeField]
        private RectTransform settingsLeft = null!;

        [Tooltip("Правая колонка строк настроек")]
        [SerializeField]
        private RectTransform settingsRight = null!;

        [Tooltip("Неактивный образец строки настройки")]
        [SerializeField]
        private SettingRow settingRowTemplate = null!;

        [Tooltip("Окно набора экипировки")]
        [SerializeField]
        private NewGameInventoryFrame preview = null!;

        [Tooltip("Строка состояния")]
        [SerializeField]
        private Text status = null!;

        [Tooltip("Кнопка начала игры")]
        [SerializeField]
        private Button start = null!;

        [Tooltip("Кнопка возврата в меню")]
        [SerializeField]
        private Button back = null!;

        private readonly List<Button> _loadoutButtons = new List<Button>();
        private readonly List<string> _loadoutIds = new List<string>();
        private NumberField _insurance = null!;
        private NumberField _trade = null!;
        private NumberField _xpLoss = null!;
        private CheckboxRow _permadeath = null!;
        private int _nicknameMax;
        private double _insuranceDefault;
        private double _insuranceStep;
        private double _tradeDefault;
        private double _tradeMax;
        private double _tradeStep;
        private double _xpLossDefault;
        private double _xpLossMax;
        private double _xpLossStep;
        private string _selectedLoadout = string.Empty;
        private int _settingsCount;

        /// <summary>Готовит экран к показу: первый набор, настройки по умолчанию и случайный ник.</summary>
        public void Show()
        {
            _selectedLoadout = _loadoutIds.Count > 0 ? _loadoutIds[0] : string.Empty;
            ResetDifficulty();
            ResetNickname();
            SetStatus(string.Empty);
            UpdateSelection();
            Host.Router.Show(Panel);
            Canvas.ForceUpdateCanvases();
            RefreshPreview();
            Canvas.ForceUpdateCanvases();
        }

        /// <summary>Esc со старта игры возвращает в меню; в фокусе поля ввода Esc снимает фокус.</summary>
        /// <returns><c>true</c>, если Esc обработан.</returns>
        public override bool HandleEscape()
        {
            if (nickname.IsFocused)
            {
                return false;
            }

            Host.ShowMainMenu();
            return true;
        }

        /// <inheritdoc />
        protected override void OnBound()
        {
            var rules = Host.Rules.Require();
            _nicknameMax = rules.NicknameMaxLength;
            _insuranceDefault = rules.InsuranceChance;
            _insuranceStep = rules.InsuranceStep;
            _tradeDefault = rules.TradeDifficulty;
            _tradeMax = rules.TradeDifficultyMax;
            _tradeStep = rules.TradeDifficultyStep;
            _xpLossDefault = rules.XpLossChance;
            _xpLossMax = rules.XpLossMax;
            _xpLossStep = rules.XpLossStep;

            diceButton.onClick.AddListener(RollNickname);
            start.onClick.AddListener(StartGame);
            back.onClick.AddListener(Host.ShowMainMenu);
            preview.Bind(Host);
            BuildSettings();
            BuildLoadouts();
        }

        private void BuildSettings()
        {
            _insurance = AddSetting(
                "InsuranceRow",
                "InsuranceField",
                UiTextKey.NewGame_Insurance_Label,
                0,
                1,
                _insuranceStep,
                _insuranceDefault);

            _trade = AddSetting(
                "TradeRow",
                "TradeField",
                UiTextKey.NewGame_Trade_Label,
                0,
                _tradeMax,
                _tradeStep,
                _tradeDefault);

            _xpLoss = AddSetting(
                "XpLossRow",
                "XpLossField",
                UiTextKey.NewGame_XpLoss_Label,
                0,
                _xpLossMax,
                _xpLossStep,
                _xpLossDefault);

            _permadeath = CheckboxRow.Create(
                Host.Ui,
                settingsRight,
                "PermadeathRow",
                UiText.Get(Host.Texts, UiTextKey.NewGame_Permadeath_Label),
                false);
        }

        private NumberField AddSetting(
            string rowName,
            string fieldName,
            UiTextKey labelKey,
            double minimum,
            double maximum,
            double step,
            double initial)
        {
            var parent = _settingsCount % 2 == 0 ? settingsLeft : settingsRight;
            _settingsCount++;
            var row = Instantiate(settingRowTemplate, parent);
            row.gameObject.name = rowName;
            row.gameObject.SetActive(true);
            row.Bind(UiText.Get(Host.Texts, labelKey), minimum, maximum, step, initial);

            var field = row.GetComponentInChildren<NumberField>(true);
            field.gameObject.name = fieldName;
            if (field.transform.childCount >= 3)
            {
                var input = field.transform.GetChild(0);
                input.gameObject.name = fieldName + "Input";
            }

            return field;
        }

        private void BuildLoadouts()
        {
            foreach (Transform child in loadoutLeft)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in loadoutRight)
            {
                Destroy(child.gameObject);
            }

            _loadoutButtons.Clear();
            _loadoutIds.Clear();

            var index = 0;
            foreach (var loadout in SortedLoadouts(Host.Loadouts))
            {
                var id = loadout.Id;
                var name = Host.Texts.Get(loadout.NameKey);
                var parent = index % 2 == 0 ? loadoutLeft : loadoutRight;
                var button = Instantiate(loadoutTemplate, parent);
                button.gameObject.name = name;
                button.gameObject.SetActive(true);

                var label = button.GetComponentInChildren<Text>(true);
                if (label != null)
                {
                    label.text = name;
                }

                button.onClick.AddListener(() => Select(id));
                _loadoutButtons.Add(button);
                _loadoutIds.Add(id);
                index++;
            }

            UpdateSelection();
        }

        private void RollNickname()
        {
            dice.Show(UnityEngine.Random.Range(1, 7));
            nickname.SetText(NextNickname());
        }

        private void ResetNickname()
        {
            dice.Show(UnityEngine.Random.Range(1, 7));
            nickname.SetText(NextNickname());
        }

        private string NextNickname() => Host.Nicknames.Generate(unchecked((int)DateTime.UtcNow.Ticks));

        private void ResetDifficulty()
        {
            _insurance.SetValue(_insuranceDefault);
            _trade.SetValue(_tradeDefault);
            _xpLoss.SetValue(_xpLossDefault);
            _permadeath.SetValue(false);
        }

        private void Select(string id)
        {
            _selectedLoadout = id;
            SetStatus(string.Empty);
            UpdateSelection();
            RefreshPreview();
            Canvas.ForceUpdateCanvases();
        }

        private void UpdateSelection()
        {
            for (var index = 0; index < _loadoutButtons.Count && index < _loadoutIds.Count; index++)
            {
                var button = _loadoutButtons[index];
                var selected = _loadoutIds[index] == _selectedLoadout;

                var background = button.GetComponent<Image>();
                if (background != null)
                {
                    background.color = selected ? MenuPalette.ButtonSelected : UiFactory.ButtonColor;
                }

                var marker = button.transform.Find("SelectedMarker");
                if (marker != null)
                {
                    marker.gameObject.SetActive(selected);
                }
            }
        }

        private void RefreshPreview()
        {
            if (string.IsNullOrEmpty(_selectedLoadout))
            {
                preview.Show(null, UiText.Get(Host.Texts, UiTextKey.NewGame_No_Loadouts), StartingMoney());
                return;
            }

            try
            {
                preview.Show(Host.LoadoutPreviews.Build(_selectedLoadout), null, StartingMoney());
            }
            catch (Exception error)
            {
                preview.Show(null, UiText.Format(Host.Texts, UiTextKey.NewGame_Preview_Failed, error.Message), StartingMoney());
            }
        }

        private int StartingMoney()
        {
            var fallback = Host.Rules.Require().StartingMoney;
            if (string.IsNullOrEmpty(_selectedLoadout) || !Host.Loadouts.Contains(_selectedLoadout))
            {
                return fallback;
            }

            return Host.Loadouts.Get(_selectedLoadout).StartingMoney ?? fallback;
        }

        private void SetStatus(string text)
        {
            status.text = text;
            status.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }

        private void StartGame()
        {
            if (string.IsNullOrEmpty(_selectedLoadout))
            {
                SetStatus(UiText.Get(Host.Texts, UiTextKey.NewGame_Status_No_Sets));
                return;
            }

            var name = nickname.Text.Trim();
            if (name.Length == 0)
            {
                SetStatus(UiText.Get(Host.Texts, UiTextKey.NewGame_Status_Enter_Name));
                return;
            }

            if (name.Length > _nicknameMax)
            {
                SetStatus(UiText.Format(Host.Texts, UiTextKey.NewGame_Status_Name_Too_Long, _nicknameMax));
                return;
            }

            try
            {
                var seed = unchecked((int)DateTime.UtcNow.Ticks);
                var settingsData = new NewGameSettings(
                    insuranceChance: _insurance.Value,
                    tradeDifficulty: _trade.Value,
                    nickname: name,
                    nicknameMaxLength: _nicknameMax,
                    permadeath: _permadeath.Value,
                    xpLossChance: _xpLoss.Value);
                Host.Session = Host.Sessions.StartNew(seed, _selectedLoadout, settingsData);
                Host.ShowGame();
            }
            catch (Exception error)
            {
                SetStatus(UiText.Format(Host.Texts, UiTextKey.NewGame_Status_Error, error.Message));
            }
        }

        private static List<LoadoutTemplate> SortedLoadouts(ILoadoutRepository repository)
        {
            var loadouts = new List<LoadoutTemplate>(repository.All);
            loadouts.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));
            return loadouts;
        }
    }
}
