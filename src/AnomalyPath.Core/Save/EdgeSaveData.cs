#nullable enable
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Save
{
    /// <summary>Сохранённое ребро перехода.</summary>
    public sealed class EdgeSaveData
    {
        /// <summary>Создаёт запись ребра.</summary>
        /// <param name="edgeId">Id ребра.</param>
        /// <param name="fromId">Откуда.</param>
        /// <param name="toId">Куда.</param>
        /// <param name="via">Способ перехода.</param>
        /// <param name="length">Длина.</param>
        /// <param name="stamina">Стоимость в выносливости.</param>
        /// <param name="hours">Стоимость в часах.</param>
        /// <param name="oneWay">Одностороннее.</param>
        /// <param name="hidden">Скрытое.</param>
        public EdgeSaveData(
            string edgeId,
            string fromId,
            string toId,
            string via,
            int length,
            int stamina,
            double hours,
            bool oneWay,
            bool hidden)
        {
            EdgeId = edgeId;
            FromId = fromId;
            ToId = toId;
            Via = via;
            Length = length;
            Stamina = stamina;
            Hours = hours;
            OneWay = oneWay;
            Hidden = hidden;
        }

        /// <summary>Id ребра.</summary>
        public string EdgeId { get; }

        /// <summary>Откуда.</summary>
        public string FromId { get; }

        /// <summary>Куда.</summary>
        public string ToId { get; }

        /// <summary>Способ перехода.</summary>
        public string Via { get; }

        /// <summary>Длина.</summary>
        public int Length { get; }

        /// <summary>Стоимость в выносливости.</summary>
        public int Stamina { get; }

        /// <summary>Стоимость в часах.</summary>
        public double Hours { get; }

        /// <summary>Одностороннее.</summary>
        public bool OneWay { get; }

        /// <summary>Скрытое.</summary>
        public bool Hidden { get; }

        /// <summary>Сериализует ребро.</summary>
        /// <returns>JSON-объект.</returns>
        public JsonValue ToJson()
        {
            var value = JsonValue.NewObject();
            value.Members["edgeId"] = JsonValue.FromString(EdgeId);
            value.Members["fromId"] = JsonValue.FromString(FromId);
            value.Members["toId"] = JsonValue.FromString(ToId);
            value.Members["via"] = JsonValue.FromString(Via);
            value.Members["length"] = JsonValue.FromNumber(Length);
            value.Members["stamina"] = JsonValue.FromNumber(Stamina);
            value.Members["hours"] = JsonValue.FromNumber(Hours);
            value.Members["oneWay"] = JsonValue.FromBool(OneWay);
            value.Members["hidden"] = JsonValue.FromBool(Hidden);
            return value;
        }

        /// <summary>Читает ребро.</summary>
        /// <param name="value">JSON-объект.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Запись ребра.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static EdgeSaveData FromJson(JsonValue value, string sourcePath)
        {
            var edgeId = DataMap.RequireString(value, "edgeId", sourcePath);
            var fromId = DataMap.RequireString(value, "fromId", sourcePath);
            var toId = DataMap.RequireString(value, "toId", sourcePath);
            var via = DataMap.RequireString(value, "via", sourcePath);
            if (!ViaRules.IsKnown(via))
            {
                throw new DataException(sourcePath, CoreErrors.WorldUnknownVia, via);
            }

            var length = DataMap.RequireInt(value, "length", sourcePath);
            var stamina = DataMap.RequireInt(value, "stamina", sourcePath);
            var hours = DataMap.RequireDouble(value, "hours", sourcePath);
            var oneWay = DataMap.OptionalBool(value, "oneWay", false);
            var hidden = DataMap.OptionalBool(value, "hidden", false);
            if (length < 1 || stamina < 0 || hours < 0 || double.IsNaN(hours) || double.IsInfinity(hours))
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "edge");
            }

            return new EdgeSaveData(edgeId, fromId, toId, via, length, stamina, hours, oneWay, hidden);
        }
    }
}
