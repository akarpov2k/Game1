#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Services
{
    /// <summary>Переходы по сгенерированному миру: проверка выносливости и ходов.</summary>
    public sealed class TravelService : ITravelService
    {
        /// <inheritdoc />
        public IReadOnlyList<WorldEdge> GetAvailableEdges(WorldState state)
        {
            var result = new List<WorldEdge>();
            foreach (var edge in state.Map.EdgesFrom(state.CurrentLocationId))
            {
                if (!edge.Hidden)
                {
                    result.Add(edge);
                }
            }

            return result;
        }

        /// <inheritdoc />
        public bool CanTravel(WorldState state, string edgeId) => Check(state, edgeId) == TravelStatus.Success;

        /// <inheritdoc />
        public TravelResult Travel(WorldState state, string edgeId)
        {
            var status = Check(state, edgeId);
            if (status != TravelStatus.Success || !state.Map.TryGetEdge(edgeId, out var edge) || edge == null)
            {
                return new TravelResult(status);
            }

            state.TrySpendStamina(edge.Cost.Stamina);
            state.Clock.Advance(edge.Cost.Hours);
            state.MoveTo(edge.ToId);
            return new TravelResult(TravelStatus.Success, edge);
        }

        private static TravelStatus Check(WorldState state, string edgeId)
        {
            if (!state.Map.TryGetEdge(edgeId, out var edge) || edge == null)
            {
                return TravelStatus.EdgeNotFound;
            }

            if (edge.FromId != state.CurrentLocationId)
            {
                return TravelStatus.WrongLocation;
            }

            if (state.Stamina < edge.Cost.Stamina)
            {
                return TravelStatus.NotEnoughStamina;
            }

            return TravelStatus.Success;
        }
    }
}
