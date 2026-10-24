#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Death;
using AnomalyPath.Core.Inventory;
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

        /// <summary>Рёбра, видимые из текущей локации; в бою список пуст.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <returns>Видимые рёбра.</returns>
        IReadOnlyList<WorldEdge> AvailableEdges(GameSession session);

        /// <summary>Выполняет переход, если он разрешён и игрок не в бою.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="edgeId">Id ребра.</param>
        /// <returns>Результат перехода.</returns>
        TravelResult Travel(GameSession session, string edgeId);

        /// <summary>Начинает бой с противником.</summary>
        /// <param name="session">Игровая сессия.</param>
        /// <param name="playerActor">Актёр игрока.</param>
        /// <param name="enemy">Противник.</param>
        /// <param name="distance">Начальная дистанция.</param>
        /// <returns><c>true</c>, если бой начат.</returns>
        bool StartCombat(GameSession session, ActorTemplate playerActor, ActorTemplate enemy, DistanceZone distance);

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

        /// <summary>Завершает бой без победителя.</summary>
        /// <param name="session">Игровая сессия.</param>
        void EndCombat(GameSession session);
    }
}
