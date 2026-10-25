#nullable enable
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>Доступ к видимым текстам интерфейса по ключу-перечислению или по коду ключа.</summary>
    public static class UiText
    {
        /// <summary>Возвращает код ключа интерфейса в каталоге текстов.</summary>
        /// <param name="key">Ключ интерфейса.</param>
        /// <returns>Код вида <c>ui.newgame_start</c>.</returns>
        public static string Code(UiTextKey key) => "ui." + SchemaEnum.ToCode(key);

        /// <summary>Возвращает текст по ключу интерфейса.</summary>
        /// <param name="texts">Каталог текстов игры.</param>
        /// <param name="key">Ключ интерфейса.</param>
        /// <returns>Локализованный текст.</returns>
        public static string Get(IGameTexts texts, UiTextKey key) => texts.Get(Code(key));

        /// <summary>Возвращает текст по коду ключа из каталога.</summary>
        /// <param name="texts">Каталог текстов игры.</param>
        /// <param name="code">Код ключа вида <c>ui.newgame_start</c>.</param>
        /// <returns>Локализованный текст.</returns>
        public static string Get(IGameTexts texts, string code) => texts.Get(code);

        /// <summary>Возвращает текст с подстановкой аргументов.</summary>
        /// <param name="texts">Каталог текстов игры.</param>
        /// <param name="key">Ключ интерфейса.</param>
        /// <param name="args">Аргументы форматирования.</param>
        /// <returns>Готовый текст.</returns>
        public static string Format(IGameTexts texts, UiTextKey key, params object[] args) =>
            string.Format(texts.Get(Code(key)), args);
    }
}
