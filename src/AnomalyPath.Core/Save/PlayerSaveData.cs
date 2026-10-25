#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Save
{
    /// <summary>Сохранённое состояние игрока: характеристики, предметы, инвентарь и экипировка.</summary>
    public sealed class PlayerSaveData
    {
        /// <summary>Создаёт запись игрока.</summary>
        /// <param name="id">Id игрока.</param>
        /// <param name="name">Ник персонажа.</param>
        /// <param name="level">Уровень.</param>
        /// <param name="xp">Опыт.</param>
        /// <param name="health">Здоровье.</param>
        /// <param name="apMax">Максимум ОД.</param>
        /// <param name="items">Все экземпляры предметов.</param>
        /// <param name="inventory">Инвентарь.</param>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="skills">Скиллы.</param>
        /// <param name="needs">Потребности.</param>
        /// <param name="money">Деньги на персонаже; по умолчанию 0.</param>
        /// <param name="completedQuests">Завершённые квесты.</param>
        /// <param name="unlocks">Полученные открытия.</param>
        /// <param name="reputation">Репутация по фракциям.</param>
        /// <param name="talentPoints">Свободные очки талантов; по умолчанию 0.</param>
        /// <param name="talentRanks">Ранги талантов по id.</param>
        /// <param name="levelHealthBonus">Прибавка к максимуму здоровья за уровни; по умолчанию 0.</param>
        public PlayerSaveData(
            string id,
            string name,
            int level,
            int xp,
            int health,
            int apMax,
            IReadOnlyList<ItemSaveData> items,
            InventorySaveData inventory,
            EquipmentSaveData equipment,
            IReadOnlyList<string> skills,
            NeedsSaveData needs,
            int money = 0,
            IReadOnlyList<string>? completedQuests = null,
            IReadOnlyList<string>? unlocks = null,
            IReadOnlyDictionary<string, int>? reputation = null,
            int talentPoints = 0,
            IReadOnlyDictionary<string, int>? talentRanks = null,
            int levelHealthBonus = 0)
        {
            Id = id;
            Name = name ?? string.Empty;
            Level = level;
            Xp = xp;
            Health = health;
            ApMax = apMax;
            Money = money;
            TalentPoints = talentPoints;
            LevelHealthBonus = levelHealthBonus;
            Items = items;
            Inventory = inventory;
            Equipment = equipment;
            Skills = skills;
            Needs = needs;
            CompletedQuests = completedQuests ?? new List<string>();
            Unlocks = unlocks ?? new List<string>();
            Reputation = reputation ?? new Dictionary<string, int>();
            TalentRanks = talentRanks ?? new Dictionary<string, int>();
        }

        /// <summary>Id игрока.</summary>
        public string Id { get; }

        /// <summary>Ник персонажа.</summary>
        public string Name { get; }

        /// <summary>Уровень.</summary>
        public int Level { get; }

        /// <summary>Опыт.</summary>
        public int Xp { get; }

        /// <summary>Здоровье.</summary>
        public int Health { get; }

        /// <summary>Максимум ОД.</summary>
        public int ApMax { get; }

        /// <summary>Деньги на персонаже.</summary>
        public int Money { get; }

        /// <summary>Свободные очки талантов.</summary>
        public int TalentPoints { get; }

        /// <summary>Прибавка к максимуму здоровья за уровни.</summary>
        public int LevelHealthBonus { get; }

        /// <summary>Все экземпляры предметов.</summary>
        public IReadOnlyList<ItemSaveData> Items { get; }

        /// <summary>Инвентарь.</summary>
        public InventorySaveData Inventory { get; }

        /// <summary>Экипировка.</summary>
        public EquipmentSaveData Equipment { get; }

        /// <summary>Скиллы.</summary>
        public IReadOnlyList<string> Skills { get; }

        /// <summary>Потребности.</summary>
        public NeedsSaveData Needs { get; }

        /// <summary>Завершённые квесты.</summary>
        public IReadOnlyList<string> CompletedQuests { get; }

        /// <summary>Полученные открытия.</summary>
        public IReadOnlyList<string> Unlocks { get; }

        /// <summary>Репутация по фракциям.</summary>
        public IReadOnlyDictionary<string, int> Reputation { get; }

        /// <summary>Вложенные ранги талантов по идентификатору.</summary>
        public IReadOnlyDictionary<string, int> TalentRanks { get; }

        /// <summary>Сериализует игрока.</summary>
        /// <returns>JSON-объект.</returns>
        public JsonValue ToJson()
        {
            var value = JsonValue.NewObject();
            value.Members["id"] = JsonValue.FromString(Id);
            value.Members["name"] = JsonValue.FromString(Name);
            value.Members["level"] = JsonValue.FromNumber(Level);
            value.Members["xp"] = JsonValue.FromNumber(Xp);
            value.Members["health"] = JsonValue.FromNumber(Health);
            value.Members["apMax"] = JsonValue.FromNumber(ApMax);
            value.Members["money"] = JsonValue.FromNumber(Money);
            value.Members["talentPoints"] = JsonValue.FromNumber(TalentPoints);
            value.Members["levelHealthBonus"] = JsonValue.FromNumber(LevelHealthBonus);
            var items = JsonValue.NewArray();
            foreach (var item in Items)
            {
                items.Items.Add(item.ToJson());
            }

            value.Members["items"] = items;
            value.Members["inventory"] = Inventory.ToJson();
            value.Members["equipment"] = Equipment.ToJson();
            var skills = JsonValue.NewArray();
            foreach (var skill in Skills)
            {
                skills.Items.Add(JsonValue.FromString(skill));
            }

            value.Members["skills"] = skills;
            value.Members["needs"] = Needs.ToJson();
            value.Members["quests"] = ToArray(CompletedQuests);
            value.Members["unlocks"] = ToArray(Unlocks);
            var reputation = JsonValue.NewObject();
            foreach (var pair in Reputation)
            {
                reputation.Members[pair.Key] = JsonValue.FromNumber(pair.Value);
            }

            value.Members["reputation"] = reputation;
            var talents = JsonValue.NewObject();
            foreach (var pair in TalentRanks)
            {
                talents.Members[pair.Key] = JsonValue.FromNumber(pair.Value);
            }

            value.Members["talents"] = talents;
            return value;
        }

        private static JsonValue ToArray(IReadOnlyList<string> values)
        {
            var array = JsonValue.NewArray();
            foreach (var value in values)
            {
                array.Items.Add(JsonValue.FromString(value));
            }

            return array;
        }

        /// <summary>Читает игрока.</summary>
        /// <param name="value">JSON-объект.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Запись игрока.</returns>
        /// <exception cref="DataException">Поля отсутствуют или заданы неверно.</exception>
        public static PlayerSaveData FromJson(JsonValue value, string sourcePath)
        {
            var id = DataMap.RequireString(value, "id", sourcePath);
            var level = DataMap.RequireInt(value, "level", sourcePath);
            var xp = DataMap.RequireInt(value, "xp", sourcePath);
            var health = DataMap.RequireInt(value, "health", sourcePath);
            var apMax = DataMap.RequireInt(value, "apMax", sourcePath);
            var talentPoints = DataMap.OptionalInt(value, "talentPoints", 0);
            var levelHealthBonus = DataMap.OptionalInt(value, "levelHealthBonus", 0);
            if (level < 1 || xp < 0 || health < 0 || apMax < 0 || talentPoints < 0 || levelHealthBonus < 0)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "player");
            }

            var items = ReadItems(value, sourcePath);
            if (!value.TryGet("inventory", out var inventoryValue) || !inventoryValue.IsObject)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "inventory");
            }

            if (!value.TryGet("equipment", out var equipmentValue) || !equipmentValue.IsObject)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "equipment");
            }

            if (!value.TryGet("needs", out var needsValue) || !needsValue.IsObject)
            {
                throw new DataException(sourcePath, CoreErrors.SaveMissingField, "needs");
            }

            return new PlayerSaveData(
                id,
                DataMap.OptionalString(value, "name") ?? string.Empty,
                level,
                xp,
                health,
                apMax,
                items,
                InventorySaveData.FromJson(inventoryValue, sourcePath),
                EquipmentSaveData.FromJson(equipmentValue, sourcePath),
                ReadSkills(value),
                NeedsSaveData.FromJson(needsValue, sourcePath),
                DataMap.OptionalInt(value, "money", 0),
                DataMap.OptionalStringArray(value, "quests"),
                DataMap.OptionalStringArray(value, "unlocks"),
                ReadReputation(value),
                talentPoints,
                ReadTalentRanks(value, sourcePath),
                levelHealthBonus);
        }

        private static IReadOnlyList<ItemSaveData> ReadItems(JsonValue value, string sourcePath)
        {
            var result = new List<ItemSaveData>();
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

                result.Add(ItemSaveData.FromJson(item, sourcePath));
            }

            return result;
        }

        private static IReadOnlyList<string> ReadSkills(JsonValue value)
        {
            var result = new List<string>();
            if (value.TryGet("skills", out var skills) && skills.IsArray)
            {
                foreach (var item in skills.Items)
                {
                    if (item.Kind == JsonKind.String)
                    {
                        result.Add(item.Text);
                    }
                }
            }

            return result;
        }

        private static IReadOnlyDictionary<string, int> ReadReputation(JsonValue value)
        {
            var result = new Dictionary<string, int>(System.StringComparer.Ordinal);
            if (!value.TryGet("reputation", out var reputation) || !reputation.IsObject)
            {
                return result;
            }

            foreach (var pair in reputation.Entries)
            {
                if (pair.Value.Kind == JsonKind.Number)
                {
                    result[pair.Key] = (int)System.Math.Round(pair.Value.Number);
                }
            }

            return result;
        }

        private static IReadOnlyDictionary<string, int> ReadTalentRanks(JsonValue value, string sourcePath)
        {
            var result = new Dictionary<string, int>(System.StringComparer.Ordinal);
            if (!value.TryGet("talents", out var talents) || !talents.IsObject)
            {
                return result;
            }

            foreach (var pair in talents.Entries)
            {
                if (pair.Value.Kind != JsonKind.Number)
                {
                    throw new DataException(sourcePath, CoreErrors.SaveMissingField, "talents");
                }

                var rank = (int)System.Math.Round(pair.Value.Number);
                if (string.IsNullOrWhiteSpace(pair.Key) || rank < 0)
                {
                    throw new DataException(sourcePath, CoreErrors.SaveMissingField, "talents");
                }

                result[pair.Key] = rank;
            }

            return result;
        }
    }
}
