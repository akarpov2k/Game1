using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Templates
{
    /// <summary>Типизированное описание оружия из файла данных.</summary>
    public sealed class WeaponBlock
    {
        /// <summary>Создаёт блок оружия.</summary>
        /// <param name="classId">Класс оружия.</param>
        /// <param name="damage">Базовый урон.</param>
        /// <param name="accuracy">Точность 0..1.</param>
        /// <param name="magazine">Патронов в магазине.</param>
        /// <param name="ammoCaliber">Калибр.</param>
        /// <param name="reloadTime">Перезарядка в ОД.</param>
        /// <param name="damageTypeId">Тип урона.</param>
        /// <param name="durabilityMax">Максимум прочности.</param>
        /// <param name="durabilityPerShot">Износ за выстрел.</param>
        /// <param name="skills">Скиллы оружия.</param>
        public WeaponBlock(
            string classId,
            double damage,
            double accuracy,
            int magazine,
            string ammoCaliber,
            double reloadTime,
            string damageTypeId,
            int durabilityMax,
            double durabilityPerShot,
            IReadOnlyList<string> skills)
        {
            ClassId = classId;
            Damage = damage;
            Accuracy = accuracy;
            Magazine = magazine;
            AmmoCaliber = ammoCaliber;
            ReloadTime = reloadTime;
            DamageTypeId = damageTypeId;
            DurabilityMax = durabilityMax;
            DurabilityPerShot = durabilityPerShot;
            Skills = skills;
        }

        /// <summary>Класс оружия.</summary>
        public string ClassId { get; }

        /// <summary>Базовый урон.</summary>
        public double Damage { get; }

        /// <summary>Точность 0..1.</summary>
        public double Accuracy { get; }

        /// <summary>Патронов в магазине.</summary>
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

        /// <summary>Скиллы оружия.</summary>
        public IReadOnlyList<string> Skills { get; }
    }

    /// <summary>Блок цены: фиксированная или от репутации.</summary>
    public sealed class PriceBlock
    {
        /// <summary>Создаёт блок цены.</summary>
        /// <param name="mode">Режим: <c>fixed</c> или <c>reputation</c>.</param>
        /// <param name="buy">Цена покупки игроком.</param>
        /// <param name="sell">Цена продажи игроком.</param>
        /// <param name="basePrice">Базовая цена для режима репутации.</param>
        /// <param name="factionId">Фракция, чья репутация влияет.</param>
        public PriceBlock(string mode, double? buy, double? sell, double? basePrice, string? factionId)
        {
            Mode = mode;
            Buy = buy;
            Sell = sell;
            Base = basePrice;
            FactionId = factionId;
        }

        /// <summary>Режим цены.</summary>
        public string Mode { get; }

        /// <summary>Цена покупки игроком.</summary>
        public double? Buy { get; }

        /// <summary>Цена продажи игроком.</summary>
        public double? Sell { get; }

        /// <summary>Базовая цена.</summary>
        public double? Base { get; }

        /// <summary>Фракция для репутационной цены.</summary>
        public string? FactionId { get; }
    }

    /// <summary>Описание предмета: только данные, без поведения.</summary>
    public sealed class ItemTemplate
    {
        private ItemTemplate(
            string id,
            string nameKey,
            string? descriptionKey,
            string category,
            int footprintWidth,
            int footprintHeight,
            bool rotatable,
            int maxStack,
            int tier,
            string icon,
            WeaponBlock? weapon,
            PriceBlock? price)
        {
            Id = id;
            NameKey = nameKey;
            DescriptionKey = descriptionKey;
            Category = category;
            FootprintWidth = footprintWidth;
            FootprintHeight = footprintHeight;
            Rotatable = rotatable;
            MaxStack = maxStack;
            Tier = tier;
            Icon = icon;
            Weapon = weapon;
            Price = price;
        }

        /// <summary>Идентификатор предмета.</summary>
        public string Id { get; }

        /// <summary>Ключ локализации названия.</summary>
        public string NameKey { get; }

        /// <summary>Ключ локализации описания.</summary>
        public string? DescriptionKey { get; }

        /// <summary>Категория.</summary>
        public string Category { get; }

        /// <summary>Ширина в клетках.</summary>
        public int FootprintWidth { get; }

        /// <summary>Высота в клетках.</summary>
        public int FootprintHeight { get; }

        /// <summary>Разрешён ли поворот.</summary>
        public bool Rotatable { get; }

        /// <summary>Предел стопки.</summary>
        public int MaxStack { get; }

        /// <summary>Тир предмета.</summary>
        public int Tier { get; }

        /// <summary>Путь к иконке.</summary>
        public string Icon { get; }

        /// <summary>Блок оружия, если категория <c>weapon</c>.</summary>
        public WeaponBlock? Weapon { get; }

        /// <summary>Блок цены.</summary>
        public PriceBlock? Price { get; }

        /// <summary>Строгое чтение: любое нарушение схемы — <see cref="DataException"/> с id и полем.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Описание предмета.</returns>
        /// <exception cref="DataException">Поле отсутствует или имеет неверный тип.</exception>
        public static ItemTemplate FromJson(JsonValue value, string sourcePath)
        {
            var id = DataMap.RequireString(value, "id", sourcePath);
            var category = DataMap.RequireString(value, "category", sourcePath);

            var footprint = DataMap.RequireIntPair(value, "footprint", sourcePath);
            var weapon = value.TryGet("weapon", out var weaponValue) ? ReadWeapon(weaponValue, id, sourcePath) : null;

            if (category == "weapon" && weapon == null)
            {
                throw new DataException(sourcePath, CoreErrors.WeaponWithoutBlock, id);
            }

            var price = value.TryGet("price", out var priceValue) ? ReadPrice(priceValue) : null;

            return new ItemTemplate(
                id,
                DataMap.RequireString(value, "nameKey", sourcePath),
                DataMap.OptionalString(value, "descriptionKey"),
                category,
                footprint.Item1,
                footprint.Item2,
                DataMap.OptionalBool(value, "rotatable", true),
                DataMap.RequireInt(value, "maxStack", sourcePath),
                DataMap.RequireInt(value, "tier", sourcePath),
                DataMap.RequireString(value, "icon", sourcePath),
                weapon,
                price);
        }

        private static WeaponBlock ReadWeapon(JsonValue value, string itemId, string sourcePath)
        {
            if (!value.IsObject)
            {
                throw new DataException(sourcePath, CoreErrors.WeaponWithoutBlock, itemId);
            }

            return new WeaponBlock(
                DataMap.RequireString(value, "classId", sourcePath),
                DataMap.RequireDouble(value, "damage", sourcePath),
                DataMap.RequireDouble(value, "accuracy", sourcePath),
                DataMap.RequireInt(value, "magazine", sourcePath),
                DataMap.RequireString(value, "ammoCaliber", sourcePath),
                DataMap.RequireDouble(value, "reloadTime", sourcePath),
                DataMap.OptionalString(value, "damageTypeId") ?? "dmg_bullet",
                DataMap.RequireInt(value, "durabilityMax", sourcePath),
                DataMap.RequireDouble(value, "durabilityPerShot", sourcePath),
                DataMap.OptionalStringArray(value, "skills"));
        }

        private static PriceBlock ReadPrice(JsonValue value)
        {
            return new PriceBlock(
                DataMap.OptionalString(value, "mode") ?? "fixed",
                DataMap.OptionalDouble(value, "buy"),
                DataMap.OptionalDouble(value, "sell"),
                DataMap.OptionalDouble(value, "base"),
                DataMap.OptionalString(value, "factionId"));
        }
    }
}
