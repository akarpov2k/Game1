#nullable enable
using System.Collections.Generic;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Генератор ников игрока из данных: детерминированный по seed.</summary>
    public interface INicknameGenerator
    {
        /// <summary>Доступные ники из данных.</summary>
        IReadOnlyList<string> Names { get; }

        /// <summary>Возвращает ник по seed; пустой список — пустая строка.</summary>
        /// <param name="seed">Seed выбора.</param>
        /// <returns>Ник из данных.</returns>
        string Generate(int seed);
    }
}
