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
        /// <param name="level">Уровень.</param>
        /// <param name="xp">Опыт.</param>
        /// <param name="health">Здоровье.</param>
        /// <param name="apMax">Максимум ОД.</param>
        /// <param name="items">Все экземпляры предметов.</param>
        /// <param name="inventory">Инвентарь.</param>
        /// <param name="equipment">Экипировка.</param>
        /// <param name="skills">Скиллы.</param>
        /// <param name="needs">Потребности.</param>
        public PlayerSaveData(
            string id,
            int level,
            int xp,
            int health,
            int apMax,
            IReadOnlyList<ItemSaveData> items,
            InventorySaveData inventory,
            EquipmentSaveData equipment,
            IReadOnlyList<string> skills,
            NeedsSaveData needs)
        {
            Id = id;
            Level = level;
            Xp = xp;
            Health = health;
            ApMax = apMax;
            Items = items;
            Inventory = inventory;
            Equipment = equipment;
            Skills = skills;
            Needs = needs;
        }

        /// <summary>Id игрока.</summary>
        public string Id { get; }

        /// <summary>Уровень.</summary>
        public int Level { get; }

        /// <summary>Опыт.</summary>
        public int Xp { get; }

        /// <summary>Здоровье.</summary>
        public int Health { get; }

        /// <summary>Максимум ОД.</summary>
        public int ApMax { get; }

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

        /// <summary>Сериализует игрока.</summary>
        /// <returns>JSON-объект.</returns>
        public JsonValue ToJson()
        {
            var value = JsonValue.NewObject();
            value.Members["id"] = JsonValue.FromString(Id);
            value.Members["level"] = JsonValue.FromNumber(Level);
            value.Members["xp"] = JsonValue.FromNumber(Xp);
            value.Members["health"] = JsonValue.FromNumber(Health);
            value.Members["apMax"] = JsonValue.FromNumber(ApMax);
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
            return value;
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
            if (level < 1 || xp < 0 || health < 0 || apMax < 0)
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
                level,
                xp,
                health,
                apMax,
                items,
                InventorySaveData.FromJson(inventoryValue, sourcePath),
                EquipmentSaveData.FromJson(equipmentValue, sourcePath),
                ReadSkills(value),
                NeedsSaveData.FromJson(needsValue, sourcePath));
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
    }
}
