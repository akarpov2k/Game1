#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Runtime.Ui.Core;

namespace AnomalyPath.Runtime.Ui.Art
{
    /// <summary>Проверяет, что для каждого id иконки в каталоге спрайтов есть спрайт.</summary>
    public static class SpriteCatalogValidator
    {
        /// <summary>
        /// Возвращает id, для которых в каталоге нет спрайта, без повторов и в порядке первого появления.
        /// </summary>
        /// <param name="iconIds">Id иконок из данных: <c>icons/wpn_ak74</c>.</param>
        /// <param name="catalog">Каталог спрайтов.</param>
        /// <returns>Список отсутствующих id; пустой, если все иконки на месте.</returns>
        /// <exception cref="ArgumentNullException">Список id или каталог не заданы.</exception>
        public static IReadOnlyList<string> FindMissing(IEnumerable<string> iconIds, ISpriteCatalog catalog)
        {
            if (iconIds == null)
            {
                throw new ArgumentNullException(nameof(iconIds));
            }

            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            var missing = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in iconIds)
            {
                if (string.IsNullOrEmpty(id) || !seen.Add(id))
                {
                    continue;
                }

                if (!catalog.TryGet(id, out var sprite) || sprite == null)
                {
                    missing.Add(id);
                }
            }

            return missing;
        }
    }
}
