#nullable enable
using AnomalyPath.Core.Abstractions;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>
    /// Статичная подпись экрана: ключ текста задаётся в инспекторе, значение берётся
    /// из каталога локализации при привязке экрана. В коде экрана текста нет.
    /// </summary>
    public sealed class LocalizedText : MonoBehaviour
    {
        [Tooltip("Компонент текста; пусто — берётся с этого объекта")]
        [SerializeField]
        private Text? label = null;

        [Tooltip("Ключ текста из strings.{ru,en}.json")]
        [SerializeField]
        private UiTextKey key = UiTextKey.MainMenu_Game_Title;

        /// <summary>Подставляет текст по ключу из каталога.</summary>
        /// <param name="texts">Каталог текстов игры.</param>
        public void Apply(IGameTexts texts)
        {
            if (label == null)
            {
                label = GetComponent<Text>();
            }

            if (label != null)
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
