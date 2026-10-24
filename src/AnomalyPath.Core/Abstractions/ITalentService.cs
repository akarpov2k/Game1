#nullable enable
using System.Collections.Generic;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Talents;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Прокачка: дерево талантов, очки, ранги и собранные бонусы.</summary>
    public interface ITalentService
    {
        /// <summary>Дерево талантов.</summary>
        TalentTreeTemplate Tree { get; }

        /// <summary>Сколько очков вложено в дерево.</summary>
        /// <param name="player">Состояние игрока.</param>
        /// <returns>Сумма рангов известных талантов.</returns>
        int SpentPoints(PlayerState player);

        /// <summary>Сколько свободных очков у игрока.</summary>
        /// <param name="player">Состояние игрока.</param>
        /// <returns>Свободные очки.</returns>
        int AvailablePoints(PlayerState player);

        /// <summary>Ранг таланта; неизвестный талант даёт 0.</summary>
        /// <param name="player">Состояние игрока.</param>
        /// <param name="talentId">Идентификатор таланта.</param>
        /// <returns>Ранг в допустимых границах.</returns>
        int RankOf(PlayerState player, string talentId);

        /// <summary>Проверяет, открыт ли талант по вложенным очкам.</summary>
        /// <param name="player">Состояние игрока.</param>
        /// <param name="node">Талант.</param>
        /// <returns><c>true</c>, если порог достигнут.</returns>
        bool IsUnlocked(PlayerState player, TalentNodeTemplate node);

        /// <summary>Изучает талант, списывая одно очко.</summary>
        /// <param name="player">Состояние игрока.</param>
        /// <param name="talentId">Идентификатор таланта.</param>
        /// <returns>Результат с новым рангом и остатком очков.</returns>
        TalentResult Learn(PlayerState player, string talentId);

        /// <summary>Собирает бонусы всех изученных талантов.</summary>
        /// <param name="player">Состояние игрока.</param>
        /// <returns>Суммарные бонусы.</returns>
        TalentModifiers GetModifiers(PlayerState player);

        /// <summary>Начисляет очки талантов: за уровни персонажа и награды.</summary>
        /// <param name="player">Состояние игрока.</param>
        /// <param name="amount">Сколько очков добавить; не меньше нуля.</param>
        void GrantPoints(PlayerState player, int amount);
    }
}
