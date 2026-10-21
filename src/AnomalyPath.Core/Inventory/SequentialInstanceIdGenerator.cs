#nullable enable
using System.Globalization;
using AnomalyPath.Core.Abstractions;

namespace AnomalyPath.Core.Inventory
{
    /// <summary>Последовательные id экземпляров: предсказуемо для тестов и отладки.</summary>
    public sealed class SequentialInstanceIdGenerator : IInstanceIdGenerator
    {
        private int _next;

        /// <summary>Создаёт генератор с начальным значением.</summary>
        /// <param name="start">Первое выдаваемое значение.</param>
        public SequentialInstanceIdGenerator(int start = 1)
        {
            _next = start;
        }

        /// <inheritdoc />
        public string Next()
        {
            var value = _next;
            _next++;
            return "inst_" + value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
