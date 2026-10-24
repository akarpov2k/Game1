#nullable enable
using System.Collections.Generic;

namespace AnomalyPath.Core.Templates
{
    /// <summary>Типизированное описание оружия из файла данных.</summary>
    public sealed class WeaponBlock
    {
        /// <summary>Создаёт блок оружия.</summary>
        /// <param name="classId">Класс оружия.</param>
        /// <param name="damage">Базовый урон.</param>
        /// <param name="accuracy">Коэффициент точности.</param>
        /// <param name="ignoresDistance">Игнорировать ли штраф дистанции.</param>
        /// <param name="magazine">Вместимость магазина.</param>
        /// <param name="ammoCaliber">Калибр.</param>
        /// <param name="reloadTime">Перезарядка в ОД.</param>
        /// <param name="damageTypeId">Тип урона.</param>
        /// <param name="durabilityMax">Максимум прочности.</param>
        /// <param name="durabilityPerShot">Износ за выстрел.</param>
        /// <param name="armorPenetration">Бронебойность.</param>
        /// <param name="skills">Скиллы оружия.</param>
        public WeaponBlock(
            string classId,
            double damage,
            double accuracy,
            bool ignoresDistance,
            int magazine,
            string ammoCaliber,
            double reloadTime,
            string damageTypeId,
            int durabilityMax,
            double durabilityPerShot,
            double armorPenetration,
            IReadOnlyList<string> skills)
        {
            ClassId = classId;
            Damage = damage;
            AccuracyCoefficient = accuracy;
            IgnoresDistance = ignoresDistance;
            Magazine = magazine;
            AmmoCaliber = ammoCaliber;
            ReloadTime = reloadTime;
            DamageTypeId = damageTypeId;
            DurabilityMax = durabilityMax;
            DurabilityPerShot = durabilityPerShot;
            ArmorPenetration = armorPenetration;
            Skills = skills;
        }

        /// <summary>Класс оружия.</summary>
        public string ClassId { get; }

        /// <summary>Базовый урон.</summary>
        public double Damage { get; }

        /// <summary>Коэффициент точности оружия.</summary>
        public double AccuracyCoefficient { get; }

        /// <summary>Игнорирует ли оружие штраф дальней дистанции.</summary>
        public bool IgnoresDistance { get; }

        /// <summary>Вместимость магазина.</summary>
        public int Magazine { get; }

        /// <summary>Калибр.</summary>
        public string AmmoCaliber { get; }

        /// <summary>Перезарядка в ОД.</summary>
        public double ReloadTime { get; }

        /// <summary>Тип урона.</summary>
        public string DamageTypeId { get; }

        /// <summary>Максимум прочности.</summary>
        public int DurabilityMax { get; }

        /// <summary>Износ за выстрел.</summary>
        public double DurabilityPerShot { get; }

        /// <summary>Бронебойность.</summary>
        public double ArmorPenetration { get; }

        /// <summary>Скиллы оружия.</summary>
        public IReadOnlyList<string> Skills { get; }
    }
}
