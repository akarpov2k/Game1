#nullable enable
namespace AnomalyPath.Core.Json
{
    /// <summary>Вид значения в мини-домене JSON.</summary>
    public enum JsonKind
    {
        /// <summary>Отсутствующее значение.</summary>
        Null,

        /// <summary>Логическое значение.</summary>
        Bool,

        /// <summary>Число.</summary>
        Number,

        /// <summary>Строка.</summary>
        String,

        /// <summary>Массив.</summary>
        Array,

        /// <summary>Объект.</summary>
        Object,
    }
}
