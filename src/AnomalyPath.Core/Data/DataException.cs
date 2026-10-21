using System;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Data
{
    /// <summary>
    /// Ошибка загрузки или валидации данных. Текст берётся из каталога ресурсов по коду,
    /// поэтому сообщения локализуются вместе с игрой.
    /// </summary>
    public sealed class DataException : Exception
    {
        private readonly string _reason;

        /// <summary>Создаёт ошибку с путём, кодом и аргументами сообщения.</summary>
        /// <param name="path">Путь к файлу или id сущности.</param>
        /// <param name="code">Код из <see cref="CoreErrors"/>.</param>
        /// <param name="args">Аргументы подстановки в текст.</param>
        public DataException(string path, string code, params object[] args)
            : base($"{path}: {CoreText.Format(code, args)}")
        {
            Path = path;
            Code = code;
            _reason = CoreText.Format(code, args);
        }

        /// <summary>Путь к файлу или id сущности.</summary>
        public string Path { get; }

        /// <summary>Код сообщения из каталога ресурсов.</summary>
        public string Code { get; }

        /// <summary>Готовое сообщение без префикса пути.</summary>
        public string Reason => _reason;
    }
}
