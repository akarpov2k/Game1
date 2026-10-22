#nullable enable
using System.Collections.Generic;

namespace AnomalyPath.Core.World
{
    /// <summary>Правила выбора способа перехода между двумя локациями.</summary>
    public static class RouteRules
    {
        /// <summary>Способы перехода, разрешённые топологией и тегами обеих локаций.</summary>
        /// <param name="from">Исходная локация.</param>
        /// <param name="to">Целевая локация.</param>
        /// <param name="topology">Топология мира.</param>
        /// <returns>Список способов; пустой, если переход невозможен.</returns>
        public static IReadOnlyList<string> Vias(LocationTemplate from, LocationTemplate to, TopologyTemplate topology)
        {
            var result = new List<string>();
            foreach (var rule in topology.Transitions)
            {
                if (rule.From != from.NodeType || rule.To != to.NodeType)
                {
                    continue;
                }

                foreach (var via in rule.Via)
                {
                    if (ViaRules.Allows(via, from.NodeType, to.NodeType, from.Tags, to.Tags) && !result.Contains(via))
                    {
                        result.Add(via);
                    }
                }
            }

            return result;
        }
    }
}
