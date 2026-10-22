#nullable enable
using AnomalyPath.Core.Data;

namespace AnomalyPath.Core.Equipment
{
    /// <summary>Кодек слотов экипировки: фиксированные слоты — это <see cref="EquipmentSlot"/>.</summary>
    public static class EquipmentSlots
    {
        /// <summary>Группа быстрого доступа, которой нет среди фиксированных слотов.</summary>
        public const string Quick = "quick";

        /// <summary>Преобразует код слота в перечисление.</summary>
        /// <param name="id">Строковый код.</param>
        /// <param name="slot">Слот.</param>
        /// <returns><c>true</c>, если код известен.</returns>
        public static bool TryParse(string id, out EquipmentSlot slot) => SchemaEnum.TryParse(id, out slot);

        /// <summary>Возвращает код слота для данных и сейвов.</summary>
        /// <param name="slot">Слот.</param>
        /// <returns>Строковый код.</returns>
        public static string Id(EquipmentSlot slot) => SchemaEnum.ToCode(slot);
    }
}
