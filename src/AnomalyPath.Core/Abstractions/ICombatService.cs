#nullable enable
using AnomalyPath.Core.Inventory;
using EquipmentState = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Бой: старт, атака, сдвиг по зонам и смена хода.</summary>
    public interface ICombatService
    {
        /// <summary>Начинает бой.</summary>
        /// <param name="playerActor">Актёр игрока.</param>
        /// <param name="playerEquipment">Экипировка игрока.</param>
        /// <param name="enemy">Противник.</param>
        /// <param name="distance">Начальная дистанция.</param>
        /// <param name="scenario">Сценарий боя; <c>null</c> — обычный бой, игрок ходит первым.</param>
        /// <param name="playerModifiers">Бонусы талантов игрока; <c>null</c> — без бонусов.</param>
        /// <returns>Состояние боя.</returns>
        AnomalyPath.Core.Combat.CombatState Start(
            AnomalyPath.Core.Combat.ActorTemplate playerActor,
            EquipmentState playerEquipment,
            AnomalyPath.Core.Combat.ActorTemplate enemy,
            AnomalyPath.Core.Combat.DistanceZone distance,
            AnomalyPath.Core.Combat.CombatScenario? scenario = null,
            AnomalyPath.Core.Talents.TalentModifiers? playerModifiers = null);

        /// <summary>Атакует текущим ходящим участником.</summary>
        /// <param name="state">Состояние боя.</param>
        /// <param name="weapon">Оружие атакующего.</param>
        /// <param name="result">Попадание, урон и смерть цели.</param>
        /// <returns><c>true</c>, если атака выполнена.</returns>
        bool TryAttack(AnomalyPath.Core.Combat.CombatState state, ItemInstance weapon, out AnomalyPath.Core.Combat.CombatAttackResult result);

        /// <summary>Противник атакует своими данными: оружием или естественной атакой.</summary>
        /// <param name="state">Состояние боя.</param>
        /// <param name="result">Попадание, урон и смерть цели.</param>
        /// <returns><c>true</c>, если атака выполнена; <c>false</c>, если она невозможна.</returns>
        bool TryEnemyAttack(AnomalyPath.Core.Combat.CombatState state, out AnomalyPath.Core.Combat.CombatAttackResult result);

        /// <summary>
        /// Побег: передаёт ход противнику, тот делает стандартную атаку, если может,
        /// и при выживании игрока закрывает бой без победы.
        /// </summary>
        /// <param name="state">Состояние боя.</param>
        /// <param name="result">Исход побега, атака противника и здоровье игрока.</param>
        /// <returns><c>true</c>, если побег выполнен; <c>false</c>, если он невозможен.</returns>
        bool TryFlee(AnomalyPath.Core.Combat.CombatState state, out AnomalyPath.Core.Combat.FleeResult result);

        /// <summary>Перезаряжает оружие из инвентаря, тратя ОД.</summary>
        /// <param name="state">Состояние боя.</param>
        /// <param name="weapon">Оружие.</param>
        /// <param name="inventory">Инвентарь с патронами.</param>
        /// <returns><c>true</c>, если перезарядка выполнена.</returns>
        bool TryReload(AnomalyPath.Core.Combat.CombatState state, ItemInstance weapon, GridContainer inventory);

        /// <summary>Считает шанс попадания текущего участника по противнику.</summary>
        /// <param name="state">Состояние боя.</param>
        /// <param name="weapon">Оружие атакующего.</param>
        /// <returns>Шанс попадания 0..1.</returns>
        double HitChance(AnomalyPath.Core.Combat.CombatState state, ItemInstance weapon);

        /// <summary>Сдвигает дистанцию за ОД.</summary>
        /// <param name="state">Состояние боя.</param>
        /// <param name="deltaZones">На сколько зон сдвинуться; знак не важен.</param>
        /// <returns><c>true</c>, если сдвиг выполнен.</returns>
        bool TryMove(AnomalyPath.Core.Combat.CombatState state, int deltaZones);

        /// <summary>Завершает ход и передаёт его противнику.</summary>
        /// <param name="state">Состояние боя.</param>
        void EndTurn(AnomalyPath.Core.Combat.CombatState state);
    }
}
