#nullable enable
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Combat
{
    /// <summary>Настройки боя из единственной записи вида <c>combat</c>.</summary>
    public sealed class CombatSettingsTemplate
    {
        /// <summary>Создаёт настройки боя.</summary>
        /// <param name="damageFormulaId">Формула урона.</param>
        /// <param name="clampMin">Нижняя граница результата по умолчанию.</param>
        /// <param name="clampMax">Верхняя граница результата по умолчанию.</param>
        /// <param name="apRestore">Режим восстановления ОД.</param>
        /// <param name="apPerTurn">Сколько ОД восстанавливается при частичном режиме.</param>
        /// <param name="initiative">Правило инициативы.</param>
        /// <param name="initiativeFormulaId">Формула инициативы, если она включена.</param>
        /// <param name="zoneShiftFree">Сколько зон можно пройти бесплатно за ход.</param>
        /// <param name="zoneShiftApCost">Цена сдвига сверх бесплатного.</param>
        /// <param name="attackApCost">Цена атаки в ОД.</param>
        /// <param name="accuracyClose">Множитель меткости вплотную.</param>
        /// <param name="accuracyMid">Множитель меткости на средней дистанции.</param>
        /// <param name="accuracyFar">Множитель меткости на дальней дистанции.</param>
        /// <param name="reloadApCost">Цена перезарядки в ОД.</param>
        public CombatSettingsTemplate(
            string damageFormulaId,
            double? clampMin,
            double? clampMax,
            string apRestore,
            int apPerTurn,
            string initiative,
            string? initiativeFormulaId,
            int zoneShiftFree,
            int zoneShiftApCost,
            int attackApCost,
            double accuracyClose,
            double accuracyMid,
            double accuracyFar,
            int reloadApCost)
        {
            DamageFormulaId = damageFormulaId;
            ClampMin = clampMin;
            ClampMax = clampMax;
            ApRestore = apRestore;
            ApPerTurn = apPerTurn;
            Initiative = initiative;
            InitiativeFormulaId = initiativeFormulaId;
            ZoneShiftFree = zoneShiftFree;
            ZoneShiftApCost = zoneShiftApCost;
            AttackApCost = attackApCost;
            AccuracyClose = accuracyClose;
            AccuracyMid = accuracyMid;
            AccuracyFar = accuracyFar;
            ReloadApCost = reloadApCost;
        }

        /// <summary>Цена перезарядки в ОД.</summary>
        public int ReloadApCost { get; }

        /// <summary>Множитель меткости вплотную.</summary>
        public double AccuracyClose { get; }

        /// <summary>Множитель меткости на средней дистанции.</summary>
        public double AccuracyMid { get; }

        /// <summary>Множитель меткости на дальней дистанции.</summary>
        public double AccuracyFar { get; }

        /// <summary>Формула урона.</summary>
        public string DamageFormulaId { get; }

        /// <summary>Нижняя граница результата по умолчанию.</summary>
        public double? ClampMin { get; }

        /// <summary>Верхняя граница результата по умолчанию.</summary>
        public double? ClampMax { get; }

        /// <summary>Режим восстановления ОД: <c>full</c>, <c>partial</c>, <c>none</c>.</summary>
        public string ApRestore { get; }

        /// <summary>Сколько ОД восстанавливается при частичном режиме.</summary>
        public int ApPerTurn { get; }

        /// <summary>Правило инициативы: <c>playerFirst</c> или <c>formula</c>.</summary>
        public string Initiative { get; }

        /// <summary>Формула инициативы.</summary>
        public string? InitiativeFormulaId { get; }

        /// <summary>Сколько зон можно пройти бесплатно за ход.</summary>
        public int ZoneShiftFree { get; }

        /// <summary>Цена сдвига сверх бесплатного в ОД.</summary>
        public int ZoneShiftApCost { get; }

        /// <summary>Цена атаки в ОД.</summary>
        public int AttackApCost { get; }

        /// <summary>Настройки по умолчанию, если файла нет.</summary>
        public static CombatSettingsTemplate Default { get; } = new CombatSettingsTemplate(
            "formula_damage_base", 0, null, "full", 0, "playerFirst", null, 1, 1, 1, 1.4, 1.0, 0.8, 1);

        /// <summary>Читает настройки боя из записи.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Настройки.</returns>
        /// <exception cref="DataException">Поля заданы неверно.</exception>
        public static CombatSettingsTemplate FromJson(JsonValue value, string sourcePath)
        {
            var damageFormulaId = DataMap.OptionalString(value, "damageFormulaId") ?? "formula_damage_base";
            var apRestore = DataMap.OptionalString(value, "apRestore") ?? "full";
            if (apRestore is not ("full" or "partial" or "none"))
            {
                throw new DataException(sourcePath, CoreErrors.MissingStringField, "apRestore");
            }

            var initiative = DataMap.OptionalString(value, "initiative") ?? "playerFirst";
            if (initiative is not ("playerFirst" or "formula"))
            {
                throw new DataException(sourcePath, CoreErrors.MissingStringField, "initiative");
            }

            return new CombatSettingsTemplate(
                damageFormulaId,
                DataMap.OptionalDouble(value, "clampMin"),
                DataMap.OptionalDouble(value, "clampMax"),
                apRestore,
                DataMap.OptionalInt(value, "apPerTurn", 0),
                initiative,
                DataMap.OptionalString(value, "initiativeFormulaId"),
                DataMap.OptionalInt(value, "zoneShiftFree", 1),
                DataMap.OptionalInt(value, "zoneShiftApCost", 1),
                DataMap.OptionalInt(value, "attackApCost", 1),
                DataMap.OptionalDouble(value, "accuracyClose") ?? 1.4,
                DataMap.OptionalDouble(value, "accuracyMid") ?? 1.0,
                DataMap.OptionalDouble(value, "accuracyFar") ?? 0.8,
                DataMap.OptionalInt(value, "reloadApCost", 1));
        }
    }

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
