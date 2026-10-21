using System;
using System.Collections.Generic;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Data
{
    /// <summary>
    /// Загрузка данных по правилам раздела 3 схем: каталог → коллекции → файлы сущностей,
    /// с наследованием <c>extends</c> и подмешиванием <c>include</c>.
    /// </summary>
    public sealed class DataLoader
    {
        private const int MaxDepth = 32;

        private readonly IDataSource _source;
        private readonly IReadOnlyDictionary<string, JsonValue>? _parsed;
        private readonly Dictionary<string, JsonValue> _resolved = new Dictionary<string, JsonValue>(StringComparer.Ordinal);
        private readonly HashSet<string> _resolving = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _registeredPaths = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _collectionPaths = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, Dictionary<string, JsonValue>> _byKind =
            new Dictionary<string, Dictionary<string, JsonValue>>(StringComparer.Ordinal);

        /// <summary>Создаёт загрузчик поверх источника файлов.</summary>
        /// <param name="source">Источник данных.</param>
        /// <exception cref="ArgumentNullException">Источник не задан.</exception>
        public DataLoader(IDataSource source)
            : this(source, null)
        {
        }

        /// <summary>Создаёт загрузчик с заранее разобранными файлами: каждый файл читается один раз.</summary>
        /// <param name="source">Источник данных.</param>
        /// <param name="parsedFiles">Кэш «путь → разобранный JSON»; отсутствующие пути читаются с диска.</param>
        public DataLoader(IDataSource source, IReadOnlyDictionary<string, JsonValue>? parsedFiles)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _parsed = parsedFiles;
        }

        /// <summary>Загружает все сущности, перечисленные каталогом.</summary>
        /// <param name="catalogPath">Путь к каталогу относительно корня источника.</param>
        /// <returns>Реестр сущностей по видам.</returns>
        /// <exception cref="DataException">Ошибка чтения, разбора или валидации данных.</exception>
        public LoadedData Load(string catalogPath = "catalog.json")
        {
            var catalog = ReadJson(catalogPath);
            foreach (var file in RequireStringArray(catalog, "files", catalogPath))
            {
                LoadFile(file);
            }

            return new LoadedData(_byKind);
        }

        /// <summary>Разрешает один файл с учётом <c>extends</c> и <c>include</c>.</summary>
        /// <param name="path">Путь к файлу относительно корня источника.</param>
        /// <returns>Итоговая запись со всеми унаследованными полями.</returns>
        /// <exception cref="DataException">Ошибка чтения, разбора или наследования.</exception>
        public JsonValue ResolveEntity(string path) => Resolve(path);

        private void LoadFile(string path)
        {
            var raw = ReadJson(path);
            if (raw.Get("collection").Kind == JsonKind.Bool && raw.Get("collection").Bool)
            {
                if (!_collectionPaths.Add(path))
                {
                    throw new DataException(path, CoreErrors.DuplicateCollection, path);
                }

                foreach (var entry in RequireStringArray(raw, "entries", path))
                {
                    LoadFile(entry);
                }

                if (raw.TryGet("include", out var includes) && includes.IsArray)
                {
                    foreach (var item in includes.Items)
                    {
                        LoadFile(item.Text);
                    }
                }

                return;
            }

            var entity = Resolve(path);
            if (entity.Get("abstract").Kind == JsonKind.Bool && entity.Get("abstract").Bool)
            {
                return;
            }

            var id = RequireString(entity, "id", path);
            var kind = RequireString(entity, "kind", path);

            if (!_registeredPaths.Add(path))
            {
                throw new DataException(path, CoreErrors.DuplicateEntity, id);
            }

            if (!_byKind.TryGetValue(kind, out var map))
            {
                map = new Dictionary<string, JsonValue>(StringComparer.Ordinal);
                _byKind[kind] = map;
            }

            if (map.ContainsKey(id))
            {
                throw new DataException(path, CoreErrors.DuplicateId, id, kind);
            }

            map[id] = entity;
        }

        private JsonValue Resolve(string path)
        {
            if (_resolved.TryGetValue(path, out var cached))
            {
                return cached;
            }

            if (_resolving.Contains(path))
            {
                throw new DataException(path, CoreErrors.InheritanceCycle);
            }

            if (_resolving.Count >= MaxDepth)
            {
                throw new DataException(path, CoreErrors.InheritanceDepth);
            }

            _resolving.Add(path);
            try
            {
                var raw = ReadJson(path);
                var kind = raw.TryGet("kind", out var kindValue) ? kindValue.Text : null;
                var result = JsonValue.NewObject();

                if (raw.TryGet("extends", out var extendsValue) && extendsValue.Kind == JsonKind.String)
                {
                    var parent = Resolve(extendsValue.Text);
                    EnsureSameKind(path, kind, extendsValue.Text, parent);
                    result = parent;
                }

                if (raw.TryGet("include", out var includes) && includes.IsArray)
                {
                    foreach (var item in includes.Items)
                    {
                        result = EntityMerger.Merge(result, Resolve(item.Text));
                    }
                }

                result = EntityMerger.Merge(result, raw);
                ApplyControlFields(result, raw);
                _resolved[path] = result;
                return result;
            }
            finally
            {
                _resolving.Remove(path);
            }
        }

        /// <summary>
        /// Служебные поля не наследуются: иначе ребёнок унаследует <c>abstract: true</c>
        /// от базового шаблона и выпадет из реестра.
        /// </summary>
        private static void ApplyControlFields(JsonValue result, JsonValue raw)
        {
            foreach (var key in new[] { "extends", "include", "collection" })
            {
                result.Members.Remove(key);
            }

            if (raw.TryGet("abstract", out var abstractValue))
            {
                result.Members["abstract"] = EntityMerger.Clone(abstractValue);
            }
            else
            {
                result.Members.Remove("abstract");
            }

            foreach (var key in new[] { "id", "kind", "schemaVersion" })
            {
                if (raw.TryGet(key, out var value))
                {
                    result.Members[key] = EntityMerger.Clone(value);
                }
            }
        }

        private static void EnsureSameKind(string path, string? childKind, string parentPath, JsonValue parent)
        {
            if (childKind == null || !parent.TryGet("kind", out var parentKind) || parentKind.Kind != JsonKind.String)
            {
                return;
            }

            if (!string.Equals(childKind, parentKind.Text, StringComparison.Ordinal))
            {
                throw new DataException(path, CoreErrors.KindMismatch, childKind, parentKind.Text, parentPath);
            }
        }

        private JsonValue ReadJson(string path)
        {
            if (_parsed != null && _parsed.TryGetValue(path, out var cached))
            {
                return cached;
            }

            if (!_source.Exists(path))
            {
                throw new DataException(path, CoreErrors.FileNotFound);
            }

            return JsonParser.Parse(_source.ReadAllText(path), path);
        }

        private static string RequireString(JsonValue value, string field, string path)
        {
            if (!value.TryGet(field, out var fieldValue) || fieldValue.Kind != JsonKind.String ||
                string.IsNullOrWhiteSpace(fieldValue.Text))
            {
                throw new DataException(path, CoreErrors.MissingStringField, field);
            }

            return fieldValue.Text;
        }

        private static IEnumerable<string> RequireStringArray(JsonValue value, string field, string path)
        {
            if (!value.TryGet(field, out var fieldValue) || !fieldValue.IsArray)
            {
                throw new DataException(path, CoreErrors.MissingArrayField, field);
            }

            var result = new List<string>();
            foreach (var item in fieldValue.Items)
            {
                if (item.Kind != JsonKind.String)
                {
                    throw new DataException(path, CoreErrors.ArrayItemNotString, field);
                }

                result.Add(item.Text);
            }

            return result;
        }
    }
}
