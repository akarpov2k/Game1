#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Quests;
using AnomalyPath.Core.Rules;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.Templates;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Services
{
    /// <summary>Часть класса <see cref="QuestService"/>: маршрут и локация.</summary>
    public sealed partial class QuestService
    {
        private bool EnsureQuestLocation(
            GameSession session,
            LocationTemplate target,
            QuestState quest,
            IWorldRandom random,
            int serial)
        {
            var map = session.State.World.Map;
            foreach (var existing in map.Locations)
            {
                if (!existing.Cleared &&
                    string.Equals(existing.TemplateId, target.Id, StringComparison.Ordinal))
                {
                    quest.LocationInstanceId = existing.InstanceId;
                    return true;
                }
            }

            var anchorId = session.State.World.CurrentLocationId;
            if (session.State.World.CurrentLocation.NodeType != LocationNodeType.Hub)
            {
                anchorId = map.StartLocationId;
            }

            var anchor = _locations.Get(map.GetLocation(anchorId).TemplateId);
            if (anchor.NodeType != LocationNodeType.Hub)
            {
                return false;
            }

            var route = SelectRoute(anchor, target, random);
            if (route == null)
            {
                return false;
            }

            var routeVias = RouteRules.Vias(anchor, route, _topology.Topology);
            var siteVias = RouteRules.Vias(route, target, _topology.Topology);
            if (routeVias.Count == 0 || siteVias.Count == 0)
            {
                return false;
            }

            var routeInstanceId = "locinst_q" + serial.ToString("D4") + "_r";
            var siteInstanceId = "locinst_q" + serial.ToString("D4") + "_s";
            if (map.ContainsLocation(routeInstanceId) || map.ContainsLocation(siteInstanceId))
            {
                return false;
            }

            var routeInstance = NewInstance(route, routeInstanceId, random);
            var siteInstance = NewInstance(target, siteInstanceId, random);
            map.AddLocation(routeInstance);
            map.AddLocation(siteInstance);

            var viaToRoute = routeVias[random.NextInt(0, routeVias.Count)];
            var viaToSite = siteVias[random.NextInt(0, siteVias.Count)];
            AddEdge(
                map,
                "edge_q" + serial.ToString("D4") + "_a",
                anchorId,
                routeInstanceId,
                viaToRoute);
            AddEdge(
                map,
                "edge_q" + serial.ToString("D4") + "_b",
                routeInstanceId,
                siteInstanceId,
                viaToSite);
            quest.LocationInstanceId = siteInstanceId;
            return true;
        }

        private LocationTemplate? SelectRoute(
            LocationTemplate anchor,
            LocationTemplate target,
            IWorldRandom random)
        {
            var candidates = new List<LocationTemplate>();
            foreach (var template in _locations.All)
            {
                if (template.NodeType != LocationNodeType.Route)
                {
                    continue;
                }

                if (RouteRules.Vias(anchor, template, _topology.Topology).Count == 0 ||
                    RouteRules.Vias(template, target, _topology.Topology).Count == 0)
                {
                    continue;
                }

                candidates.Add(template);
            }

            return candidates.Count == 0
                ? null
                : candidates[random.NextInt(0, candidates.Count)];
        }

        private static LocationInstance NewInstance(LocationTemplate template, string instanceId, IWorldRandom random)
        {
            var seed = random.NextInt(1, int.MaxValue);
            var background = template.Backgrounds[random.NextInt(0, template.Backgrounds.Count)];
            return new LocationInstance(
                instanceId,
                template.Id,
                seed,
                background,
                template.NodeType,
                template.Tier,
                template.Danger);
        }
    }
}
