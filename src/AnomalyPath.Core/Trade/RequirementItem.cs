#nullable enable
namespace AnomalyPath.Core.Trade
{
    /// <summary>Сколько предметов нужно иметь для выполнения условия.</summary>
    public sealed class RequirementItem
    {
        /// <summary>Создаёт условие по предмету.</summary>
        /// <param name="itemId">Идентификатор предмета.</param>
        /// <param name="count">Требуемое количество, не меньше единицы.</param>
        public RequirementItem(string itemId, int count)
        {
            ItemId = itemId;
            Count = count;
        }

        /// <summary>Идентификатор предмета.</summary>
        public string ItemId { get; }

        /// <summary>Требуемое количество.</summary>
        public int Count { get; }
    }
}
