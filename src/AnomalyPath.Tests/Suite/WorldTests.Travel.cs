#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.World;

namespace AnomalyPath.Tests
{
    /// <summary>Часть класса <see cref="WorldTests"/>: перемещение.</summary>
    public sealed partial class WorldTests
    {
        private void TravelTests(IWorldGenerator generator, ITravelService travel)
        {
            Check("переход: не списывает выносливость и двигает время", () =>
            {
                var map = generator.Generate(42);
                var state = WorldState.Start(map);
                var edge = FirstPaidEdge(map, state.CurrentLocationId);
                var stamina = state.Stamina;
                var hours = state.Clock.Hours;
                var result = travel.Travel(state, edge.EdgeId);
                True(result.Succeeded, "переход выполнен");
                Equal(edge.ToId, state.CurrentLocationId, "новая локация");
                Equal(stamina, state.Stamina, "выносливость не тратится");
                Equal(hours + edge.Cost.Hours, state.Clock.Hours, "часы");
            });

            Check("переход: доступен без выносливости", () =>
            {
                var map = generator.Generate(42);
                var state = new WorldState(map, map.StartLocationId, 0, map.StartingStamina, new WorldClock(1, 8, 6, 22));
                var edge = FirstPaidEdge(map, state.CurrentLocationId);
                True(travel.CanTravel(state, edge.EdgeId), "CanTravel");
                True(travel.Travel(state, edge.EdgeId).Succeeded, "переход выполнен");
                Equal(0, state.Stamina, "выносливость осталась нулевой");
            });

            Check("переход: из чужой локации запрещён", () =>
            {
                var map = generator.Generate(42);
                var state = WorldState.Start(map);
                var foreign = FirstPaidEdge(map, OfTemplate(map, "loc_site_anomaly")[0].InstanceId);
                Equal(TravelStatus.WrongLocation, travel.Travel(state, foreign.EdgeId).Status, "статус");
            });

            Check("переход: неизвестное ребро", () =>
            {
                var map = generator.Generate(42);
                var state = WorldState.Start(map);
                Equal(TravelStatus.EdgeNotFound, travel.Travel(state, "edge_9999").Status, "статус");
            });

            Check("переход: список видимых рёбер не пуст в хабе", () =>
            {
                var map = generator.Generate(42);
                var state = WorldState.Start(map);
                True(travel.GetAvailableEdges(state).Count > 0, "рёбра из хаба");
            });

            Check("время: переход через полночь меняет день", () =>
            {
                var clock = new WorldClock(1, 23, 6, 22);
                clock.Advance(2);
                Equal(2, clock.Day, "день");
                Equal(1.0, clock.Hours, "часы");
                Equal(TimePhase.Night, clock.Phase, "фаза");
            });

            Check("время: день и ночь считаются от порогов", () =>
            {
                Equal(TimePhase.Day, new WorldClock(1, 6, 6, 22).Phase, "06:00");
                Equal(TimePhase.Night, new WorldClock(1, 22, 6, 22).Phase, "22:00");
                Equal(TimePhase.Night, new WorldClock(1, 0, 6, 22).Phase, "00:00");
            });

            Check("время: FromState отвергает 24 часа", () =>
            {
                Throws<DataException>(() => WorldClock.FromState(1, 24, 6, 22), "24 часа");
            });

            Check("состояние: восстановление не выше максимума", () =>
            {
                var map = generator.Generate(42);
                var state = new WorldState(map, map.StartLocationId, 5, 10, new WorldClock(1, 8, 6, 22));
                state.RestoreStamina(10);
                Equal(10, state.Stamina, "максимум");
                Throws<ArgumentOutOfRangeException>(() => state.RestoreStamina(-1), "отрицательное восстановление");
            });

            Check("состояние: погода меняется", () =>
            {
                var map = generator.Generate(42);
                var state = WorldState.Start(map);
                state.SetWeather(WeatherKind.Fog);
                Equal(WeatherKind.Fog, state.Weather, "погода");
            });

            Check("локация: отметки посещения и зачистки", () =>
            {
                var map = generator.Generate(42);
                var location = OfSubtype(map, LocationSubtype.Path)[0];
                False(location.Visited, "до отметки");
                location.MarkVisited();
                location.MarkCleared();
                True(location.Visited, "посещена");
                True(location.Cleared, "зачищена");
            });

            Check("хаб: последний посещённый хаб обновляется и восстанавливается", () =>
            {
                var map = new WorldMap(
                    1,
                    new List<LocationInstance>
                    {
                        new LocationInstance("hub_a", "loc_hub", 1, "bg", LocationSubtype.Hub, 0, 0),
                        new LocationInstance("hub_b", "loc_hub", 2, "bg", LocationSubtype.Hub, 0, 0),
                        new LocationInstance("route", "loc_route", 3, "bg", LocationSubtype.Path, 1, 0.2)
                    },
                    new List<WorldEdge>(),
                    "hub_a",
                    10,
                    6,
                    22);
                var state = WorldState.Start(map);
                Equal("hub_a", state.LastHubId, "стартовый хаб");

                state.MoveTo("hub_b");
                Equal("hub_b", state.LastHubId, "новый хаб");
                state.MoveTo("route");
                Equal("hub_b", state.LastHubId, "дикая локация хаб не меняет");

                var restored = new WorldState(
                    map, "route", 5, 10, new WorldClock(1, 8, 6, 22), WeatherKind.Clear, "hub_a");
                Equal("hub_a", restored.LastHubId, "хаб из сейва");

                var fallback = new WorldState(map, "route", 5, 10, new WorldClock(1, 8, 6, 22));
                Equal("hub_a", fallback.LastHubId, "старый сейв без хаба");

                Throws<DataException>(
                    () => new WorldState(map, "route", 5, 10, new WorldClock(1, 8, 6, 22), WeatherKind.Clear, "route"),
                    "сохранённый хаб не хаб");
            });

            Check("лут: счётчик бросков растёт и восстанавливается из сейва", () =>
            {
                var map = new WorldMap(
                    1,
                    new List<LocationInstance>
                    {
                        new LocationInstance("hub_a", "loc_hub", 1, "bg", LocationSubtype.Hub, 0, 0)
                    },
                    new List<WorldEdge>(),
                    "hub_a",
                    10,
                    6,
                    22);
                var state = WorldState.Start(map);
                Equal(0, state.LootSerial, "новый мир");
                Equal(1, state.NextLootSerial(), "первый бросок");
                Equal(2, state.NextLootSerial(), "второй бросок");
                Equal(2, state.LootSerial, "счётчик после двух бросков");

                var restored = new WorldState(
                    map, "hub_a", 10, 10, new WorldClock(1, 8, 6, 22), WeatherKind.Clear, null, 7);
                Equal(7, restored.LootSerial, "счётчик из сейва");

                Throws<DataException>(
                    () => new WorldState(
                        map, "hub_a", 10, 10, new WorldClock(1, 8, 6, 22), WeatherKind.Clear, null, -1),
                    "отрицательный счётчик");
            });
        }
    }
}
