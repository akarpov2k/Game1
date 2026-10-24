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
        /// <param name="insuranceChance">Коэффициент страховки надетых вещей, 0..1.</param>
        /// <param name="tradeDifficulty">Коэффициент усложнения торговли, не меньше 0.</param>
        /// <param name="permadeath">Выбран ли режим постоянной смерти.</param>
        public NewGameSettingsSaveData(double insuranceChance, double tradeDifficulty, bool permadeath = false)
        {
            InsuranceChance = insuranceChance;
            TradeDifficulty = tradeDifficulty;
            Permadeath = permadeath;
        }

        /// <summary>Коэффициент страховки надетых вещей, 0..1.</summary>
        public double InsuranceChance { get; }

        /// <summary>Коэффициент усложнения торговли.</summary>
        public double TradeDifficulty { get; }

        /// <summary><c>true</c>, если выбран режим постоянной смерти.</summary>
        public bool Permadeath { get; }

        /// <summary>Сериализует настройки.</summary>
        /// <returns>JSON-объект.</returns>
        public JsonValue ToJson()
        {
            var value = JsonValue.NewObject();
            value.Members["insuranceChance"] = JsonValue.FromNumber(InsuranceChance);
            value.Members["tradeDifficulty"] = JsonValue.FromNumber(TradeDifficulty);
            value.Members["permadeath"] = JsonValue.FromBool(Permadeath);
            return value;
        }

        /// <summary>
        /// Читает настройки из сохранения. Сейвы до версии 4 хранили коэффициент страховки
        /// под старым именем <c>lootLossFactor</c>; поле читается для совместимости.
        /// </summary>
        /// <param name="value">JSON-объект.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Снимок настроек.</returns>
        /// <exception cref="DataException">Поля отсутствуют или значения вне границ.</exception>
        public static NewGameSettingsSaveData FromJson(JsonValue value, string sourcePath)
        {
            var insuranceChance = DataMap.OptionalDouble(value, "insuranceChance")
                ?? DataMap.OptionalDouble(value, "lootLossFactor")
                ?? throw new DataException(sourcePath, CoreErrors.SaveMissingField, "insuranceChance");
            if (insuranceChance < 0 || insuranceChance > 1)
            {
                throw new DataException(sourcePath, CoreErrors.SaveSettings, "insuranceChance");
            }

            var tradeDifficulty = DataMap.RequireDouble(value, "tradeDifficulty", sourcePath);
            if (tradeDifficulty < 0)
            {
                throw new DataException(sourcePath, CoreErrors.SaveSettings, "tradeDifficulty");
            }

            return new NewGameSettingsSaveData(
                insuranceChance,
                tradeDifficulty,
                DataMap.OptionalBool(value, "permadeath", false));
        }
    }
}
