#nullable enable
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Save
{
    /// <summary>Сохранённые потребности.</summary>
    public sealed class NeedsSaveData
    {
        /// <summary>Создаёт запись потребностей.</summary>
        /// <param name="hunger">Сытость.</param>
        /// <param name="thirst">Питьё.</param>
        /// <param name="sleep">Сон.</param>
        /// <param name="max">Максимум.</param>
        public NeedsSaveData(int hunger, int thirst, int sleep, int max)
        {
            Hunger = hunger;
            Thirst = thirst;
            Sleep = sleep;
            Max = max;
        }

        /// <summary>Сытость.</summary>
        public int Hunger { get; }

        /// <summary>Питьё.</summary>
        public int Thirst { get; }

        /// <summary>Сон.</summary>
        public int Sleep { get; }

        /// <summary>Максимум.</summary>
        public int Max { get; }

        /// <summary>Сериализует потребности.</summary>
        /// <returns>JSON-объект.</returns>
        public JsonValue ToJson()
        {
            var value = JsonValue.NewObject();
            value.Members["hunger"] = JsonValue.FromNumber(Hunger);
            value.Members["thirst"] = JsonValue.FromNumber(Thirst);
            value.Members["sleep"] = JsonValue.FromNumber(Sleep);
            value.Members["max"] = JsonValue.FromNumber(Max);
            return value;
        }

        /// <summary>Читает потребности.</summary>
        /// <param name="value">JSON-объект.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Запись потребностей.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static NeedsSaveData FromJson(JsonValue value, string sourcePath)
        {
            var hunger = DataMap.RequireInt(value, "hunger", sourcePath);
            var thirst = DataMap.RequireInt(value, "thirst", sourcePath);
            var sleep = DataMap.RequireInt(value, "sleep", sourcePath);
            var max = DataMap.RequireInt(value, "max", sourcePath);
            if (max < 1 || hunger < 0 || hunger > max || thirst < 0 || thirst > max || sleep < 0 || sleep > max)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "needs");
            }

            return new NeedsSaveData(hunger, thirst, sleep, max);
        }
    }
}
