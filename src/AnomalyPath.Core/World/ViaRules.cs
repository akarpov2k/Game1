#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Data;

namespace AnomalyPath.Core.World
{
    /// <summary>
    /// Таблица допустимых тегов и подтипов для каждого способа перехода.
    /// Синхронизирована со схемами (раздел 14.1.1) и не зависит от топологии.
    /// </summary>
    public static class ViaRules
    {
        /// <summary>Проверяет, известен ли способ перехода.</summary>
        /// <param name="via">Код способа перехода.</param>
        /// <returns><c>true</c>, если способ поддерживается.</returns>
        public static bool IsKnown(string via) => SchemaEnum.TryParse(via, out TravelVia _);

        /// <summary>Проверяет подтипы и теги обеих сторон перехода.</summary>
        /// <param name="via">Код способа перехода.</param>
        /// <param name="from">Подтип исходной локации.</param>
        /// <param name="to">Подтип целевой локации.</param>
        /// <param name="fromTags">Теги исходной локации.</param>
        /// <param name="toTags">Теги целевой локации.</param>
        /// <returns><c>true</c>, если переход допустим.</returns>
        public static bool Allows(
            string via,
            LocationSubtype from,
            LocationSubtype to,
            IReadOnlyCollection<LocationTag> fromTags,
            IReadOnlyCollection<LocationTag> toTags)
        {
            if (!SchemaEnum.TryParse(via, out TravelVia way))
            {
                return false;
            }

            return way switch
            {
                TravelVia.Road => HasAll(fromTags, LocationTag.Road, LocationTag.Outdoor) &&
                                  HasAll(toTags, LocationTag.Road, LocationTag.Outdoor) &&
                                  Subtype(from, LocationSubtype.Hub, LocationSubtype.Path) &&
                                  Subtype(to, LocationSubtype.Hub, LocationSubtype.Path),
                TravelVia.Path => HasAll(fromTags, LocationTag.Outdoor) && HasAll(toTags, LocationTag.Outdoor) &&
                                  Subtype(
                                      from,
                                      LocationSubtype.Hub,
                                      LocationSubtype.Path,
                                      LocationSubtype.Building,
                                      LocationSubtype.Terrain) &&
                                  Subtype(
                                      to,
                                      LocationSubtype.Hub,
                                      LocationSubtype.Path,
                                      LocationSubtype.Building,
                                      LocationSubtype.Terrain),
                TravelVia.Gate => HasAll(fromTags, LocationTag.Outdoor) && HasAll(toTags, LocationTag.Outdoor) &&
                                  Subtype(from, LocationSubtype.Building, LocationSubtype.Terrain) &&
                                  Subtype(to, LocationSubtype.Building, LocationSubtype.Terrain),
                TravelVia.Corridor => HasAll(fromTags, LocationTag.Indoor) && HasAll(toTags, LocationTag.Indoor) &&
                                      Subtype(from, LocationSubtype.Building) &&
                                      Subtype(to, LocationSubtype.Building),
                TravelVia.Tunnel => HasAll(fromTags, LocationTag.Underground) &&
                                    HasAll(toTags, LocationTag.Underground) &&
                                    Subtype(from, LocationSubtype.Path) && Subtype(to, LocationSubtype.Path),
                _ => false
            };
        }

        private static bool Subtype(LocationSubtype subtype, params LocationSubtype[] allowed)
        {
            foreach (var item in allowed)
            {
                if (subtype == item)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasAll(IReadOnlyCollection<LocationTag> tags, params LocationTag[] required)
        {
            foreach (var tag in required)
            {
                if (!Contains(tags, tag))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool Contains(IReadOnlyCollection<LocationTag> tags, LocationTag tag)
        {
            foreach (var item in tags)
            {
                if (item == tag)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
