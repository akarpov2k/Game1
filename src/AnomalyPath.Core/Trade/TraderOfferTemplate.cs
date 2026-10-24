#nullable enable
namespace AnomalyPath.Core.Trade
{
    /// <summary>Товар в ассортименте торговца: что продаётся, сколько и при каких условиях.</summary>
    public sealed class TraderOfferTemplate
    {
        /// <summary>Создаёт товар ассортимента.</summary>
        /// <param name="itemId">Идентификатор предмета.</param>
        /// <param name="stock">Сколько штук появляется при пополнении, не меньше единицы.</param>
        /// <param name="requirements">Условия, при которых товар виден в продаже.</param>
        public TraderOfferTemplate(string itemId, int stock, RequirementsTemplate requirements)
        {
            ItemId = itemId;
            Stock = stock;
            Requirements = requirements;
        }

        /// <summary>Идентификатор предмета.</summary>
        public string ItemId { get; }

        /// <summary>Максимальный запас при пополнении.</summary>
        public int Stock { get; }

        /// <summary>Условия показа товара.</summary>
        public RequirementsTemplate Requirements { get; }
    }
}
