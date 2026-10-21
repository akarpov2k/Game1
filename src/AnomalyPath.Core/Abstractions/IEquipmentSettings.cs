#nullable enable
namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Настройки экипировки из данных.</summary>
    public interface IEquipmentSettings
    {
        /// <summary>Число слотов быстрого доступа.</summary>
        int QuickSlots { get; }
    }
}
