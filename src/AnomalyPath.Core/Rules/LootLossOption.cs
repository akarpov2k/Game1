#nullable enable
using System;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Rules
{
    /// <summary>Вариант потери лута: идентификатор, ключ названия, вид потери и доля.</summary>
    public sealed class LootLossOption
    {
        /// <summary>Лут не теряется.</summary>
        public const string LossNone = "none";

        /// <summary>Теряется только доля лута.</summary>
        public const string LossPartial = "partial";

        /// <summary>Теряется весь лут.</summary>
        public const string LossAll = "all";

        /// <summary>Создаёт вариант потери лута.</summary>
        /// <param name="id">Идентификатор варианта.</param>
        /// <param name="titleKey">Ключ названия для интерфейса.</param>
        /// <param name="loss">Вид потери: <c>none</c>, <c>partial</c> или <c>all</c>.</param>
        /// <param name="share">Доля теряемого для <c>partial</c>, от 0 до 1.</param>
        /// <exception cref="ArgumentException">Идентификатор или ключ названия пуст.</exception>
        public LootLossOption(string id, string titleKey, string loss, double share)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("id варианта потери лута не задан", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(titleKey))
            {
                throw new ArgumentException("titleKey варианта потери лута не задан", nameof(titleKey));
            }

            Id = id;
            TitleKey = titleKey;
            Loss = loss;
            Share = share;
        }

        /// <summary>Идентификатор варианта.</summary>
        public string Id { get; }

        /// <summary>Ключ названия для интерфейса.</summary>
        public string TitleKey { get; }

        /// <summary>Вид потери: <c>none</c>, <c>partial</c> или <c>all</c>.</summary>
        public string Loss { get; }

        /// <summary>Доля теряемого для <c>partial</c>.</summary>
        public double Share { get; }

        /// <summary>Проверяет, известен ли вид потери.</summary>
        /// <param name="loss">Значение поля <c>loss</c>.</param>
        /// <returns><c>true</c>, если вид поддерживается.</returns>
        public static bool IsKnownLoss(string loss) =>
            loss is LossNone or LossPartial or LossAll;

        /// <summary>Читает вариант из записи данных.</summary>
        /// <param name="value">Запись варианта.</param>
        /// <param name="path">Путь для сообщения об ошибке.</param>
        /// <returns>Вариант потери лута.</returns>
        /// <exception cref="DataException">Поля отсутствуют или значения вне допустимых границ.</exception>
        public static LootLossOption FromJson(JsonValue value, string path)
        {
            var id = DataMap.RequireString(value, "id", path);
            var titleKey = DataMap.RequireString(value, "titleKey", path);
            var loss = DataMap.RequireString(value, "loss", path);
            if (!IsKnownLoss(loss))
            {
                throw new DataException(path, CoreErrors.RulesUnknownLootLoss, loss);
            }

            var share = DataMap.RequireDouble(value, "share", path);
            if (share < 0 || share > 1)
            {
                throw new DataException(path, CoreErrors.RulesChance, "share");
            }

            return new LootLossOption(id, titleKey, loss, share);
        }
    }
}
