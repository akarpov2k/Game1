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
    /// <summary>Часть класса <see cref="WorldTests"/>: валидация мира.</summary>
    public sealed partial class WorldTests
    {
        private void ValidationTests()
        {
            Check("валидация: переход на несуществующую локацию", () =>
            {
                WithFixture(WorldFixture(",\"connections\":[{\"toId\":\"loc_missing\",\"via\":\"road\"}]"), root =>
                {
                    var report = Preflight.Validate(root);
                    True(HasIssue(report, "loc_hub", "несуществующую"), "нет ошибки target");
                });
            });

            Check("валидация: запрещённый топологией переход", () =>
            {
                WithFixture(
                    WorldFixture(
                        string.Empty,
                        routeExtra: ",\"connections\":[{\"toId\":\"loc_interior\",\"via\":\"door\"}]"),
                    root =>
                    {
                        var report = Preflight.Validate(root);
                        True(HasIssue(report, "loc_route", "запрещён"), "нет ошибки перехода");
                    });
            });

            Check("валидация: мир без хаба", () =>
            {
                var files = BaseWorldFixture();
                files.Remove("locations/hub.json");
                files["catalog.json"] = Catalog("topology.json", "locations/route.json", "locations/site.json", "locations/interior.json");
                WithFixture(files, root =>
                {
                    var report = Preflight.Validate(root);
                    True(HasIssue(report, "world", "нет ни одного хаба"), "нет ошибки хаба");
                });
            });

            Check("валидация: нет маршрута до точки интереса", () =>
            {
                var files = BaseWorldFixture();
                files["locations/site.json"] = LocationJson("loc_site", "wild", "site", "[\"indoor\"]");
                WithFixture(files, root =>
                {
                    var report = Preflight.Validate(root);
                    True(HasIssue(report, "loc_site", "нельзя построить маршрут"), "нет ошибки маршрута");
                });
            });
        }
    }
}
