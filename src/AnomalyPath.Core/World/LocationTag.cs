#nullable enable
namespace AnomalyPath.Core.World
{
    /// <summary>Тег окружения локации: код в данных — имя члена в нижнем регистре.</summary>
    public enum LocationTag
    {
        /// <summary>Открытое небо.</summary>
        Outdoor,

        /// <summary>Внутри помещения.</summary>
        Indoor,

        /// <summary>Дорога, трасса.</summary>
        Road,

        /// <summary>Застройка, город.</summary>
        Urban,

        /// <summary>Деревня, поля.</summary>
        Rural,

        /// <summary>Промзона, завод.</summary>
        Industrial,

        /// <summary>Подземелье, тоннель.</summary>
        Underground,

        /// <summary>Вода, болото.</summary>
        Water,

        /// <summary>Мирная зона.</summary>
        Hub,

        /// <summary>Аномальная зона.</summary>
        Anomaly,

        /// <summary>Жилое строение.</summary>
        House,

        /// <summary>Лес.</summary>
        Forest,

        /// <summary>Тропа.</summary>
        Path
    }
}
