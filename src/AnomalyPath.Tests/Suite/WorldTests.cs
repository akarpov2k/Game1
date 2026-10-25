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
    /// <summary>Мир: шаблоны, топология, генерация маршрутов, переходы и время.</summary>
    public sealed partial class WorldTests : TestBase
    {
        private readonly string _dataRoot;

        /// <summary>Создаёт категорию тестов мира.</summary>
        /// <param name="dataRoot">Корень данных проекта.</param>
        public WorldTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        /// <inheritdoc />
        public override void Run()
        {
            var services = Bootstrap.Build(_dataRoot);
            var generator = services.Resolve<IWorldGenerator>();
            var travel = services.Resolve<ITravelService>();
            var locations = services.Resolve<ILocationRepository>();
            var topology = services.Resolve<ITopologyRepository>().Topology;

            TemplateTests();
            GenerationTests(generator, locations, topology);
            DepartureTests(generator);
            PruneTests();
            MapMutationTests(generator);
            TravelTests(generator, travel);
            ValidationTests();
        }
    }
}
