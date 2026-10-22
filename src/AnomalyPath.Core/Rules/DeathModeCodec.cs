#nullable enable
namespace AnomalyPath.Core.Rules
{
    /// <summary>Кодек режима смерти: в JSON значение записано в camelCase.</summary>
    public static class DeathModeCodec
    {
        /// <summary>Разбирает значение поля <c>deathMode</c>.</summary>
        /// <param name="code">Значение из данных.</param>
        /// <param name="mode">Режим смерти.</param>
        /// <returns><c>true</c>, если значение известно.</returns>
        public static bool TryParse(string code, out DeathMode mode)
        {
            switch (code)
            {
                case "hubReturn":
                    mode = DeathMode.HubReturn;
                    return true;
                case "permadeath":
                    mode = DeathMode.Permadeath;
                    return true;
                default:
                    mode = default;
                    return false;
            }
        }

        /// <summary>Возвращает значение для JSON.</summary>
        /// <param name="mode">Режим смерти.</param>
        /// <returns>Код режима.</returns>
        public static string ToCode(DeathMode mode) =>
            mode == DeathMode.HubReturn ? "hubReturn" : "permadeath";
    }
}
