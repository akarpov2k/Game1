#nullable enable
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Menu
{
    /// <summary>
    /// Строка настройки сложности: подпись и числовое поле. Строки собираются из одной
    /// заготовки префаба, поэтому новая настройка — это ещё один экземпляр, а не новая разметка.
    /// </summary>
    public sealed class SettingRow : MonoBehaviour
    {
        [Tooltip("Подпись настройки")]
        [SerializeField]
        private Text label = null!;

        [Tooltip("Числовое поле настройки")]
        [SerializeField]
        private NumberField field = null!;

        /// <summary>Текущее значение настройки.</summary>
        public double Value => field.Value;

        /// <summary>Заполняет строку подписью и диапазоном значения.</summary>
        /// <param name="labelText">Готовая подпись из каталога текстов.</param>
        /// <param name="minimum">Минимум диапазона.</param>
        /// <param name="maximum">Максимум диапазона.</param>
        /// <param name="step">Шаг кнопок, больше нуля.</param>
        /// <param name="initial">Начальное значение.</param>
        public void Bind(string labelText, double minimum, double maximum, double step, double initial)
        {
            label.text = labelText;
            field.Configure(minimum, maximum, step, initial);
        }
    }
}
