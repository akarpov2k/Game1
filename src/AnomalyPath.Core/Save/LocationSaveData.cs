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
        /// <param name="subtype">Подтип локации в маршруте.</param>
        /// <param name="tier">Тир.</param>
        /// <param name="danger">Сложность.</param>
        /// <param name="visited">Был ли игрок.</param>
        /// <param name="cleared">Зачищена ли.</param>
        /// <param name="enemyGone">Покинул ли противник локацию после побега.</param>
        /// <param name="stashRolled">Проверялось ли появление тайника.</param>
        /// <param name="stashPresent">Есть ли на локации тайник.</param>
        /// <param name="stashOpened">Вскрыт ли тайник.</param>
        /// <param name="objectSpawnId">Id записи объекта локации; <c>null</c> — объекта нет.</param>
        /// <param name="objectLooted">Забран ли объект.</param>
        /// <param name="routeStep">Сколько шагов от хаба; у хаба 0.</param>
        /// <param name="routeEdge">Номер звена маршрута; у хаба 0.</param>
        public LocationSaveData(
            string instanceId,
            string templateId,
            int seed,
            string background,
            LocationSubtype subtype,
            int tier,
            double danger,
            bool visited,
            bool cleared,
            bool enemyGone = false,
            bool stashRolled = false,
            bool stashPresent = false,
            bool stashOpened = false,
            string? objectSpawnId = null,
            bool objectLooted = false,
            int routeStep = 0,
            int routeEdge = 0)
        {
            InstanceId = instanceId;
            TemplateId = templateId;
            Seed = seed;
            Background = background;
            Subtype = subtype;
            Tier = tier;
            Danger = danger;
            Visited = visited;
            Cleared = cleared;
            EnemyGone = enemyGone;
            StashRolled = stashRolled;
            StashPresent = stashPresent;
            StashOpened = stashOpened;
            ObjectSpawnId = objectSpawnId;
            ObjectLooted = objectLooted;
            RouteStep = routeStep;
            RouteEdge = routeEdge;
        }

        /// <summary>Id экземпляра.</summary>
        public string InstanceId { get; }

        /// <summary>Id шаблона.</summary>
        public string TemplateId { get; }

        /// <summary>Seed наполнения.</summary>
        public int Seed { get; }

        /// <summary>Фон.</summary>
        public string Background { get; }

        /// <summary>Подтип локации.</summary>
        public LocationSubtype Subtype { get; }

        /// <summary>Сколько шагов от хаба; у хаба 0.</summary>
        public int RouteStep { get; }

        /// <summary>Номер звена маршрута; у хаба 0.</summary>
        public int RouteEdge { get; }

        /// <summary>Тир.</summary>
        public int Tier { get; }

        /// <summary>Сложность.</summary>
        public double Danger { get; }

        /// <summary>Был ли игрок.</summary>
        public bool Visited { get; }

        /// <summary>Зачищена ли.</summary>
        public bool Cleared { get; }

        /// <summary>Покинул ли противник локацию после успешного побега.</summary>
        public bool EnemyGone { get; }

        /// <summary>Проверялось ли появление тайника.</summary>
        public bool StashRolled { get; }

        /// <summary>Есть ли на локации тайник.</summary>
        public bool StashPresent { get; }

        /// <summary>Вскрыт ли тайник.</summary>
        public bool StashOpened { get; }

        /// <summary>Id записи объекта локации; <c>null</c> — объекта нет.</summary>
        public string? ObjectSpawnId { get; }

        /// <summary>Забран ли объект локации.</summary>
        public bool ObjectLooted { get; }

        /// <summary>Сериализует запись.</summary>
        /// <returns>JSON-объект.</returns>
        public JsonValue ToJson()
        {
            var value = JsonValue.NewObject();
            value.Members["instanceId"] = JsonValue.FromString(InstanceId);
            value.Members["templateId"] = JsonValue.FromString(TemplateId);
            value.Members["seed"] = JsonValue.FromNumber(Seed);
            value.Members["background"] = JsonValue.FromString(Background);
            value.Members["subtype"] = JsonValue.FromString(SchemaEnum.ToCode(Subtype));
            value.Members["routeStep"] = JsonValue.FromNumber(RouteStep);
            value.Members["routeEdge"] = JsonValue.FromNumber(RouteEdge);
            value.Members["tier"] = JsonValue.FromNumber(Tier);
            value.Members["danger"] = JsonValue.FromNumber(Danger);
            value.Members["visited"] = JsonValue.FromBool(Visited);
            value.Members["cleared"] = JsonValue.FromBool(Cleared);
            value.Members["enemyGone"] = JsonValue.FromBool(EnemyGone);
            value.Members["stashRolled"] = JsonValue.FromBool(StashRolled);
            value.Members["stashPresent"] = JsonValue.FromBool(StashPresent);
            value.Members["stashOpened"] = JsonValue.FromBool(StashOpened);
            if (ObjectSpawnId != null)
            {
                var locationObject = JsonValue.NewObject();
                locationObject.Members["spawnId"] = JsonValue.FromString(ObjectSpawnId);
                locationObject.Members["looted"] = JsonValue.FromBool(ObjectLooted);
                value.Members["object"] = locationObject;
            }

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
            var subtypeCode = DataMap.RequireString(value, "subtype", sourcePath);
            if (!SchemaEnum.TryParse(subtypeCode, out LocationSubtype subtype))
            {
                throw new DataException(sourcePath, CoreErrors.WorldUnknownSubtype, subtypeCode);
            }

            var routeStep = DataMap.OptionalInt(value, "routeStep", 0);
            var routeEdge = DataMap.OptionalInt(value, "routeEdge", 0);
            var tier = DataMap.OptionalInt(value, "tier", 0);
            var danger = DataMap.OptionalDouble(value, "danger") ?? 0;
            var visited = DataMap.OptionalBool(value, "visited", false);
            var cleared = DataMap.OptionalBool(value, "cleared", false);
            var enemyGone = DataMap.OptionalBool(value, "enemyGone", false);
            var stashRolled = DataMap.OptionalBool(value, "stashRolled", false);
            var stashPresent = DataMap.OptionalBool(value, "stashPresent", false);
            var stashOpened = DataMap.OptionalBool(value, "stashOpened", false);
            string? objectSpawnId = null;
            var objectLooted = false;
            if (value.TryGet("object", out var objectValue) && objectValue.IsObject)
            {
                objectSpawnId = DataMap.OptionalString(objectValue, "spawnId");
                objectLooted = DataMap.OptionalBool(objectValue, "looted", false);
            }

            if (tier < 0 || danger < 0 || danger > 1 || routeStep < 0 || routeEdge < 0 ||
                (stashPresent && !stashRolled) || (stashOpened && !stashPresent))
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "location");
            }

            return new LocationSaveData(
                instanceId,
                templateId,
                seed,
                background,
                subtype,
                tier,
                danger,
                visited,
                cleared,
                enemyGone,
                stashRolled,
                stashPresent,
                stashOpened,
                objectSpawnId,
                objectLooted,
                routeStep,
                routeEdge);
        }
    }
}
