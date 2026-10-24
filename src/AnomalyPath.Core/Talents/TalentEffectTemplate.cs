#nullable enable
using System;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Talents
{
    /// <summary>Эффект таланта: характеристика и прибавка за один ранг.</summary>
    public sealed class TalentEffectTemplate
    {
        /// <summary>Создаёт эффект таланта.</summary>
        /// <param name="stat">Характеристика.</param>
        /// <param name="perRank">Прибавка за ранг; больше нуля.</param>
        /// <exception cref="ArgumentOutOfRangeException">Прибавка не положительна.</exception>
        public TalentEffectTemplate(TalentStat stat, double perRank)
        {
            if (perRank <= 0 || (TalentStatCodec.IsInteger(stat) && Math.Abs(perRank - Math.Round(perRank)) > 1e-9))
            {
                throw new ArgumentOutOfRangeException(nameof(perRank));
            }

            Stat = stat;
            PerRank = perRank;
        }

        /// <summary>Характеристика.</summary>
        public TalentStat Stat { get; }

        /// <summary>Прибавка за один ранг.</summary>
        public double PerRank { get; }

        /// <summary>Читает эффект из записи таланта.</summary>
        /// <param name="value">JSON-объект эффекта.</param>
        /// <param name="talentId">Идентификатор таланта для сообщения.</param>
        /// <param name="path">Путь к файлу данных.</param>
        /// <returns>Эффект.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static TalentEffectTemplate FromJson(JsonValue value, string talentId, string path)
        {
            if (!value.IsObject)
            {
                throw new DataException(path, CoreErrors.MissingObjectField, "effects");
            }

            var statCode = DataMap.RequireString(value, "stat", path);
            if (!TalentStatCodec.TryParse(statCode, out var stat))
            {
                throw new DataException(path, CoreErrors.UnknownTalentStat, statCode);
            }

            var perRank = DataMap.RequireDouble(value, "perRank", path);
            if (perRank <= 0 || !IsValidValue(stat, perRank))
            {
                throw new DataException(path, CoreErrors.TalentEffectValue, talentId, statCode);
            }

            return new TalentEffectTemplate(stat, perRank);
        }

        private static bool IsValidValue(TalentStat stat, double value)
        {
            if (!TalentStatCodec.IsInteger(stat))
            {
                return true;
            }

            var rounded = Math.Round(value);
            return Math.Abs(value - rounded) < 1e-9;
        }
    }
}
