#nullable enable
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.World
{
    /// <summary>Авторское ребро локации: куда ведёт переход и каким путём.</summary>
    public sealed class ConnectionTemplate
    {
        /// <summary>Создаёт ребро.</summary>
        /// <param name="toId">Куда ведёт переход.</param>
        /// <param name="via">Способ перехода.</param>
        /// <param name="length">Фиксированная длина или <c>null</c>, если длина случайна.</param>
        /// <param name="oneWay">Переход только в одну сторону.</param>
        /// <param name="hidden">Переход скрыт до открытия.</param>
        /// <param name="cost">Стоимость ребра или <c>null</c>, если брать из топологии.</param>
        /// <exception cref="System.ArgumentException">Поля заданы неверно.</exception>
        public ConnectionTemplate(
            string toId,
            string via,
            int? length = null,
            bool oneWay = false,
            bool hidden = false,
            TravelCost? cost = null)
        {
            if (string.IsNullOrWhiteSpace(toId))
            {
                throw new System.ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(toId)),
                    nameof(toId));
            }

            if (!ViaRules.IsKnown(via))
            {
                throw new System.ArgumentException(
                    CoreText.Format(CoreErrors.WorldUnknownVia, via),
                    nameof(via));
            }

            if (length.HasValue && length.Value < 1)
            {
                throw new System.ArgumentException(
                    CoreText.Format(CoreErrors.MinimumOne, nameof(length)),
                    nameof(length));
            }

            ToId = toId;
            Via = via;
            Length = length;
            OneWay = oneWay;
            Hidden = hidden;
            Cost = cost;
        }

        /// <summary>Куда ведёт переход.</summary>
        public string ToId { get; }

        /// <summary>Способ перехода.</summary>
        public string Via { get; }

        /// <summary>Фиксированная длина или <c>null</c>, если длина случайна.</summary>
        public int? Length { get; }

        /// <summary>Переход только в одну сторону.</summary>
        public bool OneWay { get; }

        /// <summary>Переход скрыт до открытия.</summary>
        public bool Hidden { get; }

        /// <summary>Стоимость ребра или <c>null</c>, если брать из топологии.</summary>
        public TravelCost? Cost { get; }

        /// <summary>Читает ребро из записи данных.</summary>
        /// <param name="value">Запись ребра.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Ребро.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static ConnectionTemplate FromJson(JsonValue value, string sourcePath)
        {
            var toId = DataMap.RequireString(value, "toId", sourcePath);
            var via = DataMap.RequireString(value, "via", sourcePath);
            if (!ViaRules.IsKnown(via))
            {
                throw new DataException(sourcePath, CoreErrors.WorldUnknownVia, via);
            }

            var length = DataMap.OptionalInt(value, "length", 0);
            if (value.TryGet("length", out var lengthValue))
            {
                if (lengthValue.Kind != JsonKind.Number)
                {
                    throw new DataException(sourcePath, CoreErrors.MissingNumberField, "length");
                }

                if (length < 1)
                {
                    throw new DataException(sourcePath, CoreErrors.WorldRange, "length");
                }
            }

            TravelCost? cost = null;
            if (value.TryGet("cost", out var costValue) && costValue.IsObject)
            {
                cost = TravelCost.FromJson(costValue, "cost", sourcePath);
            }

            return new ConnectionTemplate(
                toId,
                via,
                length == 0 ? null : length,
                DataMap.OptionalBool(value, "oneWay", false),
                DataMap.OptionalBool(value, "hidden", false),
                cost);
        }
    }
}
