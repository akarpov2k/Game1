#nullable enable
using System;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Loadouts
{
    /// <summary>Предмет набора, который кладётся в стартовый инвентарь.</summary>
    public sealed class LoadoutItem
    {
        /// <summary>Создаёт предмет набора.</summary>
        /// <param name="itemId">Id шаблона предмета.</param>
        /// <param name="count">Количество, не меньше 1.</param>
        /// <exception cref="ArgumentException">Id пуст.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Количество меньше 1.</exception>
        public LoadoutItem(string itemId, int count)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(itemId)),
                    nameof(itemId));
            }

            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(count),
                    CoreText.Format(CoreErrors.MinimumOne, nameof(count)));
            }

            ItemId = itemId;
            Count = count;
        }

        /// <summary>Id шаблона предмета.</summary>
        public string ItemId { get; }

        /// <summary>Количество.</summary>
        public int Count { get; }

        /// <summary>Читает предмет набора из записи данных.</summary>
        /// <param name="value">Запись предмета.</param>
        /// <param name="path">Путь для сообщения об ошибке.</param>
        /// <returns>Предмет набора.</returns>
        /// <exception cref="DataException">Поля отсутствуют или количество меньше 1.</exception>
        public static LoadoutItem FromJson(JsonValue value, string path)
        {
            var itemId = DataMap.RequireString(value, "itemId", path);
            var count = DataMap.RequireInt(value, "count", path);
            if (count < 1)
            {
                throw new DataException(path, CoreErrors.LoadoutCount, itemId);
            }

            return new LoadoutItem(itemId, count);
        }
    }
}
