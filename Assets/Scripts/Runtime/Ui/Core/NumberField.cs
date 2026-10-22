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
        private TextInputField _input = null!;
        private double _min;
        private double _max;
        private double _step = 0.1;
        private double _value;
        private bool _syncing;

        /// <summary>Текущее значение в допустимом диапазоне.</summary>
        public double Value => Clamp(_value);

        /// <summary>Минимум диапазона.</summary>
        public double Minimum => _min;

        /// <summary>Максимум диапазона.</summary>
        public double Maximum => _max;

        /// <summary>Шаг кнопок.</summary>
        public double Step => _step;

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

            if (max < min)
            {
                throw new ArgumentOutOfRangeException(nameof(max), CoreText.Get(CoreErrors.RangeOrder));
            }

            if (step <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(step), CoreText.Get(CoreErrors.StepPositive));
            }

            _input = input;
            _min = min;
            _max = max;
            _step = step;
            _value = Clamp(value);

            input.Changed += OnTextChanged;
            input.FocusChanged += OnFocusChanged;
            increase.onClick.AddListener(Increase);
            decrease.onClick.AddListener(Decrease);
            Sync();
        }

        /// <summary>Увеличивает значение на шаг.</summary>
        public void Increase() => SetValue(Value + _step);

        /// <summary>Уменьшает значение на шаг.</summary>
        public void Decrease() => SetValue(Value - _step);

        /// <summary>Ставит значение, округляя до шага и зажимая в диапазон.</summary>
        /// <param name="value">Новое значение.</param>
        public void SetValue(double value)
        {
            _value = Clamp(Snap(value));
            Sync();
        }

        private void Sync()
        {
            if (_input == null)
            {
                return;
            }

            _syncing = true;
            _input.SetText(Format(Value));
            _syncing = false;
        }

        private void OnTextChanged(string text)
        {
            if (_syncing || _input == null)
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
                _value = Clamp(parsed);
            }
        }

        private void OnFocusChanged(bool focused)
        {
            if (!focused)
            {
                Sync();
            }
        }

        private double Snap(double value) => _min + (Math.Round((value - _min) / _step) * _step);

        private double Clamp(double value)
        {
            if (value < _min)
            {
                return _min;
            }

            return value > _max ? _max : value;
        }

        private string Format(double value)
        {
            var decimals = 0;
            var stepScale = _step;
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
