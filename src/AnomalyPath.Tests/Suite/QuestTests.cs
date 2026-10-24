#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Di;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Quests;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.Services;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.World;

namespace AnomalyPath.Tests
{
    /// <summary>Повторяемые квесты: случайные предложения, принятие/отмена, цели, прогресс и сейв.</summary>
    public sealed class QuestTests : TestBase
    {
        private const string TraderId = "trd_hub_smuggler";
        private const string OtherTraderId = "trd_hub_medic";
        private const int OfferCooldown = 6;

        private readonly string _dataRoot;

        /// <summary>Создаёт категорию тестов квестов.</summary>
        /// <param name="dataRoot">Корень данных.</param>
        public QuestTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        /// <inheritdoc />
        public override void Run()
        {
            var services = Bootstrap.Build(_dataRoot);
            var quests = services.Resolve<IQuestService>();
            var repository = services.Resolve<IQuestRepository>();
            var inventory = services.Resolve<IInventoryService>();

            Check("квесты: шаблоны загружены и все повторяемые", () =>
            {
                Equal(6, repository.All.Count, "квестов");
                foreach (var quest in repository.All)
                {
                    Equal(QuestKind.Repeatable, quest.QuestKind, "вид квеста");
                }

                True(quests.TryGetTemplate("qst_clear_house", out var template), "шаблон для экрана");
                Equal(QuestType.ClearLocation, template.Type, "тип шаблона");
            });

            Check("квесты: торговец рандомит четыре предложения и держит их", () =>
            {
                var session = NewSession(services);
                var offers = quests.GetOffers(session, TraderId);
                Equal(4, offers.Count, "предложений");
                foreach (var offer in offers)
                {
                    True(repository.TryGet(offer.QuestId, out _), "квест из репозитория");
                    Equal(TraderId, offer.State.GiverActorId, "предложение закреплено за торговцем");
                    Equal(QuestStatus.Offered, offer.State.Status, "статус предложения");
                }

                var again = quests.GetOffers(session, TraderId);
                Equal(4, again.Count, "повторный запрос");
                for (var index = 0; index < offers.Count; index++)
                {
                    Equal(offers[index].QuestId, again[index].QuestId, "набор стабилен");
                }
            });

            Check("квесты: у двух торговцев независимые предложения", () =>
            {
                var session = NewSession(services);
                var smuggler = quests.GetOffers(session, TraderId);
                var medic = quests.GetOffers(session, OtherTraderId);
                Equal(4, smuggler.Count, "у контрабандиста");
                Equal(4, medic.Count, "у фельдшера");
                foreach (var offer in smuggler)
                {
                    Equal(TraderId, offer.State.GiverActorId, "чужое предложение");
                }

                foreach (var offer in medic)
                {
                    Equal(OtherTraderId, offer.State.GiverActorId, "чужое предложение");
                }
            });

            Check("квесты: принятый квест освобождает слот и новый появляется позже", () =>
            {
                var session = NewSession(services);
                var offers = quests.GetOffers(session, TraderId);
                var offer = offers[0];
                True(quests.Accept(session, TraderId, offer.QuestId).Succeeded, "взятие");
                Equal(1, quests.GetActive(session).Count, "активных");
                Equal(3, quests.GetOffers(session, TraderId).Count, "после взятия");
                session.State.World.Clock.Advance(OfferCooldown);
                Equal(4, quests.GetOffers(session, TraderId).Count, "после задержки");
            });

            Check("квесты: отменённое предложение заменяется позже", () =>
            {
                var session = NewSession(services);
                var offer = quests.GetOffers(session, TraderId)[0];
                Equal(QuestResultStatus.Ok,
                    quests.CancelOffer(session, TraderId, offer.QuestId).Status,
                    "отмена предложения");
                Equal(3, quests.GetOffers(session, TraderId).Count, "сразу после отмены");
                session.State.World.Clock.Advance(OfferCooldown);
                Equal(4, quests.GetOffers(session, TraderId).Count, "после задержки");
            });

            Check("квесты: активный квест можно отменить", () =>
            {
                var session = NewSession(services);
                var offer = quests.GetOffers(session, TraderId)[0];
                True(quests.Accept(session, TraderId, offer.QuestId).Succeeded, "взятие");
                True(quests.CancelQuest(session, offer.QuestId).Succeeded, "отмена");
                Equal(0, quests.GetActive(session).Count, "активных после отмены");
                True(session.State.TryGetQuest(offer.QuestId, out var state), "история");
                Equal(QuestStatus.Cancelled, state!.Status, "статус");
                Equal(3, quests.GetOffers(session, TraderId).Count, "слот свободен");
                session.State.World.Clock.Advance(OfferCooldown);
                Equal(4, quests.GetOffers(session, TraderId).Count, "слот заполнен");
            });

            Check("квесты: зачистка добавляет локацию в маршрут и сдаётся", () =>
            {
                var session = NewSession(services);
                var offer = RequireOffer(quests, session, QuestType.ClearLocation);
                var state = Accept(quests, session, offer);
                Equal(offer.Template.Id, state.QuestId, "шаблон");
                Equal(offer.State.LocationTemplateId, state.LocationTemplateId, "шаблон цели");
                True(state.LocationInstanceId != null, "экземпляр цели");
                var map = session.State.World.Map;
                True(map.TryGetLocation(state.LocationInstanceId!, out var target), "локация на карте");
                Equal(state.LocationTemplateId, target!.TemplateId, "шаблон экземпляра");
                True(HasPath(session, state.LocationInstanceId!), "маршрут от хаба");

                quests.NotifyLocationCleared(session, state.LocationInstanceId!);
                True(quests.GetProgress(session, state).Ready, "цель выполнена");
                var money = session.State.Player!.Money;
                True(quests.TurnIn(session, offer.State.GiverActorId, offer.QuestId).Succeeded, "сдача");
                Equal(money + offer.Template.Rewards.Money, session.State.Player.Money, "награда");
                Equal(QuestStatus.Completed, state.Status, "статус");
                True(session.State.Player.HasCompletedQuest(offer.QuestId), "квест засчитан");
            });

            Check("квесты: убийство считает только целевых мутантов", () =>
            {
                var session = NewSession(services);
                var offer = RequireOffer(quests, session, QuestType.KillMutants);
                var state = Accept(quests, session, offer);
                Equal(2, state.ActorIds.Count, "размер пула");
                Equal(3, state.RequiredCount, "нужно убить");
                quests.NotifyActorKilled(session, "enemy_bandit");
                Equal(0, state.KillCount, "чужой не считается");
                for (var index = 0; index < state.RequiredCount; index++)
                {
                    quests.NotifyActorKilled(session, state.ActorIds[0]);
                }

                True(quests.GetProgress(session, state).Ready, "целей хватает");
                True(quests.TurnIn(session, offer.State.GiverActorId, offer.QuestId).Succeeded, "сдача");
            });

            Check("квесты: артефакты сдаются из инвентаря", () =>
            {
                var session = NewSession(services);
                var offer = RequireOffer(quests, session, QuestType.GatherArtifacts);
                var state = Accept(quests, session, offer);
                Equal(2, state.RequiredCount, "нужно артефактов");
                foreach (var target in state.Items)
                {
                    inventory.TryAdd(session.State.Player!.Inventory, target.ItemId, 1, out _);
                }

                var progress = quests.GetProgress(session, state);
                True(progress.Ready, "артефактов хватает");
                Equal(2, progress.Current, "собрано");
                var money = session.State.Player!.Money;
                True(quests.TurnIn(session, offer.State.GiverActorId, offer.QuestId).Succeeded, "сдача");
                Equal(money + 450, session.State.Player.Money, "награда");
                foreach (var target in state.Items)
                {
                    Equal(0, CountItem(session.State.Player, target.ItemId), "артефакт изъят");
                }
            });

            Check("квесты: оружие и броня принимаются в любом состоянии", () =>
            {
                var session = NewSession(services);
                var offer = RequireOffer(quests, session, QuestType.FindItems);
                var state = Accept(quests, session, offer);
                Equal(1, state.Items.Count, "видов предметов");
                var itemId = state.Items[0].ItemId;
                inventory.TryAdd(
                    session.State.Player!.Inventory,
                    itemId,
                    state.Items[0].Count,
                    0,
                    "broken",
                    out _);
                for (var index = 0; index < state.RequiredCount; index++)
                {
                    quests.NotifyActorKilled(session, state.ActorIds[0]);
                }

                True(quests.GetProgress(session, state).Ready, "битый предмет принят");
                var money = session.State.Player.Money;
                True(quests.TurnIn(session, offer.State.GiverActorId, offer.QuestId).Succeeded, "сдача");
                Equal(money + offer.Template.Rewards.Money, session.State.Player.Money, "награда");
                Equal(0, CountItem(session.State.Player, itemId), "предмет изъят");
            });

            Check("квесты: предел активных", () =>
            {
                var session = NewSession(services);
                var offers = quests.GetOffers(session, TraderId);
                True(quests.Accept(session, TraderId, offers[0].QuestId).Succeeded, "первый");
                True(quests.Accept(session, TraderId, offers[1].QuestId).Succeeded, "второй");
                True(quests.Accept(session, TraderId, offers[2].QuestId).Succeeded, "третий");
                var other = quests.GetOffers(session, OtherTraderId);
                Equal(QuestResultStatus.ActiveLimit,
                    quests.Accept(session, OtherTraderId, other[0].QuestId).Status,
                    "четвёртый не берётся");
            });

            Check("квесты: после сдачи шаблон уходит на откат", () =>
            {
                var session = NewSession(services);
                var offer = RequireOffer(quests, session, QuestType.ClearLocation);
                var state = Accept(quests, session, offer);
                quests.NotifyLocationCleared(session, state.LocationInstanceId!);
                True(quests.TurnIn(session, offer.State.GiverActorId, offer.QuestId).Succeeded, "сдача");
                session.State.World.Clock.Advance(OfferCooldown);
                foreach (var next in quests.GetOffers(session, offer.State.GiverActorId))
                {
                    False(string.Equals(next.QuestId, offer.QuestId, StringComparison.Ordinal),
                        "квест на откате не предлагается");
                }
            });

            Check("квесты: предложения, слоты и цель переживают сейв", () =>
            {
                WithSaveRoot(root =>
                {
                    var map = services.Resolve<IWorldGenerator>().Generate(42);
                    var world = WorldState.Start(map);
                    var player = NewPlayer(2000);
                    var game = new GameState(world, player);
                    var session = new GameSession(game);
                    var offer = RequireOffer(quests, session, QuestType.ClearLocation);
                    Accept(quests, session, offer);
                    var service = new SaveService(new FileSaveStorage(root), services.Resolve<IItemRepository>());
                    service.Save(game, "slot_1", 0, new DateTimeOffset(2026, 10, 24, 10, 0, 0, TimeSpan.Zero));

                    var loaded = service.Load("slot_1");
                    True(loaded.TryGetActiveQuest(offer.QuestId, out var restored), "активный квест в сейве");
                    True(loaded.World.Map.TryGetLocation(restored!.LocationInstanceId!, out _), "локация в сейве");
                    True(loaded.TryGetTrader(TraderId, out var trader), "торговец в сейве");
                    Equal(4, trader!.QuestSlotAvailableAt.Count, "слоты квестов");
                    var loadedSession = new GameSession(loaded);
                    False(quests.GetProgress(loadedSession, restored).Ready, "до зачистки не готов");
                });
            });
        }

        private static QuestOffer RequireOffer(
            IQuestService quests,
            GameSession session,
            QuestType type)
        {
            for (var attempt = 0; attempt < 100; attempt++)
            {
                foreach (var trader in new[] { TraderId, OtherTraderId })
                {
                    foreach (var offer in quests.GetOffers(session, trader))
                    {
                        if (offer.Template.Type == type)
                        {
                            return offer;
                        }
                    }
                }

                var offers = quests.GetOffers(session, TraderId);
                if (offers.Count == 0)
                {
                    break;
                }

                quests.CancelOffer(session, offers[0].State.GiverActorId, offers[0].QuestId);
                session.State.World.Clock.Advance(OfferCooldown);
            }

            throw new InvalidOperationException("Не удалось получить квест типа " + type);
        }

        private static QuestState Accept(IQuestService quests, GameSession session, QuestOffer offer)
        {
            var result = quests.Accept(session, offer.State.GiverActorId, offer.QuestId);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Не удалось взять квест: " + result.Status);
            }

            if (!session.State.TryGetActiveQuest(offer.QuestId, out var state) || state == null)
            {
                throw new InvalidOperationException("Квест не стал активным");
            }

            return state;
        }

        private static GameSession NewSession(ServiceContainer services)
        {
            var generator = services.Resolve<IWorldGenerator>();
            var world = WorldState.Start(generator.Generate(42));
            return new GameSession(new GameState(world, NewPlayer(2000)));
        }

        private static PlayerState NewPlayer(int money)
        {
            return new PlayerState(
                "player",
                2,
                0,
                100,
                8,
                new GridContainer("player", 10, 6),
                new Equipment("player", 4),
                Array.Empty<string>(),
                new NeedsState(100, 100, 100, 100),
                "Тест",
                money);
        }

        private static bool HasPath(GameSession session, string target)
        {
            var map = session.State.World.Map;
            var visited = new HashSet<string>(StringComparer.Ordinal);
            var queue = new Queue<string>();
            queue.Enqueue(session.State.World.CurrentLocationId);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!visited.Add(current))
                {
                    continue;
                }

                foreach (var edge in map.EdgesFrom(current))
                {
                    if (string.Equals(edge.ToId, target, StringComparison.Ordinal))
                    {
                        return true;
                    }

                    queue.Enqueue(edge.ToId);
                }
            }

            return false;
        }

        private static int CountItem(PlayerState player, string itemId)
        {
            var count = 0;
            foreach (var item in player.Inventory.Items)
            {
                if (item.ItemId == itemId)
                {
                    count += item.Stack;
                }
            }

            return count;
        }

        private static void WithSaveRoot(Action<string> body)
        {
            var root = Path.Combine(Path.GetTempPath(), "anomalypath-quests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                body(root);
            }
            finally
            {
                try
                {
                    Directory.Delete(root, recursive: true);
                }
                catch (IOException)
                {
                    // временная папка не критична
                }
            }
        }
    }
}
