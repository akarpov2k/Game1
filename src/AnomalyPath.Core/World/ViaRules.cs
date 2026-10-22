#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Data;

namespace AnomalyPath.Core.World
{
    /// <summary>
    /// Таблица допустимых тегов и ролей для каждого способа перехода.
    /// Синхронизирована со схемами (раздел 14.1.1) и не зависит от топологии.
    /// </summary>
    public static class ViaRules
    {
        /// <summary>Проверяет, известен ли способ перехода.</summary>
        /// <param name="via">Код способа перехода.</param>
        /// <returns><c>true</c>, если способ поддерживается.</returns>
        public static bool IsKnown(string via) => SchemaEnum.TryParse(via, out TravelVia _);

        /// <summary>Проверяет роли и теги обеих сторон перехода.</summary>
        /// <param name="via">Код способа перехода.</param>
        /// <param name="fromRole">Роль исходной локации.</param>
        /// <param name="toRole">Роль целевой локации.</param>
        /// <param name="fromTags">Теги исходной локации.</param>
        /// <param name="toTags">Теги целевой локации.</param>
        /// <returns><c>true</c>, если переход допустим.</returns>
        public static bool Allows(
            string via,
            LocationNodeType fromRole,
            LocationNodeType toRole,
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
                                  Role(fromRole, LocationNodeType.Hub, LocationNodeType.Route) &&
                                  Role(toRole, LocationNodeType.Route, LocationNodeType.Site, LocationNodeType.Hub),
                TravelVia.Path => HasAll(fromTags, LocationTag.Outdoor) && HasAll(toTags, LocationTag.Outdoor) &&
                                  Role(fromRole, LocationNodeType.Hub, LocationNodeType.Route, LocationNodeType.Site) &&
                                  Role(toRole, LocationNodeType.Route, LocationNodeType.Site, LocationNodeType.Hub),
                TravelVia.Door => HasAll(toTags, LocationTag.Indoor) &&
                                  HasAny(fromTags, LocationTag.Indoor, LocationTag.Outdoor) &&
                                  Role(fromRole, LocationNodeType.Site, LocationNodeType.Interior) &&
                                  Role(toRole, LocationNodeType.Interior),
                TravelVia.Gate => HasAll(fromTags, LocationTag.Outdoor) && HasAll(toTags, LocationTag.Outdoor) &&
                                  Role(fromRole, LocationNodeType.Site) && Role(toRole, LocationNodeType.Site),
                TravelVia.Stairs => HasAll(fromTags, LocationTag.Indoor) && HasAll(toTags, LocationTag.Indoor) &&
                                    Role(fromRole, LocationNodeType.Site, LocationNodeType.Interior) &&
                                    Role(toRole, LocationNodeType.Interior),
                TravelVia.Hatch => HasAny(toTags, LocationTag.Indoor, LocationTag.Underground) &&
                                   HasAny(fromTags, LocationTag.Indoor, LocationTag.Underground) &&
                                   Role(fromRole, LocationNodeType.Site, LocationNodeType.Interior) &&
                                   Role(toRole, LocationNodeType.Interior),
                TravelVia.Corridor => HasAll(fromTags, LocationTag.Indoor) && HasAll(toTags, LocationTag.Indoor) &&
                                      Role(fromRole, LocationNodeType.Interior) &&
                                      Role(toRole, LocationNodeType.Interior),
                TravelVia.Tunnel => HasAll(fromTags, LocationTag.Underground) &&
                                    HasAll(toTags, LocationTag.Underground) &&
                                    Role(fromRole, LocationNodeType.Route) && Role(toRole, LocationNodeType.Route),
                _ => false
            };
        }

        private static bool Role(LocationNodeType role, params LocationNodeType[] allowed)
        {
            foreach (var item in allowed)
            {
                if (role == item)
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

        private static bool HasAny(IReadOnlyCollection<LocationTag> tags, params LocationTag[] candidates)
        {
            foreach (var tag in candidates)
            {
                if (Contains(tags, tag))
                {
                    return true;
                }
            }

            return false;
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
