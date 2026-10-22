#nullable enable
using UnityEngine;

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

        /// <summary>Сторона одной клетки 1×1 в пикселях.</summary>
        public const float Cell = 80f;

        /// <summary>Зазор между клетками.</summary>
        public const float CellGap = 4f;

        /// <summary>Шаг сетки: сторона клетки плюс зазор.</summary>
        public const float CellStep = Cell + CellGap;

        /// <summary>Толщина линии сетки в пикселях UI.</summary>
        public const float GridLine = 2f;

        /// <summary>Отступ внутри фрейма.</summary>
        public const float Padding = 16f;

        /// <summary>Ширина сетки в пикселях: все клетки одинаковые, между ними один зазор.</summary>
        /// <param name="columns">Число колонок.</param>
        /// <returns>Ширина сетки.</returns>
        public static float GridWidth(int columns) => (columns * Cell) + ((columns - 1) * CellGap);

        /// <summary>Высота сетки в пикселях: все клетки одинаковые, между ними один зазор.</summary>
        /// <param name="rows">Число строк.</param>
        /// <returns>Высота сетки.</returns>
        public static float GridHeight(int rows) => (rows * Cell) + ((rows - 1) * CellGap);

        /// <summary>Размер предмета, занимающего прямоугольник из клеток 1×1.</summary>
        /// <param name="width">Ширина в клетках.</param>
        /// <param name="height">Высота в клетках.</param>
        /// <returns>Размер в пикселях с учётом зазоров.</returns>
        public static Vector2 ItemSize(int width, int height) =>
            new Vector2((width * Cell) + ((width - 1) * CellGap), (height * Cell) + ((height - 1) * CellGap));
    }
}
