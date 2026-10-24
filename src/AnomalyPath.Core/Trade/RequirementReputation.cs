#nullable enable
namespace AnomalyPath.Core.Trade
{
    /// <summary>Минимальная репутация у фракции для выполнения условия.</summary>
    public sealed class RequirementReputation
    {
        /// <summary>Создаёт условие по репутации.</summary>
        /// <param name="factionId">Идентификатор фракции.</param>
        /// <param name="min">Минимальная репутация.</param>
        public RequirementReputation(string factionId, int min)
        {
            FactionId = factionId;
            Min = min;
        }

        /// <summary>Идентификатор фракции.</summary>
        public string FactionId { get; }

        /// <summary>Минимальная репутация.</summary>
        public int Min { get; }
    }
}
