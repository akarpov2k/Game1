#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Spawns;
using AnomalyPath.Core.World;

namespace AnomalyPath.Tests
{
    /// <summary>Спавн объектов локации: разбор, фильтр по тегам и тиру, репозиторий.</summary>
    public sealed class SpawnTests : TestBase
    {
        private const string LootId = "spawn_car_wreck";
        private const string LootTable = "loot_human";

        /// <inheritdoc />
        public override void Run()
        {
            ParsingTests();
            ValidationTests();
            MatchTests();
            RepositoryTests();
        }

        private void ParsingTests()
        {
            Check("спавн: объект лута читается из JSON", () =>
            {
                var entry = Parse(LootJson());
                Equal(LootId, entry.Id, "id");
                Equal("spawn.car.title", entry.TitleKey, "titleKey");
                Equal(LocationObjectKind.Loot, entry.ObjectKind, "вид");
                Equal(2, entry.RequiresTags.Count, "required tags");
                Equal(1, entry.ExcludesTags.Count, "excluded tags");
                Near(3.0, entry.Weight, "weight");
                Equal(0, entry.MinTier, "minTier");
                Equal(2, entry.MaxTier, "maxTier");
                Equal(LootTable, entry.LootTableId, "lootTable");
            });

            Check("спавн: объект лута создаёт LootLocationObject", () =>
            {
                var entry = Parse(LootJson());
                var created = entry.CreateObject();
                Equal(LocationObjectKind.Loot, created.Kind, "вид объекта");
                var loot = (LootLocationObject)created;
                Equal(LootTable, loot.LootTableId, "таблица");
                False(loot.Looted, "не забран");
            });

            Check("спавн: тайник читается и создаёт StashLocationObject", () =>
            {
                var entry = Parse(
                    "{\"kind\":\"spawns\",\"id\":\"spawn_stash_test\",\"titleKey\":\"spawn.stash.title\"," +
                    "\"objectKind\":\"stash\",\"weight\":1," +
                    "\"requiresTags\":[\"outdoor\"],\"excludesTags\":[\"hub\"]}");
                Equal(LocationObjectKind.Stash, entry.CreateObject().Kind, "вид");
                Equal(1, entry.RequiresTags.Count, "required tags");
            });

            Check("спавн: NPC читается и создаёт NpcLocationObject", () =>
            {
                var entry = Parse(
                    "{\"kind\":\"spawns\",\"id\":\"spawn_wanderer\",\"titleKey\":\"spawn.wanderer.title\"," +
                    "\"objectKind\":\"npc\",\"weight\":2,\"actorId\":\"npc_wanderer\"}");
                var created = entry.CreateObject();
                Equal(LocationObjectKind.Npc, created.Kind, "вид");
                Equal("npc_wanderer", ((NpcLocationObject)created).ActorId, "актёр");
            });

            Check("спавн: забранный объект помечается", () =>
            {
                var created = Parse(LootJson()).CreateObject();
                created.MarkLooted();
                True(created.Looted, "забран");
            });
        }

        private void ValidationTests()
        {
            Check("спавн: вес 0 запрещён", () =>
                Throws<DataException>(() => Parse(LootJson(weight: 0)), "weight"));

            Check("спавн: отрицательный вес запрещён", () =>
                Throws<DataException>(() => Parse(LootJson(weight: -1)), "weight"));

            Check("спавн: minTier больше maxTier запрещён", () =>
                Throws<DataException>(() => Parse(LootJson(minTier: 3, maxTier: 1)), "tier"));

            Check("спавн: неизвестный тег запрещён", () =>
                Throws<DataException>(() => Parse(LootJson(requires: "[\"swamp\"]")), "тег"));

            Check("спавн: тег одновременно требуется и запрещён", () =>
                Throws<DataException>(
                    () => Parse(LootJson(requires: "[\"road\"]", excludes: "[\"road\"]")),
                    "конфликт"));

            Check("спавн: неизвестный вид объекта запрещён", () =>
                Throws<DataException>(
                    () => Parse(LootJson().Replace("\"loot\"", "\"monster\"")),
                    "вид"));

            Check("спавн: объект лута без таблицы запрещён", () =>
                Throws<DataException>(
                    () => Parse(
                        "{\"kind\":\"spawns\",\"id\":\"spawn_x\",\"titleKey\":\"n\"," +
                        "\"objectKind\":\"loot\",\"weight\":1}"),
                    "lootTable"));

            Check("спавн: NPC без актёра запрещён", () =>
                Throws<DataException>(
                    () => Parse(
                        "{\"kind\":\"spawns\",\"id\":\"spawn_x\",\"titleKey\":\"n\"," +
                        "\"objectKind\":\"npc\",\"weight\":1}"),
                    "actorId"));

            Check("спавн: пустой id запрещён", () =>
                Throws<DataException>(
                    () => Parse(
                        "{\"kind\":\"spawns\",\"id\":\"\",\"titleKey\":\"n\"," +
                        "\"objectKind\":\"stash\",\"weight\":1}"),
                    "id"));
        }

        private void MatchTests()
        {
            Check("спавн: подходит по тегам и тиру", () =>
            {
                var entry = Parse(LootJson());
                True(
                    entry.Matches(Tags(LocationTag.Road, LocationTag.Outdoor), 1, LocationSubtype.Path),
                    "дорога, тир 1");
            });

            Check("спавн: хаб не получает объектов", () =>
            {
                var entry = Parse(LootJson());
                False(
                    entry.Matches(Tags(LocationTag.Road, LocationTag.Outdoor), 1, LocationSubtype.Hub),
                    "хаб");
            });

            Check("спавн: запрещённый тег блокирует", () =>
            {
                var entry = Parse(LootJson());
                False(
                    entry.Matches(Tags(LocationTag.Road, LocationTag.Outdoor, LocationTag.Hub), 1, LocationSubtype.Path),
                    "hub в тегах");
            });

            Check("спавн: не хватает обязательного тега", () =>
            {
                var entry = Parse(LootJson());
                False(
                    entry.Matches(Tags(LocationTag.Road), 1, LocationSubtype.Path),
                    "нет outdoor");
            });

            Check("спавн: тир вне диапазона", () =>
            {
                var entry = Parse(LootJson());
                False(
                    entry.Matches(Tags(LocationTag.Road, LocationTag.Outdoor), 5, LocationSubtype.Path),
                    "тир 5");
            });

            Check("спавн: уникальный объект не выпадает случайно", () =>
            {
                var entry = Parse(LootJson(unique: true));
                False(
                    entry.Matches(Tags(LocationTag.Road, LocationTag.Outdoor), 1, LocationSubtype.Path),
                    "unique");
            });
        }

        private void RepositoryTests()
        {
            Check("спавн: репозиторий находит запись", () =>
            {
                var repository = new SpawnRepository(new[] { Parse(LootJson()) });
                Equal(LootId, repository.Get(LootId).Id, "id");
                True(repository.TryGet(LootId, out _), "TryGet");
                False(repository.TryGet("spawn_missing", out _), "нет записи");
            });

            Check("спавн: дубль id запрещён", () =>
                Throws<DataException>(
                    () => new SpawnRepository(new[] { Parse(LootJson()), Parse(LootJson()) }),
                    "дубль"));

            Check("спавн: неизвестный id запрещён", () =>
                Throws<DataException>(
                    () => new SpawnRepository(new[] { Parse(LootJson()) }).Get("spawn_missing"),
                    "нет записи"));
        }

        private SpawnEntry Parse(string json) => SpawnEntry.FromJson(JsonParser.Parse(json, "test"), "test");

        private static IReadOnlyList<LocationTag> Tags(params LocationTag[] tags) => tags;

        private static void Near(double expected, double actual, string what)
        {
            if (System.Math.Abs(expected - actual) > 1e-6)
            {
                throw new System.InvalidOperationException($"{what}: ожидалось {expected}, получено {actual}");
            }
        }

        private static string LootJson(
            double weight = 3,
            int minTier = 0,
            int maxTier = 2,
            string requires = "[\"road\",\"outdoor\"]",
            string excludes = "[\"hub\"]",
            bool unique = false)
        {
            var uniquePart = unique ? "\"unique\":true," : string.Empty;
            return "{\"kind\":\"spawns\",\"id\":\"" + LootId + "\",\"titleKey\":\"spawn.car.title\"," +
                   "\"objectKind\":\"loot\"," + uniquePart +
                   "\"requiresTags\":" + requires + ",\"excludesTags\":" + excludes + "," +
                   "\"weight\":" + weight.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," +
                   "\"minTier\":" + minTier + ",\"maxTier\":" + maxTier + "," +
                   "\"lootTable\":\"" + LootTable + "\"}";
        }
    }
}
