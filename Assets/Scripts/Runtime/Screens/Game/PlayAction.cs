#nullable enable
using System;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Runtime.Screens.Game
{
    /// <summary>Одно действие основного экрана: подпись, имя объекта и обработчик.</summary>
    public sealed class PlayAction
    {
        /// <summary>Создаёт действие.</summary>
        /// <param name="name">Имя объекта кнопки для тестов и отладки.</param>
        /// <param name="label">Видимая подпись.</param>
        /// <param name="invoke">Что выполнить по нажатию.</param>
        /// <exception cref="ArgumentNullException">Подпись или обработчик не заданы.</exception>
        public PlayAction(string name, string label, Action invoke)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(name)),
                    nameof(name));
            }

            Name = name;
            Label = label ?? throw new ArgumentNullException(nameof(label));
            Invoke = invoke ?? throw new ArgumentNullException(nameof(invoke));
        }

        /// <summary>Имя объекта кнопки.</summary>
        public string Name { get; }

        /// <summary>Видимая подпись.</summary>
        public string Label { get; }

        /// <summary>Обработчик нажатия.</summary>
        public Action Invoke { get; }
    }
}
