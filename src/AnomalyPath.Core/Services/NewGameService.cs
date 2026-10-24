#nullable enable
using System;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Loadouts;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Services
{
    /// <summary>Сборка новой игры: мир, игрок, выбранный набор снаряжения и настройки сложности.</summary>
    public sealed class NewGameService : INewGameService
    {
        private readonly IGameRulesRepository _rules;
        private readonly IWorldGenerator _world;
        private readonly IActorRepository _actors;
        private readonly ILoadoutPreviewService _previews;
        private readonly INicknameGenerator _nicknames;

        /// <summary>Создаёт сервис новой игры.</summary>
        /// <param name="rules">Правила прохождения.</param>
        /// <param name="world">Генератор мира.</param>
        /// <param name="actors">Репозиторий актёров.</param>
        /// <param name="previews">Раскладка набора снаряжения: инвентарь и слоты игрока.</param>
        /// <param name="nicknames">Генератор ников.</param>
        /// <exception cref="ArgumentNullException">Зависимость не задана.</exception>
        public NewGameService(
            IGameRulesRepository rules,
            IWorldGenerator world,
            IActorRepository actors,
            ILoadoutPreviewService previews,
            INicknameGenerator nicknames)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _actors = actors ?? throw new ArgumentNullException(nameof(actors));
            _previews = previews ?? throw new ArgumentNullException(nameof(previews));
            _nicknames = nicknames ?? throw new ArgumentNullException(nameof(nicknames));
        }

        /// <inheritdoc />
        public GameState Create(int seed, string loadoutId, NewGameSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var rules = _rules.Require();
            if (!string.IsNullOrWhiteSpace(settings.Nickname) &&
                settings.Nickname.Length > rules.NicknameMaxLength)
            {
                throw new DataException("player", CoreErrors.NicknameLength, rules.NicknameMaxLength);
            }

            var preview = _previews.Build(loadoutId);
            RequireCompleteLoadout(preview);

            var map = _world.Generate(seed);
            var world = WorldState.Start(map);
            var actor = _actors.Get(rules.PlayerActorId);
            var combat = actor.Combat ?? throw new DataException(actor.Id, CoreErrors.ActorWithoutCombat);
            var needs = new NeedsState(rules.NeedsMax, rules.NeedsMax, rules.NeedsMax, rules.NeedsMax);
            var nickname = string.IsNullOrWhiteSpace(settings.Nickname)
                ? _nicknames.Generate(seed)
                : settings.Nickname;
            var player = new PlayerState(
                "player",
                combat.Level,
                0,
                combat.Health,
                combat.Ap,
                preview.Inventory,
                preview.Equipment,
                new string[0],
                needs,
                nickname,
                preview.Loadout.StartingMoney ?? rules.StartingMoney);

            return new GameState(world, player, settings);
        }

        private static void RequireCompleteLoadout(LoadoutPreview preview)
        {
            if (preview.Rejected.Count > 0)
            {
                var pair = preview.Rejected[0];
                throw new DataException(
                    preview.Loadout.Id,
                    CoreErrors.LoadoutSlotMismatch,
                    pair.Value,
                    EquipmentSlots.Id(pair.Key));
            }

            if (preview.Overflow.Count > 0)
            {
                throw new DataException(preview.Loadout.Id, CoreErrors.LoadoutOverflow, preview.Overflow[0].ItemId);
            }
        }
    }
}
