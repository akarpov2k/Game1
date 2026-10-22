#nullable enable
namespace AnomalyPath.Core.Save
{
    /// <summary>Краткая информация о слоте для меню загрузки.</summary>
    public sealed class SaveSlotInfo
    {
        /// <summary>Создаёт описание слота.</summary>
        /// <param name="slot">Имя слота.</param>
        /// <param name="createdAt">Дата создания или <c>null</c>.</param>
        /// <param name="playtime">Наигранное время.</param>
        /// <param name="seed">Seed мира.</param>
        /// <param name="currentLocationId">Текущая локация или <c>null</c>.</param>
        /// <param name="day">День.</param>
        /// <param name="hours">Часы.</param>
        /// <param name="isReadable">Удалось ли прочитать сохранение.</param>
        /// <param name="title">Название сохранения, заданное игроком.</param>
        /// <param name="level">Уровень персонажа.</param>
        /// <param name="money">Деньги на персонаже.</param>
        public SaveSlotInfo(
            string slot,
            string? createdAt,
            double playtime,
            int seed,
            string? currentLocationId,
            int day,
            double hours,
            bool isReadable,
            string? title = null,
            int level = 0,
            int money = 0)
        {
            Slot = slot;
            CreatedAt = createdAt;
            Playtime = playtime;
            Seed = seed;
            CurrentLocationId = currentLocationId;
            Day = day;
            Hours = hours;
            IsReadable = isReadable;
            Title = title ?? string.Empty;
            Level = level;
            Money = money;
        }

        /// <summary>Имя слота.</summary>
        public string Slot { get; }

        /// <summary>Дата создания или <c>null</c>.</summary>
        public string? CreatedAt { get; }

        /// <summary>Наигранное время в секундах.</summary>
        public double Playtime { get; }

        /// <summary>Seed мира.</summary>
        public int Seed { get; }

        /// <summary>Текущая локация или <c>null</c>.</summary>
        public string? CurrentLocationId { get; }

        /// <summary>День.</summary>
        public int Day { get; }

        /// <summary>Часы.</summary>
        public double Hours { get; }

        /// <summary>Удалось ли прочитать сохранение.</summary>
        public bool IsReadable { get; }

        /// <summary>Название сохранения, заданное игроком; пусто — без названия.</summary>
        public string Title { get; }

        /// <summary>Уровень персонажа в сохранении; 0 — игрока нет.</summary>
        public int Level { get; }

        /// <summary>Деньги на персонаже в сохранении; 0 — игрока нет.</summary>
        public int Money { get; }
    }
}
