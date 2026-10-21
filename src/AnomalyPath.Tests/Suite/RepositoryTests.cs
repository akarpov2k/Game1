#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Di;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Templates;

namespace AnomalyPath.Tests
{
    public sealed class RepositoryTests : TestBase
    {
        private readonly string _dataRoot;
        public RepositoryTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        public override void Run()
        {
            RepositoryOperations(); DiTests();
        }

        private void RepositoryOperations()
        {
            var container = Bootstrap.Build(_dataRoot);
            var repository = container.Resolve<IItemRepository>();
            var catalog = container.Resolve<IItemCatalogService>();

            Check("репозиторий: все предметы", () =>
            {
                Equal(18, repository.All.Count, "All");
            });

            Check("репозиторий: поиск по id", () =>
            {
                True(repository.TryGet("wpn_ak74", out var ak74), "TryGet ak74");
                Equal("wpn_ak74", ak74.Id, "id");
                Throws<DataException>(() => repository.Get("wpn_unknown"), "Get unknown");
            });

            Check("сервис: выборка по категории", () =>
            {
                Equal(4, catalog.ByCategory("weapon").Count, "weapon");
                Equal(3, catalog.ByCategory("armor").Count, "armor");
            });

            Check("сервис: выборка по тиру", () =>
            {
                var tier2 = catalog.ByTier(2);
                Equal(5, tier2.Count, "tier 2");
                Equal("wpn_ak74", tier2[0].Id, "tier 2 id");
            });

            Check("распределение: id уникальны и разрешаются", () =>
            {
                var ids = new HashSet<string>();
                foreach (var item in repository.All)
                {
                    True(ids.Add(item.Id), $"дубль {item.Id}");
                    Equal(item.Id, repository.Get(item.Id).Id, "Get by id");
                }
            });
        }

        private void DiTests()
        {
            Check("DI: репозиторий и сервис разрешаются", () =>
            {
                var container = Bootstrap.Build(_dataRoot);
                True(container.IsRegistered<IItemRepository>(), "репозиторий зарегистрирован");
                True(container.IsRegistered<IItemCatalogService>(), "сервис зарегистрирован");
                Equal(18, container.Resolve<IItemCatalogService>().All().Count, "All через сервис");
            });

            Check("DI: повторная регистрация — ошибка", () =>
            {
                var container = new ServiceContainer();
                var repository = new ItemRepository(Array.Empty<ItemTemplate>());
                container.RegisterSingleton<IItemRepository>(repository);
                Throws<InvalidOperationException>(
                    () => container.RegisterSingleton<IItemRepository>(repository),
                    "дубль регистрации");
            });

            Check("DI: незарегистрированный сервис — ошибка", () =>
            {
                Throws<InvalidOperationException>(
                    () => new ServiceContainer().Resolve<IItemCatalogService>(),
                    "resolve missing");
            });
        }
    }
}
