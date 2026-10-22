#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Loadouts
{
    /// <summary>Подготовленный набор: предметы в инвентарь и предметы сразу в слоты.</summary>
    public sealed class LoadoutTemplate
    {
        /// <summary>Создаёт набор снаряжения.</summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="nameKey">Ключ названия.</param>
        /// <param name="descriptionKey">Ключ описания или <c>null</c>.</param>
        /// <param name="items">Предметы в стартовый инвентарь.</param>
        /// <param name="equipment">Предметы в слоты.</param>
        /// <exception cref="ArgumentException">Id или ключ названия пуст.</exception>
        /// <exception cref="ArgumentNullException">Списки не заданы.</exception>
        public LoadoutTemplate(
            string id,
            string nameKey,
            string? descriptionKey,
            IReadOnlyList<LoadoutItem> items,
            IReadOnlyDictionary<EquipmentSlot, string> equipment)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("id набора не задан", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(nameKey))
            {
                throw new ArgumentException("nameKey набора не задан", nameof(nameKey));
            }

            Id = id;
            NameKey = nameKey;
            DescriptionKey = descriptionKey;
            Items = items ?? throw new ArgumentNullException(nameof(items));
            Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
        }

        /// <summary>Идентификатор набора.</summary>
        public string Id { get; }

        /// <summary>Ключ названия.</summary>
        public string NameKey { get; }

        /// <summary>Ключ описания или <c>null</c>.</summary>
        public string? DescriptionKey { get; }

        /// <summary>Предметы в стартовый инвентарь.</summary>
        public IReadOnlyList<LoadoutItem> Items { get; }

        /// <summary>Предметы, которые сразу надеваются в слоты.</summary>
        public IReadOnlyDictionary<EquipmentSlot, string> Equipment { get; }

        /// <summary>Читает набор из записи данных.</summary>
        /// <param name="value">Запись вида <c>loadouts</c>.</param>
        /// <param name="path">Путь для сообщения об ошибке.</param>
        /// <returns>Набор снаряжения.</returns>
        /// <exception cref="DataException">Поля отсутствуют, слот неизвестен или массив битый.</exception>
        public static LoadoutTemplate FromJson(JsonValue value, string path)
        {
            var id = DataMap.RequireString(value, "id", path);
            var nameKey = DataMap.RequireString(value, "nameKey", path);
            var descriptionKey = DataMap.OptionalString(value, "descriptionKey");
            var items = ReadItems(value, path);
            var equipment = ReadEquipment(value, path);
            return new LoadoutTemplate(id, nameKey, descriptionKey, items, equipment);
        }

        private static IReadOnlyList<LoadoutItem> ReadItems(JsonValue value, string path)
        {
            var field = value.TryGet("consumables", out var consumables) ? "consumables" : "items";
            if (!value.TryGet(field, out var itemsValue))
            {
                return new List<LoadoutItem>();
            }

            if (!itemsValue.IsArray)
            {
                throw new DataException(path, CoreErrors.MissingArrayField, field);
            }

            var result = new List<LoadoutItem>();
            foreach (var item in itemsValue.Items)
            {
                if (!item.IsObject)
                {
                    throw new DataException(path, CoreErrors.JsonNotObject, field);
                }

                result.Add(LoadoutItem.FromJson(item, path));
            }

            return result;
        }

        private static IReadOnlyDictionary<EquipmentSlot, string> ReadEquipment(JsonValue value, string path)
        {
            var result = new Dictionary<EquipmentSlot, string>();
            if (value.TryGet("equipment", out var equipmentValue))
            {
                if (!equipmentValue.IsObject)
                {
                    throw new DataException(path, CoreErrors.LoadoutEquipment, "equipment");
                }

                foreach (var pair in equipmentValue.Members)
                {
                    if (!EquipmentSlots.TryParse(pair.Key, out var slot))
                    {
                        throw new DataException(path, CoreErrors.LoadoutSlot, pair.Key);
                    }

                    if (pair.Value.Kind != JsonKind.String || string.IsNullOrWhiteSpace(pair.Value.Text))
                    {
                        throw new DataException(path, CoreErrors.MissingStringField, "equipment");
                    }

                    result[slot] = pair.Value.Text;
                }
            }

            ReadSlot(value, "weapon", EquipmentSlot.Main, result, path);
            ReadSlot(value, "secondaryWeapon", EquipmentSlot.Secondary, result, path);
            ReadSlot(value, "armor", EquipmentSlot.Armor, result, path);
            ReadSlot(value, "helmet", EquipmentSlot.Helmet, result, path);
            ReadSlot(value, "backpack", EquipmentSlot.Backpack, result, path);
            ReadSlot(value, "belt", EquipmentSlot.Belt, result, path);
            return result;
        }

        private static void ReadSlot(
            JsonValue value,
            string field,
            EquipmentSlot slot,
            Dictionary<EquipmentSlot, string> result,
            string path)
        {
            if (!value.TryGet(field, out var item) || item.Kind == JsonKind.Null)
            {
                return;
            }

            if (item.Kind != JsonKind.String || string.IsNullOrWhiteSpace(item.Text))
            {
                throw new DataException(path, CoreErrors.MissingStringField, field);
            }

            result[slot] = item.Text;
        }
    }
}
