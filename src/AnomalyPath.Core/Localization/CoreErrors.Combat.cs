#nullable enable
namespace AnomalyPath.Core.Localization
{
    /// <summary>Часть класса <see cref="CoreErrors"/>: ошибки формул и боя.</summary>
    public static partial class CoreErrors
    {
        /// <summary>Пустое выражение формулы.</summary>
        public const string FormulaEmpty = "error.formula_empty";

        /// <summary>Синтаксическая ошибка в формуле.</summary>
        public const string FormulaSyntaxError = "error.formula_syntax";

        /// <summary>Формула использует неизвестное имя.</summary>
        public const string FormulaUnknownVariable = "error.formula_unknown_variable";

        /// <summary>Формула использует неизвестную функцию.</summary>
        public const string FormulaUnknownFunction = "error.formula_unknown_function";

        /// <summary>Формула должна задавать ровно одно представление.</summary>
        public const string FormulaRepresentation = "error.formula_representation";

        /// <summary>Неизвестный тип результата формулы.</summary>
        public const string FormulaResult = "error.formula_result";

        /// <summary>Ссылка на формулу не найдена.</summary>
        public const string FormulaNotFound = "error.formula_not_found";

        /// <summary>У актёра нет боевого блока, а он обязателен.</summary>
        public const string ActorWithoutCombat = "error.actor_without_combat";

        /// <summary>Неизвестный тип актёра.</summary>
        public const string UnknownActorType = "error.unknown_actor_type";

        /// <summary>Боевое действие невозможно: не тот ход, нет ОД или боя нет.</summary>
        public const string CombatActionUnavailable = "error.combat_unavailable";

        /// <summary>Атака невозможна: цель мертва или оружие сломано.</summary>
        public const string CombatAttackUnavailable = "error.combat_attack_unavailable";

        /// <summary>Неизвестный вид урона в поле kind.</summary>
        public const string UnknownDamageTypeKind = "error.unknown_damage_type_kind";

        /// <summary>Неизвестный режим восстановления ОД.</summary>
        public const string UnknownApRestore = "error.unknown_ap_restore";

        /// <summary>Блок атаки указан не у врага.</summary>
        public const string AttackOnNonEnemy = "error.attack_on_non_enemy";

        /// <summary>У врага нет блока атаки.</summary>
        public const string ActorWithoutAttack = "error.actor_without_attack";

        /// <summary>В атаке смешаны оружие и естественное оружие.</summary>
        public const string AttackMixedFields = "error.attack_mixed_fields";

        /// <summary>Неизвестная зона дистанции.</summary>
        public const string UnknownDistanceZone = "error.unknown_distance_zone";
    }
}
