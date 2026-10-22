#nullable enable
using UnityEngine;

namespace AnomalyPath.Runtime.Screens.Character
{
    /// <summary>Цвета экрана персонажа.</summary>
    public static class CharacterPalette
    {
        /// <summary>Фон фрейма.</summary>
        public static readonly Color Frame = new Color(0.09f, 0.11f, 0.12f, 1f);

        /// <summary>Фон слота снаряжения.</summary>
        public static readonly Color Slot = new Color(0.14f, 0.18f, 0.17f, 1f);

        /// <summary>Фон клетки инвентаря.</summary>
        public static readonly Color Cell = new Color(0.10f, 0.12f, 0.13f, 1f);

        /// <summary>Фон фигуры персонажа.</summary>
        public static readonly Color Figure = new Color(0.17f, 0.21f, 0.20f, 1f);

        /// <summary>Фон фигуры под ней.</summary>
        public static readonly Color FigureArea = new Color(0.08f, 0.10f, 0.10f, 1f);

        /// <summary>Цвет сообщения об ошибке.</summary>
        public static readonly Color Error = new Color(0.95f, 0.55f, 0.45f, 1f);

        /// <summary>Полоса здоровья.</summary>
        public static readonly Color Health = new Color(0.78f, 0.32f, 0.30f, 1f);

        /// <summary>Полоса выносливости.</summary>
        public static readonly Color Stamina = new Color(0.38f, 0.66f, 0.42f, 1f);

        /// <summary>Полоса питья.</summary>
        public static readonly Color Thirst = new Color(0.36f, 0.56f, 0.78f, 1f);

        /// <summary>Полоса еды.</summary>
        public static readonly Color Hunger = new Color(0.72f, 0.58f, 0.30f, 1f);

        /// <summary>Фон полосы.</summary>
        public static readonly Color Bar = new Color(0.16f, 0.18f, 0.18f, 1f);
    }
}
