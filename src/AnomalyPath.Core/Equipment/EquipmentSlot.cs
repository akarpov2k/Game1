#nullable enable
namespace AnomalyPath.Core.Equipment
{
    /// <summary>Слоты на персонаже. Быстрый доступ вынесен в отдельный список переменной длины.</summary>
    public enum EquipmentSlot
    {
        /// <summary>Основное оружие — любое.</summary>
        Main,

        /// <summary>Дополнительное оружие: холодное, пистолеты, ПП, короткие автоматы и дробовики.</summary>
        Secondary,

        /// <summary>Шлем.</summary>
        Helmet,

        /// <summary>Броня.</summary>
        Armor,

        /// <summary>Рюкзак: добавляет строки инвентаря.</summary>
        Backpack,

        /// <summary>Пояс артефактов.</summary>
        Belt,
    }
}
