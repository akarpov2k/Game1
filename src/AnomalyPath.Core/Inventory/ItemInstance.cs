#nullable enable
using System;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Templates;

namespace AnomalyPath.Core.Inventory
{
    /// <summary>
    /// Экземпляр предмета в контейнере: ссылка на шаблон плюс изменяемое состояние.
    /// Данные шаблона не копируются — они живут в <see cref="ItemTemplate"/>.
    /// </summary>
    public sealed class ItemInstance
    {
        /// <summary>Создаёт экземпляр предмета.</summary>
        /// <param name="instanceId">Уникальный id экземпляра.</param>
        /// <param name="itemId">Id шаблона предмета.</param>
        /// <param name="stack">Количество в стопке.</param>
        /// <param name="durability">Текущая прочность, если предмет изнашивается.</param>
        /// <param name="durabilityMax">Максимум прочности на момент создания.</param>
        /// <param name="variantKey">Вариант предмета (редкость, скиллы).</param>
        /// <param name="magazine">Патроны в магазине; <c>null</c> у не-оружия.</param>
        public ItemInstance(
            string instanceId,
            string itemId,
            int stack,
            int? durability = null,
            int? durabilityMax = null,
            string? variantKey = null,
            int? magazine = null)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(instanceId)),
                    nameof(instanceId));
            }

            if (stack < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(stack),
                    CoreText.Format(CoreErrors.MinimumOne, nameof(stack)));
            }

            InstanceId = instanceId;
            ItemId = itemId;
            Stack = stack;
            Durability = durability;
            DurabilityMax = durabilityMax;
            VariantKey = variantKey;
            Magazine = magazine;
        }

        /// <summary>Уникальный id экземпляра.</summary>
        public string InstanceId { get; }

        /// <summary>Id шаблона предмета.</summary>
        public string ItemId { get; }

        /// <summary>Количество в стопке.</summary>
        public int Stack { get; internal set; }

        /// <summary>Текущая прочность; <c>null</c>, если предмет не изнашивается.</summary>
        public int? Durability { get; internal set; }

        /// <summary>Максимум прочности, зафиксированный при создании.</summary>
        public int? DurabilityMax { get; }

        /// <summary><c>true</c>, если прочность на нуле: предмет сломан и не участвует в расчётах.</summary>
        public bool IsBroken => Durability.HasValue && Durability.Value <= 0;

        /// <summary>Патроны в магазине; <c>null</c> — магазин не инициализирован и считается пустым.</summary>
        public int? Magazine { get; internal set; }

        /// <summary>Патронов в магазине; неизвестный магазин считается пустым.</summary>
        public int MagazineRounds => Magazine ?? 0;

        /// <summary>Ключ варианта: редкость и раскатанные характеристики.</summary>
        public string? VariantKey { get; }

        /// <summary><c>true</c>, если предмет не поместился и лежит в переполнении.</summary>
        public bool Overflow { get; internal set; }

        /// <summary>Позиция по горизонтали; −1 вне сетки.</summary>
        public int X { get; internal set; } = -1;

        /// <summary>Позиция по вертикали; −1 вне сетки.</summary>
        public int Y { get; internal set; } = -1;

        /// <summary>Ширина занятой области с учётом поворота; 0 в переполнении.</summary>
        public int PlacedWidth { get; internal set; }

        /// <summary>Высота занятой области с учётом поворота; 0 в переполнении.</summary>
        public int PlacedHeight { get; internal set; }

        /// <summary>Повёрнут ли предмет на 90°.</summary>
        public bool Rotated { get; internal set; }

        /// <summary>Ключ стопки: одинаковые ключи и разная прочность не сливаются.</summary>
        public string StackKey =>
            $"{ItemId}|{(Durability.HasValue ? Durability.Value.ToString() : "-")}|{VariantKey ?? "-"}";
    }
}
