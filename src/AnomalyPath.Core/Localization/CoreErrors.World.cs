#nullable enable
namespace AnomalyPath.Core.Localization
{
    /// <summary>Часть класса <see cref="CoreErrors"/>: ошибки мира.</summary>
    public static partial class CoreErrors
    {
        /// <summary>Неизвестная роль локации в цепочке.</summary>
        public const string WorldUnknownNodeType = "error.world_unknown_node_type";

        /// <summary>Неизвестный тип локации.</summary>
        public const string WorldUnknownLocationKind = "error.world_unknown_location_kind";

        /// <summary>Неизвестный тег локации.</summary>
        public const string WorldUnknownLocationTag = "error.world_unknown_location_tag";

        /// <summary>Неизвестная редкость локации.</summary>
        public const string WorldUnknownRarity = "error.world_unknown_location_rarity";

        /// <summary>Неизвестный способ перехода.</summary>
        public const string WorldUnknownVia = "error.world_unknown_via";

        /// <summary>Переход ведёт на несуществующую локацию.</summary>
        public const string WorldConnectionTarget = "error.world_connection_target";

        /// <summary>Переход запрещён топологией.</summary>
        public const string WorldTransitionForbidden = "error.world_transition_forbidden";

        /// <summary>Теги локаций не подходят для выбранного способа перехода.</summary>
        public const string WorldViaTags = "error.world_via_tags";

        /// <summary>Некорректный диапазон.</summary>
        public const string WorldRange = "error.world_range";

        /// <summary>Значение должно быть в диапазоне 0..1.</summary>
        public const string WorldChance = "error.world_chance";

        /// <summary>Нет стоимости для способа перехода.</summary>
        public const string WorldCost = "error.world_cost";

        /// <summary>В данных нет ни одного хаба.</summary>
        public const string WorldNoHub = "error.world_no_hub";

        /// <summary>В данных нет ни одной точки интереса.</summary>
        public const string WorldNoSite = "error.world_no_site";

        /// <summary>Для локации нельзя построить маршрут.</summary>
        public const string WorldRouteUnavailable = "error.world_route_unavailable";

        /// <summary>Переход не найден.</summary>
        public const string WorldEdgeNotFound = "error.world_edge_not_found";

        /// <summary>Переход начинается не из текущей локации.</summary>
        public const string WorldWrongLocation = "error.world_wrong_location";

        /// <summary>Часы мира вне диапазона 0..24.</summary>
        public const string WorldClockHours = "error.world_clock_hours";

        /// <summary>Локация не найдена в сгенерированном мире.</summary>
        public const string WorldInstanceNotFound = "error.world_instance_not_found";

        /// <summary>Маршрут длиннее предела ветвления.</summary>
        public const string WorldDepth = "error.world_depth";

        /// <summary>Уникальная локация должна иметь редкость unique.</summary>
        public const string WorldUniqueRarity = "error.world_unique_rarity";
    }
}
