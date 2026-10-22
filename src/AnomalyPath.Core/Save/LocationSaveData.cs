#nullable enable
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Save
{
    /// <summary>Сохранённое состояние одного экземпляра локации.</summary>
    public sealed class LocationSaveData
    {
        /// <summary>Создаёт запись локации.</summary>
        /// <param name="instanceId">Id экземпляра.</param>
        /// <param name="templateId">Id шаблона.</param>
        /// <param name="seed">Seed наполнения.</param>
        /// <param name="background">Фон.</param>
        /// <param name="nodeType">Роль в цепочке.</param>
        /// <param name="tier">Тир.</param>
        /// <param name="danger">Сложность.</param>
        /// <param name="visited">Был ли игрок.</param>
        /// <param name="cleared">Зачищена ли.</param>
        public LocationSaveData(
            string instanceId,
            string templateId,
            int seed,
            string background,
            LocationNodeType nodeType,
            int tier,
            double danger,
            bool visited,
            bool cleared)
        {
            InstanceId = instanceId;
            TemplateId = templateId;
            Seed = seed;
            Background = background;
            NodeType = nodeType;
            Tier = tier;
            Danger = danger;
            Visited = visited;
            Cleared = cleared;
        }

        /// <summary>Id экземпляра.</summary>
        public string InstanceId { get; }

        /// <summary>Id шаблона.</summary>
        public string TemplateId { get; }

        /// <summary>Seed наполнения.</summary>
        public int Seed { get; }

        /// <summary>Фон.</summary>
        public string Background { get; }

        /// <summary>Роль в цепочке.</summary>
        public LocationNodeType NodeType { get; }

        /// <summary>Тир.</summary>
        public int Tier { get; }

        /// <summary>Сложность.</summary>
        public double Danger { get; }

        /// <summary>Был ли игрок.</summary>
        public bool Visited { get; }

        /// <summary>Зачищена ли.</summary>
        public bool Cleared { get; }

        /// <summary>Сериализует запись.</summary>
        /// <returns>JSON-объект.</returns>
        public JsonValue ToJson()
        {
            var value = JsonValue.NewObject();
            value.Members["instanceId"] = JsonValue.FromString(InstanceId);
            value.Members["templateId"] = JsonValue.FromString(TemplateId);
            value.Members["seed"] = JsonValue.FromNumber(Seed);
            value.Members["background"] = JsonValue.FromString(Background);
            value.Members["nodeType"] = JsonValue.FromString(SchemaEnum.ToCode(NodeType));
            value.Members["tier"] = JsonValue.FromNumber(Tier);
            value.Members["danger"] = JsonValue.FromNumber(Danger);
            value.Members["visited"] = JsonValue.FromBool(Visited);
            value.Members["cleared"] = JsonValue.FromBool(Cleared);
            return value;
        }

        /// <summary>Читает запись локации.</summary>
        /// <param name="value">JSON-объект.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Запись локации.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static LocationSaveData FromJson(JsonValue value, string sourcePath)
        {
            var instanceId = DataMap.RequireString(value, "instanceId", sourcePath);
            var templateId = DataMap.RequireString(value, "templateId", sourcePath);
            var seed = DataMap.RequireInt(value, "seed", sourcePath);
            var background = DataMap.OptionalString(value, "background") ?? string.Empty;
            var nodeTypeCode = DataMap.RequireString(value, "nodeType", sourcePath);
            if (!SchemaEnum.TryParse(nodeTypeCode, out LocationNodeType nodeType))
            {
                throw new DataException(sourcePath, CoreErrors.WorldUnknownNodeType, nodeTypeCode);
            }
            var tier = DataMap.OptionalInt(value, "tier", 0);
            var danger = DataMap.OptionalDouble(value, "danger") ?? 0;
            var visited = DataMap.OptionalBool(value, "visited", false);
            var cleared = DataMap.OptionalBool(value, "cleared", false);
            if (tier < 0 || danger < 0 || danger > 1)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "location");
            }

            return new LocationSaveData(
                instanceId,
                templateId,
                seed,
                background,
                nodeType,
                tier,
                danger,
                visited,
                cleared);
        }
    }
}
