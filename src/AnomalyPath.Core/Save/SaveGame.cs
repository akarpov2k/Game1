#nullable enable
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Save
{
    /// <summary>Корень файла сохранения: версия, метаданные и снимок мира.</summary>
    public sealed class SaveGame
    {
        /// <summary>Поддерживаемая версия схемы сохранения.</summary>
        public const int CurrentSchemaVersion = 4;

        /// <summary>Поддерживаемая версия формата сохранения.</summary>
        public const int CurrentSaveVersion = 4;

        /// <summary>Создаёт сохранение.</summary>
        /// <param name="schemaVersion">Версия схемы.</param>
        /// <param name="saveVersion">Версия формата.</param>
        /// <param name="createdAt">Дата создания в ISO-8601.</param>
        /// <param name="playtime">Наигранное время в секундах.</param>
        /// <param name="seed">Seed мира.</param>
        /// <param name="world">Снимок мира.</param>
        /// <param name="player">Снимок игрока или <c>null</c> для world-only сейва.</param>
        /// <param name="title">Название сохранения, заданное игроком.</param>
        public SaveGame(
            int schemaVersion,
            int saveVersion,
            string createdAt,
            double playtime,
            int seed,
            WorldSaveData world,
            PlayerSaveData? player = null,
            string? title = null)
        {
            SchemaVersion = schemaVersion;
            SaveVersion = saveVersion;
            CreatedAt = createdAt;
            Playtime = playtime;
            Seed = seed;
            World = world;
            Player = player;
            Title = title ?? string.Empty;
        }

        /// <summary>Версия схемы.</summary>
        public int SchemaVersion { get; }

        /// <summary>Версия формата.</summary>
        public int SaveVersion { get; }

        /// <summary>Дата создания в ISO-8601.</summary>
        public string CreatedAt { get; }

        /// <summary>Наигранное время в секундах.</summary>
        public double Playtime { get; }

        /// <summary>Seed мира.</summary>
        public int Seed { get; }

        /// <summary>Снимок мира.</summary>
        public WorldSaveData World { get; }

        /// <summary>Снимок игрока или <c>null</c>.</summary>
        public PlayerSaveData? Player { get; }

        /// <summary>Название сохранения, заданное игроком; пусто — без названия.</summary>
        public string Title { get; }

        /// <summary>Сериализует сохранение.</summary>
        /// <returns>JSON-объект.</returns>
        public JsonValue ToJson()
        {
            var value = JsonValue.NewObject();
            value.Members["schemaVersion"] = JsonValue.FromNumber(SchemaVersion);
            value.Members["saveVersion"] = JsonValue.FromNumber(SaveVersion);
            value.Members["createdAt"] = JsonValue.FromString(CreatedAt);
            value.Members["playtime"] = JsonValue.FromNumber(Playtime);
            value.Members["seed"] = JsonValue.FromNumber(Seed);
            if (Title.Length > 0)
            {
                value.Members["title"] = JsonValue.FromString(Title);
            }

            value.Members["world"] = World.ToJson();
            if (Player != null)
            {
                value.Members["player"] = Player.ToJson();
            }

            return value;
        }

        /// <summary>Читает сохранение и проверяет поддержку версий.</summary>
        /// <param name="value">JSON-объект.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Сохранение.</returns>
        /// <exception cref="DataException">Поля отсутствуют, версия не поддерживается или мир битый.</exception>
        public static SaveGame FromJson(JsonValue value, string sourcePath)
        {
            var schemaVersion = DataMap.RequireInt(value, "schemaVersion", sourcePath);
            if (schemaVersion < 1 || schemaVersion > CurrentSchemaVersion)
            {
                throw new DataException(
                    sourcePath,
                    CoreErrors.SaveUnsupportedVersion,
                    schemaVersion,
                    CurrentSchemaVersion);
            }

            var saveVersion = DataMap.RequireInt(value, "saveVersion", sourcePath);
            if (saveVersion < 1 || saveVersion > CurrentSaveVersion)
            {
                throw new DataException(
                    sourcePath,
                    CoreErrors.SaveUnsupportedVersion,
                    saveVersion,
                    CurrentSaveVersion);
            }

            var createdAt = DataMap.RequireString(value, "createdAt", sourcePath);
            var playtime = DataMap.OptionalDouble(value, "playtime") ?? 0;
            if (playtime < 0)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "playtime");
            }

            var seed = DataMap.RequireInt(value, "seed", sourcePath);
            if (!value.TryGet("world", out var worldValue) || !worldValue.IsObject)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "world");
            }

            PlayerSaveData? player = null;
            if (value.TryGet("player", out var playerValue) && playerValue.IsObject)
            {
                player = PlayerSaveData.FromJson(playerValue, sourcePath);
            }

            return new SaveGame(
                schemaVersion,
                saveVersion,
                createdAt,
                playtime,
                seed,
                WorldSaveData.FromJson(worldValue, sourcePath),
                player,
                DataMap.OptionalString(value, "title"));
        }
    }
}
