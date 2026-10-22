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
        private static readonly string[] KnownKinds = { "hub", "wild", "special" };
        private static readonly string[] KnownNodeTypes = { "hub", "route", "site", "interior" };
        private static readonly string[] KnownRarities = { "common", "uncommon", "rare", "unique" };

        /// <summary>Создаёт шаблон локации.</summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="titleKey">Ключ названия.</param>
        /// <param name="locationKind">Тип локации: <c>hub</c>, <c>wild</c>, <c>special</c>.</param>
        /// <param name="nodeType">Роль в цепочке: <c>hub</c>, <c>route</c>, <c>site</c>, <c>interior</c>.</param>
        /// <param name="tags">Теги совместимости.</param>
        /// <param name="tier">Тир опасности.</param>
        /// <param name="backgrounds">Пул фонов.</param>
        /// <param name="objectCount">Сколько объектов генерировать.</param>
        /// <param name="danger">Сложность от 0 до 1.</param>
        /// <param name="connections">Авторские переходы.</param>
        /// <param name="approachLength">Сколько отрезков пути ведёт к локации; <c>null</c> — из топологии.</param>
        /// <param name="fixedEvents">События, которые появляются всегда.</param>
        /// <param name="rarity">Редкость локации.</param>
        /// <param name="spawnChance">Шанс появления от 0 до 1.</param>
        /// <param name="unique">Квестовая уникальная локация.</param>
        public LocationTemplate(
            string id,
            string titleKey,
            string locationKind,
            string nodeType,
            IReadOnlyList<string> tags,
            int tier,
            IReadOnlyList<string> backgrounds,
            IntRange objectCount,
            double danger,
            IReadOnlyList<ConnectionTemplate> connections,
            IntRange? approachLength,
            IReadOnlyList<string> fixedEvents,
            string rarity,
            double spawnChance,
            bool unique)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Пустой id локации", nameof(id));
            }

            if (!IsKnownKind(locationKind))
            {
                throw new ArgumentException("Неизвестный тип локации", nameof(locationKind));
            }

            if (!IsKnownNodeType(nodeType))
            {
                throw new ArgumentException("Неизвестная роль локации", nameof(nodeType));
            }

            if (!IsKnownRarity(rarity))
            {
                throw new ArgumentException("Неизвестная редкость локации", nameof(rarity));
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
        }

        /// <summary>Идентификатор.</summary>
        public string Id { get; }

        /// <summary>Ключ названия.</summary>
        public string TitleKey { get; }

        /// <summary>Тип локации.</summary>
        public string LocationKind { get; }

        /// <summary>Роль в цепочке.</summary>
        public string NodeType { get; }

        /// <summary>Теги совместимости.</summary>
        public IReadOnlyList<string> Tags { get; }

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
        public string Rarity { get; }

        /// <summary>Шанс появления от 0 до 1.</summary>
        public double SpawnChance { get; }

        /// <summary>Квестовая уникальная локация.</summary>
        public bool Unique { get; }

        /// <summary>Проверяет, известен ли тип локации.</summary>
        /// <param name="kind">Значение поля <c>locationKind</c>.</param>
        /// <returns><c>true</c>, если тип поддерживается.</returns>
        public static bool IsKnownKind(string kind) => Contains(KnownKinds, kind);

        /// <summary>Проверяет, известна ли роль в цепочке.</summary>
        /// <param name="nodeType">Значение поля <c>nodeType</c>.</param>
        /// <returns><c>true</c>, если роль поддерживается.</returns>
        public static bool IsKnownNodeType(string nodeType) => Contains(KnownNodeTypes, nodeType);

        /// <summary>Проверяет, известна ли редкость локации.</summary>
        /// <param name="rarity">Значение поля <c>rarity</c>.</param>
        /// <returns><c>true</c>, если редкость поддерживается.</returns>
        public static bool IsKnownRarity(string rarity) => Contains(KnownRarities, rarity);

        /// <summary>Читает шаблон локации из записи данных.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Шаблон локации.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static LocationTemplate FromJson(JsonValue value, string sourcePath)
        {
            var id = DataMap.RequireString(value, "id", sourcePath);
            var titleKey = DataMap.RequireString(value, "titleKey", sourcePath);
            var locationKind = DataMap.RequireString(value, "locationKind", sourcePath);
            if (!IsKnownKind(locationKind))
            {
                throw new DataException(sourcePath, CoreErrors.WorldUnknownLocationKind, locationKind);
            }

            var nodeType = DataMap.RequireString(value, "nodeType", sourcePath);
            if (!IsKnownNodeType(nodeType))
            {
                throw new DataException(sourcePath, CoreErrors.WorldUnknownNodeType, nodeType);
            }

            var tags = DataMap.OptionalStringArray(value, "tags");
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
            var rarity = DataMap.OptionalString(value, "rarity") ?? "common";
            if (!IsKnownRarity(rarity))
            {
                throw new DataException(sourcePath, CoreErrors.WorldRange, "rarity");
            }

            var spawnChance = DataMap.OptionalDouble(value, "spawnChance") ?? 1;
            if (spawnChance < 0 || spawnChance > 1)
            {
                throw new DataException(sourcePath, CoreErrors.WorldChance, "spawnChance");
            }

            var unique = DataMap.OptionalBool(value, "unique", false);
            if (unique && rarity != "unique")
            {
                throw new DataException(sourcePath, CoreErrors.WorldUniqueRarity, id);
            }

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
                unique);
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

        private static bool Contains(string[] values, string candidate)
        {
            foreach (var value in values)
            {
                if (value == candidate)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
