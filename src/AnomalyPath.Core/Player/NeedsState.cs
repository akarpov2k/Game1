#nullable enable
using System;

namespace AnomalyPath.Core.Player
{
    /// <summary>Потребности персонажа: еда, вода и сон.</summary>
    public sealed class NeedsState
    {
        /// <summary>Создаёт состояние потребностей.</summary>
        /// <param name="hunger">Сытость.</param>
        /// <param name="thirst">Питьё.</param>
        /// <param name="sleep">Сон.</param>
        /// <param name="max">Максимум шкал.</param>
        /// <exception cref="ArgumentOutOfRangeException">Значения вне 0..max или max меньше 1.</exception>
        public NeedsState(int hunger, int thirst, int sleep, int max)
        {
            if (max < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(max));
            }

            if (hunger < 0 || hunger > max || thirst < 0 || thirst > max || sleep < 0 || sleep > max)
            {
                throw new ArgumentOutOfRangeException(nameof(hunger));
            }

            Hunger = hunger;
            Thirst = thirst;
            Sleep = sleep;
            Max = max;
        }

        /// <summary>Сытость.</summary>
        public int Hunger { get; private set; }

        /// <summary>Питьё.</summary>
        public int Thirst { get; private set; }

        /// <summary>Сон.</summary>
        public int Sleep { get; private set; }

        /// <summary>Максимум шкал.</summary>
        public int Max { get; }

        /// <summary>Меняет сытость в пределах шкалы.</summary>
        /// <param name="value">Новое значение.</param>
        public void SetHunger(int value) => Hunger = Clamp(value);

        /// <summary>Меняет питьё в пределах шкалы.</summary>
        /// <param name="value">Новое значение.</param>
        public void SetThirst(int value) => Thirst = Clamp(value);

        /// <summary>Меняет сон в пределах шкалы.</summary>
        /// <param name="value">Новое значение.</param>
        public void SetSleep(int value) => Sleep = Clamp(value);

        private int Clamp(int value) => value < 0 ? 0 : value > Max ? Max : value;
    }
}
