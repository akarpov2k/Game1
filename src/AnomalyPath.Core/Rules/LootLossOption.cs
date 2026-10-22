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
        /// <summary>Создаёт вариант потери лута.</summary>
        /// <param name="id">Идентификатор варианта.</param>
        /// <param name="titleKey">Ключ названия для интерфейса.</param>
        /// <param name="loss">Вид потери.</param>
        /// <param name="share">Доля теряемого для <see cref="LootLossKind.Partial"/>, от 0 до 1.</param>
        /// <exception cref="ArgumentException">Идентификатор или ключ названия пуст.</exception>
        public LootLossOption(string id, string titleKey, LootLossKind loss, double share)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(id)),
                    nameof(id));
            }

            if (string.IsNullOrWhiteSpace(titleKey))
            {
                throw new ArgumentException(
                    CoreText.Format(CoreErrors.RequiredField, nameof(titleKey)),
                    nameof(titleKey));
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

        /// <summary>Вид потери.</summary>
        public LootLossKind Loss { get; }

        /// <summary>Доля теряемого для <see cref="LootLossKind.Partial"/>.</summary>
        public double Share { get; }

        /// <summary>Читает вариант из записи данных.</summary>
        /// <param name="value">Запись варианта.</param>
        /// <param name="path">Путь для сообщения об ошибке.</param>
        /// <returns>Вариант потери лута.</returns>
        /// <exception cref="DataException">Поля отсутствуют или значения вне допустимых границ.</exception>
        public static LootLossOption FromJson(JsonValue value, string path)
        {
            var id = DataMap.RequireString(value, "id", path);
            var titleKey = DataMap.RequireString(value, "titleKey", path);
            var lossCode = DataMap.RequireString(value, "loss", path);
            if (!SchemaEnum.TryParse(lossCode, out LootLossKind loss))
            {
                throw new DataException(path, CoreErrors.RulesUnknownLootLoss, lossCode);
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
