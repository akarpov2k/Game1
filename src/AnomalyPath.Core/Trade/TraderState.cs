#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Trade
{
    /// <summary>Изменяемое состояние торговца: запас по товарам, время пополнения и слоты квестов.</summary>
    public sealed class TraderState
    {
        private readonly Dictionary<string, int> _stock = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly List<double> _questSlotAvailableAt;

        /// <summary>Создаёт состояние торговца с пустым запасом.</summary>
        /// <param name="actorId">Идентификатор актёра-торговца.</param>
        /// <param name="questSlotAvailableAt">Время доступности слотов квестов; <c>null</c> — пусто.</param>
        /// <exception cref="ArgumentException">Id пуст.</exception>
        public TraderState(string actorId, IReadOnlyList<double>? questSlotAvailableAt = null)
        {
            if (string.IsNullOrWhiteSpace(actorId))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(actorId)),
                    nameof(actorId));
            }

            ActorId = actorId;
            _questSlotAvailableAt = questSlotAvailableAt == null
                ? new List<double>()
                : new List<double>(questSlotAvailableAt);
        }

        /// <summary>Идентификатор торговца.</summary>
        public string ActorId { get; }

        /// <summary>Абсолютное время мира последнего пополнения в часах; −1 — пополнения ещё не было.</summary>
        public double LastRestockAt { get; private set; } = -1;

        /// <summary>Текущий запас: id предмета → количество.</summary>
        public IReadOnlyDictionary<string, int> Stock => _stock;

        /// <summary>Время доступности слотов квестов в абсолютных часах мира.</summary>
        public IReadOnlyList<double> QuestSlotAvailableAt => _questSlotAvailableAt;

        /// <summary>Дополняет список слотов квестов до нужного числа.</summary>
        /// <param name="count">Сколько слотов должно быть; не меньше нуля.</param>
        /// <exception cref="ArgumentOutOfRangeException">Количество отрицательно.</exception>
        public void EnsureQuestSlots(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            while (_questSlotAvailableAt.Count < count)
            {
                _questSlotAvailableAt.Add(0);
            }
        }

        /// <summary>Возвращает время доступности слота квеста.</summary>
        /// <param name="slot">Номер слота.</param>
        /// <returns>Абсолютное время мира; 0 — слот доступен сразу.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Слот вне списка.</exception>
        public double GetQuestSlotAvailableAt(int slot)
        {
            if (slot < 0 || slot >= _questSlotAvailableAt.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(slot));
            }

            return _questSlotAvailableAt[slot];
        }

        /// <summary>Назначает время доступности слота квеста.</summary>
        /// <param name="slot">Номер слота.</param>
        /// <param name="at">Абсолютное время мира; не меньше нуля.</param>
        /// <exception cref="ArgumentOutOfRangeException">Слот или время вне границ.</exception>
        public void SetQuestSlotAvailableAt(int slot, double at)
        {
            if (slot < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slot));
            }

            if (at < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(at));
            }

            EnsureQuestSlots(slot + 1);
            _questSlotAvailableAt[slot] = at;
        }

        /// <summary>Возвращает количество товара на складе.</summary>
        /// <param name="itemId">Идентификатор предмета.</param>
        /// <returns>Количество; 0, если товара нет.</returns>
        public int Count(string itemId) => _stock.TryGetValue(itemId, out var count) ? count : 0;

        /// <summary>Ставит точное количество товара; 0 удаляет запись.</summary>
        /// <param name="itemId">Идентификатор предмета.</param>
        /// <param name="count">Количество, не меньше нуля.</param>
        /// <exception cref="ArgumentOutOfRangeException">Количество отрицательно.</exception>
        public void SetStock(string itemId, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count == 0)
            {
                _stock.Remove(itemId);
                return;
            }

            _stock[itemId] = count;
        }

        /// <summary>Списывает товар со склада.</summary>
        /// <param name="itemId">Идентификатор предмета.</param>
        /// <param name="count">Сколько списать; не меньше единицы.</param>
        /// <returns><c>true</c>, если товара хватило.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Количество меньше единицы.</exception>
        public bool TryTake(string itemId, int count)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (Count(itemId) < count)
            {
                return false;
            }

            SetStock(itemId, Count(itemId) - count);
            return true;
        }

        /// <summary>Добавляет товар на склад.</summary>
        /// <param name="itemId">Идентификатор предмета.</param>
        /// <param name="count">Сколько добавить; не меньше единицы.</param>
        /// <exception cref="ArgumentOutOfRangeException">Количество меньше единицы.</exception>
        public void Add(string itemId, int count)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            SetStock(itemId, Count(itemId) + count);
        }

        /// <summary>Проверяет, пора ли пополнять запас.</summary>
        /// <param name="nowHours">Текущее абсолютное время мира в часах.</param>
        /// <param name="restockHours">Период пополнения в часах.</param>
        /// <returns><c>true</c>, если пополнение ещё не было или период прошёл.</returns>
        public bool NeedsRestock(double nowHours, int restockHours) =>
            LastRestockAt < 0 || nowHours < LastRestockAt || nowHours - LastRestockAt >= restockHours;

        /// <summary>Отмечает пополнение временем мира.</summary>
        /// <param name="nowHours">Абсолютное время мира в часах.</param>
        public void MarkRestocked(double nowHours) => LastRestockAt = nowHours;
    }
}
