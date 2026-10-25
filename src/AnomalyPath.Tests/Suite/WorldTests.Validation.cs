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
            Check("валидация: в хаб можно попасть только с пути", () =>
            {
                var files = BaseWorldFixture();
                files["topology.json"] = WorldTopologyJson()
                    .Replace("\"from\":\"hub\",\"to\":\"path\"", "\"from\":\"hub\",\"to\":\"building\"");
                WithFixture(files, root =>
                {
                    var report = Preflight.Validate(root);
                    True(HasIssue(report, "topology", "только с пути"), "нет ошибки доступа в хаб");
                });
            });

            Check("валидация: топология ссылается на подтип без локаций", () =>
            {
                var files = BaseWorldFixture();
                files.Remove("locations/site.json");
                files["catalog.json"] = Catalog("topology.json", "locations/hub.json", "locations/route.json");
                WithFixture(files, root =>
                {
                    var report = Preflight.Validate(root);
                    True(HasIssue(report, "topology", "Неизвестный подтип"), "нет ошибки подтипа");
                });
            });

            Check("валидация: мир без хаба", () =>
            {
                var files = BaseWorldFixture();
                files.Remove("locations/hub.json");
                files["catalog.json"] = Catalog("topology.json", "locations/route.json", "locations/site.json");
                WithFixture(files, root =>
                {
                    var report = Preflight.Validate(root);
                    True(HasIssue(report, "world", "нет ни одного хаба"), "нет ошибки хаба");
                });
            });

            Check("валидация: нет маршрута до точки интереса", () =>
            {
                var files = BaseWorldFixture();
                files["locations/site.json"] = LocationJson("loc_site", "wild", "building", "[\"indoor\"]");
                WithFixture(files, root =>
                {
                    var report = Preflight.Validate(root);
                    True(HasIssue(report, "loc_site", "нельзя построить маршрут"), "нет ошибки маршрута");
                });
            });
        }
    }
}
