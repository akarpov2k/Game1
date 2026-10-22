#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Save
{
    /// <summary>Сохранённая сетка инвентаря.</summary>
    public sealed class InventorySaveData
    {
        /// <summary>Создаёт запись сетки.</summary>
        /// <param name="id">Id контейнера.</param>
        /// <param name="width">Ширина.</param>
        /// <param name="rows">Высота.</param>
        /// <param name="placements">Размещения предметов.</param>
        public InventorySaveData(
            string id,
            int width,
            int rows,
            IReadOnlyList<PlacedItemSaveData> placements)
        {
            Id = id;
            Width = width;
            Rows = rows;
            Placements = placements;
        }

        /// <summary>Id контейнера.</summary>
        public string Id { get; }

        /// <summary>Ширина.</summary>
        public int Width { get; }

        /// <summary>Высота.</summary>
        public int Rows { get; }

        /// <summary>Размещения предметов.</summary>
        public IReadOnlyList<PlacedItemSaveData> Placements { get; }

        /// <summary>Сериализует сетку.</summary>
        /// <returns>JSON-объект.</returns>
        public JsonValue ToJson()
        {
            var value = JsonValue.NewObject();
            value.Members["id"] = JsonValue.FromString(Id);
            value.Members["width"] = JsonValue.FromNumber(Width);
            value.Members["rows"] = JsonValue.FromNumber(Rows);
            var placements = JsonValue.NewArray();
            foreach (var placement in Placements)
            {
                placements.Items.Add(placement.ToJson());
            }

            value.Members["placements"] = placements;
            return value;
        }

        /// <summary>Читает сетку.</summary>
        /// <param name="value">JSON-объект.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Запись сетки.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static InventorySaveData FromJson(JsonValue value, string sourcePath)
        {
            var id = DataMap.RequireString(value, "id", sourcePath);
            var width = DataMap.RequireInt(value, "width", sourcePath);
            var rows = DataMap.RequireInt(value, "rows", sourcePath);
            if (width < 1 || rows < 1)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "inventory");
            }

            var placements = new List<PlacedItemSaveData>();
            if (value.TryGet("placements", out var array) && array.IsArray)
            {
                foreach (var item in array.Items)
                {
                    if (!item.IsObject)
                    {
                        throw new DataException(sourcePath, CoreErrors.JsonNotObject);
                    }

                    placements.Add(PlacedItemSaveData.FromJson(item, sourcePath));
                }
            }

            return new InventorySaveData(id, width, rows, placements);
        }
    }
}
