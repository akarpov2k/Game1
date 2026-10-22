#nullable enable
namespace AnomalyPath.Runtime.Screens.Character
{
    /// <summary>Размеры экрана персонажа в пикселях опорного разрешения 1920×1080.</summary>
    public static class CharacterMetrics
    {
        /// <summary>Ширина фрейма снаряжения.</summary>
        public const float FrameWidth = 560f;

        /// <summary>Высота фреймов.</summary>
        public const float FrameHeight = 800f;

        /// <summary>Ширина фрейма инвентаря.</summary>
        public const float InventoryWidth = 768f;

        /// <summary>Зазор между фреймами.</summary>
        public const float Gap = 48f;

        /// <summary>Сторона клетки инвентаря.</summary>
        public const float Cell = 80f;

        /// <summary>Зазор между клетками.</summary>
        public const float CellGap = 4f;

        /// <summary>Отступ внутри фрейма.</summary>
        public const float Padding = 16f;
    }
}
