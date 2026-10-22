#nullable enable
namespace AnomalyPath.Core.World
{
    /// <summary>Роль локации в связной цепочке: код в данных — имя члена в нижнем регистре.</summary>
    public enum LocationNodeType
    {
        /// <summary>Мирная зона, начало маршрутов.</summary>
        Hub,

        /// <summary>Отрезок пути между точками интереса.</summary>
        Route,

        /// <summary>Точка интереса в конце пути.</summary>
        Site,

        /// <summary>Внутренность точки интереса: комната, подвал, второй этаж.</summary>
        Interior
    }
}
