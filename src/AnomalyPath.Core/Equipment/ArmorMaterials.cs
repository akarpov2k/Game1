#nullable enable
namespace AnomalyPath.Core.Equipment
{
    /// <summary>Материалы брони и шлема: чем прочнее, тем меньше износ.</summary>
    public static class ArmorMaterials
    {
        /// <summary>Лёгкий материал, прочность 1.0.</summary>
        public const string Light = "light";

        /// <summary>Средний материал, прочность 1.2.</summary>
        public const string Medium = "medium";

        /// <summary>Тяжёлый материал, прочность 1.5.</summary>
        public const string Heavy = "heavy";

        /// <summary>Экзоскелет, прочность 1.7.</summary>
        public const string Exoskeleton = "exoskeleton";

        /// <summary>Проверяет, известен ли материал.</summary>
        /// <param name="material">Значение из файла предмета.</param>
        /// <returns><c>true</c>, если материал поддерживается.</returns>
        public static bool IsKnown(string material) =>
            material == Light || material == Medium || material == Heavy || material == Exoskeleton;
    }
}
