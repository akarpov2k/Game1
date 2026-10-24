#nullable enable
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Session;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Итог боя: смерть с респавном, опыт за победу и лут с противника.</summary>
    public interface ICombatOutcomeService
    {
        /// <summary>Завершает бой и применяет его последствия; повторный вызов ничего не меняет.</summary>
        /// <param name="session">Игровая сессия с текущим боем.</param>
        /// <returns>Победа, поражение или признак незавершённого боя.</returns>
        /// <exception cref="System.ArgumentNullException">Сессия не задана.</exception>
        CombatOutcome Resolve(GameSession session);
    }
}
