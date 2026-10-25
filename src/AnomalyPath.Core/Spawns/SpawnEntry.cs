#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Spawns
{
    /// <summary>
    /// Запись спавна: где может появиться объект локации, какой он и с каким весом.
    /// Фильтр — теги локации, тир и запрет уникальных в случайном выборе.
    /// </summary>
    public sealed class SpawnEntry
    {
        /// <summary>Создаёт запись спавна.</summary>
        /// <param name="id">Идентификатор записи.</param>
        /// <param name="titleKey">Ключ названия в каталоге строк.</param>
        /// <param name="objectKind">Вид объекта.</param>
        /// <param name="requiresTags">Все теги обязаны быть у локации.</param>
        /// <param name="excludesTags">Ни одного из тегов быть не должно.</param>
        /// <param name="weight">Относительный вес в пуле.</param>
        /// <param name="minTier">Минимальный тир локации.</param>
        /// <param name="maxTier">Максимальный тир локации.</param>
        /// <param name="lootTableId">Таблица лута для объекта лута.</param>
        /// <param name="eventId">Событие, прикреплённое к объекту.</param>
        /// <param name="actorId">Актёр, появляющийся вместе с NPC-объектом.</param>
        /// <param name="unique">Квестовый объект: не выбирается случайно.</param>
        /// <exception cref="ArgumentException">Обязательные строки пусты.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Вес или тир вне границ.</exception>
        public SpawnEntry(
            string id,
            string titleKey,
            LocationObjectKind objectKind,
            IReadOnlyList<LocationTag> requiresTags,
            IReadOnlyList<LocationTag> excludesTags,
            double weight,
            int minTier,
            int maxTier,
            string? lootTableId = null,
            string? eventId = null,
            string? actorId = null,
            bool unique = false)
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

            if (weight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(weight));
            }

            if (minTier < 0 || maxTier < minTier)
            {
                throw new ArgumentOutOfRangeException(nameof(minTier));
            }

            Id = id;
            TitleKey = titleKey;
            ObjectKind = objectKind;
            RequiresTags = requiresTags ?? Array.Empty<LocationTag>();
            ExcludesTags = excludesTags ?? Array.Empty<LocationTag>();
            Weight = weight;
            MinTier = minTier;
            MaxTier = maxTier;
            LootTableId = lootTableId;
            EventId = eventId;
            ActorId = actorId;
            Unique = unique;
        }

        /// <summary>Идентификатор записи.</summary>
        public string Id { get; }

        /// <summary>Ключ названия в каталоге строк.</summary>
        public string TitleKey { get; }

        /// <summary>Вид объекта.</summary>
        public LocationObjectKind ObjectKind { get; }

        /// <summary>Все теги обязаны быть у локации.</summary>
        public IReadOnlyList<LocationTag> RequiresTags { get; }

        /// <summary>Ни одного из тегов не должно быть у локации.</summary>
        public IReadOnlyList<LocationTag> ExcludesTags { get; }

        /// <summary>Относительный вес в пуле объектов.</summary>
        public double Weight { get; }

        /// <summary>Минимальный тир локации.</summary>
        public int MinTier { get; }

        /// <summary>Максимальный тир локации.</summary>
        public int MaxTier { get; }

        /// <summary>Таблица лута объекта лута; у остальных видов <c>null</c>.</summary>
        public string? LootTableId { get; }

        /// <summary>Событие, прикреплённое к объекту.</summary>
        public string? EventId { get; }

        /// <summary>Актёр NPC-объекта; у остальных видов <c>null</c>.</summary>
        public string? ActorId { get; }

        /// <summary>Квестовый объект: не выбирается случайно.</summary>
        public bool Unique { get; }

        /// <summary>Подходит ли запись для локации с указанными тегами и тиром.</summary>
        /// <param name="locationTags">Теги локации.</param>
        /// <param name="tier">Тир локации.</param>
        /// <param name="subtype">Подтип локации в маршруте.</param>
        /// <returns><c>true</c>, если объект может появиться на локации.</returns>
        /// <exception cref="ArgumentNullException">Теги не заданы.</exception>
        public bool Matches(IReadOnlyList<LocationTag> locationTags, int tier, LocationSubtype subtype)
        {
            if (locationTags == null)
            {
                throw new ArgumentNullException(nameof(locationTags));
            }

            if (subtype == LocationSubtype.Hub || Unique)
            {
                return false;
            }

            if (tier < MinTier || tier > MaxTier)
            {
                return false;
            }

            foreach (var tag in ExcludesTags)
            {
                if (Contains(locationTags, tag))
                {
                    return false;
                }
            }

            foreach (var tag in RequiresTags)
            {
                if (!Contains(locationTags, tag))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Создаёт объект локации по этой записи.</summary>
        /// <returns>Объект NPC, тайника или лута.</returns>
        /// <exception cref="InvalidOperationException">У записи нет данных своего вида.</exception>
        public LocationObject CreateObject()
        {
            switch (ObjectKind)
            {
                case LocationObjectKind.Loot:
                    if (string.IsNullOrWhiteSpace(LootTableId))
                    {
                        throw new InvalidOperationException(
                            CoreText.Format(CoreErrors.RequiredField, nameof(LootTableId)));
                    }

                    return new LootLocationObject(Id, TitleKey, LootTableId!);

                case LocationObjectKind.Stash:
                    return new StashLocationObject(Id, TitleKey);

                case LocationObjectKind.Npc:
                    if (string.IsNullOrWhiteSpace(ActorId))
                    {
                        throw new InvalidOperationException(
                            CoreText.Format(CoreErrors.RequiredField, nameof(ActorId)));
                    }

                    return new NpcLocationObject(Id, TitleKey, ActorId!);

                default:
                    throw new InvalidOperationException(
                        CoreText.Format(CoreErrors.SpawnUnknownObjectKind, ObjectKind));
            }
        }

        /// <summary>Читает запись спавна из JSON.</summary>
        /// <param name="value">Объект сущности.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Разобранная запись.</returns>
        /// <exception cref="DataException">Поля отсутствуют, значения неизвестны или вне границ.</exception>
        public static SpawnEntry FromJson(JsonValue value, string sourcePath)
        {
            var id = DataMap.RequireString(value, "id", sourcePath);
            var titleKey = DataMap.RequireString(value, "titleKey", sourcePath);

            var kindCode = DataMap.RequireString(value, "objectKind", sourcePath);
            if (!SchemaEnum.TryParse(kindCode, out LocationObjectKind objectKind))
            {
                throw new DataException(sourcePath, CoreErrors.SpawnUnknownObjectKind, kindCode);
            }

            var requiresTags = ReadTags(value, "requiresTags", sourcePath);
            var excludesTags = ReadTags(value, "excludesTags", sourcePath);
            foreach (var tag in requiresTags)
            {
                foreach (var excluded in excludesTags)
                {
                    if (tag == excluded)
                    {
                        throw new DataException(sourcePath, CoreErrors.SpawnTagConflict, SchemaEnum.ToCode(tag));
                    }
                }
            }

            var weight = DataMap.RequireDouble(value, "weight", sourcePath);
            if (weight <= 0)
            {
                throw new DataException(sourcePath, CoreErrors.WorldRange, "weight");
            }

            var minTier = DataMap.OptionalInt(value, "minTier", 0);
            var maxTier = DataMap.OptionalInt(value, "maxTier", int.MaxValue);
            if (minTier < 0 || maxTier < minTier)
            {
                throw new DataException(sourcePath, CoreErrors.WorldRange, "tier");
            }

            var lootTableId = DataMap.OptionalString(value, "lootTable");
            var eventId = DataMap.OptionalString(value, "eventId");
            var actorId = DataMap.OptionalString(value, "actorId");
            var unique = DataMap.OptionalBool(value, "unique", false);

            if (objectKind == LocationObjectKind.Loot && string.IsNullOrWhiteSpace(lootTableId))
            {
                throw new DataException(sourcePath, CoreErrors.MissingStringField, "lootTable");
            }

            if (objectKind == LocationObjectKind.Npc && string.IsNullOrWhiteSpace(actorId))
            {
                throw new DataException(sourcePath, CoreErrors.MissingStringField, "actorId");
            }

            return new SpawnEntry(
                id,
                titleKey,
                objectKind,
                requiresTags,
                excludesTags,
                weight,
                minTier,
                maxTier,
                lootTableId,
                eventId,
                actorId,
                unique);
        }

        private static IReadOnlyList<LocationTag> ReadTags(JsonValue value, string field, string sourcePath)
        {
            var result = new List<LocationTag>();
            foreach (var code in DataMap.OptionalStringArray(value, field))
            {
                if (!SchemaEnum.TryParse(code, out LocationTag tag))
                {
                    throw new DataException(sourcePath, CoreErrors.WorldUnknownLocationTag, code);
                }

                result.Add(tag);
            }

            return result;
        }

        private static bool Contains(IReadOnlyList<LocationTag> tags, LocationTag tag)
        {
            for (var index = 0; index < tags.Count; index++)
            {
                if (tags[index] == tag)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
