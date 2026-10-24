#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Trade
{
    /// <summary>Блок торговли актёра: ассортимент, период пополнения и множители цен.</summary>
    public sealed class TraderBlockTemplate
    {
        /// <summary>Создаёт блок торговли.</summary>
        /// <param name="restockHours">Период пополнения в часах мира, не меньше часа.</param>
        /// <param name="playerBuyModifier">Множитель цены покупки игроком.</param>
        /// <param name="playerSellModifier">Множитель цены продажи игроком.</param>
        /// <param name="requirements">Условия доступа к торговцу.</param>
        /// <param name="assortment">Товары, которые торговец продаёт.</param>
        public TraderBlockTemplate(
            int restockHours,
            double playerBuyModifier,
            double playerSellModifier,
            RequirementsTemplate requirements,
            IReadOnlyList<TraderOfferTemplate> assortment)
        {
            RestockHours = restockHours;
            PlayerBuyModifier = playerBuyModifier;
            PlayerSellModifier = playerSellModifier;
            Requirements = requirements;
            Assortment = assortment;
        }

        /// <summary>Период пополнения в часах мира.</summary>
        public int RestockHours { get; }

        /// <summary>Множитель цены, по которой покупает игрок.</summary>
        public double PlayerBuyModifier { get; }

        /// <summary>Множитель цены, по которой продаёт игрок.</summary>
        public double PlayerSellModifier { get; }

        /// <summary>Условия доступа к торговцу.</summary>
        public RequirementsTemplate Requirements { get; }

        /// <summary>Ассортимент торговца.</summary>
        public IReadOnlyList<TraderOfferTemplate> Assortment { get; }

        /// <summary>Читает блок торговли.</summary>
        /// <param name="value">Объект актёра.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Блок торговли.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static TraderBlockTemplate FromJson(JsonValue value, string sourcePath)
        {
            if (!value.TryGet("trader", out var block) || !block.IsObject)
            {
                throw new DataException(sourcePath, CoreErrors.TraderWithoutBlock);
            }

            var restockHours = DataMap.RequireInt(block, "restockHours", sourcePath);
            if (restockHours < 1)
            {
                throw new DataException(sourcePath, CoreErrors.TraderRestockHours, restockHours);
            }

            var buyModifier = DataMap.RequireDouble(block, "playerBuyModifier", sourcePath);
            var sellModifier = DataMap.RequireDouble(block, "playerSellModifier", sourcePath);
            if (buyModifier <= 0 || buyModifier > 10 || sellModifier <= 0 || sellModifier > 10)
            {
                throw new DataException(sourcePath, CoreErrors.TraderModifierRange);
            }

            var requirements = RequirementsTemplate.Read(block, sourcePath);
            var assortment = ReadAssortment(block, sourcePath);
            return new TraderBlockTemplate(restockHours, buyModifier, sellModifier, requirements, assortment);
        }

        private static IReadOnlyList<TraderOfferTemplate> ReadAssortment(JsonValue block, string sourcePath)
        {
            if (!block.TryGet("assortment", out var assortment) || !assortment.IsArray)
            {
                throw new DataException(sourcePath, CoreErrors.MissingArrayField, "assortment");
            }

            if (assortment.Items.Count == 0)
            {
                throw new DataException(sourcePath, CoreErrors.TraderAssortmentEmpty);
            }

            var result = new List<TraderOfferTemplate>();
            foreach (var item in assortment.Items)
            {
                if (!item.IsObject)
                {
                    throw new DataException(sourcePath, CoreErrors.JsonNotObject);
                }

                var itemId = DataMap.RequireString(item, "itemId", sourcePath);
                var stock = DataMap.RequireInt(item, "stock", sourcePath);
                if (stock < 1)
                {
                    throw new DataException(sourcePath, CoreErrors.TraderOfferStock, itemId);
                }

                result.Add(new TraderOfferTemplate(itemId, stock, RequirementsTemplate.Read(item, sourcePath)));
            }

            return result;
        }
    }
}
