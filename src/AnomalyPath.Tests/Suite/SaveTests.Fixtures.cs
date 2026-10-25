#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.Services;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.Trade;
using AnomalyPath.Core.World;
using InventoryEquipment = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Tests
{
    /// <summary>Часть класса <see cref="SaveTests"/>: фикстуры и помощники.</summary>
    public sealed partial class SaveTests
    {
        private PlayerState NewPlayer(
            IItemRepository items,
            int talentPoints = 0,
            IReadOnlyDictionary<string, int>? talentRanks = null,
            int levelHealthBonus = 0)
        {
            var grid = new GridContainer("player", 8, 4);
            var rifle = new ItemInstance("inst_rifle", "wpn_ak74", 1, 80, 100, "ak74_variant", magazine: 30);
            grid.Place(rifle, items.Get("wpn_ak74"), 0, 0, false);

            var equipment = new InventoryEquipment("player", 4);
            equipment.Set(EquipmentSlot.Main, new ItemInstance("inst_pistol", "wpn_pm", 1, 100, 100));
            equipment.SetQuick(0, new ItemInstance("inst_quick", "wpn_ak74", 1, 100, 100));
            equipment.SetArtifact(0, new ItemInstance("inst_artifact", "art_fire_ember", 1));

            return new PlayerState(
                "player",
                3,
                120,
                90,
                10,
                grid,
                equipment,
                new[] { "skill_test" },
                new NeedsState(80, 70, 90, 100),
                "Игрок",
                1500,
                talentPoints: talentPoints,
                talentRanks: talentRanks,
                levelHealthBonus: levelHealthBonus);
        }

        private void WithSaveRoot(Action<string> body)
        {
            var root = Path.Combine(Path.GetTempPath(), "anomalypath-saves", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                body(root);
            }
            finally
            {
                try
                {
                    Directory.Delete(root, recursive: true);
                }
                catch (IOException)
                {
                    // временная папка не критична для результата теста
                }
            }
        }

        private static WorldEdge FirstPaidEdge(WorldMap map, string fromId)
        {
            foreach (var edge in map.EdgesFrom(fromId))
            {
                if (edge.Cost.Stamina > 0)
                {
                    return edge;
                }
            }

            throw new InvalidOperationException("Нет платного ребра из " + fromId);
        }
    }
}
