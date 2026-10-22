#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Inventory
{
    /// <summary>Экипировка персонажа: фиксированные слоты и слоты быстрого доступа.</summary>
    public sealed class Equipment
    {
        private readonly Dictionary<EquipmentSlot, ItemInstance> _slots =
            new Dictionary<EquipmentSlot, ItemInstance>();

        private readonly ItemInstance?[] _quick;
        private readonly List<ItemInstance?> _artifacts = new List<ItemInstance?>();

        /// <summary>Создаёт экипировку с заданным числом быстрых слотов.</summary>
        /// <param name="ownerId">Id владельца (игрок или актёр).</param>
        /// <param name="quickSlots">Число слотов быстрого доступа.</param>
        public Equipment(string ownerId, int quickSlots)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(ownerId)),
                    nameof(ownerId));
            }

            if (quickSlots < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(quickSlots),
                    CoreText.Format(CoreErrors.NonNegative, nameof(quickSlots)));
            }

            OwnerId = ownerId;
            _quick = new ItemInstance?[quickSlots];
        }

        /// <summary>Id владельца.</summary>
        public string OwnerId { get; }

        /// <summary>Занятые фиксированные слоты.</summary>
        public IReadOnlyDictionary<EquipmentSlot, ItemInstance> Slots => _slots;

        /// <summary>Слоты быстрого доступа; <c>null</c> — пусто.</summary>
        public IReadOnlyList<ItemInstance?> QuickSlots => _quick;

        /// <summary>Надетые артефакты: занимают слоты пояса, а не инвентаря.</summary>
        public IReadOnlyList<ItemInstance?> Artifacts => _artifacts;

        /// <summary>Число слотов быстрого доступа.</summary>
        public int QuickSlotCount => _quick.Length;

        /// <summary>Возвращает предмет в слоте.</summary>
        /// <param name="slot">Слот.</param>
        /// <returns>Экземпляр или <c>null</c>.</returns>
        public ItemInstance? Get(EquipmentSlot slot) =>
            _slots.TryGetValue(slot, out var item) ? item : null;

        /// <summary>Возвращает предмет в быстром слоте.</summary>
        /// <param name="index">Индекс слота.</param>
        /// <returns>Экземпляр или <c>null</c>.</returns>
        public ItemInstance? GetQuick(int index) =>
            index >= 0 && index < _quick.Length ? _quick[index] : null;

        /// <summary>Ставит предмет в слот; <c>null</c> очищает его.</summary>
        /// <param name="slot">Слот.</param>
        /// <param name="item">Экземпляр или <c>null</c>.</param>
        public void Set(EquipmentSlot slot, ItemInstance? item)
        {
            if (item == null)
            {
                _slots.Remove(slot);
                return;
            }

            _slots[slot] = item;
        }

        /// <summary>Гарантирует наличие слотов артефактов.</summary>
        /// <param name="count">Нужное число слотов.</param>
        public void EnsureArtifactSlots(int count)
        {
            while (_artifacts.Count < count)
            {
                _artifacts.Add(null);
            }
        }

        /// <summary>Ставит артефакт в слот пояса.</summary>
        /// <param name="index">Индекс слота.</param>
        /// <param name="item">Экземпляр артефакта или <c>null</c>.</param>
        public void SetArtifact(int index, ItemInstance? item)
        {
            EnsureArtifactSlots(index + 1);
            _artifacts[index] = item;
        }

        /// <summary>Ставит предмет в быстрый слот; <c>null</c> очищает его.</summary>
        /// <param name="index">Индекс слота.</param>
        /// <param name="item">Экземпляр или <c>null</c>.</param>
        public void SetQuick(int index, ItemInstance? item)
        {
            if (index < 0 || index >= _quick.Length)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    CoreText.Format(CoreErrors.IndexOutOfRange, nameof(index)));
            }

            _quick[index] = item;
        }
    }
}
