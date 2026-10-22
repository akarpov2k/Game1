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
            if (!value.TryGet("items", out var itemsValue) || !itemsValue.IsArray)
            {
                throw new DataException(path, CoreErrors.MissingArrayField, "items");
            }

            var result = new List<LoadoutItem>();
            foreach (var item in itemsValue.Items)
            {
                if (!item.IsObject)
                {
                    throw new DataException(path, CoreErrors.JsonNotObject, "items");
                }

                result.Add(LoadoutItem.FromJson(item, path));
            }

            return result;
        }

        private static IReadOnlyDictionary<EquipmentSlot, string> ReadEquipment(JsonValue value, string path)
        {
            if (!value.TryGet("equipment", out var equipmentValue) || !equipmentValue.IsObject)
            {
                throw new DataException(path, CoreErrors.LoadoutEquipment, "equipment");
            }

            var result = new Dictionary<EquipmentSlot, string>();
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

            return result;
        }
    }
}
