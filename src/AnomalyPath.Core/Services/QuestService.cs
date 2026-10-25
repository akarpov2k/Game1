#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Quests;
using AnomalyPath.Core.Rules;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.Templates;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Services
{
    /// <summary>
    /// Повторяемые квесты: случайные предложения у торговца, принятие и отмена,
    /// прогресс по событиям и сдача с наградой.
    /// </summary>
    public sealed partial class QuestService : IQuestService
    {
        private readonly IQuestRepository _quests;
        private readonly ITradeService _trade;
        private readonly IActorRepository _actors;
        private readonly ILocationRepository _locations;
        private readonly ITopologyRepository _topology;
        private readonly IItemRepository _items;
        private readonly IInventoryService _inventory;
        private readonly IGameRulesRepository _rules;

        /// <summary>Создаёт сервис квестов.</summary>
        /// <param name="quests">Шаблоны квестов.</param>
        /// <param name="trade">Доступность торговцев.</param>
        /// <param name="actors">Актёры: пулы мутантов и NPC.</param>
        /// <param name="locations">Шаблоны локаций: пул целей и маршрут.</param>
        /// <param name="topology">Топология: способы перехода.</param>
        /// <param name="items">Предметы: пулы артефактов, оружия и брони.</param>
        /// <param name="inventory">Инвентарь: проверка и выдача предметов.</param>
        /// <param name="rules">Правила: число предложений, откат и предел активных.</param>
        /// <exception cref="ArgumentNullException">Зависимость не задана.</exception>
        public QuestService(
            IQuestRepository quests,
            ITradeService trade,
            IActorRepository actors,
            ILocationRepository locations,
            ITopologyRepository topology,
            IItemRepository items,
            IInventoryService inventory,
            IGameRulesRepository rules)
        {
            _quests = quests ?? throw new ArgumentNullException(nameof(quests));
            _trade = trade ?? throw new ArgumentNullException(nameof(trade));
            _actors = actors ?? throw new ArgumentNullException(nameof(actors));
            _locations = locations ?? throw new ArgumentNullException(nameof(locations));
            _topology = topology ?? throw new ArgumentNullException(nameof(topology));
            _items = items ?? throw new ArgumentNullException(nameof(items));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }
    }
}
