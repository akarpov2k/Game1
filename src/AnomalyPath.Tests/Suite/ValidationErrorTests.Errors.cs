#nullable enable
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Core;
using AnomalyPath.Core.Validation;

namespace AnomalyPath.Tests
{
    /// <summary>Часть класса <see cref="ValidationErrorTests"/>: тексты ошибок.</summary>
    public sealed partial class ValidationErrorTests
    {
        private void ErrorTests()
        {
            var cases = new List<(string Name, Dictionary<string, string> Files, string Fragment)>
            {
                (
                    "ошибка: дубль id в коллекции",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/col.json"),
                        ["items/col.json"] = Collection("items/a.json", "items/a.json"),
                        ["items/a.json"] = Weapon("wpn_ak74"),
                    },
                    "подключён дважды"),
                (
                    "ошибка: extends на несуществующий файл",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json"),
                        ["items/a.json"] = Weapon("wpn_ak74", extends: "_base/missing.json"),
                    },
                    "Файл не найден"),
                (
                    "ошибка: extends на другой вид",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json"),
                        ["_base/loc.json"] = "{\"schemaVersion\":1,\"kind\":\"locations\",\"abstract\":true}",
                        ["items/a.json"] = Weapon("wpn_ak74", extends: "_base/loc.json"),
                    },
                    "extends на другой вид"),
                (
                    "ошибка: битый json",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json"),
                        ["items/a.json"] = "{ \"kind\": \"items\", ",
                    },
                    "Ожидалось имя поля"),
                (
                    "ошибка: неизвестный материал брони",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json"),
                        ["items/a.json"] = "{ \"kind\":\"items\",\"id\":\"arm_wood\",\"nameKey\":\"n\",\"category\":\"armor\",\"footprint\":[1,1],\"maxStack\":1,\"tier\":1,\"icon\":\"i\",\"durabilityMax\":10,\"material\":\"wood\",\"durabilityFactor\":1.0 }",
                    },
                    "Неизвестный материал"),
                (
                    "ошибка: неизвестный тип урона в защите",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json"),
                        ["items/a.json"] = "{ \"kind\":\"items\",\"id\":\"arm_wood\",\"nameKey\":\"n\",\"category\":\"armor\",\"footprint\":[1,1],\"maxStack\":1,\"tier\":1,\"icon\":\"i\",\"durabilityMax\":10,\"material\":\"light\",\"durabilityFactor\":1.0,\"quality\":1,\"protection\":{\"dmg_wood\":1} }",
                    },
                    "Неизвестный тип урона"),
                (
                    "ошибка: путь выходит за корень",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("../secret.json"),
                    },
                    "выходит за корень"),
                (
                    "ошибка: weapon без блока weapon",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json"),
                        ["items/a.json"] = "{ \"kind\":\"items\",\"id\":\"wpn_x\",\"nameKey\":\"n\",\"category\":\"weapon\",\"footprint\":[1,1],\"maxStack\":1,\"tier\":1,\"icon\":\"i\" }",
                    },
                    "без блока weapon"),
                (
                    "ошибка: у врага нет опыта",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("actors/a.json"),
                        ["actors/a.json"] = "{ \"kind\":\"actors\",\"id\":\"enemy_x\",\"type\":\"enemy\",\"nameKey\":\"n\"," +
                                             "\"combat\":{\"tier\":1,\"level\":1,\"health\":10,\"ap\":1,\"ai\":\"aggressive\",\"accuracy\":0.5} }",
                    },
                    "xp"),
                (
                    "ошибка: магазин оружия меньше одного патрона",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json"),
                        ["items/a.json"] = "{ \"kind\":\"items\",\"id\":\"wpn_x\",\"nameKey\":\"n\",\"category\":\"weapon\",\"footprint\":[1,1],\"maxStack\":1,\"tier\":1,\"icon\":\"i\",\"weapon\":{\"classId\":\"w\",\"damageTypeId\":\"dmg_bullet\",\"damage\":1,\"accuracy\":0.5,\"magazine\":0,\"ammoCaliber\":\"c\",\"reloadTime\":1,\"armorPenetration\":0,\"durabilityMax\":10,\"durabilityPerShot\":0.1}}",
                    },
                    "вместимость магазина"),
                (
                    "ошибка: у врага нет блока attack",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("actors/a.json"),
                        ["actors/a.json"] = "{ \"kind\":\"actors\",\"id\":\"enemy_x\",\"type\":\"enemy\",\"nameKey\":\"n\"," +
                                             "\"combat\":{\"tier\":1,\"level\":1,\"health\":10,\"ap\":1,\"xp\":1,\"ai\":\"aggressive\",\"accuracy\":0.5} }",
                    },
                    "блока attack"),
                (
                    "ошибка: блок атаки у не-врага",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("actors/a.json"),
                        ["actors/a.json"] = "{ \"kind\":\"actors\",\"id\":\"npc_x\",\"type\":\"npc\",\"nameKey\":\"n\"," +
                                             "\"combat\":{\"tier\":1,\"level\":1,\"health\":10,\"ap\":1,\"ai\":\"passive\",\"accuracy\":0.5," +
                                             "\"attack\":{\"damage\":1,\"damageTypeId\":\"dmg_rupture\"}} }",
                    },
                    "только у врага"),
                (
                    "ошибка: в атаке смешаны оружие и естественная атака",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("actors/a.json"),
                        ["actors/a.json"] = "{ \"kind\":\"actors\",\"id\":\"enemy_x\",\"type\":\"enemy\",\"nameKey\":\"n\"," +
                                             "\"combat\":{\"tier\":1,\"level\":1,\"health\":10,\"ap\":1,\"xp\":1,\"ai\":\"aggressive\",\"accuracy\":0.5," +
                                             "\"attack\":{\"weaponId\":\"wpn_x\",\"damage\":5}} }",
                    },
                    "смешаны"),
                (
                    "ошибка: тип урона атаки не найден",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("actors/a.json", "combat.json", "equipment.json", "topology.json"),
                        ["combat.json"] = CombatJson(),
                        ["equipment.json"] = EquipmentJson(),
                        ["topology.json"] = TopologyJson(),
                        ["actors/a.json"] = "{ \"kind\":\"actors\",\"id\":\"enemy_x\",\"type\":\"enemy\",\"nameKey\":\"n\"," +
                                             "\"combat\":{\"tier\":1,\"level\":1,\"health\":10,\"ap\":1,\"xp\":1,\"ai\":\"aggressive\",\"accuracy\":0.5," +
                                             "\"attack\":{\"damage\":1,\"damageTypeId\":\"dmg_missing\"}} }",
                    },
                    "Неизвестный тип урона"),
                (
                    "ошибка: оружие атаки не найдено",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("actors/a.json", "combat.json", "equipment.json", "topology.json"),
                        ["combat.json"] = CombatJson(),
                        ["equipment.json"] = EquipmentJson(),
                        ["topology.json"] = TopologyJson(),
                        ["actors/a.json"] = "{ \"kind\":\"actors\",\"id\":\"enemy_x\",\"type\":\"enemy\",\"nameKey\":\"n\"," +
                                             "\"combat\":{\"tier\":1,\"level\":1,\"health\":10,\"ap\":1,\"xp\":1,\"ai\":\"aggressive\",\"accuracy\":0.5," +
                                             "\"attack\":{\"weaponId\":\"wpn_missing\"}} }",
                    },
                    "Предмет не найден"),
                (
                    "ошибка: оружие атаки без блока weapon",
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("items/a.json", "actors/a.json", "combat.json", "equipment.json", "topology.json"),
                        ["combat.json"] = CombatJson(),
                        ["equipment.json"] = EquipmentJson(),
                        ["topology.json"] = TopologyJson(),
                        ["items/a.json"] = "{ \"kind\":\"items\",\"id\":\"wpn_x\",\"nameKey\":\"n\",\"category\":\"trophy\",\"footprint\":[1,1],\"maxStack\":1,\"tier\":1,\"icon\":\"i\" }",
                        ["actors/a.json"] = "{ \"kind\":\"actors\",\"id\":\"enemy_x\",\"type\":\"enemy\",\"nameKey\":\"n\"," +
                                             "\"combat\":{\"tier\":1,\"level\":1,\"health\":10,\"ap\":1,\"xp\":1,\"ai\":\"aggressive\",\"accuracy\":0.5," +
                                             "\"attack\":{\"weaponId\":\"wpn_x\"}} }",
                    },
                    "без блока weapon"),
            };

            foreach (var testCase in cases)
            {
                Check(testCase.Name, () =>
                {
                    WithFixture(testCase.Files, root =>
                    {
                        var error = Throws<ValidationException>(() => Bootstrap.Build(root), testCase.Name);
                        Contains(error.Message, testCase.Fragment, "причина");
                    });
                });
            }
        }
    }
}
