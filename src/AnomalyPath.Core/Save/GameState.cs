#nullable enable
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.World;

namespace AnomalyPath.Core.Save
{
    /// <summary>Состояние прохождения: мир, игрок и выбранные на старте настройки.</summary>
    public sealed class GameState
    {
        /// <summary>Создаёт состояние прохождения.</summary>
        /// <param name="world">Состояние мира.</param>
        /// <param name="player">Состояние игрока или <c>null</c> для world-only сейва.</param>
        /// <param name="settings">Настройки новой игры или <c>null</c> для старых сейвов.</param>
        public GameState(WorldState world, PlayerState? player, NewGameSettings? settings = null)
        {
            World = world;
            Player = player;
            Settings = settings;
        }

        /// <summary>Состояние мира.</summary>
        public WorldState World { get; }

        /// <summary>Состояние игрока или <c>null</c>.</summary>
        public PlayerState? Player { get; }

        /// <summary>Настройки новой игры или <c>null</c>.</summary>
        public NewGameSettings? Settings { get; }
    }
}
