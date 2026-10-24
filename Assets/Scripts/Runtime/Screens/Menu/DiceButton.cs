#nullable enable
using System;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Menu
{
    /// <summary>
    /// Кнопка-кубик: показывает грань точками и напоминает, что имя выбирается случайно.
    /// Точки лежат в сетке 3×3 и включаются по узору грани.
    /// </summary>
    public sealed class DiceButton : MonoBehaviour
    {
        private static readonly int[][] Faces =
        {
            new[] { 4 },
            new[] { 0, 8 },
            new[] { 0, 4, 8 },
            new[] { 0, 2, 6, 8 },
            new[] { 0, 2, 4, 6, 8 },
            new[] { 0, 2, 3, 5, 6, 8 }
        };

        [Tooltip("Точки грани: девять ячеек 3×3 слева направо, сверху вниз")]
        [SerializeField]
        private Image[] pips = Array.Empty<Image>();

        [Tooltip("Показанная грань, от 1 до 6")]
        [SerializeField]
        private int face = 1;

        /// <summary>Показанная грань, от 1 до 6.</summary>
        public int Face => face;

        /// <summary>Показывает грань кубика: точки вне узора скрываются.</summary>
        /// <param name="value">Грань от 1 до 6; значения вне диапазона зажимаются.</param>
        public void Show(int value)
        {
            face = Mathf.Clamp(value, 1, 6);
            var visible = Faces[face - 1];
            for (var index = 0; index < pips.Length; index++)
            {
                if (pips[index] == null)
                {
                    continue;
                }

                pips[index].enabled = Array.IndexOf(visible, index) >= 0;
            }
        }

        private void Awake()
        {
            Show(face);
        }
    }
}
