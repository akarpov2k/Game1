#nullable enable
namespace AnomalyPath.Core.Equipment
{
    /// <summary>Строковые id слотов для данных и сейвов.</summary>
    public static class EquipmentSlots
    {
        /// <summary>Основное оружие.</summary>
        public const string Main = "main";

        /// <summary>Дополнительное оружие.</summary>
        public const string Secondary = "secondary";

        /// <summary>Шлем.</summary>
        public const string Helmet = "helmet";

        /// <summary>Броня.</summary>
        public const string Armor = "armor";

        /// <summary>Рюкзак.</summary>
        public const string Backpack = "backpack";

        /// <summary>Пояс артефактов.</summary>
        public const string Belt = "belt";

        /// <summary>Быстрый доступ; в данных обозначает группу слотов.</summary>
        public const string Quick = "quick";

        /// <summary>Преобразует id слота в перечисление.</summary>
        /// <param name="id">Строковый id.</param>
        /// <param name="slot">Слот.</param>
        /// <returns><c>true</c>, если id известен.</returns>
        public static bool TryParse(string id, out EquipmentSlot slot)
        {
            switch (id)
            {
                case Main: slot = EquipmentSlot.Main; return true;
                case Secondary: slot = EquipmentSlot.Secondary; return true;
                case Helmet: slot = EquipmentSlot.Helmet; return true;
                case Armor: slot = EquipmentSlot.Armor; return true;
                case Backpack: slot = EquipmentSlot.Backpack; return true;
                case Belt: slot = EquipmentSlot.Belt; return true;
                default: slot = default; return false;
            }
        }

        /// <summary>Возвращает строковый id слота.</summary>
        /// <param name="slot">Слот.</param>
        /// <returns>Строковый id.</returns>
        public static string Id(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Main: return Main;
                case EquipmentSlot.Secondary: return Secondary;
                case EquipmentSlot.Helmet: return Helmet;
                case EquipmentSlot.Armor: return Armor;
                case EquipmentSlot.Backpack: return Backpack;
                default: return Belt;
            }
        }
    }
}
