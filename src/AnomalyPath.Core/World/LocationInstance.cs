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
        /// <param name="subtype">Подтип локации.</param>
        /// <param name="tier">Тир опасности.</param>
        /// <param name="danger">Сложность от 0 до 1.</param>
        /// <param name="routeStep">Сколько шагов от хаба до локации; у хаба 0.</param>
        /// <param name="routeEdge">Номер звена маршрута; у хаба 0.</param>
        /// <exception cref="ArgumentException">Обязательные строки пусты.</exception>
        public LocationInstance(
            string instanceId,
            string templateId,
            int seed,
            string background,
            LocationSubtype subtype,
            int tier,
            double danger,
            int routeStep = 0,
            int routeEdge = 0)
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
            Subtype = subtype;
            Tier = tier;
            Danger = danger;
            RouteStep = routeStep;
            RouteEdge = routeEdge;
        }

        /// <summary>Уникальный id экземпляра.</summary>
        public string InstanceId { get; }

        /// <summary>Из какого шаблона создан.</summary>
        public string TemplateId { get; }

        /// <summary>Seed наполнения.</summary>
        public int Seed { get; }

        /// <summary>Выбранный фон.</summary>
        public string Background { get; }

        /// <summary>Подтип локации.</summary>
        public LocationSubtype Subtype { get; }

        /// <summary>Сколько шагов от хаба до локации; у хаба 0, у ветки — как у её входа на путь.</summary>
        public int RouteStep { get; }

        /// <summary>Номер звена маршрута; у хаба 0, у ветки — звену, от которого она отходит.</summary>
        public int RouteEdge { get; }

        /// <summary>Тир опасности.</summary>
        public int Tier { get; }

        /// <summary>Сложность от 0 до 1.</summary>
        public double Danger { get; }

        /// <summary>Был ли игрок в локации.</summary>
        public bool Visited { get; private set; }

        /// <summary>Зачищена ли локация.</summary>
        public bool Cleared { get; private set; }

        /// <summary>Покинул ли противник локацию после успешного побега.</summary>
        public bool EnemyGone { get; private set; }

        /// <summary>Проверялось ли появление тайника при входе.</summary>
        public bool StashRolled { get; private set; }

        /// <summary>Есть ли на локации невскрытый тайник.</summary>
        public bool StashPresent { get; private set; }

        /// <summary>Вскрыт ли тайник локации.</summary>
        public bool StashOpened { get; private set; }

        /// <summary>Объект локации: NPC, тайник или объект лута; <c>null</c> — объекта нет.</summary>
        public AnomalyPath.Core.Spawns.LocationObject? Object { get; private set; }

        /// <summary>Назначает объект локации.</summary>
        /// <param name="locationObject">Выбранный объект.</param>
        /// <exception cref="ArgumentNullException">Объект не задан.</exception>
        public void SetObject(AnomalyPath.Core.Spawns.LocationObject locationObject)
        {
            Object = locationObject ?? throw new ArgumentNullException(nameof(locationObject));
        }

        /// <summary>Отмечает объект локации забранным.</summary>
        public void MarkObjectLooted() => Object?.MarkLooted();

        /// <summary>Проверялся ли выбор объекта при входе; повторно объект не перебрасывается.</summary>
        public bool ObjectSelected { get; private set; }

        /// <summary>Отмечает, что объект локации выбран (в том числе пустой результат).</summary>
        public void MarkObjectSelected() => ObjectSelected = true;

        /// <summary>Отмечает, что игрок был здесь.</summary>
        public void MarkVisited() => Visited = true;

        /// <summary>Отмечает проверку появления тайника при входе.</summary>
        public void MarkStashRolled() => StashRolled = true;

        /// <summary>Отмечает, что на локации появился тайник.</summary>
        public void MarkStashPresent() => StashPresent = true;

        /// <summary>Отмечает, что тайник локации вскрыт.</summary>
        public void MarkStashOpened() => StashOpened = true;

        /// <summary>Отмечает локацию зачищенной.</summary>
        public void MarkCleared() => Cleared = true;

        /// <summary>Отмечает, что противник покинул локацию и больше здесь не появляется.</summary>
        public void MarkEnemyGone() => EnemyGone = true;
    }
}
