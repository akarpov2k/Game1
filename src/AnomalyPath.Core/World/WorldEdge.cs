#nullable enable
using System;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.World
{
    /// <summary>Ребро сгенерированного мира: переход между двумя экземплярами локаций.</summary>
    public sealed class WorldEdge
    {
        /// <summary>Создаёт ребро.</summary>
        /// <param name="edgeId">Уникальный id ребра.</param>
        /// <param name="fromId">Откуда ведёт переход.</param>
        /// <param name="toId">Куда ведёт переход.</param>
        /// <param name="via">Способ перехода.</param>
        /// <param name="length">Длина ребра в шагах.</param>
        /// <param name="cost">Стоимость перехода.</param>
        /// <param name="oneWay">Переход только в одну сторону.</param>
        /// <param name="hidden">Переход скрыт до открытия.</param>
        /// <exception cref="ArgumentException">Поля заданы неверно.</exception>
        public WorldEdge(
            string edgeId,
            string fromId,
            string toId,
            string via,
            int length,
            TravelCost cost,
            bool oneWay,
            bool hidden)
        {
            if (string.IsNullOrWhiteSpace(edgeId) || string.IsNullOrWhiteSpace(fromId) || string.IsNullOrWhiteSpace(toId))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(edgeId)),
                    nameof(edgeId));
            }

            if (!ViaRules.IsKnown(via))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.WorldUnknownVia, via),
                    nameof(via));
            }

            if (length < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            EdgeId = edgeId;
            FromId = fromId;
            ToId = toId;
            Via = via;
            Length = length;
            Cost = cost ?? throw new ArgumentNullException(nameof(cost));
            OneWay = oneWay;
            Hidden = hidden;
        }

        /// <summary>Уникальный id ребра.</summary>
        public string EdgeId { get; }

        /// <summary>Откуда ведёт переход.</summary>
        public string FromId { get; }

        /// <summary>Куда ведёт переход.</summary>
        public string ToId { get; }

        /// <summary>Способ перехода.</summary>
        public string Via { get; }

        /// <summary>Длина ребра в шагах.</summary>
        public int Length { get; }

        /// <summary>Стоимость перехода.</summary>
        public TravelCost Cost { get; }

        /// <summary>Переход только в одну сторону.</summary>
        public bool OneWay { get; }

        /// <summary>Переход скрыт до открытия.</summary>
        public bool Hidden { get; }
    }
}
