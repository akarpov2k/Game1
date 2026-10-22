#nullable enable
using System;

namespace AnomalyPath.Core.Data
{
    /// <summary>
    /// Соглашение «значение схемы ↔ enum»: код в JSON — имя члена в нижнем регистре.
    /// Словари допустимых значений живут в типах-перечислениях, а не в списках строк.
    /// </summary>
    public static class SchemaEnum
    {
        /// <summary>Разбирает код схемы и проверяет, что значение определено.</summary>
        /// <typeparam name="T">Тип перечисления схемы.</typeparam>
        /// <param name="code">Текст значения из JSON.</param>
        /// <param name="value">Разобранное значение.</param>
        /// <returns><c>true</c>, если код канонический и значение известно.</returns>
        public static bool TryParse<T>(string code, out T value)
            where T : struct, Enum
        {
            value = default;
            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            if (!Enum.TryParse(code, true, out T parsed) || !Enum.IsDefined(typeof(T), parsed))
            {
                return false;
            }

            if (!string.Equals(ToCode(parsed), code, StringComparison.Ordinal))
            {
                return false;
            }

            value = parsed;
            return true;
        }

        /// <summary>Возвращает канонический код значения для JSON.</summary>
        /// <typeparam name="T">Тип перечисления схемы.</typeparam>
        /// <param name="value">Значение.</param>
        /// <returns>Имя члена в нижнем регистре.</returns>
        public static string ToCode<T>(T value)
            where T : struct, Enum => value.ToString().ToLowerInvariant();
    }
}
