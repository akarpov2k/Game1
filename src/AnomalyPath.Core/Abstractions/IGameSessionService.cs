#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Death;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Loot;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Действия игровой сессии: сохранение, загрузка, переходы, бой и ходы.</summary>
    public interface IGameSessionService
    {
        /// <summary>Сохраняет состояние сессии в слот.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="slot">Имя слота.</param>
        /// <param name="playtime">Наигранное время в секундах.</param>
        /// <param name="createdAt">Дата создания; по умолчанию текущая.</param>
        /// <param name="title">Название сохранения, заданное игроком.</param>
        void Save(
            GameSession session,
            string slot,
            double playtime = 0,
            DateTimeOffset? createdAt = null,
            string? title = null);

        /// <summary>Загружает сессию из слота; бой не восстанавливается.</summary>
        /// <param name="slot">Имя слота.</param>
        /// <returns>Сессия с загруженным состоянием.</returns>
        GameSession Load(string slot);

        /// <summary>Создаёт новую игру.</summary>
        /// <param name="seed">Seed генерации мира.</param>
        /// <param name="loadoutId">Id выбранного набора снаряжения.</param>
        /// <param name="settings">Настройки сложности.</param>
        /// <returns>Новая сессия.</returns>
        GameSession StartNew(int seed, string loadoutId, NewGameSettings settings);

        /// <summary>
        /// Применяет последствия смерти к вещам игрока: инвентарь теряется полностью,
        /// кроме неутрачиваемых предметов, а каждый надетый предмет страхуется отдельным броском.
        /// </summary>
        /// <param name="session">Игровая сессия.</param>
        /// <returns>Что потеряно, что осталось и что спасла страховка.</returns>
        DeathResult ApplyDeath(GameSession session);

        /// <summary>Завершает бой и применяет его последствия: опыт и лут либо смерть и респавн.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <returns>Результат боя; повторный вызов на завершённом бое ничего не меняет.</returns>
        CombatOutcome ApplyCombatOutcome(GameSession session);

        /// <summary>Рёбра, видимые из текущей локации; в бою список пуст.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <returns>Видимые рёбра.</returns>
        IReadOnlyList<WorldEdge> AvailableEdges(GameSession session);

        /// <summary>Выполняет переход, если он разрешён и игрок не в бою.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="edgeId">Id ребра.</param>
        /// <returns>Результат перехода.</returns>
        TravelResult Travel(GameSession session, string edgeId);

        /// <summary>
        /// Покидает хаб: строит новый маршрут (каждый выход даёт другой) и сразу ведёт игрока
        /// в первую локацию пути первого звена. Доступно только из хаба и вне боя.
        /// </summary>
        /// <param name="session">Игровая сессия.</param>
        /// <returns>Результат перехода; <see cref="TravelStatus.WrongLocation"/>, если игрок не в хабе.</returns>
        TravelResult LeaveHub(GameSession session);

        /// <summary>
        /// Возвращает игрока в последний хаб. Доступно только с пути: цена —
        /// <c>route.hubReturn</c> × <c>routeStep</c> выносливости и часов, переход мгновенный.
        /// </summary>
        /// <param name="session">Игровая сессия.</param>
        /// <returns>Результат возврата.</returns>
        TravelResult ReturnToHub(GameSession session);

        /// <summary>
        /// Проверяет появление тайника в текущей локации. Хаб не проверяется, одна локация
        /// проверяется один раз; повторный вызов на уже проверенной локации ничего не делает.
        /// </summary>
        /// <param name="session">Игровая сессия.</param>
        /// <returns><c>true</c>, если на локации появился тайник.</returns>
        bool RollLocationStash(GameSession session);

        /// <summary>Вскрывает тайник текущей локации: пул по тиру либо пул джекпота.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <returns>Статус, лут и признак джекпота.</returns>
        StashOpenResult OpenStash(GameSession session);

        /// <summary>Начинает бой с противником.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="playerActor">Актёр игрока.</param>
        /// <param name="enemy">Противник.</param>
        /// <param name="distance">Начальная дистанция.</param>
        /// <param name="scenario">Сценарий боя; <c>null</c> — обычный бой, игрок ходит первым.</param>
        /// <returns><c>true</c>, если бой начат.</returns>
        bool StartCombat(
            GameSession session,
            ActorTemplate playerActor,
            ActorTemplate enemy,
            DistanceZone distance,
            CombatScenario? scenario = null);

        /// <summary>Выполняет атаку текущим участником.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="weapon">Оружие атакующего.</param>
        /// <param name="result">Результат атаки.</param>
        /// <returns><c>true</c>, если атака выполнена.</returns>
        bool TryAttack(GameSession session, ItemInstance weapon, out CombatAttackResult result);

        /// <summary>Перезаряжает оружие из инвентаря игрока.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="weapon">Оружие.</param>
        /// <returns><c>true</c>, если перезарядка выполнена.</returns>
        bool TryReload(GameSession session, ItemInstance weapon);

        /// <summary>Сдвигает дистанцию за ОД.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="deltaZones">На сколько зон сдвинуться.</param>
        /// <returns><c>true</c>, если сдвиг выполнен.</returns>
        bool TryMove(GameSession session, int deltaZones);

        /// <summary>Завершает ход и передаёт его противнику.</summary>
        /// <param name="session">Игровая сессия.</param>
        void EndTurn(GameSession session);

        /// <summary>
        /// Побег из боя: ход переходит противнику, он атакует, если может; при выживании
        /// игрока бой закрывается без победы, здоровье и износ сохраняются, противник
        /// исчезает с текущей локации. При смерти бой остаётся завершённым — вызывающий
        /// применяет обычный итог поражения.
        /// </summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="result">Исход побега, атака противника и здоровье игрока.</param>
        /// <returns><c>true</c>, если побег выполнен; <c>false</c>, если он невозможен.</returns>
        bool TryFlee(GameSession session, out FleeResult result);

        /// <summary>Завершает бой без победителя.</summary>
        /// <param name="session">Игровая сессия.</param>
        void EndCombat(GameSession session);
    }
}
