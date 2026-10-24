#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Quests;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.Trade;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Save
{
    /// <summary>Состояние прохождения: мир, игрок, торговцы, квесты и выбранные на старте настройки.</summary>
    public sealed class GameState
    {
        private readonly List<TraderState> _traders;
        private readonly List<QuestState> _quests;

        /// <summary>Создаёт состояние прохождения.</summary>
        /// <param name="world">Состояние мира.</param>
        /// <param name="player">Состояние игрока или <c>null</c> для world-only сейва.</param>
        /// <param name="settings">Настройки новой игры или <c>null</c> для старых сейвов.</param>
        /// <param name="traders">Состояние торговцев; <c>null</c> — пустой список.</param>
        /// <param name="quests">Состояние квестов; <c>null</c> — пустой список.</param>
        /// <param name="questSerial">Счётчик выданных квестов для уникальных id целей.</param>
        public GameState(
            WorldState world,
            PlayerState? player,
            NewGameSettings? settings = null,
            IReadOnlyList<TraderState>? traders = null,
            IReadOnlyList<QuestState>? quests = null,
            int questSerial = 0)
        {
            World = world;
            Player = player;
            Settings = settings;
            _traders = traders == null ? new List<TraderState>() : new List<TraderState>(traders);
            _quests = quests == null ? new List<QuestState>() : new List<QuestState>(quests);
            QuestSerial = questSerial;
        }

        /// <summary>Состояние мира.</summary>
        public WorldState World { get; }

        /// <summary>Состояние игрока или <c>null</c>.</summary>
        public PlayerState? Player { get; }

        /// <summary>Настройки новой игры или <c>null</c>.</summary>
        public NewGameSettings? Settings { get; }

        /// <summary>Состояние торговцев.</summary>
        public IReadOnlyList<TraderState> Traders => _traders;

        /// <summary>Состояние квестов: активные, сданные и проваленные.</summary>
        public IReadOnlyList<QuestState> Quests => _quests;

        /// <summary>Счётчик выданных квестов для уникальных id целей.</summary>
        public int QuestSerial { get; private set; }

        /// <summary>Выдаёт следующий номер квеста.</summary>
        /// <returns>Положительный номер.</returns>
        public int NextQuestSerial()
        {
            QuestSerial++;
            return QuestSerial;
        }

        /// <summary>Добавляет состояние квеста.</summary>
        /// <param name="quest">Состояние квеста.</param>
        /// <exception cref="ArgumentNullException">Состояние не задано.</exception>
        public void AddQuest(QuestState quest)
        {
            if (quest == null)
            {
                throw new ArgumentNullException(nameof(quest));
            }

            _quests.Add(quest);
        }

        /// <summary>Ищет квест по идентификатору.</summary>
        /// <param name="questId">Идентификатор квеста.</param>
        /// <param name="quest">Найденное состояние или <c>null</c>.</param>
        /// <returns><c>true</c>, если квест есть в журнале.</returns>
        public bool TryGetQuest(string questId, out QuestState? quest)
        {
            foreach (var item in _quests)
            {
                if (string.Equals(item.QuestId, questId, StringComparison.Ordinal))
                {
                    quest = item;
                    return true;
                }
            }

            quest = null;
            return false;
        }

        /// <summary>Ищет активный квест по идентификатору.</summary>
        /// <param name="questId">Идентификатор квеста.</param>
        /// <param name="quest">Найденное состояние или <c>null</c>.</param>
        /// <returns><c>true</c>, если квест взят и ещё не сдан.</returns>
        public bool TryGetActiveQuest(string questId, out QuestState? quest)
        {
            foreach (var item in _quests)
            {
                if (item.Status == QuestStatus.Active &&
                    string.Equals(item.QuestId, questId, StringComparison.Ordinal))
                {
                    quest = item;
                    return true;
                }
            }

            quest = null;
            return false;
        }

        /// <summary>Находит состояние торговца или создаёт пустое.</summary>
        /// <param name="actorId">Идентификатор торговца.</param>
        /// <returns>Изменяемое состояние торговца.</returns>
        public TraderState GetOrCreateTrader(string actorId)
        {
            foreach (var trader in _traders)
            {
                if (string.Equals(trader.ActorId, actorId, StringComparison.Ordinal))
                {
                    return trader;
                }
            }

            var created = new TraderState(actorId);
            _traders.Add(created);
            return created;
        }

        /// <summary>Пытается найти состояние торговца.</summary>
        /// <param name="actorId">Идентификатор торговца.</param>
        /// <param name="state">Найденное состояние или <c>null</c>.</param>
        /// <returns><c>true</c>, если состояние уже создано.</returns>
        public bool TryGetTrader(string actorId, out TraderState? state)
        {
            foreach (var trader in _traders)
            {
                if (string.Equals(trader.ActorId, actorId, StringComparison.Ordinal))
                {
                    state = trader;
                    return true;
                }
            }

            state = null;
            return false;
        }
    }
}
