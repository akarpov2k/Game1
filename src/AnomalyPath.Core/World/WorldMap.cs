#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.World
{
    /// <summary>Сгенерированная карта мира: экземпляры локаций и рёбра между ними.</summary>
    public sealed class WorldMap
    {
        private readonly Dictionary<string, LocationInstance> _locations;
        private readonly Dictionary<string, WorldEdge> _edges;
        private readonly List<WorldEdge> _edgeList;

        /// <summary>Создаёт карту.</summary>
        /// <param name="seed">Seed генерации.</param>
        /// <param name="locations">Экземпляры локаций.</param>
        /// <param name="edges">Рёбра переходов.</param>
        /// <param name="startLocationId">Стартовая локация.</param>
        /// <param name="startingStamina">Стартовая и максимальная выносливость.</param>
        /// <param name="dayStartHour">Час начала дня.</param>
        /// <param name="nightStartHour">Час начала ночи.</param>
        /// <exception cref="DataException">Дубли id, битые рёбра или отсутствующая стартовая локация.</exception>
        public WorldMap(
            int seed,
            IReadOnlyList<LocationInstance> locations,
            IReadOnlyList<WorldEdge> edges,
            string startLocationId,
            int startingStamina,
            double dayStartHour,
            double nightStartHour)
        {
            Seed = seed;
            StartingStamina = startingStamina;
            DayStartHour = dayStartHour;
            NightStartHour = nightStartHour;

            _locations = new Dictionary<string, LocationInstance>(StringComparer.Ordinal);
            foreach (var location in locations)
            {
                if (_locations.ContainsKey(location.InstanceId))
                {
                    throw new DataException(location.InstanceId, CoreErrors.DuplicateRepositoryId);
                }

                _locations[location.InstanceId] = location;
            }

            _edges = new Dictionary<string, WorldEdge>(StringComparer.Ordinal);
            _edgeList = new List<WorldEdge>();
            foreach (var edge in edges)
            {
                if (_edges.ContainsKey(edge.EdgeId))
                {
                    throw new DataException(edge.EdgeId, CoreErrors.DuplicateRepositoryId);
                }

                if (!_locations.ContainsKey(edge.FromId) || !_locations.ContainsKey(edge.ToId))
                {
                    throw new DataException(edge.EdgeId, CoreErrors.WorldConnectionTarget, edge.ToId);
                }

                _edges[edge.EdgeId] = edge;
                _edgeList.Add(edge);
            }

            if (!_locations.ContainsKey(startLocationId))
            {
                throw new DataException(startLocationId, CoreErrors.WorldInstanceNotFound, startLocationId);
            }

            StartLocationId = startLocationId;
        }

        /// <summary>Seed генерации.</summary>
        public int Seed { get; }

        /// <summary>Стартовая и максимальная выносливость.</summary>
        public int StartingStamina { get; }

        /// <summary>Час начала дня.</summary>
        public double DayStartHour { get; }

        /// <summary>Час начала ночи.</summary>
        public double NightStartHour { get; }

        /// <summary>Стартовая локация.</summary>
        public string StartLocationId { get; }

        /// <summary>Все экземпляры локаций.</summary>
        public IReadOnlyList<LocationInstance> Locations => new List<LocationInstance>(_locations.Values);

        /// <summary>Все рёбра.</summary>
        public IReadOnlyList<WorldEdge> Edges => _edgeList;

        /// <summary>Возвращает локацию по id экземпляра.</summary>
        /// <param name="instanceId">Id экземпляра.</param>
        /// <returns>Локация.</returns>
        /// <exception cref="DataException">Локация не найдена.</exception>
        public LocationInstance GetLocation(string instanceId) =>
            _locations.TryGetValue(instanceId, out var location)
                ? location
                : throw new DataException(instanceId, CoreErrors.WorldInstanceNotFound, instanceId);

        /// <summary>Ищет локацию по id экземпляра.</summary>
        /// <param name="instanceId">Id экземпляра.</param>
        /// <param name="location">Найденная локация или <c>null</c>.</param>
        /// <returns><c>true</c>, если локация найдена.</returns>
        public bool TryGetLocation(string instanceId, out LocationInstance? location) =>
            _locations.TryGetValue(instanceId, out location);

        /// <summary>Проверяет, есть ли локация с таким id экземпляра.</summary>
        /// <param name="instanceId">Id экземпляра.</param>
        /// <returns><c>true</c>, если локация есть на карте.</returns>
        public bool ContainsLocation(string instanceId) => _locations.ContainsKey(instanceId);

        /// <summary>Добавляет локацию на карту, например цель квеста.</summary>
        /// <param name="location">Экземпляр локации.</param>
        /// <exception cref="ArgumentNullException">Локация не задана.</exception>
        /// <exception cref="DataException">Такой id уже есть.</exception>
        public void AddLocation(LocationInstance location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (_locations.ContainsKey(location.InstanceId))
            {
                throw new DataException(location.InstanceId, CoreErrors.DuplicateRepositoryId);
            }

            _locations[location.InstanceId] = location;
        }

        /// <summary>Добавляет ребро перехода на карту.</summary>
        /// <param name="edge">Ребро.</param>
        /// <exception cref="ArgumentNullException">Ребро не задано.</exception>
        /// <exception cref="DataException">Такой id уже есть или локация не найдена.</exception>
        public void AddEdge(WorldEdge edge)
        {
            if (edge == null)
            {
                throw new ArgumentNullException(nameof(edge));
            }

            if (_edges.ContainsKey(edge.EdgeId))
            {
                throw new DataException(edge.EdgeId, CoreErrors.DuplicateRepositoryId);
            }

            if (!_locations.ContainsKey(edge.FromId) || !_locations.ContainsKey(edge.ToId))
            {
                throw new DataException(edge.EdgeId, CoreErrors.WorldConnectionTarget, edge.ToId);
            }

            _edges[edge.EdgeId] = edge;
            _edgeList.Add(edge);
        }

        /// <summary>Ищет ребро по id.</summary>
        /// <param name="edgeId">Id ребра.</param>
        /// <param name="edge">Найденное ребро или <c>null</c>.</param>
        /// <returns><c>true</c>, если ребро найдено.</returns>
        public bool TryGetEdge(string edgeId, out WorldEdge? edge) => _edges.TryGetValue(edgeId, out edge);

        /// <summary>Рёбра, выходящие из локации.</summary>
        /// <param name="locationId">Id экземпляра локации.</param>
        /// <returns>Список рёбер; пустой, если выхода нет.</returns>
        public IReadOnlyList<WorldEdge> EdgesFrom(string locationId)
        {
            var result = new List<WorldEdge>();
            foreach (var edge in _edgeList)
            {
                if (edge.FromId == locationId)
                {
                    result.Add(edge);
                }
            }

            return result;
        }
    }
}
