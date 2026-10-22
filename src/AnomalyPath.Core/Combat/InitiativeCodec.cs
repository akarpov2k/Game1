#nullable enable
namespace AnomalyPath.Core.Combat
{
    /// <summary>Кодек правила инициативы: в JSON значение записано в camelCase.</summary>
    public static class InitiativeCodec
    {
        /// <summary>Разбирает значение поля <c>initiative</c>.</summary>
        /// <param name="code">Значение из данных.</param>
        /// <param name="initiative">Правило инициативы.</param>
        /// <returns><c>true</c>, если значение известно.</returns>
        public static bool TryParse(string code, out InitiativeKind initiative)
        {
            switch (code)
            {
                case "playerFirst":
                    initiative = InitiativeKind.PlayerFirst;
                    return true;
                case "formula":
                    initiative = InitiativeKind.Formula;
                    return true;
                default:
                    initiative = default;
                    return false;
            }
        }

        /// <summary>Возвращает значение для JSON.</summary>
        /// <param name="initiative">Правило инициативы.</param>
        /// <returns>Код правила.</returns>
        public static string ToCode(InitiativeKind initiative) =>
            initiative == InitiativeKind.PlayerFirst ? "playerFirst" : "formula";
    }
}
