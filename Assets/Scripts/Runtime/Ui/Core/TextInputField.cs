#nullable enable
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>
    /// Поле ввода на новом Input System: клик — фокус, символы и Backspace — правка текста,
    /// Enter и Escape снимают фокус. Legacy InputField не используется: проект живёт только на новом вводе.
    /// </summary>
    public sealed class TextInputField : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("Компонент текста поля")]
        [SerializeField]
        private Text? text = null;

        [Tooltip("Подсказка при пустом поле")]
        [SerializeField]
        private Text? placeholder = null;

        [Tooltip("Фон поля")]
        [SerializeField]
        private Image? background = null;

        [Tooltip("Предел длины вводимого текста")]
        [SerializeField]
        private int maxLength = 24;

        private bool _focused;
        private bool _subscribed;
        private bool _initialized;
        private Color _normalColor = Color.white;
        private Color _focusColor = Color.white;
        private static int _focusedCount;
        private static int _escapeConsumedFrame = -1;

        /// <summary><c>true</c>, если хотя бы одно поле принимает ввод.</summary>
        public static bool HasFocus => _focusedCount > 0;

        /// <summary>Кадр, в котором Esc был потрачен на снятие фокуса: экраны не должны обрабатывать его повторно.</summary>
        public static int EscapeConsumedFrame => _escapeConsumedFrame;

        /// <summary>Текущий текст.</summary>
        public string Text => text != null ? text.text : string.Empty;

        /// <summary>Ставит фокус в поле, чтобы сразу принимать ввод.</summary>
        public void Focus() => SetFocused(true);

        /// <summary>
        /// Забирать ли Esc на снятие фокуса. Для поля в диалоге выключается, чтобы Esc закрывал диалог.
        /// </summary>
        public bool ConsumeEscape { get; set; } = true;

        /// <summary><c>true</c>, если поле сейчас в фокусе и принимает ввод.</summary>
        public bool IsFocused => _focused;

        /// <summary>Текст изменился: ввод с клавиатуры или установка значения из кода.</summary>
        public event Action<string>? Changed;

        /// <summary>Фокус поля изменился.</summary>
        public event Action<bool>? FocusChanged;

        /// <summary>Настраивает поле: текст, подсказку, фон и предел длины.</summary>
        /// <param name="text">Компонент текста.</param>
        /// <param name="placeholder">Подсказка при пустом поле.</param>
        /// <param name="background">Фон поля.</param>
        /// <param name="maxLength">Предел длины.</param>
        public void Configure(Text text, Text placeholder, Image background, int maxLength)
        {
            this.text = text;
            this.placeholder = placeholder;
            this.background = background;
            this.maxLength = maxLength < 1 ? 1 : maxLength;
            _initialized = true;
            _normalColor = background.color;
            _focusColor = Color.Lerp(_normalColor, Color.white, 0.18f);
            UpdateVisual();
        }

        /// <summary>Ставит текст, обрезая лишнее.</summary>
        /// <param name="value">Новый текст.</param>
        public void SetText(string value)
        {
            if (text == null)
            {
                return;
            }

            text.text = Clamp(value ?? string.Empty);
            UpdateVisual();
            Changed?.Invoke(text.text);
        }

        /// <inheritdoc />
        public void OnPointerClick(PointerEventData eventData) => SetFocused(true);

        private void Awake()
        {
            if (_initialized || text == null || placeholder == null || background == null)
            {
                return;
            }

            _initialized = true;
            _normalColor = background.color;
            _focusColor = Color.Lerp(_normalColor, Color.white, 0.18f);
            UpdateVisual();
        }

        private void OnEnable()
        {
            if (_subscribed)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                keyboard.onTextInput += OnTextInput;
                _subscribed = true;
            }
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                var keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    keyboard.onTextInput -= OnTextInput;
                }

                _subscribed = false;
            }

            SetFocused(false);
        }

        private void Update()
        {
            if (!_focused)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.backspaceKey.wasPressedThisFrame && Text.Length > 0)
            {
                SetText(Text.Substring(0, Text.Length - 1));
            }

            if (keyboard.enterKey.wasPressedThisFrame)
            {
                SetFocused(false);
            }

            if (keyboard.escapeKey.wasPressedThisFrame && ConsumeEscape)
            {
                _escapeConsumedFrame = Time.frameCount;
                SetFocused(false);
            }
        }

        private void OnTextInput(char symbol)
        {
            if (!_focused || char.IsControl(symbol))
            {
                return;
            }

            SetText(Text + symbol);
        }

        private void SetFocused(bool value)
        {
            if (_focused == value)
            {
                return;
            }

            _focused = value;
            _focusedCount = Mathf.Max(0, _focusedCount + (value ? 1 : -1));
            UpdateVisual();
            FocusChanged?.Invoke(value);
        }

        private void UpdateVisual()
        {
            if (text == null || placeholder == null || background == null)
            {
                return;
            }

            placeholder.gameObject.SetActive(text.text.Length == 0);
            background.color = _focused ? _focusColor : _normalColor;
        }

        private string Clamp(string value) => value.Length > maxLength ? value.Substring(0, maxLength) : value;
    }
}
