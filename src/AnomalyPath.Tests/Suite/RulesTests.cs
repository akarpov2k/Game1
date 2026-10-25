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
                Equal(4, rules.BaseRows, "baseRows");
                Equal(4, rules.TierCount, "tierCount");
                Equal(8, rules.GridWidth, "gridWidth");
                Equal(100, rules.NeedsMax, "needsMax");
                Equal("actor_player", rules.PlayerActorId, "playerActorId");
                True(Math.Abs(rules.InsuranceChance - 0.15) < 1e-9, "insuranceChance");
                True(Math.Abs(rules.InsuranceStep - 0.05) < 1e-9, "insuranceStep");
                True(Math.Abs(rules.TradeDifficulty - 1) < 1e-9, "tradeDifficulty");
                True(Math.Abs(rules.TradeDifficultyMax - 3) < 1e-9, "tradeDifficultyMax");
                True(Math.Abs(rules.TradeDifficultyStep - 0.1) < 1e-9, "tradeDifficultyStep");
                True(Math.Abs(rules.RespawnPercent - 0.3) < 1e-9, "respawnPercent");
                True(Math.Abs(rules.XpLossChance - 0.4) < 1e-9, "xpLossChance");
                True(Math.Abs(rules.XpLossStep - 0.1) < 1e-9, "xpLossStep");
                True(Math.Abs(rules.XpLossMax - 1) < 1e-9, "xpLossMax");
                Equal(24, rules.NicknameMaxLength, "nicknameMaxLength");
                Equal(32, rules.PreviewMaxRows, "previewMaxRows");
                Equal(6, rules.SavePageSize, "savePageSize");
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
                True(Math.Abs(rules.InsuranceChance - 0.15) < 1e-9, "insuranceChance");
                True(Math.Abs(rules.InsuranceStep - 0.05) < 1e-9, "insuranceStep");
                True(Math.Abs(rules.RespawnPercent - 0.3) < 1e-9, "respawnPercent");
                True(Math.Abs(rules.XpLossChance - 0.4) < 1e-9, "xpLossChance");
                True(Math.Abs(rules.XpLossStep - 0.1) < 1e-9, "xpLossStep");
                True(Math.Abs(rules.XpLossMax - 1) < 1e-9, "xpLossMax");
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
                ("чужой id", Rules(id: "other"), CoreErrors.RulesId),
                ("baseRows 0", Rules(baseRows: 0), CoreErrors.RulesRange),
                ("tierCount 0", Rules(tierCount: 0), CoreErrors.RulesRange),
                ("gridWidth 0", Rules(gridWidth: 0), CoreErrors.RulesRange),
                ("needsMax 0", Rules(needsMax: 0), CoreErrors.RulesRange),
                ("startingMoney -1", Rules(startingMoney: -1), CoreErrors.RulesRange),
                ("пустой playerActorId", Rules(playerActorId: ""), CoreErrors.MissingStringField),
                ("insuranceChance 1.5", Rules(insuranceChance: 1.5), CoreErrors.RulesChance),
                ("insuranceChance -0.1", Rules(insuranceChance: -0.1), CoreErrors.RulesChance),
                ("insuranceStep 0", Rules(insuranceStep: 0), CoreErrors.PositiveValue),
                ("respawnPercent 0", Rules(respawnPercent: 0), CoreErrors.RulesChance),
                ("respawnPercent 1.5", Rules(respawnPercent: 1.5), CoreErrors.RulesChance),
                ("respawnPercent -0.1", Rules(respawnPercent: -0.1), CoreErrors.RulesChance),
                ("xpLossChance 1.5", Rules(xpLossChance: 1.5), CoreErrors.RulesChance),
                ("xpLossChance -0.1", Rules(xpLossChance: -0.1), CoreErrors.RulesChance),
                ("xpLossStep 0", Rules(xpLossStep: 0), CoreErrors.PositiveValue),
                ("xpLossMax 0", Rules(xpLossMax: 0), CoreErrors.RulesChance),
                ("xpLossMax 1.5", Rules(xpLossMax: 1.5), CoreErrors.RulesChance),
                ("xpLossChance выше максимума", Rules(xpLossChance: 0.8, xpLossMax: 0.5), CoreErrors.RangeOrder)
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
        }

        private void ValidatorTests()
        {
            Check("правила: валидатор ловит нулевой шаг страховки", () =>
            {
                WithFixture(
                    new Dictionary<string, string>
                    {
                        ["catalog.json"] = Catalog("rules.json"),
                        ["rules.json"] = Rules(insuranceStep: 0)
                    },
                    root =>
                    {
                        var report = Preflight.Validate(root);
                        True(HasIssue(report, "rules.json", "должно быть больше нуля"), "нет ошибки");
                    });
            });
        }

        private static string Rules(
            string id = "rules",
            double insuranceChance = 0.15,
            double insuranceStep = 0.05,
            int baseRows = 4,
            int tierCount = 4,
            string playerActorId = "actor_player",
            int gridWidth = 8,
            int needsMax = 100,
            int startingMoney = 1000,
            double respawnPercent = 0.3,
            double xpLossChance = 0.4,
            double xpLossStep = 0.1,
            double xpLossMax = 1)
        {
            var insurance = insuranceChance.ToString(CultureInfo.InvariantCulture);
            var step = insuranceStep.ToString(CultureInfo.InvariantCulture);
            var respawn = respawnPercent.ToString(CultureInfo.InvariantCulture);
            var xpLoss = xpLossChance.ToString(CultureInfo.InvariantCulture);
            var xpStep = xpLossStep.ToString(CultureInfo.InvariantCulture);
            var xpMax = xpLossMax.ToString(CultureInfo.InvariantCulture);
            return $"{{\"schemaVersion\":1,\"kind\":\"rules\",\"id\":\"{id}\",\"insuranceChance\":{insurance},\"insuranceStep\":{step},\"xpLossChance\":{xpLoss},\"xpLossStep\":{xpStep},\"xpLossMax\":{xpMax},\"respawnPercent\":{respawn},\"baseRows\":{baseRows},\"tierCount\":{tierCount},\"playerActorId\":\"{playerActorId}\",\"gridWidth\":{gridWidth},\"needsMax\":{needsMax},\"startingMoney\":{startingMoney},\"tradeDifficulty\":1,\"tradeDifficultyMax\":3,\"tradeDifficultyStep\":0.1,\"nicknameMaxLength\":24,\"previewMaxRows\":32,\"savePageSize\":6}}";
        }
    }
}
