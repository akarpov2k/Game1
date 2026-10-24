#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Localization;
using InventoryEquipment = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Core.Player
{
    /// <summary>Состояние игрока: здоровье, опыт, инвентарь, экипировка и потребности.</summary>
    public sealed class PlayerState
    {
        private readonly List<string> _completedQuests;
        private readonly List<string> _unlocks;
        private readonly Dictionary<string, int> _reputation;
        private readonly Dictionary<string, int> _talentRanks;

        /// <summary>Создаёт состояние игрока.</summary>
        /// <param name="id">Идентификатор игрока.</param>
        /// <param name="level">Уровень.</param>
        /// <param name="xp">Опыт.</param>
        /// <param name="health">Текущее здоровье.</param>
        /// <param name="apMax">Максимум очков действия.</param>
        /// <param name="inventory">Сетка инвентаря.</param>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="skills">Выученные скиллы.</param>
        /// <param name="needs">Потребности.</param>
        /// <param name="name">Ник персонажа; пусто — взять id.</param>
        /// <param name="money">Деньги на персонаже; по умолчанию 0.</param>
        /// <param name="completedQuests">Завершённые квесты.</param>
        /// <param name="unlocks">Полученные открытия.</param>
        /// <param name="reputation">Репутация по фракциям.</param>
        /// <param name="talentPoints">Свободные очки талантов; по умолчанию 0.</param>
        /// <param name="talentRanks">Ранги талантов по id; пусто — ни одного.</param>
        /// <exception cref="ArgumentException">id пуст или зависимости не заданы.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Числовые значения вне допустимых границ.</exception>
        public PlayerState(
            string id,
            int level,
            int xp,
            int health,
            int apMax,
            GridContainer inventory,
            InventoryEquipment equipment,
            IReadOnlyList<string> skills,
            NeedsState needs,
            string? name = null,
            int money = 0,
            IReadOnlyList<string>? completedQuests = null,
            IReadOnlyList<string>? unlocks = null,
            IReadOnlyDictionary<string, int>? reputation = null,
            int talentPoints = 0,
            IReadOnlyDictionary<string, int>? talentRanks = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(id)),
                    nameof(id));
            }

            if (level < 1 || xp < 0 || health < 0 || apMax < 0 || money < 0 || talentPoints < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            if (talentRanks != null)
            {
                foreach (var pair in talentRanks)
                {
                    if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value < 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(talentRanks));
                    }
                }
            }

            Id = id;
            Name = string.IsNullOrWhiteSpace(name) ? id : name.Trim();
            Level = level;
            Xp = xp;
            Health = health;
            ApMax = apMax;
            Money = money;
            TalentPoints = talentPoints;
            Inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
            Skills = skills ?? throw new ArgumentNullException(nameof(skills));
            Needs = needs ?? throw new ArgumentNullException(nameof(needs));
            _completedQuests = completedQuests == null ? new List<string>() : new List<string>(completedQuests);
            _unlocks = unlocks == null ? new List<string>() : new List<string>(unlocks);
            _reputation = reputation == null
                ? new Dictionary<string, int>(StringComparer.Ordinal)
                : new Dictionary<string, int>(reputation, StringComparer.Ordinal);
            _talentRanks = talentRanks == null
                ? new Dictionary<string, int>(StringComparer.Ordinal)
                : new Dictionary<string, int>(talentRanks, StringComparer.Ordinal);
            CompletedQuests = _completedQuests;
            Unlocks = _unlocks;
            Reputation = _reputation;
            TalentRanks = _talentRanks;
        }

        /// <summary>Идентификатор игрока.</summary>
        public string Id { get; }

        /// <summary>Ник персонажа.</summary>
        public string Name { get; }

        /// <summary>Уровень.</summary>
        public int Level { get; }

        /// <summary>Опыт.</summary>
        public int Xp { get; private set; }

        /// <summary>Текущее здоровье.</summary>
        public int Health { get; private set; }

        /// <summary>Максимум очков действия.</summary>
        public int ApMax { get; }

        /// <summary>Деньги на персонаже.</summary>
        public int Money { get; private set; }

        /// <summary>Свободные очки талантов.</summary>
        public int TalentPoints { get; private set; }

        /// <summary>Сетка инвентаря.</summary>
        public GridContainer Inventory { get; }

        /// <summary>Экипировка.</summary>
        public InventoryEquipment Equipment { get; }

        /// <summary>Выученные скиллы.</summary>
        public IReadOnlyList<string> Skills { get; }

        /// <summary>Потребности.</summary>
        public NeedsState Needs { get; }

        /// <summary>Завершённые квесты.</summary>
        public IReadOnlyList<string> CompletedQuests { get; }

        /// <summary>Полученные открытия.</summary>
        public IReadOnlyList<string> Unlocks { get; }

        /// <summary>Репутация по фракциям.</summary>
        public IReadOnlyDictionary<string, int> Reputation { get; }

        /// <summary>Вложенные ранги талантов по идентификатору.</summary>
        public IReadOnlyDictionary<string, int> TalentRanks { get; }

        /// <summary>Добавляет деньги.</summary>
        /// <param name="amount">Сумма; не может быть отрицательной.</param>
        /// <exception cref="ArgumentOutOfRangeException">Сумма отрицательна.</exception>
        public void AddMoney(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            Money += amount;
        }

        /// <summary>Добавляет опыт.</summary>
        /// <param name="amount">Сколько опыта добавить; не меньше нуля.</param>
        /// <exception cref="ArgumentOutOfRangeException">Количество отрицательно.</exception>
        public void AddXp(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            Xp += amount;
        }

        /// <summary>Устанавливает текущее здоровье.</summary>
        /// <param name="health">Новое значение; не меньше нуля.</param>
        /// <exception cref="ArgumentOutOfRangeException">Здоровье отрицательно.</exception>
        public void SetHealth(int health)
        {
            if (health < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(health));
            }

            Health = health;
        }

        /// <summary>Списывает деньги, если их хватает.</summary>
        /// <param name="amount">Сумма; не может быть отрицательной.</param>
        /// <returns><c>true</c>, если деньги списаны.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Сумма отрицательна.</exception>
        public bool TrySpendMoney(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (Money < amount)
            {
                return false;
            }

            Money -= amount;
            return true;
        }

        /// <summary>Проверяет, завершён ли квест.</summary>
        /// <param name="questId">Идентификатор квеста.</param>
        /// <returns><c>true</c>, если квест завершён.</returns>
        public bool HasCompletedQuest(string questId)
        {
            foreach (var quest in CompletedQuests)
            {
                if (string.Equals(quest, questId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Проверяет, получено ли открытие.</summary>
        /// <param name="unlockId">Идентификатор открытия.</param>
        /// <returns><c>true</c>, если открытие получено.</returns>
        public bool HasUnlock(string unlockId)
        {
            foreach (var unlock in Unlocks)
            {
                if (string.Equals(unlock, unlockId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Возвращает репутацию у фракции; 0, если фракция неизвестна.</summary>
        /// <param name="factionId">Идентификатор фракции.</param>
        /// <returns>Репутация.</returns>
        public int ReputationWith(string factionId) =>
            Reputation.TryGetValue(factionId, out var value) ? value : 0;

        /// <summary>Добавляет квест в список завершённых.</summary>
        /// <param name="questId">Идентификатор квеста.</param>
        public void CompleteQuest(string questId)
        {
            if (!string.IsNullOrWhiteSpace(questId) && !_completedQuests.Contains(questId))
            {
                _completedQuests.Add(questId);
            }
        }

        /// <summary>Добавляет открытие.</summary>
        /// <param name="unlockId">Идентификатор открытия.</param>
        public void AddUnlock(string unlockId)
        {
            if (!string.IsNullOrWhiteSpace(unlockId) && !_unlocks.Contains(unlockId))
            {
                _unlocks.Add(unlockId);
            }
        }

        /// <summary>Изменяет репутацию фракции.</summary>
        /// <param name="factionId">Идентификатор фракции.</param>
        /// <param name="delta">Изменение; может быть отрицательным.</param>
        public void AddReputation(string factionId, int delta)
        {
            if (string.IsNullOrWhiteSpace(factionId))
            {
                return;
            }

            _reputation[factionId] = ReputationWith(factionId) + delta;
        }

        /// <summary>Возвращает ранг таланта; 0, если талант не изучен.</summary>
        /// <param name="talentId">Идентификатор таланта.</param>
        /// <returns>Вложенный ранг.</returns>
        public int GetTalentRank(string talentId) =>
            !string.IsNullOrWhiteSpace(talentId) && _talentRanks.TryGetValue(talentId, out var rank) ? rank : 0;

        /// <summary>Записывает ранг таланта; 0 убирает запись.</summary>
        /// <param name="talentId">Идентификатор таланта.</param>
        /// <param name="rank">Новый ранг; не меньше нуля.</param>
        /// <exception cref="ArgumentException">Идентификатор пуст.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Ранг отрицателен.</exception>
        public void SetTalentRank(string talentId, int rank)
        {
            if (string.IsNullOrWhiteSpace(talentId))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(talentId)),
                    nameof(talentId));
            }

            if (rank < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rank));
            }

            if (rank == 0)
            {
                _talentRanks.Remove(talentId);
                return;
            }

            _talentRanks[talentId] = rank;
        }

        /// <summary>Добавляет свободные очки талантов.</summary>
        /// <param name="amount">Сколько добавить; не меньше нуля.</param>
        /// <exception cref="ArgumentOutOfRangeException">Количество отрицательно.</exception>
        public void AddTalentPoints(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            TalentPoints += amount;
        }

        /// <summary>Списывает одно свободное очко таланта.</summary>
        /// <returns><c>true</c>, если очко было и списано.</returns>
        public bool TrySpendTalentPoint()
        {
            if (TalentPoints <= 0)
            {
                return false;
            }

            TalentPoints--;
            return true;
        }
    }
}
