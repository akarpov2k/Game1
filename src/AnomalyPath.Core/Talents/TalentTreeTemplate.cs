#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Talents
{
    /// <summary>Дерево талантов: ветки развития, таланты и пороги открытия по вложенным очкам.</summary>
    public sealed class TalentTreeTemplate
    {
        private readonly List<TalentNodeTemplate> _nodes;
        private readonly Dictionary<string, TalentNodeTemplate> _byId;

        /// <summary>Создаёт дерево талантов.</summary>
        /// <param name="id">Идентификатор дерева.</param>
        /// <param name="titleKey">Ключ названия дерева.</param>
        /// <param name="descriptionKey">Ключ описания дерева.</param>
        /// <param name="nodes">Таланты дерева; хотя бы один.</param>
        /// <exception cref="ArgumentException">Строковые поля пусты или талантов нет.</exception>
        public TalentTreeTemplate(
            string id,
            string titleKey,
            string descriptionKey,
            IReadOnlyList<TalentNodeTemplate> nodes)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(id)),
                    nameof(id));
            }

            if (string.IsNullOrWhiteSpace(titleKey))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(titleKey)),
                    nameof(titleKey));
            }

            if (string.IsNullOrWhiteSpace(descriptionKey))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(descriptionKey)),
                    nameof(descriptionKey));
            }

            if (nodes == null || nodes.Count == 0)
            {
                throw new ArgumentException(CoreText.Get(CoreErrors.TalentTreeEmpty), nameof(nodes));
            }

            Id = id;
            TitleKey = titleKey;
            DescriptionKey = descriptionKey;
            _nodes = new List<TalentNodeTemplate>(nodes);
            _nodes.Sort(CompareNodes);
            _byId = new Dictionary<string, TalentNodeTemplate>(StringComparer.Ordinal);
            foreach (var node in _nodes)
            {
                if (_byId.ContainsKey(node.Id))
                {
                    throw new ArgumentException(
                        CoreText.Format(CoreErrors.TalentNodeDuplicate, node.Id),
                        nameof(nodes));
                }

                _byId[node.Id] = node;
            }
        }

        /// <summary>Идентификатор дерева.</summary>
        public string Id { get; }

        /// <summary>Ключ названия дерева.</summary>
        public string TitleKey { get; }

        /// <summary>Ключ описания дерева.</summary>
        public string DescriptionKey { get; }

        /// <summary>Таланты, отсортированные по строке и колонке.</summary>
        public IReadOnlyList<TalentNodeTemplate> Nodes => _nodes;

        /// <summary>Ищет талант по идентификатору.</summary>
        /// <param name="id">Идентификатор таланта.</param>
        /// <param name="node">Найденный талант или <c>null</c>.</param>
        /// <returns><c>true</c>, если талант есть в дереве.</returns>
        public bool TryGetNode(string id, out TalentNodeTemplate? node)
        {
            if (_byId.TryGetValue(id, out var found))
            {
                node = found;
                return true;
            }

            node = null;
            return false;
        }

        /// <summary>Возвращает талант по идентификатору.</summary>
        /// <param name="id">Идентификатор таланта.</param>
        /// <returns>Талант.</returns>
        /// <exception cref="DataException">Талант не найден.</exception>
        public TalentNodeTemplate GetNode(string id) =>
            _byId.TryGetValue(id, out var node)
                ? node
                : throw new DataException(id, CoreErrors.TalentNotFound, id);

        /// <summary>Сколько очков можно вложить в дерево до указанного таланта.</summary>
        /// <param name="node">Талант.</param>
        /// <returns>Сумма предельных рангов талантов выше по дереву.</returns>
        /// <exception cref="ArgumentNullException">Талант не задан.</exception>
        public int PointsAvailableBefore(TalentNodeTemplate node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var total = 0;
            foreach (var item in _nodes)
            {
                if (item.Row < node.Row)
                {
                    total += item.MaxRank;
                }
            }

            return total;
        }

        /// <summary>Читает дерево талантов из записи вида <c>talents</c>.</summary>
        /// <param name="value">JSON-объект дерева.</param>
        /// <param name="path">Путь к файлу данных.</param>
        /// <returns>Дерево талантов.</returns>
        /// <exception cref="DataException">Поля отсутствуют, повторяются или заданы неверно.</exception>
        public static TalentTreeTemplate FromJson(JsonValue value, string path)
        {
            var id = DataMap.RequireString(value, "id", path);
            var titleKey = DataMap.RequireString(value, "titleKey", path);
            var descriptionKey = DataMap.RequireString(value, "descriptionKey", path);
            if (!value.TryGet("nodes", out var nodesValue) || !nodesValue.IsArray || nodesValue.Items.Count == 0)
            {
                throw new DataException(path, CoreErrors.TalentTreeEmpty);
            }

            var nodes = new List<TalentNodeTemplate>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var positions = new Dictionary<string, TalentNodeTemplate>(StringComparer.Ordinal);
            foreach (var item in nodesValue.Items)
            {
                var node = TalentNodeTemplate.FromJson(item, path);
                if (!ids.Add(node.Id))
                {
                    throw new DataException(path, CoreErrors.TalentNodeDuplicate, node.Id);
                }

                var position = node.Row + ":" + node.Column;
                if (positions.ContainsKey(position))
                {
                    throw new DataException(path, CoreErrors.TalentPositionDuplicate, node.Id);
                }

                positions[position] = node;
                nodes.Add(node);
            }

            var tree = new TalentTreeTemplate(id, titleKey, descriptionKey, nodes);
            foreach (var node in tree.Nodes)
            {
                var available = tree.PointsAvailableBefore(node);
                if (node.RequiredPoints > available)
                {
                    throw new DataException(
                        path,
                        CoreErrors.TalentGate,
                        node.Id,
                        node.RequiredPoints,
                        available);
                }
            }

            return tree;
        }

        private static int CompareNodes(TalentNodeTemplate left, TalentNodeTemplate right)
        {
            var row = left.Row.CompareTo(right.Row);
            return row != 0 ? row : left.Column.CompareTo(right.Column);
        }
    }
}
