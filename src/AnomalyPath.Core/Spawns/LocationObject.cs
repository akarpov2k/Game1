#nullable enable
using System;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Spawns
{
    /// <summary>
    /// Объект локации: общая часть для NPC, тайника и объекта лута.
    /// На локации не больше одного объекта, поэтому вид задаётся при создании.
    /// </summary>
    public abstract class LocationObject
    {
        /// <summary>Создаёт объект локации.</summary>
        /// <param name="spawnId">Id записи спавна.</param>
        /// <param name="titleKey">Ключ названия в каталоге строк.</param>
        /// <param name="kind">Вид объекта.</param>
        /// <exception cref="ArgumentException">Обязательная строка пуста.</exception>
        protected LocationObject(string spawnId, string titleKey, LocationObjectKind kind)
        {
            if (string.IsNullOrWhiteSpace(spawnId))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(spawnId)),
                    nameof(spawnId));
            }

            if (string.IsNullOrWhiteSpace(titleKey))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(titleKey)),
                    nameof(titleKey));
            }

            SpawnId = spawnId;
            TitleKey = titleKey;
            Kind = kind;
        }

        /// <summary>Id записи спавна, из которой создан объект.</summary>
        public string SpawnId { get; }

        /// <summary>Ключ названия в каталоге строк.</summary>
        public string TitleKey { get; }

        /// <summary>Вид объекта.</summary>
        public LocationObjectKind Kind { get; }

        /// <summary>Забран ли объект; повторно он не появляется.</summary>
        public bool Looted { get; private set; }

        /// <summary>Отмечает объект забранным.</summary>
        public void MarkLooted() => Looted = true;
    }
}
