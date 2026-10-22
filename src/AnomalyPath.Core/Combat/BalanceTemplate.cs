#nullable enable
using AnomalyPath.Core.Json;

namespace AnomalyPath.Core.Combat
{
    /// <summary>Константы баланса, доступные формулам по имени.</summary>
    public sealed class BalanceTemplate
    {
        /// <summary>Создаёт набор констант.</summary>
        /// <param name="constants">Значения по именам.</param>
        public BalanceTemplate(System.Collections.Generic.IReadOnlyDictionary<string, double> constants)
        {
            Constants = constants;
        }

        /// <summary>Константы по именам.</summary>
        public System.Collections.Generic.IReadOnlyDictionary<string, double> Constants { get; }

        /// <summary>Пустой набор.</summary>
        public static BalanceTemplate Empty { get; } =
            new BalanceTemplate(new System.Collections.Generic.Dictionary<string, double>());

        /// <summary>Читает константы из записи.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <returns>Набор констант.</returns>
        public static BalanceTemplate FromJson(JsonValue value)
        {
            var constants = new System.Collections.Generic.Dictionary<string, double>(System.StringComparer.Ordinal);
            if (value.TryGet("constants", out var block) && block.IsObject)
            {
                foreach (var pair in block.Entries)
                {
                    if (pair.Value.Kind == JsonKind.Number)
                    {
                        constants[pair.Key] = pair.Value.Number;
                    }
                }
            }

            return new BalanceTemplate(constants);
        }
    }
}
