#nullable enable
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Save
{
    /// <summary>Запись остатка товара торговца в сохранении.</summary>
    public sealed class TraderStockSaveData
    {
        /// <summary>Создаёт запись остатка.</summary>
        /// <param name="itemId">Идентификатор предмета.</param>
        /// <param name="count">Количество, не меньше единицы.</param>
        public TraderStockSaveData(string itemId, int count)
        {
            ItemId = itemId;
            Count = count;
        }

        /// <summary>Идентификатор предмета.</summary>
        public string ItemId { get; }

        /// <summary>Количество.</summary>
        public int Count { get; }

        /// <summary>Сериализует запись.</summary>
        /// <returns>JSON-объект.</returns>
        public JsonValue ToJson()
        {
            var value = JsonValue.NewObject();
            value.Members["itemId"] = JsonValue.FromString(ItemId);
            value.Members["count"] = JsonValue.FromNumber(Count);
            return value;
        }

        /// <summary>Читает запись остатка.</summary>
        /// <param name="value">JSON-объект.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Запись остатка.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static TraderStockSaveData FromJson(JsonValue value, string sourcePath)
        {
            var itemId = DataMap.RequireString(value, "itemId", sourcePath);
            var count = DataMap.RequireInt(value, "count", sourcePath);
            if (count < 1)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "stock");
            }

            return new TraderStockSaveData(itemId, count);
        }
    }
}
