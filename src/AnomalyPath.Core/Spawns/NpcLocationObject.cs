#nullable enable
using System;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Spawns
{
    /// <summary>NPC-объект локации: персонаж, с которым предстоит взаимодействие.</summary>
    public sealed class NpcLocationObject : LocationObject
    {
        /// <summary>Создаёт NPC-объект.</summary>
        /// <param name="spawnId">Id записи спавна.</param>
        /// <param name="titleKey">Ключ названия в каталоге строк.</param>
        /// <param name="actorId">Id актёра.</param>
        /// <exception cref="ArgumentException">Id актёра пуст.</exception>
        public NpcLocationObject(string spawnId, string titleKey, string actorId)
            : base(spawnId, titleKey, LocationObjectKind.Npc)
        {
            if (string.IsNullOrWhiteSpace(actorId))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(actorId)),
                    nameof(actorId));
            }

            ActorId = actorId;
        }

        /// <summary>Id актёра.</summary>
        public string ActorId { get; }
    }
}
