#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Data;

namespace AnomalyPath.Tests
{
    public sealed class FileFormatTests : TestBase
    {



        public override void Run()
        {
            Check("формат: сущность прямо из каталога", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/wpn_x.json"),
                        ["items/wpn_x.json"] = Weapon("wpn_x")
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("items").Count, "items");
                    });
            });

            Check("формат: коллекция с вложенной коллекцией", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/col_a.json"),
                        ["items/col_a.json"] = CollectionWithInclude("items/a.json", "items/col_b.json"),
                        ["items/col_b.json"] = Collection("items/b.json"),
                        ["items/a.json"] = Weapon("wpn_a"),
                        ["items/b.json"] = Weapon("wpn_b")
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(2, data.All("items").Count, "items");
                    });
            });

            Check("формат: абстрактный шаблон не регистрируется, но наследуется", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/wpn_x.json"),
                        ["_base/base.json"] = Abstract("{\"sellable\":true}"),
                        ["items/wpn_x.json"] = Weapon("wpn_x", extends: "_base/base.json")
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("items").Count, "items");
                        True(data.OfKind("items")["wpn_x"].Get("sellable").Bool, "sellable из базы");
                        False(data.TryGet("items", "base", out _), "абстрактный файл не должен регистрироваться");
                    });
            });

            Check("формат: null удаляет унаследованное поле", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/wpn_x.json"),
                        ["_base/base.json"] = Abstract("{\"sellable\":true}"),
                        ["items/wpn_x.json"] = Weapon("wpn_x", extends: "_base/base.json", extra: ",\"sellable\":null")
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        False(data.OfKind("items")["wpn_x"].TryGet("sellable", out _), "sellable должен исчезнуть");
                    });
            });

            Check("формат: массивы заменяются, а не сливаются", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/wpn_x.json"),
                        ["_base/base.json"] = Abstract("{\"tags\":[\"a\",\"b\"]}"),
                        ["items/wpn_x.json"] = Weapon("wpn_x", extends: "_base/base.json", extra: ",\"tags\":[\"c\"]")
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        var tags = data.OfKind("items")["wpn_x"].Get("tags");
                        Equal(1, tags.Items.Count, "tags");
                        Equal("c", tags.Items[0].Text, "tag");
                    });
            });

            Check("формат: тип урона грузится как сущность damageTypes", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("damage/dmg_x.json"),
                        ["damage/dmg_x.json"] = "{ \"kind\":\"damageTypes\",\"id\":\"dmg_x\",\"nameKey\":\"n\",\"damageKind\":\"anomaly\",\"stackable\":true }"
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("damageTypes").Count, "damageTypes");
                        Equal("anomaly", data.OfKind("damageTypes")["dmg_x"].Get("damageKind").Text, "вид");
                    });
            });

            Check("формат: формула грузится как сущность formulas", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("formulas/f.json"),
                        ["formulas/f.json"] = "{ \"kind\":\"formulas\",\"id\":\"f\",\"value\":7,\"variables\":[\"x\"],\"result\":\"int\" }"
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("formulas").Count, "formulas");
                    });
            });

            Check("формат: локация грузится как сущность locations", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("locations/loc_x.json"),
                        ["locations/loc_x.json"] = "{\"schemaVersion\":1,\"kind\":\"locations\",\"id\":\"loc_x\"," +
                                                   "\"titleKey\":\"t\",\"locationKind\":\"wild\",\"nodeType\":\"route\"," +
                                                   "\"tags\":[\"road\",\"outdoor\"],\"tier\":0,\"backgrounds\":[\"bg\"]," +
                                                   "\"objectCount\":[1,2],\"danger\":0.2,\"connections\":[]}"
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("locations").Count, "locations");
                        Equal("route", data.OfKind("locations")["loc_x"].Get("nodeType").Text, "роль");
                    });
            });

            Check("формат: топология грузится как сущность topology", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("topology.json"),
                        ["topology.json"] = "{\"schemaVersion\":1,\"kind\":\"topology\",\"id\":\"topology\"," +
                                            "\"transitions\":[{\"from\":\"hub\",\"to\":\"route\",\"via\":[\"road\"]}]," +
                                            "\"routeLength\":[1,2],\"edgeLength\":[1,2]," +
                                            "\"costs\":{\"road\":{\"stamina\":2,\"hours\":1}}}"
                    },
                    root =>
                    {
                        var data = new DataLoader(new FileSystemDataSource(root)).Load();
                        Equal(1, data.All("topology").Count, "topology");
                        Equal(2, data.OfKind("topology")["topology"].Get("edgeLength").Items[1].Number, "edgeLength");
                    });
            });

            Check("формат: повторная коллекция — ошибка", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/col_a.json"),
                        ["items/col_a.json"] = CollectionWithInclude("items/a.json", "items/col_a.json"),
                        ["items/a.json"] = Weapon("wpn_a")
                    },
                    root => Throws<DataException>(() => new DataLoader(new FileSystemDataSource(root)).Load(), "цикл"));
            });
        }
    }
}
