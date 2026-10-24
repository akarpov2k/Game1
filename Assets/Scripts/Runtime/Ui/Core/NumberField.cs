#nullable enable
using System;
using System.Globalization;
using AnomalyPath.Core.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>
    /// Числовое поле: цифровой ввод и кнопки увеличения и уменьшения значения с заданным шагом.
    /// Пока игрок печатает, недописанное число не трогается; при уходе фокуса текст нормализуется,
    /// а значение всегда остаётся в пределах диапазона.
    /// </summary>
    public sealed class NumberField : MonoBehaviour
    {
        [Tooltip("Поле ввода цифр")]
        [SerializeField]
        private TextInputField? input = null;

        [Tooltip("Кнопка увеличения значения")]
        [SerializeField]
        private Button? increase = null;

        [Tooltip("Кнопка уменьшения значения")]
        [SerializeField]
        private Button? decrease = null;

        [Tooltip("Минимум диапазона")]
        [SerializeField]
        private double min = 0;

        [Tooltip("Максимум диапазона")]
        [SerializeField]
        private double max = 1;

        [Tooltip("Шаг кнопок, больше нуля")]
        [SerializeField]
        private double step = 0.1;

        [SerializeField]
        private double value = 0;

        private bool _syncing;
        private bool _subscribed;

        /// <summary>Текущее значение в допустимом диапазоне.</summary>
        public double Value => Clamp(value);

        /// <summary>Минимум диапазона.</summary>
        public double Minimum => min;

        /// <summary>Максимум диапазона.</summary>
        public double Maximum => max;

        /// <summary>Шаг кнопок.</summary>
        public double Step => step;

        /// <summary>Настраивает поле из префаба: диапазон, шаг и значение.</summary>
        /// <param name="minimum">Минимум диапазона.</param>
        /// <param name="maximum">Максимум диапазона.</param>
        /// <param name="step">Шаг кнопок, больше нуля.</param>
        /// <param name="initial">Начальное значение.</param>
        /// <exception cref="ArgumentOutOfRangeException">Максимум меньше минимума или шаг не положительный.</exception>
        public void Configure(double minimum, double maximum, double step, double initial)
        {
            Validate(minimum, maximum, step);
            min = minimum;
            max = maximum;
            this.step = step;
            value = Clamp(initial);
            EnsureSubscribed();
            Sync();
        }

        /// <summary>Настраивает поле: диапазон, шаг, начальное значение и подписки на кнопки.</summary>
        /// <param name="input">Поле ввода цифр.</param>
        /// <param name="increase">Кнопка увеличения.</param>
        /// <param name="decrease">Кнопка уменьшения.</param>
        /// <param name="min">Минимум диапазона.</param>
        /// <param name="max">Максимум диапазона.</param>
        /// <param name="step">Шаг кнопок, больше нуля.</param>
        /// <param name="value">Начальное значение.</param>
        /// <exception cref="ArgumentNullException">Виджет не задан.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Максимум меньше минимума или шаг не положительный.</exception>
        public void Configure(
            TextInputField input,
            Button increase,
            Button decrease,
            double min,
            double max,
            double step,
            double value)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (increase == null)
            {
                throw new ArgumentNullException(nameof(increase));
            }

            if (decrease == null)
            {
                throw new ArgumentNullException(nameof(decrease));
            }

            Validate(min, max, step);
            this.input = input;
            this.increase = increase;
            this.decrease = decrease;
            this.min = min;
            this.max = max;
            this.step = step;
            this.value = Clamp(value);

            EnsureSubscribed();
            Sync();
        }

        /// <summary>Увеличивает значение на шаг.</summary>
        public void Increase() => SetValue(Value + step);

        /// <summary>Уменьшает значение на шаг.</summary>
        public void Decrease() => SetValue(Value - step);

        /// <summary>Ставит значение, округляя до шага и зажимая в диапазон.</summary>
        /// <param name="value">Новое значение.</param>
        public void SetValue(double value)
        {
            this.value = Clamp(Snap(value));
            Sync();
        }

        private void Awake()
        {
            EnsureSubscribed();
            Sync();
        }

        private void EnsureSubscribed()
        {
            if (_subscribed || input == null || increase == null || decrease == null)
            {
                return;
            }

            input.Changed += OnTextChanged;
            input.FocusChanged += OnFocusChanged;
            increase.onClick.AddListener(Increase);
            decrease.onClick.AddListener(Decrease);
            _subscribed = true;
        }

        private static void Validate(double minimum, double maximum, double step)
        {
            if (maximum < minimum)
            {
                throw new ArgumentOutOfRangeException(nameof(maximum), CoreText.Get(CoreErrors.RangeOrder));
            }

            if (step <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(step), CoreText.Get(CoreErrors.StepPositive));
            }
        }

        private void Sync()
        {
            if (input == null)
            {
                return;
            }

            _syncing = true;
            input.SetText(Format(Value));
            _syncing = false;
        }

        private void OnTextChanged(string text)
        {
            if (_syncing || input == null)
            {
                return;
            }

            var normalized = (text ?? string.Empty).Replace(',', '.');
            if (!IsPartialNumber(normalized))
            {
                Sync();
                return;
            }

            if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                value = Clamp(parsed);
            }
        }

        private void OnFocusChanged(bool focused)
        {
            if (!focused)
            {
                Sync();
            }
        }

        private double Snap(double value) => min + (Math.Round((value - min) / step) * step);

        private double Clamp(double value)
        {
            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
        }

        private string Format(double value)
        {
            var decimals = 0;
            var stepScale = step;
            while (decimals < 4 && Math.Abs(stepScale - Math.Round(stepScale)) > 1e-9)
            {
                stepScale *= 10;
                decimals++;
            }

            return value.ToString("F" + decimals.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        }

        private static bool IsPartialNumber(string text)
        {
            var separators = 0;
            for (var index = 0; index < text.Length; index++)
            {
                var symbol = text[index];
                if (symbol >= '0' && symbol <= '9')
                {
                    continue;
                }

                if (symbol == '-' && index == 0)
                {
                    continue;
                }

                if (symbol == '.')
                {
                    separators++;
                    if (separators > 1)
                    {
                        return false;
                    }

                    continue;
                }

                return false;
            }

            return true;
        }
    }
}
