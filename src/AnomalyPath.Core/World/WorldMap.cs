#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly Dictionary<int, int> _routeBranches = new Dictionary<int, int>();
        private readonly List<string> _routeFront = new List<string>();

        /// <summary>Создаёт карту.</summary>
        /// <param name="seed">Seed генерации.</param>
        /// <param name="locations">Экземпляры локаций.</param>
        /// <param name="edges">Рёбра переходов.</param>
        /// <param name="startLocationId">Стартовая локация.</param>
        /// <param name="startingStamina">Стартовая и максимальная выносливость.</param>
        /// <param name="dayStartHour">Час начала дня.</param>
        /// <param name="nightStartHour">Час начала ночи.</param>
        /// <param name="routeEdges">Сколько звеньев маршрута сгенерировано.</param>
        /// <param name="routeTailId">Id конца маршрута; <c>null</c> — маршрут не строился.</param>
        /// <param name="routeLastBranches">Веток на последнем звене.</param>
        /// <param name="routeFrontIds">Выходы последнего звена — концы его веток.</param>
        /// <param name="routeSerial">Номер выхода из хаба: входит в seed маршрута.</param>
        /// <exception cref="DataException">Дубли id, битые рёбра, отсутствующая стартовая локация
        /// или отрицательный номер маршрута.</exception>
        public WorldMap(
            int seed,
            IReadOnlyList<LocationInstance> locations,
            IReadOnlyList<WorldEdge> edges,
            string startLocationId,
            int startingStamina,
            double dayStartHour,
            double nightStartHour,
            int routeEdges = 0,
            string? routeTailId = null,
            int routeLastBranches = 0,
            IReadOnlyList<string>? routeFrontIds = null,
            int routeSerial = 0)
        {
            if (routeSerial < 0)
            {
                throw new DataException("world.route", CoreErrors.WorldRange, "routeSerial");
            }

            RouteSerial = routeSerial;
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
            MarkRoute(routeEdges, routeTailId, routeLastBranches, routeFrontIds);
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

        /// <summary>Номер выхода из хаба; 0 — маршрут ещё не начинался.</summary>
        public int RouteSerial { get; private set; }

        /// <summary>
        /// Seed текущего маршрута: складывается из seed мира и номера выхода из хаба,
        /// поэтому каждый выход даёт свой маршрут, а повторная загрузка продолжает тот же.
        /// </summary>
        public int RouteSeed => StableHash.Combine(
            Seed,
            "route:" + RouteSerial.ToString(CultureInfo.InvariantCulture));

        /// <summary>Сколько звеньев маршрута уже сгенерировано.</summary>
        public int RouteEdges { get; private set; }

        /// <summary>Id последней локации конца маршрута; <c>null</c> — маршрут не строился.</summary>
        public string? RouteTailId { get; private set; }

        /// <summary>Сколько веток дало последнее сгенерированное звено.</summary>
        public int RouteLastBranches { get; private set; }

        /// <summary>
        /// Выходы последнего построенного звена — концы его веток. Из них достройка ведёт
        /// переход в первое звено пути следующего звена; после достройки список заменяется.
        /// </summary>
        public IReadOnlyList<string> RouteFrontIds => new List<string>(_routeFront);

        /// <summary>Сколько веток дало звено маршрута.</summary>
        /// <param name="edgeNumber">Номер звена от хаба.</param>
        /// <returns>Число веток; 0, если звено не генерировалось.</returns>
        public int BranchCount(int edgeNumber) =>
            _routeBranches.TryGetValue(edgeNumber, out var count) ? count : 0;

        /// <summary>Запоминает число веток звена.</summary>
        /// <param name="edgeNumber">Номер звена от хаба.</param>
        /// <param name="count">Сколько веток отходит от звена.</param>
        /// <exception cref="DataException">Номер звена или число веток отрицательны.</exception>
        public void MarkBranchCount(int edgeNumber, int count)
        {
            if (edgeNumber < 0 || count < 0)
            {
                throw new DataException("world.route", CoreErrors.WorldRange, "branches");
            }

            _routeBranches[edgeNumber] = count;
        }

        /// <summary>Запоминает, докуда достроен маршрут.</summary>
        /// <param name="edges">Сколько звеньев сгенерировано.</param>
        /// <param name="tailId">Id конца маршрута; <c>null</c> — маршрут не строился.</param>
        /// <param name="lastBranches">Веток на последнем звене.</param>
        /// <param name="frontIds">Выходы звена — концы его веток; <c>null</c> или пусто — выходов нет.</param>
        /// <exception cref="DataException">Отрицательные значения, неизвестный конец маршрута или выход.</exception>
        public void MarkRoute(int edges, string? tailId, int lastBranches, IReadOnlyList<string>? frontIds = null)
        {
            if (edges < 0 || lastBranches < 0)
            {
                throw new DataException("world.route", CoreErrors.WorldRange, "routeEdges");
            }

            if (tailId != null && !_locations.ContainsKey(tailId))
            {
                throw new DataException(tailId, CoreErrors.WorldInstanceNotFound, tailId);
            }

            _routeFront.Clear();
            if (frontIds != null)
            {
                foreach (var id in frontIds)
                {
                    if (!_locations.ContainsKey(id))
                    {
                        throw new DataException(id, CoreErrors.WorldInstanceNotFound, id);
                    }

                    _routeFront.Add(id);
                }
            }

            RouteEdges = edges;
            RouteTailId = tailId;
            RouteLastBranches = lastBranches;
        }

        /// <summary>
        /// Начинает новый маршрут: номер выхода из хаба растёт, а отметки звеньев сбрасываются.
        /// Маршрут при этом строится заново, поэтому старые звенья не продолжаются.
        /// </summary>
        /// <exception cref="DataException">Номер выхода переполнен.</exception>
        public void BeginRoute()
        {
            if (RouteSerial == int.MaxValue)
            {
                throw new DataException("world.route", CoreErrors.WorldRange, "routeSerial");
            }

            RouteSerial++;
            _routeBranches.Clear();
            _routeFront.Clear();
            RouteEdges = 0;
            RouteTailId = null;
            RouteLastBranches = 0;
        }

        /// <summary>
        /// Убирает с карты пройденную часть маршрута: локации позади текущей и ветки,
        /// в которые из неё уже не попасть. Хаб и локации, добавленные не маршрутом
        /// (цели квестов), остаются.
        /// </summary>
        /// <param name="currentLocationId">Где стоит игрок.</param>
        /// <returns>Сколько локаций удалено.</returns>
        /// <exception cref="DataException">Текущая локация не найдена.</exception>
        public int PrunePassed(string currentLocationId)
        {
            var current = GetLocation(currentLocationId);
            var remove = new List<string>();
            foreach (var location in _locations.Values)
            {
                if (!IsAhead(location, current))
                {
                    remove.Add(location.InstanceId);
                }
            }

            foreach (var id in remove)
            {
                RemoveLocation(id);
            }

            return remove.Count;
        }

        /// <summary>Удаляет рёбра между двумя локациями в обе стороны: назад пути нет.</summary>
        /// <param name="firstId">Первая локация.</param>
        /// <param name="secondId">Вторая локация.</param>
        /// <returns>Сколько рёбер удалено.</returns>
        public int RemoveEdgesBetween(string firstId, string secondId)
        {
            var removed = 0;
            for (var index = _edgeList.Count - 1; index >= 0; index--)
            {
                var edge = _edgeList[index];
                var forward = edge.FromId == firstId && edge.ToId == secondId;
                var backward = edge.FromId == secondId && edge.ToId == firstId;
                if (forward || backward)
                {
                    _edges.Remove(edge.EdgeId);
                    _edgeList.RemoveAt(index);
                    removed++;
                }
            }

            return removed;
        }

        /// <summary>Локация ещё впереди игрока: текущая, хаб, добавленная не маршрутом или будущее звено.</summary>
        private bool IsAhead(LocationInstance location, LocationInstance current) =>
            location.InstanceId == current.InstanceId ||
            location.InstanceId == StartLocationId ||
            location.Subtype == LocationSubtype.Hub ||
            location.RouteEdge == 0 ||
            location.RouteEdge > current.RouteEdge ||
            (location.RouteEdge == current.RouteEdge && location.RouteStep >= current.RouteStep);

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

        /// <summary>Удаляет локацию и все рёбра, которые её касаются.</summary>
        /// <param name="instanceId">Id экземпляра локации.</param>
        /// <exception cref="DataException">Локация не найдена.</exception>
        public void RemoveLocation(string instanceId)
        {
            if (!_locations.Remove(instanceId))
            {
                throw new DataException(instanceId, CoreErrors.WorldInstanceNotFound, instanceId);
            }

            _routeFront.RemoveAll(id => string.Equals(id, instanceId, StringComparison.Ordinal));

            for (var index = _edgeList.Count - 1; index >= 0; index--)
            {
                var edge = _edgeList[index];
                if (edge.FromId == instanceId || edge.ToId == instanceId)
                {
                    _edges.Remove(edge.EdgeId);
                    _edgeList.RemoveAt(index);
                }
            }
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
