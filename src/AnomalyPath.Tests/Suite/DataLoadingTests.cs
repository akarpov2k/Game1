#nullable enable
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Templates;

namespace AnomalyPath.Tests
{
    public sealed class DataLoadingTests : TestBase
    {
        private readonly string _dataRoot;
        public DataLoadingTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        public override void Run()
        {
            LoadingTests(); InheritanceTests();
        }

        private void LoadingTests()
        {
            var data = Load(_dataRoot);
            Check("загрузка: тринадцать сущностей в коллекциях", () =>
            {
                Equal(18, data.All("items").Count, "items");
            });

            Check("загрузка: абстрактные шаблоны не зарегистрированы", () =>
            {
                var items = data.All("items");
                foreach (var item in items)
                {
                    var isAbstract = item.TryGet("abstract", out var value) && value.Kind == JsonKind.Bool && value.Bool;
                    False(isAbstract, "абстрактный файл попал в реестр");
                }
            });

            Check("загрузка: id и вид на месте", () =>
            {
                True(data.TryGet("items", "wpn_ak74", out _), "wpn_ak74 не найден");
                True(data.TryGet("items", "wpn_akm", out _), "wpn_akm не найден");
                False(data.TryGet("items", "wpn_unknown", out _), "лишний id");
            });

            Check("загрузка: правила прохождения", () =>
            {
                True(data.TryGet("rules", "rules", out var rules), "rules не найдены");
                Equal("hubReturn", rules.Get("deathMode").Text, "deathMode");
                Equal(4.0, rules.Get("baseRows").Number, "baseRows");
                Equal(4.0, rules.Get("tierCount").Number, "tierCount");
                Equal(3, rules.Get("lootLossOptions").Items.Count, "варианты потери лута");
            });
        }

        private void InheritanceTests()
        {
            var container = Bootstrap.Build(_dataRoot);
            var items = container.Resolve<IItemRepository>();

            Check("наследование: поля базового шаблона", () =>
            {
                var ak74 = items.Get("wpn_ak74");
                Equal(2.0, ak74.Weapon!.ReloadTime, "reloadTime из _base");
                Equal("dmg_bullet", ak74.Weapon!.DamageTypeId, "damageTypeId из _base");
                Equal(100, ak74.Weapon!.DurabilityMax, "durabilityMax из _base");
                Equal(1, ak74.MaxStack, "maxStack из _base");
                True(ak74.Rotatable, "rotatable из _base");
            });

            Check("наследование: свои числа перекрывают базу", () =>
            {
                var ak74 = items.Get("wpn_ak74");
                var akm = items.Get("wpn_akm");
                Equal(12.0, ak74.Weapon!.Damage, "ak74 damage");
                Equal(14.0, akm.Weapon!.Damage, "akm damage");
                Equal(1.0, ak74.Weapon!.AccuracyCoefficient, "ak74 accuracy");
                Equal(1.0, akm.Weapon!.AccuracyCoefficient, "akm accuracy");
                Equal(1.5, items.Get("wpn_sv98").Weapon!.AccuracyCoefficient, "sv98 accuracy");
                True(items.Get("wpn_sv98").Weapon!.IgnoresDistance, "sv98 ignores distance");
                Equal(2, ak74.Tier, "ak74 tier");
                Equal(3, akm.Tier, "akm tier");
            });

            Check("include: миксин цены и свои перекрытия", () =>
            {
                var ak74 = items.Get("wpn_ak74");
                Equal(PriceMode.Reputation, ak74.Price!.Mode, "ak74 price.mode");
                Equal(1200.0, ak74.Price!.Base, "ak74 price.base");
                Equal("fct_caravan", ak74.Price!.FactionId, "ak74 price.factionId");

                var akm = items.Get("wpn_akm");
                Equal(PriceMode.Fixed, akm.Price!.Mode, "akm price.mode");
                Equal(900.0, akm.Price!.Buy, "akm price.buy");
            });

            Check("include: sellable и droppable из миксина", () =>
            {
                var data = Load(_dataRoot);
                JsonValue akm = default!;
                True(data.TryGet("items", "wpn_akm", out akm), "akm не найден");
                True(akm.Get("sellable").Bool, "sellable");
                True(akm.Get("droppable").Bool, "droppable");
            });
        }
    }
}
