#nullable enable
namespace AnomalyPath.Runtime.Ui.Items
{
    /// <summary>Откуда начали тащить предмет.</summary>
    public enum DragOrigin
    {
        /// <summary>Из сетки инвентаря.</summary>
        Inventory,

        /// <summary>Из слота снаряжения.</summary>
        Equipment,

        /// <summary>Из слота артефакта.</summary>
        Artifact,
    }
}
