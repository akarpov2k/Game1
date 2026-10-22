#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Rules;

namespace AnomalyPath.Tests
{
    /// <summary>Правила прохождения: загрузка, репозиторий, значения и ошибки разбора.</summary>
    public sealed class RulesTests : TestBase
    {
        private readonly string _dataRoot;

        /// <summary>Создаёт категорию тестов правил.</summary>
        /// <param name="dataRoot">Корень данных проекта.</param>
        public RulesTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        /// <inheritdoc />
        public override void Run()
        {
            RepositoryTests();
            ParsingTests();
            ValidatorTests();
        }

        private void RepositoryTests()
        {
            Check("правила: загружаются из данных", () =>
            {
                var services = Bootstrap.Build(_dataRoot);
                var repository = services.Resolve<IGameRulesRepository>();
                var rules = repository.Require();

                Equal("rules", rules.Id, "id");
                Equal(GameRulesTemplate.DeathModeHubReturn, rules.DeathMode, "deathMode");
                Equal(4, rules.BaseRows, "baseRows");
                Equal(4, rules.TierCount, "tierCount");
                Equal(8, rules.GridWidth, "gridWidth");
                Equal(100, rules.NeedsMax, "needsMax");
                Equal("actor_player", rules.PlayerActorId, "playerActorId");
                True(Math.Abs(rules.InsuranceChance - 0.15) < 1e-9, "insuranceChance");
                Equal(GameRulesTemplate.InsuranceLoot, rules.InsuranceAppliesTo, "insuranceAppliesTo");
                Equal(3, rules.LootLossOptions.Count, "варианты потери лута");
                Equal(LootLossOption.LossPartial, rules.LootLossOptions[1].Loss, "loss partial");
                Equal("partial", rules.DefaultLootLoss, "defaultLootLoss");
                True(repository.Rules != null, "Rules не null");
            });

            Check("правила: без файла репозиторий пуст, Require бросает", () =>
            {
                var repository = GameRulesRepository.FromLoadedData(
                    new LoadedData(new Dictionary<string, Dictionary<string, JsonValue>>()));

                True(repository.Rules == null, "правил нет");
                var error = Throws<DataException>(() => repository.Require(), "Require");
                Equal(CoreErrors.RulesMissing, error.Code, "код");
            });

            Check("правила: две записи — ошибка репозитория", () =>
            {
                var records = new Dictionary<string, JsonValue>
                {
                    ["rules"] = JsonParser.Parse(Rules(), "rules"),
                    ["rules2"] = JsonParser.Parse(Rules(), "rules2")
                };
                var data = new LoadedData(new Dictionary<string, Dictionary<string, JsonValue>>
                {
                    ["rules"] = records
                });

                var error = Throws<DataException>(
                    () => GameRulesRepository.FromLoadedData(data),
                    "две записи");
                Equal(CoreErrors.RulesSingleRecord, error.Code, "код");
            });
        }

        private void ParsingTests()
        {
            Check("правила: значения из данных", () =>
            {
                var rules = GameRulesTemplate.FromJson(JsonParser.Parse(Rules(), "rules"), "rules");
                Equal(4, rules.BaseRows, "baseRows");
                Equal(4, rules.TierCount, "tierCount");
                Equal(8, rules.GridWidth, "gridWidth");
                Equal(100, rules.NeedsMax, "needsMax");
                Equal("actor_player", rules.PlayerActorId, "playerActorId");
                Equal(3, rules.LootLossOptions.Count, "варианты");
                Equal("rules", rules.Id, "id");
                Equal(1000, rules.StartingMoney, "startingMoney по умолчанию");
            });

            Check("правила: стартовые деньги из данных", () =>
            {
                var rules = GameRulesTemplate.FromJson(
                    JsonParser.Parse(Rules(startingMoney: 2500), "rules"),
                    "rules");
                Equal(2500, rules.StartingMoney, "startingMoney");
            });

            var invalid = new (string Name, string Json, string Code)[]
            {
                ("неизвестный deathMode", Rules(deathMode: "unknown"), CoreErrors.RulesUnknownDeathMode),
                ("чужой id", Rules(id: "other"), CoreErrors.RulesId),
                ("неизвестный defaultLootLoss", Rules(defaultLoss: "missing"), CoreErrors.RulesDefaultLootLoss),
                ("baseRows 0", Rules(baseRows: 0), CoreErrors.RulesRange),
                ("tierCount 0", Rules(tierCount: 0), CoreErrors.RulesRange),
                ("gridWidth 0", Rules(gridWidth: 0), CoreErrors.RulesRange),
                ("needsMax 0", Rules(needsMax: 0), CoreErrors.RulesRange),
                ("startingMoney -1", Rules(startingMoney: -1), CoreErrors.RulesRange),
                ("пустой playerActorId", Rules(playerActorId: ""), CoreErrors.MissingStringField),
                ("insuranceChance 1.5", Rules(insuranceChance: 1.5), CoreErrors.RulesChance),
                ("неизвестная страховка", Rules(insuranceScope: "unknown"), CoreErrors.RulesUnknownInsuranceScope),
                ("неизвестный loss", Rules(loss: "unknown"), CoreErrors.RulesUnknownLootLoss),
                ("share вне диапазона", Rules(share: -0.1), CoreErrors.RulesChance)
            };

            foreach (var item in invalid)
            {
                Check($"правила: {item.Name}", () =>
                {
                    var error = Throws<DataException>(
                        () => GameRulesTemplate.FromJson(JsonParser.Parse(item.Json, "rules"), "rules"),
                        item.Name);
                    Equal(item.Code, error.Code, "код");
                });
            }

            Check("правила: пустой список вариантов", () =>
            {
                var json = "{\"schemaVersion\":1,\"kind\":\"rules\",\"id\":\"rules\",\"deathMode\":\"hubReturn\",\"lootLossOptions\":[],\"defaultLootLoss\":\"partial\",\"insuranceChance\":0.15,\"insuranceAppliesTo\":\"loot\",\"baseRows\":4,\"tierCount\":4}";
                var error = Throws<DataException>(
                    () => GameRulesTemplate.FromJson(JsonParser.Parse(json, "rules"), "rules"),
                    "пустой список");
                Equal(CoreErrors.RulesMissingLootLoss, error.Code, "код");
            });
        }

        private void ValidatorTests()
        {
            Check("правила: валидатор ловит неизвестный режим", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("rules.json"),
                        ["rules.json"] = Rules(deathMode: "unknown")
                    },
                    root =>
                    {
                        var report = Preflight.Validate(root);
                        True(HasIssue(report, "rules.json", "Неизвестный режим смерти"), "нет ошибки");
                    });
            });
        }

        private static string Rules(
            string id = "rules",
            string deathMode = "hubReturn",
            string defaultLoss = "partial",
            double insuranceChance = 0.15,
            string insuranceScope = "loot",
            int baseRows = 4,
            int tierCount = 4,
            string loss = "partial",
            double share = 0.5,
            string playerActorId = "actor_player",
            int gridWidth = 8,
            int needsMax = 100,
            int startingMoney = 1000)
        {
            var insurance = insuranceChance.ToString(CultureInfo.InvariantCulture);
            var shareText = share.ToString(CultureInfo.InvariantCulture);
            return $"{{\"schemaVersion\":1,\"kind\":\"rules\",\"id\":\"{id}\",\"deathMode\":\"{deathMode}\",\"lootLossOptions\":[{{\"id\":\"none\",\"titleKey\":\"t.none\",\"loss\":\"none\",\"share\":0}},{{\"id\":\"partial\",\"titleKey\":\"t.partial\",\"loss\":\"{loss}\",\"share\":{shareText}}},{{\"id\":\"all\",\"titleKey\":\"t.all\",\"loss\":\"all\",\"share\":1}}],\"defaultLootLoss\":\"{defaultLoss}\",\"insuranceChance\":{insurance},\"insuranceAppliesTo\":\"{insuranceScope}\",\"baseRows\":{baseRows},\"tierCount\":{tierCount},\"playerActorId\":\"{playerActorId}\",\"gridWidth\":{gridWidth},\"needsMax\":{needsMax},\"startingMoney\":{startingMoney}}}";
        }
    }
}
