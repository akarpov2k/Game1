#nullable enable
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Combat
{
    /// <summary>
    /// Атака актёра из данных: либо оружие (<see cref="WeaponId"/>), либо естественное оружие
    /// (<see cref="Damage"/> и <see cref="DamageTypeId"/>); ровно один из двух наборов.
    /// </summary>
    public sealed class ActorAttackTemplate
    {
        private ActorAttackTemplate(string? weaponId, double damage, string? damageTypeId, DistanceZone range)
        {
            WeaponId = weaponId;
            Damage = damage;
            DamageTypeId = damageTypeId;
            Range = range;
        }

        /// <summary>Id оружия; <c>null</c> у естественной атаки.</summary>
        public string? WeaponId { get; }

        /// <summary>Урон естественной атаки; 0 у атаки оружием.</summary>
        public double Damage { get; }

        /// <summary>Тип урона естественной атаки; <c>null</c> у атаки оружием.</summary>
        public string? DamageTypeId { get; }

        /// <summary>Максимальная дистанция естественной атаки; у оружия не используется.</summary>
        public DistanceZone Range { get; }

        /// <summary><c>true</c>, если актёр бьёт без оружия.</summary>
        public bool IsNatural => WeaponId == null;

        /// <summary>Читает атаку из блока <c>combat.attack</c>.</summary>
        /// <param name="value">Загруженный объект атаки.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Атака актёра.</returns>
        /// <exception cref="DataException">Поля заданы неверно или смешаны.</exception>
        public static ActorAttackTemplate FromJson(JsonValue value, string sourcePath)
        {
            var weaponId = DataMap.OptionalString(value, "weaponId");
            var damage = DataMap.OptionalDouble(value, "damage") ?? 0;
            var damageTypeId = DataMap.OptionalString(value, "damageTypeId");
            var range = ReadRange(value, sourcePath);

            if (weaponId != null)
            {
                if (damage > 0 || damageTypeId != null)
                {
                    throw new DataException(sourcePath, CoreErrors.AttackMixedFields, "attack");
                }

                return new ActorAttackTemplate(weaponId, 0, null, range);
            }

            if (damage <= 0)
            {
                throw new DataException(sourcePath, CoreErrors.MissingNumberField, "attack.damage");
            }

            if (damageTypeId == null)
            {
                throw new DataException(sourcePath, CoreErrors.MissingStringField, "attack.damageTypeId");
            }

            return new ActorAttackTemplate(null, damage, damageTypeId, range);
        }

        private static DistanceZone ReadRange(JsonValue value, string sourcePath)
        {
            var code = DataMap.OptionalString(value, "range");
            if (code == null)
            {
                return DistanceZone.Melee;
            }

            if (!SchemaEnum.TryParse(code, out DistanceZone range))
            {
                throw new DataException(sourcePath, CoreErrors.UnknownDistanceZone, code);
            }

            return range;
        }
    }
}
