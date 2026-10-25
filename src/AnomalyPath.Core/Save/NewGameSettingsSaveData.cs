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
        /// <param name="xpLossChance">
        /// Доля опыта текущего уровня, теряемая при смерти, 0..1; <c>null</c> — в старом сейве
        /// поля нет, загрузчик подставит значение из правил.
        /// </param>
        public NewGameSettingsSaveData(
            double insuranceChance,
            double tradeDifficulty,
            bool permadeath = false,
            double? xpLossChance = null)
        {
            InsuranceChance = insuranceChance;
            TradeDifficulty = tradeDifficulty;
            Permadeath = permadeath;
            XpLossChance = xpLossChance;
        }

        /// <summary>Коэффициент страховки надетых вещей, 0..1.</summary>
        public double InsuranceChance { get; }

        /// <summary>Коэффициент усложнения торговли.</summary>
        public double TradeDifficulty { get; }

        /// <summary><c>true</c>, если выбран режим постоянной смерти.</summary>
        public bool Permadeath { get; }

        /// <summary>Доля опыта текущего уровня, теряемая при смерти; <c>null</c> в старых сейвах.</summary>
        public double? XpLossChance { get; }

        /// <summary>Сериализует настройки.</summary>
        /// <returns>JSON-объект.</returns>
        public JsonValue ToJson()
        {
            var value = JsonValue.NewObject();
            value.Members["insuranceChance"] = JsonValue.FromNumber(InsuranceChance);
            value.Members["tradeDifficulty"] = JsonValue.FromNumber(TradeDifficulty);
            value.Members["permadeath"] = JsonValue.FromBool(Permadeath);
            if (XpLossChance.HasValue)
            {
                value.Members["xpLossChance"] = JsonValue.FromNumber(XpLossChance.Value);
            }

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

            var xpLossChance = DataMap.OptionalDouble(value, "xpLossChance");
            if (xpLossChance.HasValue && (xpLossChance.Value < 0 || xpLossChance.Value > 1))
            {
                throw new DataException(sourcePath, CoreErrors.SaveSettings, "xpLossChance");
            }

            return new NewGameSettingsSaveData(
                insuranceChance,
                tradeDifficulty,
                DataMap.OptionalBool(value, "permadeath", false),
                xpLossChance);
        }
    }
}
