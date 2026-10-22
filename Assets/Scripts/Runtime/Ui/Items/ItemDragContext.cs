#nullable enable
using UnityEngine;

namespace AnomalyPath.Runtime.Ui.Items
{
    /// <summary>Что тащат прямо сейчас: предмет, источник, габариты, поворот и позиция курсора.</summary>
    public sealed class ItemDragContext
    {
        private const float RoundingGuard = 0.0001f;

        /// <summary>Идёт ли перетаскивание.</summary>
        public bool Active { get; private set; }

        /// <summary>Id экземпляра предмета.</summary>
        public string InstanceId { get; private set; } = string.Empty;

        /// <summary>Откуда начали перетаскивание.</summary>
        public DragOrigin Origin { get; private set; }

        /// <summary>Ширина габарита в клетках без поворота.</summary>
        public int FootprintWidth { get; private set; } = 1;

        /// <summary>Высота габарита в клетках без поворота.</summary>
        public int FootprintHeight { get; private set; } = 1;

        /// <summary>Разрешён ли поворот предмета.</summary>
        public bool Rotatable { get; private set; }

        /// <summary>Повёрнут ли предмет сейчас.</summary>
        public bool Rotated { get; private set; }

        /// <summary>Позиция курсора в экранных координатах.</summary>
        public Vector2 Pointer { get; private set; }

        /// <summary>Доля точки захвата по ширине плашки, 0 — левый край, 1 — правый.</summary>
        public float GrabFractionX { get; private set; }

        /// <summary>Доля точки захвата по высоте плашки, 0 — верх, 1 — низ.</summary>
        public float GrabFractionY { get; private set; }

        /// <summary>Ширина занятого прямоугольника с учётом текущего поворота.</summary>
        public int EffectiveWidth => Rotated ? FootprintHeight : FootprintWidth;

        /// <summary>Высота занятого прямоугольника с учётом текущего поворота.</summary>
        public int EffectiveHeight => Rotated ? FootprintWidth : FootprintHeight;

        /// <summary>Клетка по горизонтали, за которую взялись при перетаскивании.</summary>
        public int GrabOffsetX => Offset(GrabFractionX, EffectiveWidth);

        /// <summary>Клетка по вертикали, за которую взялись при перетаскивании.</summary>
        public int GrabOffsetY => Offset(GrabFractionY, EffectiveHeight);

        /// <summary>Начинает перетаскивание и запоминает точку захвата.</summary>
        /// <param name="instanceId">Id экземпляра.</param>
        /// <param name="origin">Источник.</param>
        /// <param name="footprintWidth">Ширина габарита в клетках без поворота.</param>
        /// <param name="footprintHeight">Высота габарита в клетках без поворота.</param>
        /// <param name="rotatable">Разрешён ли поворот.</param>
        /// <param name="rotated">Начальное состояние поворота.</param>
        /// <param name="grabFractionX">Доля точки захвата по ширине плашки.</param>
        /// <param name="grabFractionY">Доля точки захвата по высоте плашки.</param>
        /// <param name="pointer">Позиция курсора.</param>
        public void Begin(
            string instanceId,
            DragOrigin origin,
            int footprintWidth,
            int footprintHeight,
            bool rotatable,
            bool rotated,
            float grabFractionX,
            float grabFractionY,
            Vector2 pointer)
        {
            InstanceId = instanceId;
            Origin = origin;
            FootprintWidth = Mathf.Max(1, footprintWidth);
            FootprintHeight = Mathf.Max(1, footprintHeight);
            Rotatable = rotatable;
            Rotated = rotatable && rotated;
            GrabFractionX = Mathf.Clamp01(grabFractionX);
            GrabFractionY = Mathf.Clamp01(grabFractionY);
            Pointer = pointer;
            Active = true;
        }

        /// <summary>Обновляет позицию курсора.</summary>
        /// <param name="pointer">Экранная позиция.</param>
        public void Move(Vector2 pointer)
        {
            if (!Active)
            {
                return;
            }

            Pointer = pointer;
        }

        /// <summary>Поворачивает предмет на 90°, если перетаскивание активно и поворот разрешён.</summary>
        /// <returns><c>true</c>, если состояние поворота изменилось.</returns>
        public bool ToggleRotation()
        {
            if (!Active || !Rotatable)
            {
                return false;
            }

            Rotated = !Rotated;
            return true;
        }

        /// <summary>Завершает перетаскивание.</summary>
        public void End()
        {
            InstanceId = string.Empty;
            FootprintWidth = 1;
            FootprintHeight = 1;
            Rotatable = false;
            Rotated = false;
            GrabFractionX = 0f;
            GrabFractionY = 0f;
            Pointer = Vector2.zero;
            Active = false;
        }

        private static int Offset(float fraction, int size) =>
            Mathf.Clamp(Mathf.FloorToInt((fraction * size) + RoundingGuard), 0, Mathf.Max(0, size - 1));
    }
}
