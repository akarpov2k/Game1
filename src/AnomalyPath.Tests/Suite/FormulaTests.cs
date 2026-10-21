#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Di;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Repositories;

namespace AnomalyPath.Tests
{
    public sealed class FormulaTests : TestBase
    {
        private readonly ServiceContainer _services;
        public FormulaTests(string dataRoot)
        {
            _services = Bootstrap.Build(dataRoot);
        }

        public override void Run()
        {
            var wear = _services.Resolve<IArmorWearService>();

            Check("износ брони: экзоскелет, урон 100 даёт 6", () =>
            {
                Equal(6, wear.CalculateWear(100, Instance("itm_armor_exoskeleton")), "износ");
            });

            Check("износ брони: куртка, урон 100 даёт 9", () =>
            {
                Equal(9, wear.CalculateWear(100, Instance("itm_armor_jacket")), "износ");
            });

            Check("износ брони: шлем, урон 100 даёт 10", () =>
            {
                Equal(10, wear.CalculateWear(100, Instance("itm_helmet_light")), "износ");
            });

            Check("износ брони: плита, урон 150 даёт 10", () =>
            {
                Equal(10, wear.CalculateWear(150, Instance("itm_armor_plate")), "износ");
            });

            Check("износ брони: чем прочнее материал, тем меньше износ", () =>
            {
                var jacket = wear.CalculateWear(100, Instance("itm_armor_jacket"));
                var exoskeleton = wear.CalculateWear(100, Instance("itm_armor_exoskeleton"));
                True(exoskeleton < jacket, "экзоскелет изнашивается медленнее куртки");
            });

            Check("износ брони: минимальный износ 1", () =>
            {
                Equal(1, wear.CalculateWear(0.01, Instance("itm_armor_jacket")), "износ");
            });

            Check("износ брони: без урона износа нет", () =>
            {
                Equal(0, wear.CalculateWear(0, Instance("itm_armor_jacket")), "износ");
            });

            Check("износ брони: материал и качество из данных", () =>
            {
                var custom = new ItemRepository(new[]
                {
                    Template("{\"kind\":\"items\",\"id\":\"armor_q\",\"nameKey\":\"n\",\"category\":\"armor\",\"footprint\":[1,1],\"maxStack\":1,\"tier\":1,\"icon\":\"i\",\"durabilityMax\":10,\"durabilityFactor\":2.0,\"quality\":2}")
                });
                var customWear = new ArmorWearService(custom);
                Equal(3, customWear.CalculateWear(100, new ItemInstance("i", "armor_q", 1, 10, 10)), "100/(2*2*10)");
            });

            Check("износ брони: прочность уменьшается и не уходит ниже нуля", () =>
            {
                var armor = new ItemInstance("a", "itm_armor_jacket", 1, 40, 40);
                Equal(9, wear.ApplyWear(armor, 100), "первое попадание");
                Equal(31, armor.Durability!.Value, "остаток");
                Equal(31, wear.ApplyWear(armor, 1000), "второе попадание");
                Equal(0, armor.Durability!.Value, "ниже нуля не уходит");
            });

            Check("износ брони: сломанная броня не изнашивается", () =>
            {
                var armor = new ItemInstance("a", "itm_armor_jacket", 1, 0, 40);
                Equal(0, wear.ApplyWear(armor, 100), "износ");
                Equal(0, armor.Durability!.Value, "остаток");
            });

            Check("формула: защита не уводит урон в минус", () =>
            {
                var formula = _services.Resolve<IFormulaRepository>().Get("formula_damage_base");
                var variables = new Dictionary<string, double> { ["damage"] = 3, ["protection"] = 10, ["penetration"] = 0, ["distance"] = 2 };
                Equal(0.0, formula.Evaluate(variables, new Dictionary<string, double>()), "урон");
            });

            Check("формула: деление на ноль даёт 0", () =>
            {
                var expression = FormulaExpression.Parse("10 / 0", new[] { "damage" }, "test");
                Equal(0.0, expression.Evaluate(new Dictionary<string, double>(), new Dictionary<string, double>()), "результат");
            });

            Check("формула: неизвестное имя — ошибка при вычислении", () =>
            {
                var expression = FormulaExpression.Parse("damage * 2", new[] { "protection" }, "test");
                Throws<DataException>(
                    () => expression.Evaluate(new Dictionary<string, double>(), new Dictionary<string, double>()),
                    "имя");
            });

            Check("формула: неизвестная функция — ошибка загрузки", () =>
            {
                Throws<DataException>(() => FormulaExpression.Parse("pow(damage, 2)", new[] { "damage" }, "test"), "функция");
            });

            Check("формула: функции и константы", () =>
            {
                var expression = FormulaExpression.Parse("clamp(damage * k, 0, 10)", new[] { "damage" }, "test");
                var variables = new Dictionary<string, double> { ["damage"] = 8 };
                var constants = new Dictionary<string, double> { ["k"] = 2 };
                Equal(10.0, expression.Evaluate(variables, constants), "clamp");
            });

            Check("формула: значение и таблица", () =>
            {
                var valueJson = "{ \"id\":\"f\",\"value\":42,\"variables\":[\"x\"],\"result\":\"int\" }";
                var value = FormulaTemplate.FromJson(JsonParser.Parse(valueJson, "test"), "test");
                Equal(42.0, value.Evaluate(new Dictionary<string, double>(), new Dictionary<string, double>()), "значение");

                var tableJson = "{ \"id\":\"t\",\"table\":[{\"x\":0,\"y\":0},{\"x\":10,\"y\":100}],\"variables\":[\"damage\"],\"result\":\"float\" }";
                var table = FormulaTemplate.FromJson(JsonParser.Parse(tableJson, "test"), "test");
                var variables = new Dictionary<string, double> { ["damage"] = 5 };
                Equal(50.0, table.Evaluate(variables, new Dictionary<string, double>()), "таблица");
            });

            Check("DI: сервис износа доступен", () =>
            {
                True(_services.IsRegistered<IArmorWearService>(), "зарегистрирован");
            });
        }
    }
}
