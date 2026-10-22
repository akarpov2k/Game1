#nullable enable
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>Доступ к видимым текстам интерфейса по ключу-перечислению.</summary>
    public static class UiText
    {
        /// <summary>Возвращает текст по ключу интерфейса.</summary>
        /// <param name="texts">Каталог текстов игры.</param>
        /// <param name="key">Ключ интерфейса.</param>
        /// <returns>Локализованный текст.</returns>
        public static string Get(IGameTexts texts, UiTextKey key) => texts.Get("ui." + SchemaEnum.ToCode(key));

        /// <summary>Возвращает текст с подстановкой аргументов.</summary>
        /// <param name="texts">Каталог текстов игры.</param>
        /// <param name="key">Ключ интерфейса.</param>
        /// <param name="args">Аргументы форматирования.</param>
        /// <returns>Готовый текст.</returns>
        public static string Format(IGameTexts texts, UiTextKey key, params object[] args) =>
            string.Format(texts.Get("ui." + SchemaEnum.ToCode(key)), args);
    }
}
