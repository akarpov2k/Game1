#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Inventory;
using EquipmentState = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Core.Combat
{
    /// <summary>Участник боя: здоровье, ОД и экипировка.</summary>
    public sealed class Combatant
    {
        /// <summary>Создаёт участника боя из шаблона.</summary>
        /// <param name="template">Актёр.</param>
        /// <param name="equipment">Экипировка игрока, если есть.</param>
        /// <exception cref="System.ArgumentNullException">Шаблон не передан.</exception>
        public Combatant(ActorTemplate template, EquipmentState? equipment = null)
        {
            Template = template ?? throw new System.ArgumentNullException(nameof(template));
            Equipment = equipment;
            MaxHealth = template.Combat?.Health ?? 0;
            Health = MaxHealth;
            MaxAp = template.Combat?.Ap ?? 0;
            Ap = MaxAp;
        }

        /// <summary>Актёр.</summary>
        public ActorTemplate Template { get; }

        /// <summary>Экипировка; у врагов обычно отсутствует.</summary>
        public EquipmentState? Equipment { get; }

        /// <summary>Текущее здоровье.</summary>
        public double Health { get; internal set; }

        /// <summary>Максимальное здоровье.</summary>
        public int MaxHealth { get; }

        /// <summary>Текущие очки действия.</summary>
        public int Ap { get; internal set; }

        /// <summary>Максимум ОД.</summary>
        public int MaxAp { get; }

        /// <summary>Уровень актёра.</summary>
        public int Level => Template.Combat?.Level ?? 1;

        /// <summary>Накопленный бонус «пожалей»: 5% базового шанса за каждый промах.</summary>
        public double PityBonus { get; internal set; }

        /// <summary><c>true</c>, если здоровье на нуле.</summary>
        public bool IsDead => Health <= 0;

        /// <summary>Собственная защита актёра по типу урона.</summary>
        /// <param name="damageType">Тип урона.</param>
        /// <returns>Значение защиты; 0, если нет.</returns>
        public int OwnProtection(string damageType)
        {
            var protection = Template.Combat?.Protection;
            if (protection == null)
            {
                return 0;
            }

            return protection.TryGetValue(damageType, out var value) ? value : 0;
        }
    }
}
