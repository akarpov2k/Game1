#nullable enable
namespace AnomalyPath.Core.Localization
{
    /// <summary>Часть класса <see cref="CoreErrors"/>: ошибки квестов.</summary>
    public static partial class CoreErrors
    {
        /// <summary>Неизвестный вид квеста.</summary>
        public const string UnknownQuestKind = "error.unknown_quest_kind";

        /// <summary>Неизвестный тип цели квеста.</summary>
        public const string UnknownQuestType = "error.unknown_quest_type";

        /// <summary>Неизвестное состояние квеста в сохранении.</summary>
        public const string UnknownQuestStatus = "error.unknown_quest_status";

        /// <summary>Квест не найден.</summary>
        public const string QuestNotFound = "error.quest_not_found";

        /// <summary>Диапазон тиров цели квеста задан неверно.</summary>
        public const string QuestTierRange = "error.quest_tier_range";

        /// <summary>Пул целей квеста не дал ни одного кандидата.</summary>
        public const string QuestTargetPool = "error.quest_target_pool";

        /// <summary>Цель зачистки должна быть дикой точкой интереса.</summary>
        public const string QuestLocationNotSite = "error.quest_location_not_site";

        /// <summary>Цель квеста должна быть врагом с нужной ролью.</summary>
        public const string QuestActorRole = "error.quest_actor_role";

        /// <summary>Цель квеста должна быть предметом нужной категории.</summary>
        public const string QuestItemCategory = "error.quest_item_category";

        /// <summary>Предмет награды квеста не найден.</summary>
        public const string QuestRewardItemMissing = "error.quest_reward_item_missing";

        /// <summary>Талант не найден в дереве.</summary>
        public const string TalentNotFound = "error.talent_not_found";

        /// <summary>В дереве талантов нет ни одного таланта.</summary>
        public const string TalentTreeEmpty = "error.talent_tree_empty";

        /// <summary>Дерево талантов должно быть единственным.</summary>
        public const string TalentTreeMultiple = "error.talent_tree_multiple";

        /// <summary>Идентификатор таланта повторяется.</summary>
        public const string TalentNodeDuplicate = "error.talent_node_duplicate";

        /// <summary>Позиция таланта в дереве повторяется.</summary>
        public const string TalentPositionDuplicate = "error.talent_position_duplicate";

        /// <summary>Строка или колонка таланта отрицательна.</summary>
        public const string TalentPosition = "error.talent_position";

        /// <summary>Максимальный ранг таланта вне допустимого диапазона.</summary>
        public const string TalentMaxRank = "error.talent_max_rank";

        /// <summary>У таланта нет ни одного эффекта.</summary>
        public const string TalentNoEffects = "error.talent_no_effects";

        /// <summary>Одна и та же характеристика указана у таланта дважды.</summary>
        public const string TalentEffectDuplicate = "error.talent_effect_duplicate";

        /// <summary>Неизвестная характеристика таланта.</summary>
        public const string UnknownTalentStat = "error.unknown_talent_stat";

        /// <summary>Значение эффекта таланта не положительное или не целое.</summary>
        public const string TalentEffectValue = "error.talent_effect_value";

        /// <summary>Талант требует больше очков, чем можно вложить до него.</summary>
        public const string TalentGate = "error.talent_gate";

        /// <summary>Окно округления цены меньше единицы.</summary>
        public const string EconomyRounding = "error.economy_rounding";

        /// <summary>Минимальная цена отрицательна.</summary>
        public const string EconomyMinPrice = "error.economy_min_price";
    }
}
