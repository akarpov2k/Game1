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
        /// <param name="xpFormulaId">Формула опыта за победу.</param>
        /// <param name="clampMin">Нижняя граница результата по умолчанию.</param>
        /// <param name="clampMax">Верхняя граница результата по умолчанию.</param>
        /// <param name="apRestore">Режим восстановления ОД.</param>
        /// <param name="apPerTurn">Сколько ОД восстанавливается при частичном режиме.</param>
        /// <param name="zoneShiftFree">Сколько зон можно пройти бесплатно за ход.</param>
        /// <param name="zoneShiftApCost">Цена сдвига сверх бесплатного.</param>
        /// <param name="attackApCost">Цена атаки в ОД.</param>
        /// <param name="accuracyClose">Множитель меткости вплотную.</param>
        /// <param name="accuracyMid">Множитель меткости на средней дистанции.</param>
        /// <param name="accuracyFar">Множитель меткости на дальней дистанции.</param>
        /// <param name="reloadApCost">Цена перезарядки в ОД.</param>
        public CombatSettingsTemplate(
            string damageFormulaId,
            string xpFormulaId,
            double? clampMin,
            double? clampMax,
            ApRestoreKind apRestore,
            int apPerTurn,
            int zoneShiftFree,
            int zoneShiftApCost,
            int attackApCost,
            double accuracyClose,
            double accuracyMid,
            double accuracyFar,
            int reloadApCost)
        {
            DamageFormulaId = damageFormulaId;
            XpFormulaId = xpFormulaId;
            ClampMin = clampMin;
            ClampMax = clampMax;
            ApRestore = apRestore;
            ApPerTurn = apPerTurn;
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

        /// <summary>Формула опыта за победу.</summary>
        public string XpFormulaId { get; }

        /// <summary>Нижняя граница результата по умолчанию.</summary>
        public double? ClampMin { get; }

        /// <summary>Верхняя граница результата по умолчанию.</summary>
        public double? ClampMax { get; }

        /// <summary>Режим восстановления ОД.</summary>
        public ApRestoreKind ApRestore { get; }

        /// <summary>Сколько ОД восстанавливается при частичном режиме.</summary>
        public int ApPerTurn { get; }

        /// <summary>Сколько зон можно пройти бесплатно за ход.</summary>
        public int ZoneShiftFree { get; }

        /// <summary>Цена сдвига сверх бесплатного в ОД.</summary>
        public int ZoneShiftApCost { get; }

        /// <summary>Цена атаки в ОД.</summary>
        public int AttackApCost { get; }

        /// <summary>Читает настройки боя из записи.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Настройки.</returns>
        /// <exception cref="DataException">Поля заданы неверно.</exception>
        public static CombatSettingsTemplate FromJson(JsonValue value, string sourcePath)
        {
            var damageFormulaId = DataMap.RequireString(value, "damageFormulaId", sourcePath);
            var xpFormulaId = DataMap.RequireString(value, "xpFormulaId", sourcePath);

            var apRestoreCode = DataMap.RequireString(value, "apRestore", sourcePath);
            if (!SchemaEnum.TryParse(apRestoreCode, out ApRestoreKind apRestore))
            {
                throw new DataException(sourcePath, CoreErrors.UnknownApRestore, apRestoreCode);
            }

            return new CombatSettingsTemplate(
                damageFormulaId,
                xpFormulaId,
                DataMap.OptionalDouble(value, "clampMin"),
                DataMap.OptionalDouble(value, "clampMax"),
                apRestore,
                DataMap.OptionalInt(value, "apPerTurn", 0),
                DataMap.RequireInt(value, "zoneShiftFree", sourcePath),
                DataMap.RequireInt(value, "zoneShiftApCost", sourcePath),
                DataMap.RequireInt(value, "attackApCost", sourcePath),
                DataMap.RequireDouble(value, "accuracyClose", sourcePath),
                DataMap.RequireDouble(value, "accuracyMid", sourcePath),
                DataMap.RequireDouble(value, "accuracyFar", sourcePath),
                DataMap.RequireInt(value, "reloadApCost", sourcePath));
        }
    }
}
