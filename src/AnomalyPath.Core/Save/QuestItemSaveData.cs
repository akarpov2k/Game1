#nullable enable
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Save
{
    /// <summary>Требуемый предмет квеста в сохранении.</summary>
    public sealed class QuestItemSaveData
    {
        /// <summary>Создаёт запись предмета.</summary>
        /// <param name="itemId">Идентификатор предмета.</param>
        /// <param name="count">Количество, не меньше единицы.</param>
        public QuestItemSaveData(string itemId, int count)
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

        /// <summary>Читает запись предмета.</summary>
        /// <param name="value">JSON-объект.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Запись предмета.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static QuestItemSaveData FromJson(JsonValue value, string sourcePath)
        {
            var itemId = DataMap.RequireString(value, "itemId", sourcePath);
            var count = DataMap.RequireInt(value, "count", sourcePath);
            if (count < 1)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "items");
            }

            return new QuestItemSaveData(itemId, count);
        }
    }
}
