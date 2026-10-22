#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Combat
{
    /// <summary>Актёр из данных: персонаж, торговец или враг.</summary>
    public sealed class ActorTemplate
    {
        /// <summary>Создаёт актёра.</summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="type">Тип: <c>npc</c>, <c>trader</c>, <c>enemy</c>.</param>
        /// <param name="nameKey">Ключ имени.</param>
        /// <param name="combat">Боевой блок, если есть.</param>
        public ActorTemplate(string id, ActorType type, string nameKey, CombatBlockTemplate? combat)
        {
            Id = id;
            Type = type;
            NameKey = nameKey;
            Combat = combat;
        }

        /// <summary>Идентификатор.</summary>
        public string Id { get; }

        /// <summary>Тип актёра.</summary>
        public ActorType Type { get; }

        /// <summary>Ключ имени.</summary>
        public string NameKey { get; }

        /// <summary>Боевой блок; у NPC и торговца может отсутствовать.</summary>
        public CombatBlockTemplate? Combat { get; }

        /// <summary>Читает актёра из записи данных.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Актёр.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static ActorTemplate FromJson(JsonValue value, string sourcePath)
        {
            var id = DataMap.RequireString(value, "id", sourcePath);
            var typeCode = DataMap.RequireString(value, "type", sourcePath);
            if (!SchemaEnum.TryParse(typeCode, out ActorType type))
            {
                throw new DataException(sourcePath, CoreErrors.UnknownActorType, typeCode);
            }

            var nameKey = DataMap.RequireString(value, "nameKey", sourcePath);
            var combat = ReadCombat(value, type, id, sourcePath);
            return new ActorTemplate(id, type, nameKey, combat);
        }

        private static CombatBlockTemplate? ReadCombat(JsonValue value, ActorType type, string id, string sourcePath)
        {
            if (!value.TryGet("combat", out var block) || !block.IsObject)
            {
                if (type == ActorType.Enemy)
                {
                    throw new DataException(sourcePath, CoreErrors.ActorWithoutCombat, id);
                }

                return null;
            }

            var tier = DataMap.RequireInt(block, "tier", sourcePath);
            var level = DataMap.RequireInt(block, "level", sourcePath);
            var health = DataMap.RequireInt(block, "health", sourcePath);
            var ap = DataMap.RequireInt(block, "ap", sourcePath);
            if (health <= 0 || ap < 0)
            {
                throw new DataException(sourcePath, CoreErrors.MissingNumberField, health <= 0 ? "health" : "ap");
            }

            var protection = new Dictionary<string, int>(StringComparer.Ordinal);
            if (block.TryGet("protection", out var protectionBlock) && protectionBlock.IsObject)
            {
                foreach (var pair in protectionBlock.Entries)
                {
                    if (pair.Value.Kind != JsonKind.Number)
                    {
                        throw new DataException(sourcePath, CoreErrors.MissingNumberField, "protection");
                    }

                    protection[pair.Key] = (int)Math.Round(pair.Value.Number);
                }
            }

            var ai = DataMap.RequireString(block, "ai", sourcePath);
            var accuracy = DataMap.RequireDouble(block, "accuracy", sourcePath);
            var accuracyBonus = DataMap.OptionalDouble(block, "accuracyBonus") ?? 0;
            return new CombatBlockTemplate(tier, level, health, ap, protection, ai, accuracy, accuracyBonus);
        }
    }
}
