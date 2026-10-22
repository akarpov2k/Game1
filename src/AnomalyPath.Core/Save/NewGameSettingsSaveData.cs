#nullable enable
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Save
{
    /// <summary>Снимок настроек новой игры в сохранении.</summary>
    public sealed class NewGameSettingsSaveData
    {
        /// <summary>Создаёт снимок настроек.</summary>
        /// <param name="lootLossFactor">Коэффициент потери лута, 0..1.</param>
        /// <param name="tradeDifficulty">Коэффициент усложнения торговли, не меньше 0.</param>
        public NewGameSettingsSaveData(double lootLossFactor, double tradeDifficulty)
        {
            LootLossFactor = lootLossFactor;
            TradeDifficulty = tradeDifficulty;
        }

        /// <summary>Коэффициент потери лута при смерти.</summary>
        public double LootLossFactor { get; }

        /// <summary>Коэффициент усложнения торговли.</summary>
        public double TradeDifficulty { get; }

        /// <summary>Сериализует настройки.</summary>
        /// <returns>JSON-объект.</returns>
        public JsonValue ToJson()
        {
            var value = JsonValue.NewObject();
            value.Members["lootLossFactor"] = JsonValue.FromNumber(LootLossFactor);
            value.Members["tradeDifficulty"] = JsonValue.FromNumber(TradeDifficulty);
            return value;
        }

        /// <summary>Читает настройки из сохранения.</summary>
        /// <param name="value">JSON-объект.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Снимок настроек.</returns>
        /// <exception cref="DataException">Поля отсутствуют или значения вне границ.</exception>
        public static NewGameSettingsSaveData FromJson(JsonValue value, string sourcePath)
        {
            var lootLossFactor = DataMap.RequireDouble(value, "lootLossFactor", sourcePath);
            if (lootLossFactor < 0 || lootLossFactor > 1)
            {
                throw new DataException(sourcePath, CoreErrors.SaveSettings, "lootLossFactor");
            }

            var tradeDifficulty = DataMap.RequireDouble(value, "tradeDifficulty", sourcePath);
            if (tradeDifficulty < 0)
            {
                throw new DataException(sourcePath, CoreErrors.SaveSettings, "tradeDifficulty");
            }

            return new NewGameSettingsSaveData(lootLossFactor, tradeDifficulty);
        }
    }
}
