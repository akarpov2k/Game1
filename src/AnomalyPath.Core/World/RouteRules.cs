#nullable enable
using System.Collections.Generic;

namespace AnomalyPath.Core.World
{
    /// <summary>Правила выбора способа перехода между двумя локациями по таблице маршрута.</summary>
    public static class RouteRules
    {
        /// <summary>Способы перехода, разрешённые маршрутом и тегами обеих локаций.</summary>
        /// <param name="from">Исходная локация.</param>
        /// <param name="to">Целевая локация.</param>
        /// <param name="topology">Топология мира.</param>
        /// <returns>Список способов; пустой, если переход невозможен.</returns>
        public static IReadOnlyList<string> Vias(LocationTemplate from, LocationTemplate to, TopologyTemplate topology)
        {
            var result = new List<string>();
            foreach (var rule in topology.Route.Rules(from, to))
            {
                foreach (var via in rule.Via)
                {
                    if (ViaRules.Allows(via, from.Subtype, to.Subtype, from.Tags, to.Tags) &&
                        !result.Contains(via))
                    {
                        result.Add(via);
                    }
                }
            }

            return result;
        }

        /// <summary>Проверяет, возможен ли переход между двумя локациями.</summary>
        /// <param name="from">Исходная локация.</param>
        /// <param name="to">Целевая локация.</param>
        /// <param name="topology">Топология мира.</param>
        /// <returns><c>true</c>, если есть хотя бы один разрешённый способ.</returns>
        public static bool CanTravel(LocationTemplate from, LocationTemplate to, TopologyTemplate topology) =>
            Vias(from, to, topology).Count > 0;
    }
}
