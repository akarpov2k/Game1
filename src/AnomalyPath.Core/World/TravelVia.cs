#nullable enable
namespace AnomalyPath.Core.World
{
    /// <summary>Способ перехода: код в данных — имя члена в нижнем регистре.</summary>
    public enum TravelVia
    {
        /// <summary>Дорога.</summary>
        Road,

        /// <summary>Тропа.</summary>
        Path,

        /// <summary>Ворота.</summary>
        Gate,

        /// <summary>Коридор.</summary>
        Corridor,

        /// <summary>Тоннель.</summary>
        Tunnel
    }
}
