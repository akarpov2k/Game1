#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Di;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Talents;
using InventoryEquipment = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Tests
{
    /// <summary>Дерево талантов: ранги, пороги открытия, очки и собранные бонусы.</summary>
    public sealed class TalentTests : TestBase
    {
        private const string First = "tal_steady_hand";
        private const string Second = "tal_second_wind";
        private const string Third = "tal_endurance";
        private const string Fourth = "tal_luck";
        private const string Fifth = "tal_focus";

        private readonly string _dataRoot;

        /// <summary>Создаёт категорию тестов прокачки.</summary>
        /// <param name="dataRoot">Корень данных.</param>
        public TalentTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        /// <inheritdoc />
        public override void Run()
        {
            var services = Bootstrap.Build(_dataRoot);
            var repository = services.Resolve<ITalentRepository>();
            var talents = services.Resolve<ITalentService>();

            Check("таланты: дерево загружено с ветками и картинками", () =>
            {
                var tree = repository.Tree;
                Equal("talents", tree.Id, "id дерева");
                Equal(8, tree.Nodes.Count, "талантов");
                Equal(0, tree.Nodes[0].Row, "первая строка");
                Equal(1, tree.Nodes[0].Column, "корень по центру");
                Equal(0, tree.Nodes[0].RequiredPoints, "стартовый талант открыт");

                var firstRow = 0;
                var branchRow = 0;
                var lastRow = 0;
                var images = 0;
                foreach (var node in tree.Nodes)
                {
                    if (node.Row == 1)
                    {
                        firstRow++;
                    }

                    if (node.Row == 2)
                    {
                        branchRow++;
                    }

                    lastRow = Math.Max(lastRow, node.Row);
                    if (!string.IsNullOrWhiteSpace(node.Icon))
                    {
                        images++;
                    }
                }

                Equal(2, firstRow, "первая строка ветвится");
                Equal(2, branchRow, "вторая строка ветвится");
                Equal(4, lastRow, "последняя строка");
                Equal(8, images, "у всех талантов есть картинка");
                Equal("icons/tal_steady_hand", tree.GetNode(First).Icon, "картинка корня");
                Equal(18, tree.GetNode("tal_survivor").RequiredPoints, "порог общего таланта");
            });

            Check("таланты: без очков изучить нельзя", () =>
            {
                var player = NewPlayer();
                Equal(TalentResultStatus.NoPoints, talents.Learn(player, First).Status, "нет очков");
                Equal(0, talents.RankOf(player, First), "ранг");
                Equal(0, talents.SpentPoints(player), "вложено");
            });

            Check("таланты: очко списывается и повышает ранг", () =>
            {
                var player = NewPlayer(points: 2);
                var result = talents.Learn(player, First);
                True(result.Succeeded, "изучение");
                Equal(1, result.Rank, "новый ранг");
                Equal(1, result.AvailablePoints, "остаток");
                Equal(1, result.SpentPoints, "вложено");
                Equal(1, talents.RankOf(player, First), "ранг в состоянии");
                Equal(0.01, talents.GetModifiers(player).Accuracy, "меткость за ранг");
            });

            Check("таланты: предельный ранг останавливает изучение", () =>
            {
                var player = NewPlayer(points: 10);
                for (var index = 0; index < 5; index++)
                {
                    True(talents.Learn(player, First).Succeeded, "ранг " + (index + 1));
                }

                var result = talents.Learn(player, First);
                Equal(TalentResultStatus.MaxRank, result.Status, "предел");
                Equal(5, talents.RankOf(player, First), "ранг не растёт");
                Equal(5, talents.SpentPoints(player), "вложено");
                Equal(0.05, talents.GetModifiers(player).Accuracy, "меткость на максимуме");
            });

            Check("таланты: следующий талант закрыт до порога", () =>
            {
                var player = NewPlayer(points: 20);
                Equal(TalentResultStatus.Locked, talents.Learn(player, Second).Status, "порог не достигнут");
                Equal(0, talents.RankOf(player, Second), "ранг");
                Equal(20, player.TalentPoints, "очки не списаны");

                for (var index = 0; index < 5; index++)
                {
                    True(talents.Learn(player, First).Succeeded, "вложение в первый талант");
                }

                True(talents.Learn(player, Second).Succeeded, "второй талант открылся");
                Equal(1, talents.RankOf(player, Second), "ранг второго");
                Equal(6, talents.SpentPoints(player), "вложено");
                Equal(1, talents.GetModifiers(player).ApRestore, "восстановление ОД");
            });

            Check("таланты: бонусы складываются по рангам", () =>
            {
                var player = NewPlayer(points: 30);
                for (var index = 0; index < 5; index++)
                {
                    talents.Learn(player, First);
                }

                for (var index = 0; index < 3; index++)
                {
                    talents.Learn(player, Second);
                }

                True(talents.Learn(player, Third).Succeeded, "третий талант открыт");
                True(talents.Learn(player, Third).Succeeded, "второй ранг третьего");
                var modifiers = talents.GetModifiers(player);
                Equal(3, modifiers.ApRestore, "восстановление ОД");
                Equal(2, modifiers.ApMax, "максимум ОД");
                Equal(0.05, modifiers.Accuracy, "меткость");
                Equal(10, talents.SpentPoints(player), "вложено");
            });

            Check("таланты: правая ветка даёт здоровье, удачу и строки", () =>
            {
                var player = NewPlayer(points: 40);
                for (var index = 0; index < 5; index++)
                {
                    talents.Learn(player, First);
                }

                for (var index = 0; index < 4; index++)
                {
                    True(talents.Learn(player, "tal_toughness").Succeeded, "ранг крепости");
                }

                for (var index = 0; index < 5; index++)
                {
                    True(talents.Learn(player, Fourth).Succeeded, "ранг удачи");
                }

                for (var index = 0; index < 3; index++)
                {
                    True(talents.Learn(player, "tal_scavenger").Succeeded, "ранг запасливости");
                }

                Equal(
                    TalentResultStatus.Locked,
                    talents.Learn(player, "tal_survivor").Status,
                    "общий талант закрыт до 18 очков");

                True(talents.Learn(player, Fifth).Succeeded, "собранность открывает порог");
                True(talents.Learn(player, "tal_survivor").Succeeded, "общий талант изучен");

                var modifiers = talents.GetModifiers(player);
                Equal(13, modifiers.HealthMax, "максимум здоровья");
                Equal(5, modifiers.Luck, "удача");
                Equal(3, modifiers.CarryRows, "строки инвентаря");
                True(Math.Abs(modifiers.Accuracy - 0.06) < 1e-9, "меткость");
                Equal(19, talents.SpentPoints(player), "вложено");
            });

            Check("таланты: талант с двумя эффектами", () =>
            {
                var player = NewPlayer(points: 30);
                for (var index = 0; index < 5; index++)
                {
                    talents.Learn(player, First);
                }

                for (var index = 0; index < 3; index++)
                {
                    talents.Learn(player, Second);
                }

                for (var index = 0; index < 5; index++)
                {
                    talents.Learn(player, Third);
                }

                for (var index = 0; index < 5; index++)
                {
                    talents.Learn(player, Fourth);
                }

                True(talents.Learn(player, Fifth).Succeeded, "последний талант");
                var modifiers = talents.GetModifiers(player);
                Equal(4, modifiers.ApRestore, "восстановление от последнего");
                Equal(6, modifiers.ApMax, "максимум ОД от последнего");
                Equal(5, modifiers.Luck, "удача");
                Equal(19, talents.SpentPoints(player), "вложено");
            });

            Check("таланты: неизвестный талант", () =>
            {
                var player = NewPlayer(points: 1);
                var result = talents.Learn(player, "tal_missing");
                Equal(TalentResultStatus.UnknownTalent, result.Status, "статус");
                Equal(1, player.TalentPoints, "очки не списаны");
                Equal(0, talents.RankOf(player, "tal_missing"), "ранг неизвестного");
            });

            Check("таланты: ранг выше предела зажимается", () =>
            {
                var player = NewPlayer(points: 0, ranks: new Dictionary<string, int> { [First] = 99 });
                Equal(5, talents.RankOf(player, First), "ранг");
                Equal(5, talents.SpentPoints(player), "вложено");
                Equal(0.05, talents.GetModifiers(player).Accuracy, "бонус не растёт сверх предела");
            });

            Check("таланты: очки начисляются и проверяются", () =>
            {
                var player = NewPlayer();
                talents.GrantPoints(player, 3);
                Equal(3, player.TalentPoints, "начислено");
                Throws<ArgumentOutOfRangeException>(() => talents.GrantPoints(player, -1), "отрицательное начисление");
                Throws<ArgumentOutOfRangeException>(() => player.AddTalentPoints(-2), "отрицательные очки состояния");
                Throws<ArgumentOutOfRangeException>(() => player.SetTalentRank(First, -1), "отрицательный ранг");
            });

            Check("таланты: коды характеристик читаются и пишутся", () =>
            {
                foreach (TalentStat stat in Enum.GetValues(typeof(TalentStat)))
                {
                    var code = TalentStatCodec.ToCode(stat);
                    True(TalentStatCodec.TryParse(code, out var parsed), "код " + code);
                    Equal(stat, parsed, "характеристика " + code);
                }

                False(TalentStatCodec.TryParse("mana", out _), "неизвестный код");
            });

            Check("таланты: нулевой ранг убирает запись", () =>
            {
                var player = NewPlayer(points: 1);
                talents.Learn(player, First);
                Equal(1, player.TalentRanks.Count, "запись есть");
                player.SetTalentRank(First, 0);
                Equal(0, player.TalentRanks.Count, "запись убрана");
                Equal(0, talents.RankOf(player, First), "ранг");
            });
        }

        private static PlayerState NewPlayer(
            int points = 0,
            IReadOnlyDictionary<string, int>? ranks = null) =>
            new PlayerState(
                "player",
                1,
                0,
                100,
                8,
                new GridContainer("player", 8, 4),
                new InventoryEquipment("player", 4),
                Array.Empty<string>(),
                new NeedsState(100, 100, 100, 100),
                talentPoints: points,
                talentRanks: ranks);
    }
}
