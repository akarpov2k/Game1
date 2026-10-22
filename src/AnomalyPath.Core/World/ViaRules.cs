#nullable enable
using System.Collections.Generic;

namespace AnomalyPath.Core.World
{
    /// <summary>
    /// Таблица допустимых тегов и ролей для каждого способа перехода.
    /// Синхронизирована со схемами (раздел 14.1.1) и не зависит от топологии.
    /// </summary>
    public static class ViaRules
    {
        /// <summary>Проверяет, известен ли способ перехода.</summary>
        /// <param name="via">Способ перехода.</param>
        /// <returns><c>true</c>, если способ поддерживается.</returns>
        public static bool IsKnown(string via) =>
            via is "road" or "path" or "door" or "gate" or "stairs" or "hatch" or "corridor" or "tunnel";

        /// <summary>Проверяет роли и теги обеих сторон перехода.</summary>
        /// <param name="via">Способ перехода.</param>
        /// <param name="fromRole">Роль исходной локации.</param>
        /// <param name="toRole">Роль целевой локации.</param>
        /// <param name="fromTags">Теги исходной локации.</param>
        /// <param name="toTags">Теги целевой локации.</param>
        /// <returns><c>true</c>, если переход допустим.</returns>
        public static bool Allows(
            string via,
            string fromRole,
            string toRole,
            IReadOnlyCollection<string> fromTags,
            IReadOnlyCollection<string> toTags)
        {
            return via switch
            {
                "road" => HasAll(fromTags, "road", "outdoor") && HasAll(toTags, "road", "outdoor") &&
                          Role(fromRole, "hub", "route") && Role(toRole, "route", "site", "hub"),
                "path" => HasAll(fromTags, "outdoor") && HasAll(toTags, "outdoor") &&
                          Role(fromRole, "hub", "route", "site") && Role(toRole, "route", "site", "hub"),
                "door" => HasAll(toTags, "indoor") && HasAny(fromTags, "indoor", "outdoor") &&
                          Role(fromRole, "site", "interior") && Role(toRole, "interior"),
                "gate" => HasAll(fromTags, "outdoor") && HasAll(toTags, "outdoor") &&
                          Role(fromRole, "site") && Role(toRole, "site"),
                "stairs" => HasAll(fromTags, "indoor") && HasAll(toTags, "indoor") &&
                            Role(fromRole, "site", "interior") && Role(toRole, "interior"),
                "hatch" => HasAny(toTags, "indoor", "underground") && HasAny(fromTags, "indoor", "underground") &&
                           Role(fromRole, "site", "interior") && Role(toRole, "interior"),
                "corridor" => HasAll(fromTags, "indoor") && HasAll(toTags, "indoor") &&
                              Role(fromRole, "interior") && Role(toRole, "interior"),
                "tunnel" => HasAll(fromTags, "underground") && HasAll(toTags, "underground") &&
                            Role(fromRole, "route") && Role(toRole, "route"),
                _ => false
            };
        }

        private static bool Role(string role, params string[] allowed)
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

        private static bool HasAll(IReadOnlyCollection<string> tags, params string[] required)
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

        private static bool HasAny(IReadOnlyCollection<string> tags, params string[] candidates)
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

        private static bool Contains(IReadOnlyCollection<string> tags, string tag)
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
