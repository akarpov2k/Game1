#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.World
{
    /// <summary>Авторский шаблон локации: роль в цепочке, теги, переходы и шанс появления.</summary>
    public sealed class LocationTemplate
    {
        /// <summary>Создаёт шаблон локации.</summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="titleKey">Ключ названия.</param>
        /// <param name="locationKind">Тип локации из <see cref="LocationKind"/>.</param>
        /// <param name="nodeType">Роль в цепочке из <see cref="LocationNodeType"/>.</param>
        /// <param name="tags">Теги совместимости.</param>
        /// <param name="tier">Тир опасности.</param>
        /// <param name="backgrounds">Пул фонов.</param>
        /// <param name="objectCount">Сколько объектов генерировать.</param>
        /// <param name="danger">Сложность от 0 до 1.</param>
        /// <param name="connections">Авторские переходы.</param>
        /// <param name="approachLength">Сколько отрезков пути ведёт к локации; <c>null</c> — из топологии.</param>
        /// <param name="fixedEvents">События, которые появляются всегда.</param>
        /// <param name="rarity">Редкость локации из <see cref="LocationRarity"/>.</param>
        /// <param name="spawnChance">Шанс появления от 0 до 1.</param>
        /// <param name="unique">Квестовая уникальная локация.</param>
        /// <param name="traders">Id торговцев, привязанных к этому хабу.</param>
        /// <exception cref="ArgumentException">Id пуст или значение перечисления не определено.</exception>
        public LocationTemplate(
            string id,
            string titleKey,
            LocationKind locationKind,
            LocationNodeType nodeType,
            IReadOnlyList<LocationTag> tags,
            int tier,
            IReadOnlyList<string> backgrounds,
            IntRange objectCount,
            double danger,
            IReadOnlyList<ConnectionTemplate> connections,
            IntRange? approachLength,
            IReadOnlyList<string> fixedEvents,
            LocationRarity rarity,
            double spawnChance,
            bool unique,
            IReadOnlyList<string>? traders = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(CoreText.Format(CoreErrors.RequiredField, nameof(id)), nameof(id));
            }

            if (!Enum.IsDefined(typeof(LocationKind), locationKind))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.WorldUnknownLocationKind, locationKind),
                    nameof(locationKind));
            }

            if (!Enum.IsDefined(typeof(LocationNodeType), nodeType))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.WorldUnknownNodeType, nodeType),
                    nameof(nodeType));
            }

            if (!Enum.IsDefined(typeof(LocationRarity), rarity))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.WorldUnknownRarity, rarity),
                    nameof(rarity));
            }

            if (tier < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tier));
            }

            if (danger < 0 || danger > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(danger));
            }

            if (spawnChance < 0 || spawnChance > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(spawnChance));
            }

            Id = id;
            TitleKey = titleKey;
            LocationKind = locationKind;
            NodeType = nodeType;
            Tags = tags;
            Tier = tier;
            Backgrounds = backgrounds;
            ObjectCount = objectCount;
            Danger = danger;
            Connections = connections;
            ApproachLength = approachLength;
            FixedEvents = fixedEvents;
            Rarity = rarity;
            SpawnChance = spawnChance;
            Unique = unique;
            Traders = traders ?? Array.Empty<string>();
        }

        /// <summary>Идентификатор.</summary>
        public string Id { get; }

        /// <summary>Ключ названия.</summary>
        public string TitleKey { get; }

        /// <summary>Тип локации.</summary>
        public LocationKind LocationKind { get; }

        /// <summary>Роль в цепочке.</summary>
        public LocationNodeType NodeType { get; }

        /// <summary>Теги совместимости.</summary>
        public IReadOnlyList<LocationTag> Tags { get; }

        /// <summary>Тир опасности.</summary>
        public int Tier { get; }

        /// <summary>Пул фонов.</summary>
        public IReadOnlyList<string> Backgrounds { get; }

        /// <summary>Сколько объектов генерировать.</summary>
        public IntRange ObjectCount { get; }

        /// <summary>Сложность от 0 до 1.</summary>
        public double Danger { get; }

        /// <summary>Авторские переходы.</summary>
        public IReadOnlyList<ConnectionTemplate> Connections { get; }

        /// <summary>Сколько отрезков пути ведёт к локации; <c>null</c> — из топологии.</summary>
        public IntRange? ApproachLength { get; }

        /// <summary>События, которые появляются всегда.</summary>
        public IReadOnlyList<string> FixedEvents { get; }

        /// <summary>Редкость локации.</summary>
        public LocationRarity Rarity { get; }

        /// <summary>Шанс появления от 0 до 1.</summary>
        public double SpawnChance { get; }

        /// <summary>Квестовая уникальная локация.</summary>
        public bool Unique { get; }

        /// <summary>Id торговцев, привязанных к хабу.</summary>
        public IReadOnlyList<string> Traders { get; }

        /// <summary>Читает шаблон локации из записи данных.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Шаблон локации.</returns>
        /// <exception cref="DataException">Поля отсутствуют, значения неизвестны или вне границ.</exception>
        public static LocationTemplate FromJson(JsonValue value, string sourcePath)
        {
            var id = DataMap.RequireString(value, "id", sourcePath);
            var titleKey = DataMap.RequireString(value, "titleKey", sourcePath);

            var locationKindCode = DataMap.RequireString(value, "locationKind", sourcePath);
            if (!SchemaEnum.TryParse(locationKindCode, out LocationKind locationKind))
            {
                throw new DataException(sourcePath, CoreErrors.WorldUnknownLocationKind, locationKindCode);
            }

            var nodeTypeCode = DataMap.RequireString(value, "nodeType", sourcePath);
            if (!SchemaEnum.TryParse(nodeTypeCode, out LocationNodeType nodeType))
            {
                throw new DataException(sourcePath, CoreErrors.WorldUnknownNodeType, nodeTypeCode);
            }

            var tags = ReadTags(value, sourcePath);
            var backgrounds = DataMap.OptionalStringArray(value, "backgrounds");
            if (backgrounds.Count == 0)
            {
                throw new DataException(sourcePath, CoreErrors.MissingArrayField, "backgrounds");
            }

            var tier = DataMap.OptionalInt(value, "tier", 0);
            if (tier < 0)
            {
                throw new DataException(sourcePath, CoreErrors.WorldRange, "tier");
            }

            var objectCount = IntRange.Optional(value, "objectCount", sourcePath, new IntRange(0, 0));
            var danger = DataMap.OptionalDouble(value, "danger") ?? 0;
            if (danger < 0 || danger > 1)
            {
                throw new DataException(sourcePath, CoreErrors.WorldChance, "danger");
            }

            var connections = ReadConnections(value, sourcePath);
            IntRange? approachLength = null;
            if (value.TryGet("approachLength", out _))
            {
                approachLength = IntRange.FromJson(value, "approachLength", sourcePath);
                if (approachLength.Value.Min < 1)
                {
                    throw new DataException(sourcePath, CoreErrors.WorldRange, "approachLength");
                }
            }

            var fixedEvents = DataMap.OptionalStringArray(value, "fixedEvents");

            var rarity = LocationRarity.Common;
            var rarityCode = DataMap.OptionalString(value, "rarity");
            if (rarityCode != null && !SchemaEnum.TryParse(rarityCode, out rarity))
            {
                throw new DataException(sourcePath, CoreErrors.WorldUnknownRarity, rarityCode);
            }

            var spawnChance = DataMap.OptionalDouble(value, "spawnChance") ?? 1;
            if (spawnChance < 0 || spawnChance > 1)
            {
                throw new DataException(sourcePath, CoreErrors.WorldChance, "spawnChance");
            }

            var unique = DataMap.OptionalBool(value, "unique", false);
            if (unique && rarity != LocationRarity.Unique)
            {
                throw new DataException(sourcePath, CoreErrors.WorldUniqueRarity, id);
            }

            var traders = DataMap.OptionalStringArray(value, "traders");
            return new LocationTemplate(
                id,
                titleKey,
                locationKind,
                nodeType,
                tags,
                tier,
                backgrounds,
                objectCount,
                danger,
                connections,
                approachLength,
                fixedEvents,
                rarity,
                spawnChance,
                unique,
                traders);
        }

        private static IReadOnlyList<LocationTag> ReadTags(JsonValue value, string sourcePath)
        {
            var result = new List<LocationTag>();
            foreach (var code in DataMap.OptionalStringArray(value, "tags"))
            {
                if (!SchemaEnum.TryParse(code, out LocationTag tag))
                {
                    throw new DataException(sourcePath, CoreErrors.WorldUnknownLocationTag, code);
                }

                result.Add(tag);
            }

            return result;
        }

        private static IReadOnlyList<ConnectionTemplate> ReadConnections(JsonValue value, string sourcePath)
        {
            var result = new List<ConnectionTemplate>();
            if (!value.TryGet("connections", out var connections) || !connections.IsArray)
            {
                return result;
            }

            foreach (var item in connections.Items)
            {
                if (!item.IsObject)
                {
                    throw new DataException(sourcePath, CoreErrors.JsonNotObject);
                }

                result.Add(ConnectionTemplate.FromJson(item, sourcePath));
            }

            return result;
        }
    }
}
