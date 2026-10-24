#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Equipment;
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
            ItemCategory category,
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
            ArmorMaterial? armorMaterial,
            double durabilityFactor,
            double quality,
            string? caliber,
            bool unlosable)
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
            Unlosable = unlosable;
        }

        /// <summary>Идентификатор предмета.</summary>
        public string Id { get; }

        /// <summary>Ключ локализации названия.</summary>
        public string NameKey { get; }

        /// <summary>Ключ локализации описания.</summary>
        public string? DescriptionKey { get; }

        /// <summary>Категория.</summary>
        public ItemCategory Category { get; }

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

        /// <summary>Материал брони или шлема; <c>null</c> у остальных категорий.</summary>
        public ArmorMaterial? ArmorMaterial { get; }

        /// <summary>Множитель прочности материала в формуле износа.</summary>
        public double DurabilityFactor { get; }

        /// <summary>Качество предмета: множитель износа, по умолчанию 1.</summary>
        public double Quality { get; }

        /// <summary>Калибр патронов; только для категории <c>ammo</c>.</summary>
        public string? Caliber { get; }

        /// <summary>
        /// <c>true</c>, если предмет нельзя потерять при смерти: он остаётся в инвентаре
        /// или в слоте независимо от страховки и режима полной потери. По умолчанию <c>false</c>.
        /// </summary>
        public bool Unlosable { get; }

        /// <summary>Строгое чтение: любое нарушение схемы — <see cref="DataException"/> с id и полем.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Описание предмета.</returns>
        /// <exception cref="DataException">Поле отсутствует или имеет неверный тип.</exception>
        public static ItemTemplate FromJson(JsonValue value, string sourcePath)
        {
            var id = DataMap.RequireString(value, "id", sourcePath);
            var categoryCode = DataMap.RequireString(value, "category", sourcePath);
            if (!SchemaEnum.TryParse(categoryCode, out ItemCategory category))
            {
                throw new DataException(sourcePath, CoreErrors.UnknownItemCategory, categoryCode);
            }

            var footprint = DataMap.RequireIntPair(value, "footprint", sourcePath);
            var weapon = value.TryGet("weapon", out var weaponValue) ? ReadWeapon(weaponValue, id, sourcePath) : null;

            if (category == ItemCategory.Weapon && weapon == null)
            {
                throw new DataException(sourcePath, CoreErrors.WeaponWithoutBlock, id);
            }

            var price = value.TryGet("price", out var priceValue) ? ReadPrice(priceValue, sourcePath) : null;
            var artifactType = DataMap.OptionalString(value, "artifactType");

            var caliber = DataMap.OptionalString(value, "caliber");
            if (category == ItemCategory.Ammo && string.IsNullOrWhiteSpace(caliber))
            {
                throw new DataException(sourcePath, CoreErrors.MissingStringField, "caliber");
            }

            if (category == ItemCategory.Artifact && string.IsNullOrWhiteSpace(artifactType))
            {
                throw new DataException(sourcePath, CoreErrors.ArtifactWithoutType, id);
            }

            ArmorMaterial? armorMaterial = null;
            double durabilityFactor = 0;
            double quality = 0;
            if (category == ItemCategory.Armor || category == ItemCategory.Helmet)
            {
                var materialCode = DataMap.RequireString(value, "material", sourcePath);
                if (!SchemaEnum.TryParse(materialCode, out ArmorMaterial material))
                {
                    throw new DataException(sourcePath, CoreErrors.UnknownMaterial, materialCode);
                }

                armorMaterial = material;
                durabilityFactor = DataMap.RequireDouble(value, "durabilityFactor", sourcePath);
                quality = DataMap.RequireDouble(value, "quality", sourcePath);
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
                durabilityFactor,
                quality,
                caliber,
                DataMap.OptionalBool(value, "unlosable", false));
        }

        /// <summary>Слоты экипировки по категории, если в данных не заданы явно.</summary>
        /// <param name="category">Категория предмета.</param>
        /// <returns>Список id слотов.</returns>
        public static IReadOnlyList<string> DefaultEquipSlots(ItemCategory category)
        {
            switch (category)
            {
                case ItemCategory.Weapon: return new[] { EquipmentSlots.Id(EquipmentSlot.Main) };
                case ItemCategory.Helmet: return new[] { EquipmentSlots.Id(EquipmentSlot.Helmet) };
                case ItemCategory.Armor: return new[] { EquipmentSlots.Id(EquipmentSlot.Armor) };
                case ItemCategory.Backpack: return new[] { EquipmentSlots.Id(EquipmentSlot.Backpack) };
                case ItemCategory.Belt: return new[] { EquipmentSlots.Id(EquipmentSlot.Belt) };
                case ItemCategory.Consumable: return new[] { EquipmentSlots.Quick };
                default: return Array.Empty<string>();
            }
        }

        private static IReadOnlyList<string> ReadEquipSlots(JsonValue value, ItemCategory category)
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
                DataMap.RequireString(value, "damageTypeId", sourcePath),
                DataMap.RequireInt(value, "durabilityMax", sourcePath),
                DataMap.RequireDouble(value, "durabilityPerShot", sourcePath),
            DataMap.RequireDouble(value, "armorPenetration", sourcePath),
                DataMap.OptionalStringArray(value, "skills"));
        }

        private static PriceBlock ReadPrice(JsonValue value, string sourcePath)
        {
            var modeCode = DataMap.RequireString(value, "mode", sourcePath);
            if (!SchemaEnum.TryParse(modeCode, out PriceMode mode))
            {
                throw new DataException(sourcePath, CoreErrors.UnknownPriceMode, modeCode);
            }

            return new PriceBlock(
                mode,
                DataMap.OptionalDouble(value, "buy"),
                DataMap.OptionalDouble(value, "sell"),
                DataMap.OptionalDouble(value, "base"),
                DataMap.OptionalString(value, "factionId"));
        }
    }
}
