#nullable enable
namespace AnomalyPath.Core.World
{
    /// <summary>Подтип локации: код в данных — имя члена в нижнем регистре.</summary>
    public enum LocationSubtype
    {
        /// <summary>Мирная зона: начало маршрута.</summary>
        Hub,

        /// <summary>Путь: дорога, трасса, тропа. Единственный подтип с выходом в хаб.</summary>
        Path,

        /// <summary>Здание: жилое, промышленное, лаборатория.</summary>
        Building,

        /// <summary>Местность: лес, поле, болото.</summary>
        Terrain
    }
}
