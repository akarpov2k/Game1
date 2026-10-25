#nullable enable
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Loot
{
    /// <summary>Запись пула лута: какой предмет, с каким шансом и сколько падает.</summary>
    public sealed class LootEntry
    {
        /// <summary>Создаёт запись пула лута.</summary>
        /// <param name="itemId">Id предмета.</param>
        /// <param name="chance">Базовый шанс 0..1; у записи <c>always</c> не используется.</param>
        /// <param name="always">Предмет падает всегда, бросков нет.</param>
        /// <param name="countMin">Минимум единиц в стопке.</param>
        /// <param name="countMax">Максимум единиц в стопке.</param>
        /// <param name="ignoreTierBand">Не проверять полосу тира локации.</param>
        public LootEntry(
            string itemId,
            double chance,
            bool always,
            int countMin,
            int countMax,
            bool ignoreTierBand)
        {
            ItemId = itemId;
            Chance = chance;
            Always = always;
            CountMin = countMin;
            CountMax = countMax;
            IgnoreTierBand = ignoreTierBand;
        }

        /// <summary>Id предмета.</summary>
        public string ItemId { get; }

        /// <summary>Базовый шанс 0..1; умножается на редкость противника.</summary>
        public double Chance { get; }

        /// <summary><c>true</c> — предмет падает всегда, без бросков.</summary>
        public bool Always { get; }

        /// <summary>Минимум единиц в стопке.</summary>
        public int CountMin { get; }

        /// <summary>Максимум единиц в стопке.</summary>
        public int CountMax { get; }

        /// <summary><c>true</c> — полоса тира локации не проверяется.</summary>
        public bool IgnoreTierBand { get; }

        /// <summary>Читает запись пула лута из данных.</summary>
        /// <param name="value">Загруженная запись.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Запись пула.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static LootEntry FromJson(JsonValue value, string sourcePath)
        {
            var itemId = DataMap.RequireString(value, "itemId", sourcePath);
            var always = DataMap.OptionalBool(value, "always", false);

            var chance = 0.0;
            if (value.TryGet("chance", out _))
            {
                chance = DataMap.RequireDouble(value, "chance", sourcePath);
                if (chance < 0 || chance > 1)
                {
                    throw new DataException(sourcePath, CoreErrors.LootChanceRange, itemId);
                }
            }
            else if (!always)
            {
                throw new DataException(sourcePath, CoreErrors.MissingNumberField, "chance");
            }

            var countMin = 1;
            var countMax = 1;
            if (value.TryGet("count", out _))
            {
                var count = DataMap.RequireIntPair(value, "count", sourcePath);
                countMin = count.Item1;
                countMax = count.Item2;
                if (countMin < 1 || countMax < countMin)
                {
                    throw new DataException(sourcePath, CoreErrors.LootCountRange, itemId);
                }
            }

            return new LootEntry(
                itemId,
                chance,
                always,
                countMin,
                countMax,
                DataMap.OptionalBool(value, "ignoreTierBand", false));
        }
    }
}
