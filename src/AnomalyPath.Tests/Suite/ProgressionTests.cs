#nullable enable
using System;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Progression;
using AnomalyPath.Core.Services;
using AnomalyPath.Core.Session;
using InventoryEquipment = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Tests
{
    /// <summary>Левелинг: перенос излишка опыта, награды за уровень и потеря опыта при смерти.</summary>
    public sealed class ProgressionTests : TestBase
    {
        private readonly IProgressionService _progression;

        /// <summary>Создаёт категорию тестов прогрессии.</summary>
        /// <param name="dataRoot">Корень данных проекта.</param>
        public ProgressionTests(string dataRoot)
        {
            _progression = Bootstrap.Build(dataRoot).Resolve<IProgressionService>();
        }

        /// <inheritdoc />
        public override void Run()
        {
            AddXpTests();
            RewardTests();
            SyncTests();
            DeathLossTests();
            EdgeCaseTests();
        }

        private void AddXpTests()
        {
            Check("левелинг: опыт ниже требования уровень не повышает", () =>
            {
                var player = NewPlayer();
                var gained = _progression.AddXp(player, 99);

                Equal(0, gained, "уровней добавлено");
                Equal(1, player.Level, "уровень");
                Equal(99, player.Xp, "опыт");
            });

            Check("левелинг: ровно требование повышает уровень", () =>
            {
                var player = NewPlayer();
                var gained = _progression.AddXp(player, 100);

                Equal(1, gained, "уровней добавлено");
                Equal(2, player.Level, "уровень");
                Equal(100, player.Xp, "опыт");
            });

            Check("левелинг: излишек опыта переносится на следующий уровень", () =>
            {
                var player = NewPlayer();
                _progression.AddXp(player, 130);

                Equal(2, player.Level, "сразу второй уровень");
                Equal(130, player.Xp, "излишек сохранён");

                _progression.AddXp(player, 90);
                Equal(3, player.Level, "сумма 220 закрывает второй переход");
                Equal(220, player.Xp, "опыт не потерян");
            });

            Check("левелинг: за одно начисление можно получить несколько уровней", () =>
            {
                var player = NewPlayer();
                var gained = _progression.AddXp(player, 220);

                Equal(2, gained, "уровней добавлено");
                Equal(3, player.Level, "уровень");
            });
        }

        private void RewardTests()
        {
            Check("награды: до пятого уровня очков и здоровья не даётся", () =>
            {
                var player = NewPlayer();
                _progression.AddXp(player, 536);

                Equal(4, player.Level, "уровень");
                Equal(0, player.TalentPoints, "очки талантов");
                Equal(0, player.LevelHealthBonus, "прибавка здоровья");
            });

            Check("награды: достижение пятого уровня даёт очко талантов и +2 здоровья", () =>
            {
                var player = NewPlayer();
                _progression.AddXp(player, 537);

                Equal(5, player.Level, "уровень");
                Equal(1, player.TalentPoints, "очки талантов");
                Equal(2, player.LevelHealthBonus, "прибавка здоровья");
            });

            Check("награды: каждый следующий уровень снова даёт очко и +2 здоровья", () =>
            {
                var player = NewPlayer();
                _progression.AddXp(player, 744);

                Equal(6, player.Level, "уровень");
                Equal(2, player.TalentPoints, "очки талантов");
                Equal(4, player.LevelHealthBonus, "прибавка здоровья");
            });

            Check("награды: числа берутся из настроек прогрессии", () =>
            {
                var custom = new ProgressionService(
                    new ProgressionSettings("progression", 100, 20, 10, 10, 5, 5, 3, 2, 5));
                var player = NewPlayer();
                custom.AddXp(player, 220);

                Equal(3, player.Level, "уровень");
                Equal(2, player.TalentPoints, "очки из данных");
                Equal(5, player.LevelHealthBonus, "здоровье из данных");
            });

            Check("награды: нули в данных отключают награды", () =>
            {
                var custom = new ProgressionService(
                    new ProgressionSettings("progression", 100, 20, 10, 10, 5, 5, 1, 0, 0));
                var player = NewPlayer();
                custom.AddXp(player, 100);

                Equal(2, player.Level, "уровень");
                Equal(0, player.TalentPoints, "очки");
                Equal(0, player.LevelHealthBonus, "здоровье");
            });
        }

        private void SyncTests()
        {
            Check("левелинг: старый сейв с накопленным опытом подтягивает уровень", () =>
            {
                var player = NewPlayer();
                player.AddXp(744);

                var gained = _progression.SyncLevel(player);

                Equal(5, gained, "уровней добавлено");
                Equal(6, player.Level, "уровень");
                Equal(2, player.TalentPoints, "награды по уровням 5 и 6");
                Equal(4, player.LevelHealthBonus, "здоровье по уровням 5 и 6");
            });

            Check("левелинг: повторная синхронизация не выдаёт награды заново", () =>
            {
                var player = NewPlayer();
                player.AddXp(744);
                _progression.SyncLevel(player);
                var points = player.TalentPoints;
                var health = player.LevelHealthBonus;

                var again = _progression.SyncLevel(player);

                Equal(0, again, "уровней добавлено");
                Equal(points, player.TalentPoints, "очки не изменились");
                Equal(health, player.LevelHealthBonus, "здоровье не изменилось");
            });
        }

        private void DeathLossTests()
        {
            Check("смерть: теряется 40% опыта текущего уровня", () =>
            {
                var player = NewPlayer(level: 3, xp: 270);
                var lost = _progression.LoseXpOnDeath(player, 0.4);

                Equal(20, lost, "потеряно");
                Equal(250, player.Xp, "опыт");
                Equal(3, player.Level, "уровень не понизился");
            });

            Check("смерть: 100% обнуляют прогресс уровня, но не понижают уровень", () =>
            {
                var player = NewPlayer(level: 3, xp: 270);
                var lost = _progression.LoseXpOnDeath(player, 1);

                Equal(50, lost, "потеряно");
                Equal(220, player.Xp, "опыт на начале уровня");
                Equal(3, player.Level, "уровень");
            });

            Check("смерть: потеря не уходит ниже нуля прогресса", () =>
            {
                var player = NewPlayer(level: 3, xp: 220);
                var lost = _progression.LoseXpOnDeath(player, 1);

                Equal(0, lost, "терять нечего");
                Equal(220, player.Xp, "опыт");
            });

            Check("смерть: потеря округляется от нуля", () =>
            {
                var player = NewPlayer(level: 2, xp: 103);
                var lost = _progression.LoseXpOnDeath(player, 0.4);

                Equal(1, lost, "1,2 округляется до 1");
                Equal(102, player.Xp, "опыт");
            });

            Check("смерть: нулевой коэффициент не трогает опыт", () =>
            {
                var player = NewPlayer(level: 3, xp: 270);
                var lost = _progression.LoseXpOnDeath(player, 0);

                Equal(0, lost, "потеряно");
                Equal(270, player.Xp, "опыт");
            });
        }

        private void EdgeCaseTests()
        {
            Check("левелинг: границы аргументов", () =>
            {
                var player = NewPlayer();
                Throws<ArgumentNullException>(() => _progression.AddXp(null!, 1), "игрок");
                Throws<ArgumentOutOfRangeException>(() => _progression.AddXp(player, -1), "опыт");
                Throws<ArgumentNullException>(() => _progression.SyncLevel(null!), "игрок в синхронизации");
                Throws<ArgumentNullException>(() => _progression.LoseXpOnDeath(null!, 0.4), "игрок в смерти");
                Throws<ArgumentOutOfRangeException>(() => _progression.LoseXpOnDeath(player, -0.1), "доля");
                Throws<ArgumentOutOfRangeException>(() => _progression.LoseXpOnDeath(player, 1.1), "доля");
            });

            Check("настройки: потеря опыта ограничена диапазоном 0..1", () =>
            {
                Equal(0.6, new NewGameSettings(0, 1, xpLossChance: 0.6).XpLossChance, "значение");
                Throws<ArgumentOutOfRangeException>(
                    () => new NewGameSettings(0, 1, xpLossChance: -0.1),
                    "доля ниже нуля");
                Throws<ArgumentOutOfRangeException>(
                    () => new NewGameSettings(0, 1, xpLossChance: 1.1),
                    "доля выше единицы");
            });

            Check("состояние: уровень и опыт меняются только через проверки", () =>
            {
                var player = NewPlayer();

                Throws<ArgumentOutOfRangeException>(() => player.SetLevel(0), "уровень 0");
                Throws<ArgumentOutOfRangeException>(() => player.RemoveXp(-1), "отрицательный опыт");
                Throws<ArgumentOutOfRangeException>(
                    () => player.AddLevelHealthBonus(-1),
                    "отрицательная прибавка здоровья");

                Equal(0, player.RemoveXp(2), "опыта было 0");
                Equal(0, player.Xp, "опыт не ушёл в минус");
            });
        }

        private static PlayerState NewPlayer(int level = 1, int xp = 0)
        {
            return new PlayerState(
                "player",
                level,
                xp,
                10,
                10,
                new GridContainer("player", 8, 4),
                new InventoryEquipment("player", 1),
                Array.Empty<string>(),
                new NeedsState(10, 10, 10, 10));
        }
    }
}
