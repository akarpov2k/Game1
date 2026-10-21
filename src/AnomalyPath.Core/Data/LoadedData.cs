#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Data
{
    /// <summary>Результат загрузки: сущности, разложенные по видам.</summary>
    public sealed class LoadedData
    {
        private readonly Dictionary<string, Dictionary<string, JsonValue>> _byKind;

        /// <summary>Создаёт реестр поверх словаря видов.</summary>
        /// <param name="byKind">Загруженные сущности: вид → id → запись.</param>
        public LoadedData(Dictionary<string, Dictionary<string, JsonValue>> byKind)
        {
            _byKind = byKind;
        }

        /// <summary>Все сущности указанного вида.</summary>
        /// <param name="kind">Вид сущности.</param>
        /// <returns>Словарь id → запись.</returns>
        /// <exception cref="DataException">Вид не загружен.</exception>
        public IReadOnlyDictionary<string, JsonValue> OfKind(string kind) =>
            _byKind.TryGetValue(kind, out var map)
                ? map
                : throw new DataException("<registry>", CoreErrors.MissingArrayField, kind);

        /// <summary>Ищет сущность по виду и id.</summary>
        /// <param name="kind">Вид сущности.</param>
        /// <param name="id">Идентификатор.</param>
        /// <param name="entity">Найденная запись или пустое значение.</param>
        /// <returns><c>true</c>, если сущность найдена.</returns>
        public bool TryGet(string kind, string id, out JsonValue entity)
        {
            if (_byKind.TryGetValue(kind, out var map) && map.TryGetValue(id, out entity))
            {
                return true;
            }

            entity = JsonValue.Null();
            return false;
        }

        /// <summary>Все сущности вида списком.</summary>
        /// <param name="kind">Вид сущности.</param>
        /// <returns>Список записей; пустой, если вид не загружен.</returns>
        public IReadOnlyList<JsonValue> All(string kind) =>
            _byKind.TryGetValue(kind, out var map)
                ? new List<JsonValue>(map.Values)
                : new List<JsonValue>();

        /// <summary>Виды, которые удалось загрузить.</summary>
        public IReadOnlyCollection<string> Kinds => _byKind.Keys;
    }
}
