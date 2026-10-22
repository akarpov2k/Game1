#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Save
{
    /// <summary>Сохранённая экипировка: фиксированные, быстрые слоты и артефакты.</summary>
    public sealed class EquipmentSaveData
    {
        /// <summary>Создаёт запись экипировки.</summary>
        /// <param name="ownerId">Владелец.</param>
        /// <param name="slots">Фиксированные слоты.</param>
        /// <param name="quickSlots">Быстрые слоты: id или <c>null</c>.</param>
        /// <param name="artifacts">Артефакты: id или <c>null</c>.</param>
        public EquipmentSaveData(
            string ownerId,
            IReadOnlyList<EquipmentSlotSaveData> slots,
            IReadOnlyList<string?> quickSlots,
            IReadOnlyList<string?> artifacts)
        {
            OwnerId = ownerId;
            Slots = slots;
            QuickSlots = quickSlots;
            Artifacts = artifacts;
        }

        /// <summary>Владелец.</summary>
        public string OwnerId { get; }

        /// <summary>Фиксированные слоты.</summary>
        public IReadOnlyList<EquipmentSlotSaveData> Slots { get; }

        /// <summary>Быстрые слоты: id или <c>null</c>.</summary>
        public IReadOnlyList<string?> QuickSlots { get; }

        /// <summary>Артефакты: id или <c>null</c>.</summary>
        public IReadOnlyList<string?> Artifacts { get; }

        /// <summary>Сериализует экипировку.</summary>
        /// <returns>JSON-объект.</returns>
        public JsonValue ToJson()
        {
            var value = JsonValue.NewObject();
            value.Members["ownerId"] = JsonValue.FromString(OwnerId);
            var slots = JsonValue.NewArray();
            foreach (var slot in Slots)
            {
                slots.Items.Add(slot.ToJson());
            }

            value.Members["slots"] = slots;
            value.Members["quickSlots"] = WriteIds(QuickSlots);
            value.Members["artifacts"] = WriteIds(Artifacts);
            return value;
        }

        /// <summary>Читает экипировку.</summary>
        /// <param name="value">JSON-объект.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Запись экипировки.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static EquipmentSaveData FromJson(JsonValue value, string sourcePath)
        {
            var ownerId = DataMap.RequireString(value, "ownerId", sourcePath);
            var slots = new List<EquipmentSlotSaveData>();
            if (value.TryGet("slots", out var slotsValue) && slotsValue.IsArray)
            {
                foreach (var item in slotsValue.Items)
                {
                    if (!item.IsObject)
                    {
                        throw new DataException(sourcePath, CoreErrors.JsonNotObject);
                    }

                    slots.Add(EquipmentSlotSaveData.FromJson(item, sourcePath));
                }
            }

            return new EquipmentSaveData(
                ownerId,
                slots,
                ReadIds(value, "quickSlots", sourcePath),
                ReadIds(value, "artifacts", sourcePath));
        }

        private static JsonValue WriteIds(IReadOnlyList<string?> ids)
        {
            var array = JsonValue.NewArray();
            foreach (var id in ids)
            {
                array.Items.Add(id == null ? JsonValue.Null() : JsonValue.FromString(id));
            }

            return array;
        }

        private static IReadOnlyList<string?> ReadIds(JsonValue value, string field, string sourcePath)
        {
            var result = new List<string?>();
            if (!value.TryGet(field, out var array) || !array.IsArray)
            {
                return result;
            }

            foreach (var item in array.Items)
            {
                if (item.Kind == JsonKind.Null)
                {
                    result.Add(null);
                }
                else if (item.Kind == JsonKind.String)
                {
                    result.Add(item.Text);
                }
                else
                {
                    throw new DataException(sourcePath, CoreErrors.SaveMissingField, field);
                }
            }

            return result;
        }
    }
}
