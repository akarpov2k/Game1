#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.Services;
using AnomalyPath.Core.Spawns;
using AnomalyPath.Core.World;

namespace AnomalyPath.Tests
{
    /// <summary>Выбор единственного объекта локации: фильтр, вес, детерминизм и роль локации.</summary>
    public sealed class LocationObjectTests : TestBase
    {
        /// <inheritdoc />
        public override void Run()
        {
            SelectionTests();
        }

        private void SelectionTests()
        {
            Check("объект: вес выбирает из подходящих записей", () =>
            {
                var service = Service();
                var location = NewInstance("loc_road");
                var selected = service.Select(null, location, 12345);
                True(selected != null, "объект выбран");
                True(
                    selected!.SpawnId == "spawn_a" || selected.SpawnId == "spawn_b",
                    "один из пула");
            });

            Check("объект: одинаковый seed даёт тот же объект", () =>
            {
                var first = Service().Select(null, NewInstance("loc_road"), 777);
                var second = Service().Select(null, NewInstance("loc_road"), 777);
                Equal(first!.SpawnId, second!.SpawnId, "id");
            });

            Check("объект: разные seed дают разные записи пула", () =>
            {
                var seen = new HashSet<string>();
                var service = Service();
                for (var seed = 1; seed <= 50; seed++)
                {
                    var location = NewInstance("loc_road");
                    seen.Add(service.Select(null, location, seed)!.SpawnId);
                }

                True(seen.Contains("spawn_a") && seen.Contains("spawn_b"), "обе записи встречаются");
            });

            Check("объект: повторный выбор возвращает тот же объект", () =>
            {
                var service = Service();
                var location = NewInstance("loc_road");
                var first = service.Select(null, location, 5);
                var second = service.Select(null, location, 999);
                True(ReferenceEquals(first, second), "тот же экземпляр");
            });

            Check("объект: на хабе объекта нет", () =>
            {
                var location = new LocationInstance(
                    "locinst_hub",
                    "loc_hub",
                    1,
                    "bg",
                    LocationSubtype.Hub,
                    0,
                    0);
                True(Service().Select(null, location, 1) == null, "хаб");
            });

            Check("объект: нет подходящих записей — объекта нет", () =>
            {
                var location = new LocationInstance(
                    "locinst_house",
                    "loc_house",
                    1,
                    "bg",
                    LocationSubtype.Building,
                    1,
                    0);
                True(Service().Select(null, location, 1) == null, "пул пуст");
            });

            Check("объект: тир вне диапазона — объекта нет", () =>
            {
                var location = new LocationInstance(
                    "locinst_high",
                    "loc_road",
                    1,
                    "bg",
                    LocationSubtype.Path,
                    9,
                    0.5);
                True(Service().Select(null, location, 1) == null, "тир 9");
            });

            Check("объект: уникальная запись не выбирается случайно", () =>
            {
                var services = new SpawnRepository(new[]
                {
                    Entry("spawn_a", "loot_human", unique: false),
                    Entry("spawn_u", "loot_human", unique: true)
                });
                var service = new LocationObjectService(services, Locations());
                for (var seed = 1; seed <= 30; seed++)
                {
                    var selected = service.Select(null, NewInstance("loc_road"), seed);
                    Equal("spawn_a", selected!.SpawnId, "уникальная не выпала");
                }
            });

            Check("объект: забранный объект помечается", () =>
            {
                var location = NewInstance("loc_road");
                var selected = Service().Select(null, location, 3)!;
                location.MarkObjectLooted();
                True(selected.Looted, "забран");
            });

            Check("объект: состояние объекта переживает сериализацию локации", () =>
            {
                var save = new LocationSaveData(
                    "locinst_1",
                    "loc_road",
                    1,
                    "bg",
                    LocationSubtype.Path,
                    1,
                    0.2,
                    visited: true,
                    cleared: false,
                    objectSpawnId: "spawn_a",
                    objectLooted: true);
                var restored = LocationSaveData.FromJson(save.ToJson(), "test");
                Equal("spawn_a", restored.ObjectSpawnId, "id записи");
                True(restored.ObjectLooted, "забран");
            });

            Check("объект: без объекта поле не пишется", () =>
            {
                var save = new LocationSaveData(
                    "locinst_2",
                    "loc_hub",
                    1,
                    "bg",
                    LocationSubtype.Hub,
                    0,
                    0,
                    visited: false,
                    cleared: false);
                var restored = LocationSaveData.FromJson(save.ToJson(), "test");
                True(restored.ObjectSpawnId == null, "объекта нет");
                False(restored.ObjectLooted, "не забран");
            });
        }

        private static LocationObjectService Service() =>
            new LocationObjectService(
                new SpawnRepository(new[]
                {
                    Entry("spawn_a", "loot_human"),
                    Entry("spawn_b", "loot_human")
                }),
                Locations());

        private static LocationRepository Locations() =>
            new LocationRepository(new[]
            {
                Template("loc_road", "path", "[\"road\",\"outdoor\"]", 0, 2),
                Template("loc_house", "building", "[\"indoor\",\"house\"]", 0, 2),
                Template("loc_hub", "hub", "[\"hub\",\"outdoor\"]", 0, 0)
            });

        private static LocationInstance NewInstance(string templateId) =>
            new LocationInstance("locinst_0001", templateId, 42, "bg", LocationSubtype.Path, 1, 0.2);

        private static SpawnEntry Entry(string id, string lootTable, bool unique = false) =>
            SpawnEntry.FromJson(
                AnomalyPath.Core.Json.JsonParser.Parse(
                    "{\"kind\":\"spawns\",\"id\":\"" + id + "\",\"titleKey\":\"spawn.title\"," +
                    "\"objectKind\":\"loot\",\"requiresTags\":[\"road\",\"outdoor\"]," +
                    "\"excludesTags\":[\"hub\"],\"weight\":1,\"minTier\":0,\"maxTier\":2," +
                    "\"unique\":" + (unique ? "true" : "false") + "," +
                    "\"lootTable\":\"" + lootTable + "\"}",
                    "test"),
                "test");

        private static LocationTemplate Template(string id, string subtype, string tags, int minTier, int maxTier) =>
            LocationTemplate.FromJson(
                AnomalyPath.Core.Json.JsonParser.Parse(
                    "{\"kind\":\"locations\",\"id\":\"" + id + "\",\"titleKey\":\"n\"," +
                    "\"locationKind\":\"" + (subtype == "hub" ? "hub" : "wild") + "\",\"subtype\":\"" + subtype + "\"," +
                    "\"tags\":" + tags + ",\"tier\":1,\"backgrounds\":[\"bg\"]," +
                    "\"chainLength\":[1,1]}",
                    "test"),
                "test");
    }
}
