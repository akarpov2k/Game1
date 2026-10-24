#nullable enable
using UnityEngine;

namespace AnomalyPath.Runtime.Screens.Quests
{
    /// <summary>Цвета экрана заданий.</summary>
    public static class QuestPalette
    {
        /// <summary>Фон строки.</summary>
        public static readonly Color Row = new Color(0.10f, 0.13f, 0.12f, 0.92f);

        /// <summary>Фон строки готового задания.</summary>
        public static readonly Color ReadyRow = new Color(0.12f, 0.20f, 0.14f, 0.95f);

        /// <summary>Цвет ошибки.</summary>
        public static readonly Color Error = new Color(0.95f, 0.55f, 0.45f, 1f);
    }
}
