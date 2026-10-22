#nullable enable
using System;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.World
{
    /// <summary>Один сгенерированный экземпляр локации.</summary>
    public sealed class LocationInstance
    {
        /// <summary>Создаёт экземпляр локации.</summary>
        /// <param name="instanceId">Уникальный id экземпляра.</param>
        /// <param name="templateId">Из какого шаблона создан.</param>
        /// <param name="seed">Seed наполнения.</param>
        /// <param name="background">Выбранный фон.</param>
        /// <param name="nodeType">Роль в цепочке.</param>
        /// <param name="tier">Тир опасности.</param>
        /// <param name="danger">Сложность от 0 до 1.</param>
        /// <exception cref="ArgumentException">Обязательные строки пусты.</exception>
        public LocationInstance(
            string instanceId,
            string templateId,
            int seed,
            string background,
            LocationNodeType nodeType,
            int tier,
            double danger)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(instanceId)),
                    nameof(instanceId));
            }

            if (string.IsNullOrWhiteSpace(templateId))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(templateId)),
                    nameof(templateId));
            }

            InstanceId = instanceId;
            TemplateId = templateId;
            Seed = seed;
            Background = background;
            NodeType = nodeType;
            Tier = tier;
            Danger = danger;
        }

        /// <summary>Уникальный id экземпляра.</summary>
        public string InstanceId { get; }

        /// <summary>Из какого шаблона создан.</summary>
        public string TemplateId { get; }

        /// <summary>Seed наполнения.</summary>
        public int Seed { get; }

        /// <summary>Выбранный фон.</summary>
        public string Background { get; }

        /// <summary>Роль в цепочке.</summary>
        public LocationNodeType NodeType { get; }

        /// <summary>Тир опасности.</summary>
        public int Tier { get; }

        /// <summary>Сложность от 0 до 1.</summary>
        public double Danger { get; }

        /// <summary>Был ли игрок в локации.</summary>
        public bool Visited { get; private set; }

        /// <summary>Зачищена ли локация.</summary>
        public bool Cleared { get; private set; }

        /// <summary>Отмечает, что игрок был здесь.</summary>
        public void MarkVisited() => Visited = true;

        /// <summary>Отмечает локацию зачищенной.</summary>
        public void MarkCleared() => Cleared = true;
    }
}
