#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Loadouts;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Loot;
using AnomalyPath.Core.Nicks;
using AnomalyPath.Core.Quests;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Rules;
using AnomalyPath.Core.Talents;
using AnomalyPath.Core.Templates;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Validation
{
    /// <summary>Часть класса <see cref="DataValidator"/>: торговцы.</summary>
    public sealed partial class DataValidator
    {
        private static void ValidateTraders(LoadedData data, List<ValidationIssue> issues)
        {
            ActorRepository actors;
            LocationRepository locations;
            ItemRepository items;
            try
            {
                actors = ActorRepository.FromLoadedData(data);
                locations = LocationRepository.FromLoadedData(data);
                items = ItemRepository.FromLoadedData(data);
            }
            catch (DataException)
            {
                return;
            }

            var bound = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var location in locations.All)
            {
                foreach (var traderId in location.Traders)
                {
                    if (location.LocationKind != LocationKind.Hub)
                    {
                        issues.Add(new ValidationIssue(
                            location.Id,
                            CoreErrors.TraderBoundToNonHub,
                            CoreText.Format(CoreErrors.TraderBoundToNonHub, traderId, location.Id)));
                    }

                    if (!actors.Contains(traderId))
                    {
                        issues.Add(new ValidationIssue(
                            location.Id,
                            CoreErrors.ActorNotFound,
                            CoreText.Format(CoreErrors.ActorNotFound, traderId)));
                        continue;
                    }

                    var actor = actors.Get(traderId);
                    if (actor.Type != ActorType.Trader || actor.Trader == null)
                    {
                        issues.Add(new ValidationIssue(
                            location.Id,
                            CoreErrors.TradeNotTrader,
                            CoreText.Format(CoreErrors.TradeNotTrader, traderId)));
                    }

                    if (actor.LocationId != null &&
                        !string.Equals(actor.LocationId, location.Id, StringComparison.Ordinal))
                    {
                        issues.Add(new ValidationIssue(
                            actor.Id,
                            CoreErrors.TraderLocationMismatch,
                            CoreText.Format(CoreErrors.TraderLocationMismatch, actor.Id, location.Id)));
                    }

                    if (bound.TryGetValue(traderId, out var first))
                    {
                        issues.Add(new ValidationIssue(
                            location.Id,
                            CoreErrors.TraderDuplicateBinding,
                            CoreText.Format(CoreErrors.TraderDuplicateBinding, traderId, first)));
                    }
                    else
                    {
                        bound[traderId] = location.Id;
                    }
                }
            }

            foreach (var actor in actors.All)
            {
                if (actor.Type != ActorType.Trader || actor.Trader == null)
                {
                    continue;
                }

                if (!bound.ContainsKey(actor.Id))
                {
                    issues.Add(new ValidationIssue(
                        actor.Id,
                        CoreErrors.TraderNotBound,
                        CoreText.Format(CoreErrors.TraderNotBound, actor.Id)));
                }

                foreach (var offer in actor.Trader.Assortment)
                {
                    if (!items.TryGet(offer.ItemId, out var template))
                    {
                        issues.Add(new ValidationIssue(
                            actor.Id,
                            CoreErrors.TraderOfferItemMissing,
                            CoreText.Format(CoreErrors.TraderOfferItemMissing, offer.ItemId)));
                        continue;
                    }

                    if (template.Price == null ||
                        (!template.Price.Buy.HasValue && !template.Price.Base.HasValue))
                    {
                        issues.Add(new ValidationIssue(
                            actor.Id,
                            CoreErrors.TraderOfferPriceMissing,
                            CoreText.Format(CoreErrors.TraderOfferPriceMissing, offer.ItemId)));
                    }
                }
            }
        }
    }
}
