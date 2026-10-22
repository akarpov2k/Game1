#nullable enable
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.World
{
    /// <summary>Преобразование погоды в код сохранения и обратно.</summary>
    public static class WeatherCodec
    {
        /// <summary>Код погоды для сохранения.</summary>
        /// <param name="weather">Погода.</param>
        /// <returns><c>clear</c>, <c>rain</c> или <c>fog</c>.</returns>
        public static string ToCode(WeatherKind weather) => weather switch
        {
            WeatherKind.Clear => "clear",
            WeatherKind.Rain => "rain",
            WeatherKind.Fog => "fog",
            _ => "clear"
        };

        /// <summary>Читает погоду из кода.</summary>
        /// <param name="code">Код погоды.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Погода.</returns>
        /// <exception cref="DataException">Код неизвестен.</exception>
        public static WeatherKind FromCode(string code, string sourcePath) => code switch
        {
            "clear" => WeatherKind.Clear,
            "rain" => WeatherKind.Rain,
            "fog" => WeatherKind.Fog,
            _ => throw new DataException(sourcePath, CoreErrors.SaveUnknownWeather, code)
        };
    }
}
