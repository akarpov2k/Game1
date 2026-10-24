#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Trade;

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
        /// <param name="descriptionKey">Ключ описания.</param>
        /// <param name="portrait">Id портрета в каталоге спрайтов.</param>
        /// <param name="portraitMode">Кадрирование портрета.</param>
        /// <param name="factionId">Фракция или <c>null</c>.</param>
        /// <param name="locationId">Локация или <c>null</c>.</param>
        /// <param name="roles">Роли актёра.</param>
        /// <param name="trader">Блок торговли у торговца.</param>
        public ActorTemplate(
            string id,
            ActorType type,
            string nameKey,
            CombatBlockTemplate? combat,
            string? descriptionKey = null,
            string? portrait = null,
            PortraitMode portraitMode = PortraitMode.Full,
            string? factionId = null,
            string? locationId = null,
            IReadOnlyList<string>? roles = null,
            TraderBlockTemplate? trader = null)
        {
            Id = id;
            Type = type;
            NameKey = nameKey;
            Combat = combat;
            DescriptionKey = descriptionKey;
            Portrait = portrait;
            PortraitMode = portraitMode;
            FactionId = factionId;
            LocationId = locationId;
            Roles = roles ?? Array.Empty<string>();
            Trader = trader;
        }

        /// <summary>Идентификатор.</summary>
        public string Id { get; }

        /// <summary>Тип актёра.</summary>
        public ActorType Type { get; }

        /// <summary>Ключ имени.</summary>
        public string NameKey { get; }

        /// <summary>Боевой блок; у NPC и торговца может отсутствовать.</summary>
        public CombatBlockTemplate? Combat { get; }

        /// <summary>Ключ описания.</summary>
        public string? DescriptionKey { get; }

        /// <summary>Id портрета в каталоге спрайтов: <c>portraits/trd_hub_smuggler</c>.</summary>
        public string? Portrait { get; }

        /// <summary>Кадрирование портрета: в полный рост или по пояс.</summary>
        public PortraitMode PortraitMode { get; }

        /// <summary>Фракция или <c>null</c> у одиночки.</summary>
        public string? FactionId { get; }

        /// <summary>Локация; у торговца привязка задаётся списком <c>traders</c> хаба.</summary>
        public string? LocationId { get; }

        /// <summary>Роли актёра.</summary>
        public IReadOnlyList<string> Roles { get; }

        /// <summary>Блок торговли; только у <c>type = trader</c>.</summary>
        public TraderBlockTemplate? Trader { get; }

        /// <summary>Проверяет, есть ли у актёра роль.</summary>
        /// <param name="role">Код роли.</param>
        /// <returns><c>true</c>, если роль назначена.</returns>
        public bool HasRole(string role)
        {
            foreach (var item in Roles)
            {
                if (string.Equals(item, role, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

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
            var roles = DataMap.OptionalStringArray(value, "roles");
            var portraitMode = ReadPortraitMode(value, sourcePath);
            var combat = ReadCombat(value, type, id, sourcePath);
            var trader = ReadTrader(value, type, roles, sourcePath);
            return new ActorTemplate(
                id,
                type,
                nameKey,
                combat,
                DataMap.OptionalString(value, "descriptionKey"),
                DataMap.OptionalString(value, "portrait"),
                portraitMode,
                DataMap.OptionalString(value, "factionId"),
                DataMap.OptionalString(value, "locationId"),
                roles,
                trader);
        }

        private static PortraitMode ReadPortraitMode(JsonValue value, string sourcePath)
        {
            var code = DataMap.OptionalString(value, "portraitMode");
            if (code == null)
            {
                return PortraitMode.Full;
            }

            if (!SchemaEnum.TryParse(code, out PortraitMode mode))
            {
                throw new DataException(sourcePath, CoreErrors.UnknownPortraitMode, code);
            }

            return mode;
        }

        private static TraderBlockTemplate? ReadTrader(
            JsonValue value,
            ActorType type,
            IReadOnlyList<string> roles,
            string sourcePath)
        {
            if (type != ActorType.Trader)
            {
                if (value.TryGet("trader", out _))
                {
                    throw new DataException(sourcePath, CoreErrors.TraderBlockOnNonTrader);
                }

                if (ContainsRole(roles, ActorRoles.Trader))
                {
                    throw new DataException(sourcePath, CoreErrors.TraderRoleOnNonTrader);
                }

                return null;
            }

            var trader = TraderBlockTemplate.FromJson(value, sourcePath);
            if (!ContainsRole(roles, ActorRoles.Trader))
            {
                throw new DataException(sourcePath, CoreErrors.TraderWithoutRole);
            }

            return trader;
        }

        private static bool ContainsRole(IReadOnlyList<string> roles, string role)
        {
            foreach (var item in roles)
            {
                if (string.Equals(item, role, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
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
            var xp = DataMap.OptionalInt(block, "xp", 0);
            if (type == ActorType.Enemy && xp <= 0)
            {
                throw new DataException(sourcePath, CoreErrors.MissingNumberField, "xp");
            }

            return new CombatBlockTemplate(tier, level, health, ap, protection, ai, accuracy, accuracyBonus, xp);
        }
    }
}
