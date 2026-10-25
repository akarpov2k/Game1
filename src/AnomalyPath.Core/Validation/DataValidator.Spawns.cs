#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Loot;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Spawns;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Validation
{
    /// <summary>Часть класса <see cref="DataValidator"/>: объекты локаций.</summary>
    public sealed partial class DataValidator
    {
        private static void ValidateSpawns(LoadedData data, List<ValidationIssue> issues)
        {
            SpawnRepository spawns;
            LootRepository loot;
            ActorRepository actors;
            try
            {
                spawns = SpawnRepository.FromLoadedData(data);
                loot = LootRepository.FromLoadedData(data);
                actors = ActorRepository.FromLoadedData(data);
            }
            catch (DataException)
            {
                return;
            }

            foreach (var spawn in spawns.All)
            {
                if (spawn.LootTableId != null && !loot.TryGet(spawn.LootTableId, out _))
                {
                    issues.Add(new ValidationIssue(
                        spawn.Id,
                        CoreErrors.LootTableNotFound,
                        CoreText.Format(CoreErrors.LootTableNotFound, spawn.LootTableId)));
                }

                if (spawn.ActorId == null)
                {
                    continue;
                }

                if (!actors.Contains(spawn.ActorId))
                {
                    issues.Add(new ValidationIssue(
                        spawn.Id,
                        CoreErrors.ActorNotFound,
                        CoreText.Format(CoreErrors.ActorNotFound, spawn.ActorId)));
                    continue;
                }

                var actor = actors.Get(spawn.ActorId);
                if (actor.Type != ActorType.Npc && actor.Type != ActorType.Enemy)
                {
                    issues.Add(new ValidationIssue(
                        spawn.Id,
                        CoreErrors.SpawnActorType,
                        CoreText.Format(CoreErrors.SpawnActorType, spawn.ActorId)));
                }
            }

            foreach (var record in data.All("locations"))
            {
                LocationTemplate template;
                try
                {
                    template = LocationTemplate.FromJson(record, "locations");
                }
                catch (DataException)
                {
                    continue;
                }

                var min = template.ObjectCount.Min;
                var max = template.ObjectCount.Max;
                if ((min == 0 && max == 0) || (min == 1 && max == 1))
                {
                    continue;
                }

                issues.Add(new ValidationIssue(
                    template.Id,
                    CoreErrors.SpawnObjectCount,
                    CoreText.Format(CoreErrors.SpawnObjectCount, template.Id)));
            }
        }
    }
}
