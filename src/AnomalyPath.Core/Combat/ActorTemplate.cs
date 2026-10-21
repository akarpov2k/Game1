#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Combat
{
    /// <summary>Боевые характеристики актёра из данных.</summary>
    public sealed class CombatBlockTemplate
    {
        /// <summary>Создаёт боевой блок.</summary>
        /// <param name="tier">Тир.</param>
        /// <param name="level">Уровень актёра.</param>
        /// <param name="health">Здоровье.</param>
        /// <param name="ap">Очки действия.</param>
        /// <param name="protection">Защита по типам урона.</param>
        /// <param name="ai">Модель поведения.</param>
        /// <param name="accuracy">Базовая меткость 0..1.</param>
        /// <param name="accuracyBonus">Бонус меткости от талантов.</param>
        public CombatBlockTemplate(int tier, int level, int health, int ap, IReadOnlyDictionary<string, int> protection, string ai, double accuracy, double accuracyBonus)
        {
            Tier = tier;
            Level = level;
            Health = health;
            Ap = ap;
            Protection = protection;
            Ai = ai;
            Accuracy = accuracy;
            AccuracyBonus = accuracyBonus;
        }

        /// <summary>Базовая меткость актёра, 0..1.</summary>
        public double Accuracy { get; }

        /// <summary>Бонус меткости от талантов, прибавляется к базовой.</summary>
        public double AccuracyBonus { get; }

        /// <summary>Тир.</summary>
        public int Tier { get; }

        /// <summary>Уровень актёра.</summary>
        public int Level { get; }

        /// <summary>Здоровье.</summary>
        public int Health { get; }

        /// <summary>Очки действия.</summary>
        public int Ap { get; }

        /// <summary>Защита по типам урона.</summary>
        public IReadOnlyDictionary<string, int> Protection { get; }

        /// <summary>Модель поведения.</summary>
        public string Ai { get; }
    }

    /// <summary>Актёр из данных: персонаж, торговец или враг.</summary>
    public sealed class ActorTemplate
    {
        /// <summary>Создаёт актёра.</summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="type">Тип: <c>npc</c>, <c>trader</c>, <c>enemy</c>.</param>
        /// <param name="nameKey">Ключ имени.</param>
        /// <param name="combat">Боевой блок, если есть.</param>
        public ActorTemplate(string id, string type, string nameKey, CombatBlockTemplate? combat)
        {
            Id = id;
            Type = type;
            NameKey = nameKey;
            Combat = combat;
        }

        /// <summary>Идентификатор.</summary>
        public string Id { get; }

        /// <summary>Тип актёра.</summary>
        public string Type { get; }

        /// <summary>Ключ имени.</summary>
        public string NameKey { get; }

        /// <summary>Боевой блок; у NPC и торговца может отсутствовать.</summary>
        public CombatBlockTemplate? Combat { get; }

        /// <summary>Проверяет, известен ли тип актёра.</summary>
        /// <param name="type">Значение поля <c>type</c>.</param>
        /// <returns><c>true</c>, если тип поддерживается.</returns>
        public static bool IsKnownType(string type) => type is "npc" or "trader" or "enemy";

        /// <summary>Читает актёра из записи данных.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Актёр.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static ActorTemplate FromJson(JsonValue value, string sourcePath)
        {
            var id = DataMap.RequireString(value, "id", sourcePath);
            var type = DataMap.RequireString(value, "type", sourcePath);
            if (!IsKnownType(type))
            {
                throw new DataException(sourcePath, CoreErrors.UnknownActorType, type);
            }

            var nameKey = DataMap.RequireString(value, "nameKey", sourcePath);
            var combat = ReadCombat(value, type, id, sourcePath);
            return new ActorTemplate(id, type, nameKey, combat);
        }

        private static CombatBlockTemplate? ReadCombat(JsonValue value, string type, string id, string sourcePath)
        {
            if (!value.TryGet("combat", out var block) || !block.IsObject)
            {
                if (type == "enemy")
                {
                    throw new DataException(sourcePath, CoreErrors.ActorWithoutCombat, id);
                }

                return null;
            }

            var tier = DataMap.OptionalInt(block, "tier", 1);
            var level = DataMap.OptionalInt(block, "level", 1);
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

            var ai = DataMap.OptionalString(block, "ai") ?? "aggressive";
            var accuracy = DataMap.OptionalDouble(block, "accuracy") ?? 0.8;
            var accuracyBonus = DataMap.OptionalDouble(block, "accuracyBonus") ?? 0;
            return new CombatBlockTemplate(tier, level, health, ap, protection, ai, accuracy, accuracyBonus);
        }
    }
}
