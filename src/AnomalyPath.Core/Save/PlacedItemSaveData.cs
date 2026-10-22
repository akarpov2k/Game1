#nullable enable
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Save
{
    /// <summary>Размещение предмета в сетке инвентаря.</summary>
    public sealed class PlacedItemSaveData
    {
        /// <summary>Создаёт запись размещения.</summary>
        /// <param name="instanceId">Id экземпляра.</param>
        /// <param name="x">Колонка.</param>
        /// <param name="y">Строка.</param>
        /// <param name="rotated">Поворот.</param>
        /// <param name="overflow">Лежит в переполнении.</param>
        public PlacedItemSaveData(string instanceId, int x, int y, bool rotated, bool overflow)
        {
            InstanceId = instanceId;
            X = x;
            Y = y;
            Rotated = rotated;
            Overflow = overflow;
        }

        /// <summary>Id экземпляра.</summary>
        public string InstanceId { get; }

        /// <summary>Колонка.</summary>
        public int X { get; }

        /// <summary>Строка.</summary>
        public int Y { get; }

        /// <summary>Поворот.</summary>
        public bool Rotated { get; }

        /// <summary>Лежит в переполнении.</summary>
        public bool Overflow { get; }

        /// <summary>Сериализует размещение.</summary>
        /// <returns>JSON-объект.</returns>
        public JsonValue ToJson()
        {
            var value = JsonValue.NewObject();
            value.Members["instanceId"] = JsonValue.FromString(InstanceId);
            value.Members["x"] = JsonValue.FromNumber(X);
            value.Members["y"] = JsonValue.FromNumber(Y);
            value.Members["rotated"] = JsonValue.FromBool(Rotated);
            value.Members["overflow"] = JsonValue.FromBool(Overflow);
            return value;
        }

        /// <summary>Читает размещение.</summary>
        /// <param name="value">JSON-объект.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Запись размещения.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static PlacedItemSaveData FromJson(JsonValue value, string sourcePath)
        {
            var instanceId = DataMap.RequireString(value, "instanceId", sourcePath);
            var x = DataMap.OptionalInt(value, "x", -1);
            var y = DataMap.OptionalInt(value, "y", -1);
            var rotated = DataMap.OptionalBool(value, "rotated", false);
            var overflow = DataMap.OptionalBool(value, "overflow", false);
            if (x < -1 || y < -1)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "placement");
            }

            return new PlacedItemSaveData(instanceId, x, y, rotated, overflow);
        }
    }
}
