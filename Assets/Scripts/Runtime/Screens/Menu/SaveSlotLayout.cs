#nullable enable
using UnityEngine;

namespace AnomalyPath.Runtime.Screens.Menu
{
    /// <summary>Общая раскладка квадратных ячеек сохранения: 2–3 в строке по ширине экрана.</summary>
    public static class SaveSlotLayout
    {
        /// <summary>Зазор между квадратными ячейками.</summary>
        public const float Gap = 16f;

        /// <summary>Минимальная сторона квадрата.</summary>
        public const float MinTile = 120f;

        /// <summary>Максимальная ширина сетки.</summary>
        public const float MaxGridWidth = 1200f;

        /// <summary>Сколько ячеек помещается на одной странице.</summary>
        public const int PageSize = 6;

        /// <summary>Порог ширины, после которого в строке три ячейки.</summary>
        public const float ColumnsThreshold = 900f;

        /// <summary>Сколько квадратных ячеек ставить в строку при данной ширине.</summary>
        /// <param name="availableWidth">Доступная ширина сетки.</param>
        /// <returns>3 на широком экране, 2 на узком.</returns>
        public static int ColumnsForWidth(float availableWidth) =>
            availableWidth >= ColumnsThreshold ? 3 : 2;

        /// <summary>Число рядов для данного количества ячеек.</summary>
        /// <param name="count">Число ячеек.</param>
        /// <param name="columns">Число колонок.</param>
        /// <returns>Число рядов, минимум один.</returns>
        public static int Rows(int count, int columns) =>
            Mathf.Max(1, Mathf.CeilToInt(Mathf.Max(1, count) / (float)Mathf.Max(1, columns)));

        /// <summary>Сторона квадрата, вписанного в доступные ширину и высоту сетки.</summary>
        /// <param name="availableWidth">Доступная ширина сетки.</param>
        /// <param name="gridHeight">Доступная высота сетки.</param>
        /// <param name="count">Число ячеек.</param>
        /// <param name="columns">Число колонок.</param>
        /// <returns>Сторона квадрата.</returns>
        public static float TileSize(float availableWidth, float gridHeight, int count, int columns)
        {
            var rows = Rows(count, columns);
            var byWidth = (availableWidth - ((columns - 1) * Gap)) / Mathf.Max(1, columns);
            var byHeight = (gridHeight - ((rows - 1) * Gap)) / rows;
            return Mathf.Floor(Mathf.Max(MinTile, Mathf.Min(byWidth, byHeight)));
        }

        /// <summary>Размер заголовка ячейки под её сторону.</summary>
        /// <param name="tile">Сторона квадрата.</param>
        /// <returns>Размер шрифта.</returns>
        public static int TitleSize(float tile) => tile >= 320f ? 22 : tile >= 220f ? 18 : 16;

        /// <summary>Размер строки прогресса под сторону ячейки.</summary>
        /// <param name="tile">Сторона квадрата.</param>
        /// <returns>Размер шрифта.</returns>
        public static int LineSize(float tile) => tile >= 320f ? 18 : 16;

        /// <summary>Размер мелкой подписи под сторону ячейки.</summary>
        /// <param name="tile">Сторона квадрата.</param>
        /// <returns>Размер шрифта.</returns>
        public static int SmallSize(float tile) => tile >= 320f ? 15 : 14;
    }
}
