#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Di;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Progression;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Services;

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
                    Template("{\"kind\":\"items\",\"id\":\"armor_q\",\"nameKey\":\"n\",\"category\":\"armor\",\"footprint\":[1,1],\"maxStack\":1,\"tier\":1,\"icon\":\"i\",\"material\":\"medium\",\"durabilityMax\":10,\"durabilityFactor\":2.0,\"quality\":2}")
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

            var progression = _services.Resolve<IProgressionService>();

            Check("прогрессия: требование уровня растёт на 20% от предыдущего", () =>
            {
                Equal(100, progression.RequiredXp(1), "ур.1");
                Equal(120, progression.RequiredXp(2), "ур.2");
                Equal(144, progression.RequiredXp(3), "ур.3");
                Equal(173, progression.RequiredXp(4), "ур.4: 172,8 округляется от нуля");
                Equal(516, progression.RequiredXp(10), "ур.10");
                Equal(3195, progression.RequiredXp(20), "ур.20: 3194,8");
            });

            Check("прогрессия: опыт за убийство = 10 × ранг × (1 + 10% × редкость)", () =>
            {
                Equal(11, progression.KillXp(1, 1), "ранг 1, редкость 1");
                Equal(22, progression.KillXp(2, 1), "ранг 2, редкость 1");
                Equal(45, progression.KillXp(3, 5), "ранг 3, редкость 5");
                Equal(75, progression.KillXp(5, 5), "ранг 5, редкость 5");
            });

            Check("прогрессия: накопленный опыт до уровня", () =>
            {
                Equal(0, progression.TotalXpToLevel(1), "ур.1");
                Equal(100, progression.TotalXpToLevel(2), "ур.2");
                Equal(2080, progression.TotalXpToLevel(10), "ур.10");
                Equal(15475, progression.TotalXpToLevel(20), "ур.20");
            });

            Check("прогрессия: убийств на уровень", () =>
            {
                Equal(10, progression.KillsForLevel(1, 1, 1), "ур.1, худший противник");
                Equal(43, progression.KillsForLevel(20, 5, 5), "ур.20, лучший противник");
                Equal(291, progression.KillsForLevel(20, 1, 1), "ур.20, худший противник");
                Equal(207, progression.KillsToLevel(20, 5, 5), "всего убийств до ур.20");
            });

            Check("прогрессия: уровень по опыту", () =>
            {
                Equal(1, progression.LevelForXp(0), "без опыта");
                Equal(1, progression.LevelForXp(99), "не хватает до второго");
                Equal(2, progression.LevelForXp(100), "ровно на второй");
                Equal(10, progression.LevelForXp(2080), "на десятый");
                Equal(42, progression.LevelForXp(1000000), "миллион опыта");
                Equal(84, progression.LevelForXp(int.MaxValue), "предел объёма опыта");
            });

            Check("прогрессия: уровни сверху не ограничены", () =>
            {
                Equal(3834, progression.RequiredXp(21), "ур.21");
                Equal(19781, progression.RequiredXp(30), "ур.30");
                Equal(18670, progression.TotalXpToLevel(21), "всего до ур.21");
                Equal(98410, progression.TotalXpToLevel(30), "всего до ур.30");
                Equal(int.MaxValue, progression.RequiredXp(200), "требование выше int насыщается");
                Equal(int.MaxValue, progression.TotalXpToLevel(200), "сумма выше int насыщается");
            });

            Check("прогрессия: границы аргументов", () =>
            {
                Throws<ArgumentOutOfRangeException>(() => progression.RequiredXp(0), "уровень 0");
                Throws<ArgumentOutOfRangeException>(() => progression.TotalXpToLevel(0), "уровень 0 в сумме");
                Throws<ArgumentOutOfRangeException>(() => progression.KillXp(0, 1), "ранг 0");
                Throws<ArgumentOutOfRangeException>(() => progression.KillXp(1, 6), "редкость выше потолка");
                Throws<ArgumentOutOfRangeException>(() => progression.LevelForXp(-1), "отрицательный опыт");
            });

            Check("прогрессия: числа кривой берутся из данных", () =>
            {
                var custom = new ProgressionService(new ProgressionSettings("progression", 50, 0, 5, 0, 2, 2, 5, 1, 2));
                Equal(50, custom.RequiredXp(5), "рост 0% — требование не меняется");
                Equal(5, custom.KillXp(1, 1), "без бонуса редкости");
                Equal(10, custom.KillXp(2, 2), "ранг 2 без бонуса");
                Equal(10, custom.KillsForLevel(1, 1, 1), "50 / 5");
                Equal(20001, custom.LevelForXp(1000000), "плоская кривая: 1 + 1 000 000 / 50");
                Throws<ArgumentOutOfRangeException>(() => custom.KillXp(3, 1), "ранг выше своего потолка");
            });

            Check("DI: сервис износа доступен", () =>
            {
                True(_services.IsRegistered<IArmorWearService>(), "зарегистрирован");
            });

            Check("DI: сервис прогрессии доступен", () =>
            {
                True(_services.IsRegistered<IProgressionService>(), "зарегистрирован");
            });
        }
    }
}
