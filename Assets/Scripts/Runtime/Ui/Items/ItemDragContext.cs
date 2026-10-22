#nullable enable
namespace AnomalyPath.Runtime.Ui.Items
{
    /// <summary>Что тащат прямо сейчас: id предмета и откуда начали.</summary>
    public sealed class ItemDragContext
    {
        /// <summary>Идёт ли перетаскивание.</summary>
        public bool Active { get; private set; }

        /// <summary>Id экземпляра предмета.</summary>
        public string InstanceId { get; private set; } = string.Empty;

        /// <summary>Откуда начали перетаскивание.</summary>
        public DragOrigin Origin { get; private set; }

        /// <summary>Начинает перетаскивание.</summary>
        /// <param name="instanceId">Id экземпляра.</param>
        /// <param name="origin">Источник.</param>
        public void Begin(string instanceId, DragOrigin origin)
        {
            InstanceId = instanceId;
            Origin = origin;
            Active = true;
        }

        /// <summary>Завершает перетаскивание.</summary>
        public void End()
        {
            InstanceId = string.Empty;
            Active = false;
        }
    }
}
