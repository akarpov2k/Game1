#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Inventory;
using InventoryEquipment = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Core.Player
{
    /// <summary>Состояние игрока: здоровье, опыт, инвентарь, экипировка и потребности.</summary>
    public sealed class PlayerState
    {
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
            int money = 0)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("id игрока не задан", nameof(id));
            }

            if (level < 1 || xp < 0 || health < 0 || apMax < 0 || money < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            Id = id;
            Name = string.IsNullOrWhiteSpace(name) ? id : name.Trim();
            Level = level;
            Xp = xp;
            Health = health;
            ApMax = apMax;
            Money = money;
            Inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
            Skills = skills ?? throw new ArgumentNullException(nameof(skills));
            Needs = needs ?? throw new ArgumentNullException(nameof(needs));
        }

        /// <summary>Идентификатор игрока.</summary>
        public string Id { get; }

        /// <summary>Ник персонажа.</summary>
        public string Name { get; }

        /// <summary>Уровень.</summary>
        public int Level { get; }

        /// <summary>Опыт.</summary>
        public int Xp { get; }

        /// <summary>Текущее здоровье.</summary>
        public int Health { get; }

        /// <summary>Максимум очков действия.</summary>
        public int ApMax { get; }

        /// <summary>Деньги на персонаже.</summary>
        public int Money { get; }

        /// <summary>Сетка инвентаря.</summary>
        public GridContainer Inventory { get; }

        /// <summary>Экипировка.</summary>
        public InventoryEquipment Equipment { get; }

        /// <summary>Выученные скиллы.</summary>
        public IReadOnlyList<string> Skills { get; }

        /// <summary>Потребности.</summary>
        public NeedsState Needs { get; }
    }
}
