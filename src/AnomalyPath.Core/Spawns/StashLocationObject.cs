#nullable enable
namespace AnomalyPath.Core.Spawns
{
    /// <summary>Тайник: открывается отдельным пулом по тиру локации и джекпотом.</summary>
    public sealed class StashLocationObject : LocationObject
    {
        /// <summary>Служебный id системного тайника.</summary>
        public const string DefaultSpawnId = "spawn_stash";

        /// <summary>Ключ названия тайника в каталоге строк.</summary>
        public const string DefaultTitleKey = "ui.game_stash";

        /// <summary>Создаёт тайник.</summary>
        /// <param name="spawnId">Id записи спавна.</param>
        /// <param name="titleKey">Ключ названия в каталоге строк.</param>
        public StashLocationObject(string spawnId, string titleKey)
            : base(spawnId, titleKey, LocationObjectKind.Stash)
        {
        }

        /// <summary>Создаёт системный тайник с id и названием по умолчанию.</summary>
        /// <returns>Тайник для загрузки из сейва.</returns>
        public static StashLocationObject CreateDefault() => new StashLocationObject(DefaultSpawnId, DefaultTitleKey);
    }
}
