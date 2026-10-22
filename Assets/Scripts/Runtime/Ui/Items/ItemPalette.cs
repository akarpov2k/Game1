#nullable enable
using UnityEngine;

namespace AnomalyPath.Runtime.Ui.Items
{
    /// <summary>Цвета плашек предметов по категории.</summary>
    public static class ItemPalette
    {
        /// <summary>Предмет категории «оружие».</summary>
        public static readonly Color Weapon = new Color(0.24f, 0.31f, 0.24f, 1f);

        /// <summary>Предмет категории «патроны».</summary>
        public static readonly Color Ammo = new Color(0.34f, 0.30f, 0.18f, 1f);

        /// <summary>Броня, шлем, рюкзак, пояс.</summary>
        public static readonly Color Armor = new Color(0.20f, 0.26f, 0.32f, 1f);

        /// <summary>Артефакт.</summary>
        public static readonly Color Artifact = new Color(0.30f, 0.22f, 0.34f, 1f);

        /// <summary>Остальные предметы.</summary>
        public static readonly Color Other = new Color(0.26f, 0.26f, 0.24f, 1f);
    }
}
