#nullable enable
using AnomalyPath.Core.Save;
using AnomalyPath.Core.Session;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Создание состояния новой игры по seed, набору снаряжения и настройкам сложности.</summary>
    public interface INewGameService
    {
        /// <summary>Создаёт мир и игрока новой игры.</summary>
        /// <param name="seed">Seed генерации мира.</param>
        /// <param name="loadoutId">Id выбранного набора снаряжения.</param>
        /// <param name="settings">Настройки сложности.</param>
        /// <returns>Начальное состояние прохождения.</returns>
        GameState Create(int seed, string loadoutId, NewGameSettings settings);
    }
}
