#nullable enable
using System;
using AnomalyPath.Core.Player;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>
    /// Прогрессия персонажа: сколько опыта требует уровень, сколько опыта приносит убийство
    /// противника и как опыт меняет уровень. Уровни сверху не ограничены; повышение уровня
    /// выдаёт награды из данных и записывается в состояние игрока.
    /// </summary>
    public interface IProgressionService
    {
        /// <summary>Предел ранга противника из данных.</summary>
        int MaxRank { get; }

        /// <summary>Предел редкости противника из данных.</summary>
        int MaxRarity { get; }

        /// <summary>Сколько опыта нужно, чтобы перейти с уровня <paramref name="level"/> на следующий.</summary>
        /// <param name="level">Текущий уровень, больше нуля; верхней границы нет.</param>
        /// <returns>
        /// Требование уровня; если оно не помещается в <see cref="int"/>, возвращается <see cref="int.MaxValue"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Уровень меньше единицы.</exception>
        int RequiredXp(int level);

        /// <summary>Сколько всего опыта нужно, чтобы достичь уровня <paramref name="level"/>.</summary>
        /// <param name="level">Целевой уровень, больше нуля; 1 — ноль опыта.</param>
        /// <returns>
        /// Сумма требований переходов ниже целевого уровня; если она не помещается в <see cref="int"/>,
        /// возвращается <see cref="int.MaxValue"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Уровень меньше единицы.</exception>
        int TotalXpToLevel(int level);

        /// <summary>Сколько опыта даёт убийство противника.</summary>
        /// <param name="rank">Ранг противника, от 1 до <see cref="MaxRank"/>.</param>
        /// <param name="rarity">Редкость противника, от 1 до <see cref="MaxRarity"/>.</param>
        /// <returns>Опыт за убийство.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Ранг или редкость вне допустимого диапазона.</exception>
        int KillXp(int rank, int rarity);

        /// <summary>Сколько убийств таких противников нужно на переход с уровня.</summary>
        /// <param name="level">Текущий уровень, больше нуля.</param>
        /// <param name="rank">Ранг противника, от 1 до <see cref="MaxRank"/>.</param>
        /// <param name="rarity">Редкость противника, от 1 до <see cref="MaxRarity"/>.</param>
        /// <returns>Число убийств, округлённое вверх.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Хотя бы один аргумент вне допустимого диапазона.</exception>
        int KillsForLevel(int level, int rank, int rarity);

        /// <summary>Сколько убийств таких противников нужно, чтобы достичь уровня.</summary>
        /// <param name="level">Целевой уровень, больше нуля.</param>
        /// <param name="rank">Ранг противника, от 1 до <see cref="MaxRank"/>.</param>
        /// <param name="rarity">Редкость противника, от 1 до <see cref="MaxRarity"/>.</param>
        /// <returns>Число убийств, округлённое вверх.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Хотя бы один аргумент вне допустимого диапазона.</exception>
        int KillsToLevel(int level, int rank, int rarity);

        /// <summary>Какой уровень соответствует накопленному опыту.</summary>
        /// <param name="totalXp">Накопленный опыт, не меньше нуля.</param>
        /// <returns>Уровень, больше нуля; выше уровня, достижимого за объём опыта в <see cref="int"/>, не поднимается.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Опыт отрицателен.</exception>
        int LevelForXp(int totalXp);

        /// <summary>
        /// Подтягивает уровень игрока к накопленному опыту и выдаёт награды за пропущенные
        /// повышения. Нужен для старых сохранений, где опыт копился без повышения уровня.
        /// </summary>
        /// <param name="player">Состояние игрока.</param>
        /// <returns>Сколько уровней добавлено; 0, если уровень уже соответствует опыту.</returns>
        /// <exception cref="ArgumentNullException">Игрок не задан.</exception>
        int SyncLevel(PlayerState player);

        /// <summary>
        /// Начисляет опыт и повышает уровень столько раз, сколько позволяет кривая.
        /// Излишек опыта переносится на следующий уровень; за каждое повышение с уровня
        /// <c>levelRewardStartLevel</c> выдаются очки талантов и прибавка к максимуму здоровья.
        /// </summary>
        /// <param name="player">Состояние игрока.</param>
        /// <param name="amount">Сколько опыта добавить; не меньше нуля.</param>
        /// <returns>Сколько уровней добавлено.</returns>
        /// <exception cref="ArgumentNullException">Игрок не задан.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Количество опыта отрицательно.</exception>
        int AddXp(PlayerState player, int amount);

        /// <summary>
        /// Забирает долю опыта, накопленного на текущем уровне. Потеря не опускает прогресс
        /// ниже нуля и никогда не понижает уровень.
        /// </summary>
        /// <param name="player">Состояние игрока.</param>
        /// <param name="fraction">Доля опыта текущего уровня, 0..1.</param>
        /// <returns>Сколько опыта потеряно.</returns>
        /// <exception cref="ArgumentNullException">Игрок не задан.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Доля вне диапазона 0..1.</exception>
        int LoseXpOnDeath(PlayerState player, double fraction);
    }
}
