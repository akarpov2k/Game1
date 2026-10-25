#nullable enable
namespace AnomalyPath.Core.Loot
{
    /// <summary>Полоса тира лута: сдвиг тира относительно тира локации и его вклад в шанс.</summary>
    public readonly struct TierBand
    {
        /// <summary>Создаёт полосу тира.</summary>
        /// <param name="offset">Сдвиг тира предмета относительно тира локации.</param>
        /// <param name="chance">Вклад полосы 0..1.</param>
        public TierBand(int offset, double chance)
        {
            Offset = offset;
            Chance = chance;
        }

        /// <summary>Сдвиг тира предмета относительно тира локации: −1, 0, +1.</summary>
        public int Offset { get; }

        /// <summary>Вклад полосы в шанс выпадения: 0..1.</summary>
        public double Chance { get; }
    }
}
