#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Talents
{
    /// <summary>Талант в дереве: место, предельный ранг, порог открытия и эффекты.</summary>
    public sealed class TalentNodeTemplate
    {
        /// <summary>Предельный ранг таланта: защитный предел разбора данных.</summary>
        public const int MaxRankLimit = 100;

        private readonly List<TalentEffectTemplate> _effects;

        /// <summary>Создаёт талант.</summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="titleKey">Ключ названия.</param>
        /// <param name="descriptionKey">Ключ описания.</param>
        /// <param name="row">Строка дерева, начиная с 0.</param>
        /// <param name="column">Колонка дерева, начиная с 0.</param>
        /// <param name="maxRank">Предельный ранг, от 1 до <see cref="MaxRankLimit"/>.</param>
        /// <param name="requiredPoints">Сколько очков нужно вложить в дерево до открытия.</param>
        /// <param name="effects">Эффекты за ранг; хотя бы один.</param>
        /// <param name="icon">Id картинки таланта из каталога спрайтов; пусто — рисуется заглушка.</param>
        /// <exception cref="ArgumentException">Строковые поля пусты или эффектов нет.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Числовые значения вне границ.</exception>
        public TalentNodeTemplate(
            string id,
            string titleKey,
            string descriptionKey,
            int row,
            int column,
            int maxRank,
            int requiredPoints,
            IReadOnlyList<TalentEffectTemplate> effects,
            string? icon = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(id)),
                    nameof(id));
            }

            if (string.IsNullOrWhiteSpace(titleKey))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(titleKey)),
                    nameof(titleKey));
            }

            if (string.IsNullOrWhiteSpace(descriptionKey))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(descriptionKey)),
                    nameof(descriptionKey));
            }

            if (row < 0 || column < 0 || maxRank < 1 || maxRank > MaxRankLimit || requiredPoints < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRank));
            }

            if (effects == null || effects.Count == 0)
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.TalentNoEffects, id),
                    nameof(effects));
            }

            Id = id;
            TitleKey = titleKey;
            DescriptionKey = descriptionKey;
            Row = row;
            Column = column;
            MaxRank = maxRank;
            RequiredPoints = requiredPoints;
            Icon = icon ?? string.Empty;
            _effects = new List<TalentEffectTemplate>(effects);
            Effects = _effects;
        }

        /// <summary>Идентификатор таланта.</summary>
        public string Id { get; }

        /// <summary>Ключ названия.</summary>
        public string TitleKey { get; }

        /// <summary>Ключ описания.</summary>
        public string DescriptionKey { get; }

        /// <summary>Строка дерева, начиная с 0.</summary>
        public int Row { get; }

        /// <summary>Колонка дерева, начиная с 0.</summary>
        public int Column { get; }

        /// <summary>Предельный ранг.</summary>
        public int MaxRank { get; }

        /// <summary>Сколько очков нужно вложить в дерево, чтобы талант открылся.</summary>
        public int RequiredPoints { get; }

        /// <summary>Id картинки таланта из каталога спрайтов; пусто — картинки нет.</summary>
        public string Icon { get; }

        /// <summary>Эффекты за один ранг.</summary>
        public IReadOnlyList<TalentEffectTemplate> Effects { get; }

        /// <summary>Читает талант из записи дерева.</summary>
        /// <param name="value">JSON-объект таланта.</param>
        /// <param name="path">Путь к файлу данных.</param>
        /// <returns>Талант.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static TalentNodeTemplate FromJson(JsonValue value, string path)
        {
            if (!value.IsObject)
            {
                throw new DataException(path, CoreErrors.MissingObjectField, "nodes");
            }

            var id = DataMap.RequireString(value, "id", path);
            var titleKey = DataMap.RequireString(value, "titleKey", path);
            var descriptionKey = DataMap.RequireString(value, "descriptionKey", path);
            var icon = DataMap.OptionalString(value, "icon") ?? string.Empty;
            var row = DataMap.RequireInt(value, "row", path);
            var column = DataMap.RequireInt(value, "column", path);
            if (row < 0 || column < 0)
            {
                throw new DataException(path, CoreErrors.TalentPosition, id);
            }

            var maxRank = DataMap.RequireInt(value, "maxRank", path);
            if (maxRank < 1 || maxRank > MaxRankLimit)
            {
                throw new DataException(path, CoreErrors.TalentMaxRank, id, MaxRankLimit, maxRank);
            }

            var requiredPoints = DataMap.RequireInt(value, "requiredPoints", path);
            if (requiredPoints < 0)
            {
                throw new DataException(path, CoreErrors.TalentGate, id, requiredPoints, 0);
            }

            if (!value.TryGet("effects", out var effectsValue) || !effectsValue.IsArray || effectsValue.Items.Count == 0)
            {
                throw new DataException(path, CoreErrors.TalentNoEffects, id);
            }

            var effects = new List<TalentEffectTemplate>();
            var stats = new HashSet<TalentStat>();
            foreach (var item in effectsValue.Items)
            {
                var effect = TalentEffectTemplate.FromJson(item, id, path);
                if (!stats.Add(effect.Stat))
                {
                    throw new DataException(
                        path,
                        CoreErrors.TalentEffectDuplicate,
                        id,
                        TalentStatCodec.ToCode(effect.Stat));
                }

                effects.Add(effect);
            }

            return new TalentNodeTemplate(
                id,
                titleKey,
                descriptionKey,
                row,
                column,
                maxRank,
                requiredPoints,
                effects,
                icon);
        }
    }
}
