#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Quests;

namespace AnomalyPath.Core.Save
{
    /// <summary>Снимок взятого квеста: цель, прогресс и откат.</summary>
    public sealed class QuestSaveData
    {
        /// <summary>Создаёт снимок квеста.</summary>
        /// <param name="questId">Идентификатор квеста.</param>
        /// <param name="giverActorId">Кто выдал.</param>
        /// <param name="status">Состояние.</param>
        /// <param name="acceptedAt">Когда взят.</param>
        /// <param name="cooldownUntil">До какого времени откат.</param>
        /// <param name="completedAt">Когда сдан.</param>
        /// <param name="locationTemplateId">Шаблон целевой локации.</param>
        /// <param name="locationInstanceId">Экземпляр целевой локации.</param>
        /// <param name="actorIds">Целевые актёры.</param>
        /// <param name="items">Целевые предметы.</param>
        /// <param name="killCount">Сколько целей убито.</param>
        /// <param name="cleared">Зачищена ли локация.</param>
        /// <param name="requiredCount">Сколько целей нужно всего.</param>
        /// <param name="slot">Номер слота предложения.</param>
        public QuestSaveData(
            string questId,
            string giverActorId,
            QuestStatus status,
            double acceptedAt,
            double cooldownUntil,
            double completedAt,
            string? locationTemplateId = null,
            string? locationInstanceId = null,
            IReadOnlyList<string>? actorIds = null,
            IReadOnlyList<QuestItemSaveData>? items = null,
            int killCount = 0,
            bool cleared = false,
            int requiredCount = 1,
            int slot = 0)
        {
            QuestId = questId;
            GiverActorId = giverActorId;
            Status = status;
            AcceptedAt = acceptedAt;
            CooldownUntil = cooldownUntil;
            CompletedAt = completedAt;
            LocationTemplateId = locationTemplateId;
            LocationInstanceId = locationInstanceId;
            ActorIds = actorIds ?? new List<string>();
            Items = items ?? new List<QuestItemSaveData>();
            KillCount = killCount;
            Cleared = cleared;
            RequiredCount = requiredCount;
            Slot = slot;
        }

        /// <summary>Идентификатор квеста.</summary>
        public string QuestId { get; }

        /// <summary>Кто выдал.</summary>
        public string GiverActorId { get; }

        /// <summary>Состояние.</summary>
        public QuestStatus Status { get; }

        /// <summary>Когда взят.</summary>
        public double AcceptedAt { get; }

        /// <summary>До какого времени откат.</summary>
        public double CooldownUntil { get; }

        /// <summary>Когда сдан.</summary>
        public double CompletedAt { get; }

        /// <summary>Шаблон целевой локации.</summary>
        public string? LocationTemplateId { get; }

        /// <summary>Экземпляр целевой локации.</summary>
        public string? LocationInstanceId { get; }

        /// <summary>Целевые актёры.</summary>
        public IReadOnlyList<string> ActorIds { get; }

        /// <summary>Целевые предметы.</summary>
        public IReadOnlyList<QuestItemSaveData> Items { get; }

        /// <summary>Сколько целей убито.</summary>
        public int KillCount { get; }

        /// <summary>Зачищена ли локация.</summary>
        public bool Cleared { get; }

        /// <summary>Сколько целей нужно всего.</summary>
        public int RequiredCount { get; }

        /// <summary>Номер слота предложения.</summary>
        public int Slot { get; }

        /// <summary>Сериализует снимок.</summary>
        /// <returns>JSON-объект.</returns>
        public JsonValue ToJson()
        {
            var value = JsonValue.NewObject();
            value.Members["questId"] = JsonValue.FromString(QuestId);
            value.Members["giverActorId"] = JsonValue.FromString(GiverActorId);
            value.Members["status"] = JsonValue.FromString(SchemaEnum.ToCode(Status));
            value.Members["acceptedAt"] = JsonValue.FromNumber(AcceptedAt);
            value.Members["cooldownUntil"] = JsonValue.FromNumber(CooldownUntil);
            value.Members["completedAt"] = JsonValue.FromNumber(CompletedAt);
            if (LocationTemplateId != null)
            {
                value.Members["locationTemplateId"] = JsonValue.FromString(LocationTemplateId);
            }

            if (LocationInstanceId != null)
            {
                value.Members["locationInstanceId"] = JsonValue.FromString(LocationInstanceId);
            }

            var actors = JsonValue.NewArray();
            foreach (var actorId in ActorIds)
            {
                actors.Items.Add(JsonValue.FromString(actorId));
            }

            value.Members["actorIds"] = actors;
            var items = JsonValue.NewArray();
            foreach (var item in Items)
            {
                items.Items.Add(item.ToJson());
            }

            value.Members["items"] = items;
            value.Members["killCount"] = JsonValue.FromNumber(KillCount);
            value.Members["cleared"] = JsonValue.FromBool(Cleared);
            value.Members["requiredCount"] = JsonValue.FromNumber(RequiredCount);
            value.Members["slot"] = JsonValue.FromNumber(Slot);
            return value;
        }

        /// <summary>Читает снимок квеста.</summary>
        /// <param name="value">JSON-объект.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Снимок квеста.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static QuestSaveData FromJson(JsonValue value, string sourcePath)
        {
            var questId = DataMap.RequireString(value, "questId", sourcePath);
            var giverActorId = DataMap.RequireString(value, "giverActorId", sourcePath);
            var status = QuestStatus.Active;
            var statusCode = DataMap.OptionalString(value, "status");
            if (statusCode != null && !SchemaEnum.TryParse(statusCode, out status))
            {
                throw new DataException(sourcePath, CoreErrors.UnknownQuestStatus, statusCode);
            }

            var acceptedAt = DataMap.OptionalDouble(value, "acceptedAt") ?? 0;
            var cooldownUntil = DataMap.OptionalDouble(value, "cooldownUntil") ?? 0;
            var completedAt = DataMap.OptionalDouble(value, "completedAt") ?? 0;
            var killCount = DataMap.OptionalInt(value, "killCount", 0);
            var requiredCount = DataMap.OptionalInt(value, "requiredCount", 1);
            var slot = DataMap.OptionalInt(value, "slot", 0);
            if (acceptedAt < 0 || cooldownUntil < 0 || completedAt < 0 || killCount < 0 ||
                requiredCount < 1 || slot < 0)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "quests");
            }

            return new QuestSaveData(
                questId,
                giverActorId,
                status,
                acceptedAt,
                cooldownUntil,
                completedAt,
                DataMap.OptionalString(value, "locationTemplateId"),
                DataMap.OptionalString(value, "locationInstanceId"),
                DataMap.OptionalStringArray(value, "actorIds"),
                ReadItems(value, sourcePath),
                killCount,
                DataMap.OptionalBool(value, "cleared", false),
                requiredCount,
                slot);
        }

        private static IReadOnlyList<QuestItemSaveData> ReadItems(JsonValue value, string sourcePath)
        {
            var result = new List<QuestItemSaveData>();
            if (!value.TryGet("items", out var items) || !items.IsArray)
            {
                return result;
            }

            foreach (var item in items.Items)
            {
                if (!item.IsObject)
                {
                    throw new DataException(sourcePath, CoreErrors.JsonNotObject);
                }

                result.Add(QuestItemSaveData.FromJson(item, sourcePath));
            }

            return result;
        }
    }
}
