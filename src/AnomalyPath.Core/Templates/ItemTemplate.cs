#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Templates
{
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
            PriceBlock? price,
            System.Collections.Generic.IReadOnlyList<string> equipSlots,
            int extraRows,
            int artifactSlots,
            System.Collections.Generic.IReadOnlyDictionary<string, int> protection,
            string? artifactType,
            int armorDurabilityMax,
            double armorDurabilityPerHit,
            string armorMaterial,
            double durabilityFactor,
            double quality,
            string? caliber)
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
            EquipSlots = equipSlots;
            ExtraRows = extraRows;
            ArtifactSlots = artifactSlots;
            Protection = protection;
            ArtifactType = artifactType;
            ArmorDurabilityMax = armorDurabilityMax;
            ArmorMaterial = armorMaterial;
            DurabilityFactor = durabilityFactor;
            Quality = quality;
            Caliber = caliber;
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

        /// <summary>Слоты экипировки, куда предмет можно надеть.</summary>
        public System.Collections.Generic.IReadOnlyList<string> EquipSlots { get; }

        /// <summary>Дополнительные строки рюкзака (для категории <c>backpack</c>).</summary>
        public int ExtraRows { get; }

        /// <summary>Сколько слотов артефактов даёт предмет (броня и пояс).</summary>
        public int ArtifactSlots { get; }

        /// <summary>Защита по типам урона (шлем и броня).</summary>
        public System.Collections.Generic.IReadOnlyDictionary<string, int> Protection { get; }

        /// <summary>Тип артефакта: <c>fire</c>, <c>electric</c>, <c>chemical</c>, <c>gravity</c>.</summary>
        public string? ArtifactType { get; }

        /// <summary>Максимум прочности брони или шлема; 0 — не изнашивается.</summary>
        public int ArmorDurabilityMax { get; }

        /// <summary>Материал брони или шлема: <c>light</c>, <c>medium</c>, <c>heavy</c>.</summary>
        public string ArmorMaterial { get; }

        /// <summary>Множитель прочности материала в формуле износа.</summary>
        public double DurabilityFactor { get; }

        /// <summary>Качество предмета: множитель износа, по умолчанию 1.</summary>
        public double Quality { get; }

        /// <summary>Калибр патронов; только для категории <c>ammo</c>.</summary>
        public string? Caliber { get; }

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
            var artifactType = DataMap.OptionalString(value, "artifactType");
            var armorMaterial = DataMap.OptionalString(value, "material") ?? AnomalyPath.Core.Equipment.ArmorMaterials.Medium;

            var caliber = DataMap.OptionalString(value, "caliber");
            if (category == "ammo" && string.IsNullOrWhiteSpace(caliber))
            {
                throw new DataException(sourcePath, CoreErrors.MissingStringField, "caliber");
            }

            if (category == "artifact" && string.IsNullOrWhiteSpace(artifactType))
            {
                throw new DataException(sourcePath, CoreErrors.ArtifactWithoutType, id);
            }

            if ((category == "armor" || category == "helmet") &&
                !AnomalyPath.Core.Equipment.ArmorMaterials.IsKnown(armorMaterial))
            {
                throw new DataException(sourcePath, CoreErrors.UnknownMaterial, armorMaterial);
            }

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
                price,
                ReadEquipSlots(value, category),
                DataMap.OptionalInt(value, "extraRows", 0),
                DataMap.OptionalInt(value, "artifactSlots", 0),
                ReadProtection(value, sourcePath),
                artifactType,
                DataMap.OptionalInt(value, "durabilityMax", 0),
                DataMap.OptionalDouble(value, "durabilityPerHit") ?? 0,
                armorMaterial,
                DataMap.OptionalDouble(value, "durabilityFactor") ?? 1.0,
                DataMap.OptionalDouble(value, "quality") ?? 1.0,
                caliber);
        }

        /// <summary>Слоты экипировки по категории, если в данных не заданы явно.</summary>
        /// <param name="category">Категория предмета.</param>
        /// <returns>Список id слотов.</returns>
        public static IReadOnlyList<string> DefaultEquipSlots(string category)
        {
            switch (category)
            {
                case "weapon": return new[] { "main" };
                case "helmet": return new[] { "helmet" };
                case "armor": return new[] { "armor" };
                case "backpack": return new[] { "backpack" };
                case "belt": return new[] { "belt" };
                case "consumable": return new[] { "quick" };
                default: return new string[0];
            }
        }

        private static IReadOnlyList<string> ReadEquipSlots(JsonValue value, string category)
        {
            if (!value.TryGet("equipSlots", out var slots) || !slots.IsArray)
            {
                return DefaultEquipSlots(category);
            }

            return DataMap.OptionalStringArray(value, "equipSlots");
        }

        private static System.Collections.Generic.IReadOnlyDictionary<string, int> ReadProtection(JsonValue value, string sourcePath)
        {
            var protection = new System.Collections.Generic.Dictionary<string, int>(System.StringComparer.Ordinal);
            if (!value.TryGet("protection", out var block) || !block.IsObject)
            {
                return protection;
            }

            foreach (var pair in block.Members)
            {
                if (pair.Value.Kind == JsonKind.Number)
                {
                    protection[pair.Key] = (int)System.Math.Round(pair.Value.Number);
                }
            }

            return protection;
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
                DataMap.OptionalDouble(value, "accuracyCoefficient") ?? DataMap.RequireDouble(value, "accuracy", sourcePath),
            DataMap.OptionalBool(value, "ignoresDistance", false),
                DataMap.RequireInt(value, "magazine", sourcePath),
                DataMap.RequireString(value, "ammoCaliber", sourcePath),
                DataMap.RequireDouble(value, "reloadTime", sourcePath),
                DataMap.OptionalString(value, "damageTypeId") ?? "dmg_bullet",
                DataMap.RequireInt(value, "durabilityMax", sourcePath),
                DataMap.RequireDouble(value, "durabilityPerShot", sourcePath),
            DataMap.OptionalDouble(value, "armorPenetration") ?? 0,
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
