#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;

namespace AnomalyPath.Core.Nicks
{
    /// <summary>Выбирает ник из списка детерминированно по seed.</summary>
    public sealed class NicknameGenerator : INicknameGenerator
    {
        private readonly List<string> _names;

        /// <summary>Создаёт генератор поверх списка ников.</summary>
        /// <param name="names">Ники; пустые строки пропускаются.</param>
        /// <exception cref="ArgumentNullException">Список не задан.</exception>
        public NicknameGenerator(IReadOnlyList<string> names)
        {
            if (names == null)
            {
                throw new ArgumentNullException(nameof(names));
            }

            _names = new List<string>(names.Count);
            foreach (var name in names)
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    _names.Add(name.Trim());
                }
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<string> Names => _names;

        /// <inheritdoc />
        public string Generate(int seed)
        {
            if (_names.Count == 0)
            {
                return string.Empty;
            }

            var index = (int)((uint)seed % (uint)_names.Count);
            return _names[index];
        }
    }
}
