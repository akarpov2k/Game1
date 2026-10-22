#nullable enable
using AnomalyPath.Core.Player;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Save
{
    /// <summary>Состояние прохождения: мир и игрок (игрок может отсутствовать в старом сейве).</summary>
    public sealed class GameState
    {
        /// <summary>Создаёт состояние прохождения.</summary>
        /// <param name="world">Состояние мира.</param>
        /// <param name="player">Состояние игрока или <c>null</c> для world-only сейва.</param>
        public GameState(WorldState world, PlayerState? player)
        {
            World = world;
            Player = player;
        }

        /// <summary>Состояние мира.</summary>
        public WorldState World { get; }

        /// <summary>Состояние игрока или <c>null</c>.</summary>
        public PlayerState? Player { get; }
    }
}
