#nullable enable
using AnomalyPath.Core.Abstractions;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>
    /// Статичная подпись экрана: ключ текста задаётся в инспекторе строкой, значение берётся
    /// из каталога локализации при привязке экрана. В коде экрана текста нет.
    /// </summary>
    /// <remarks>
    /// Ключ хранится кодом из каталога (<c>ui.newgame_start</c>), а не номером члена
    /// <see cref="UiTextKey"/>: номер сменился бы от любой вставки в перечисление, и подпись
    /// в префабе начала бы показывать чужой текст.
    /// </remarks>
    public sealed class LocalizedText : MonoBehaviour
    {
        [Tooltip("Компонент текста; пусто — берётся с этого объекта")]
        [SerializeField]
        private Text? label = null;

        [Tooltip("Код ключа из strings.{ru,en}.json, например ui.newgame_start")]
        [SerializeField]
        private string key = string.Empty;

        /// <summary>Код ключа текста в каталоге строк.</summary>
        public string Key
        {
            get => key;
            set => key = value;
        }

        /// <summary>Подставляет текст по ключу из каталога.</summary>
        /// <param name="texts">Каталог текстов игры.</param>
        public void Apply(IGameTexts texts)
        {
            if (label == null)
            {
                label = GetComponent<Text>();
            }

            if (label != null && !string.IsNullOrEmpty(key))
            {
                label.text = UiText.Get(texts, key);
            }
        }

        private void Reset()
        {
            label = GetComponent<Text>();
        }

        private void OnValidate()
        {
            if (label == null)
            {
                label = GetComponent<Text>();
            }
        }
    }
}
