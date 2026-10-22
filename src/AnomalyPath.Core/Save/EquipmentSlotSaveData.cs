#nullable enable
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;

namespace AnomalyPath.Core.Save
{
    /// <summary>Один фиксированный слот экипировки в сохранении.</summary>
    public sealed class EquipmentSlotSaveData
    {
        /// <summary>Создаёт запись слота.</summary>
        /// <param name="slot">Имя слота.</param>
        /// <param name="instanceId">Id предмета.</param>
        public EquipmentSlotSaveData(string slot, string instanceId)
        {
            Slot = slot;
            InstanceId = instanceId;
        }

        /// <summary>Имя слота.</summary>
        public string Slot { get; }

        /// <summary>Id предмета.</summary>
        public string InstanceId { get; }

        /// <summary>Сериализует слот.</summary>
        /// <returns>JSON-объект.</returns>
        public JsonValue ToJson()
        {
            var value = JsonValue.NewObject();
            value.Members["slot"] = JsonValue.FromString(Slot);
            value.Members["instanceId"] = JsonValue.FromString(InstanceId);
            return value;
        }

        /// <summary>Читает слот.</summary>
        /// <param name="value">JSON-объект.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Запись слота.</returns>
        /// <exception cref="DataException">Поля отсутствуют.</exception>
        public static EquipmentSlotSaveData FromJson(JsonValue value, string sourcePath) =>
            new EquipmentSlotSaveData(
                DataMap.RequireString(value, "slot", sourcePath),
                DataMap.RequireString(value, "instanceId", sourcePath));
    }
}
