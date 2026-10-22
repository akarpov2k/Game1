#nullable enable
using System;
using AnomalyPath.Core.Save;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Menu
{
    /// <summary>Квадратная плитка сохранения: название, уровень, деньги и день мира; общая для загрузки и сохранения.</summary>
    internal sealed class SaveSlotTile
    {
        /// <summary>Цвет выбранной плитки.</summary>
        public static readonly Color SelectedColor =
            Color.Lerp(UiFactory.ButtonColor, UiFactory.TextColor, 0.16f);

        private SaveSlotTile(string slot, Image background, Text title, string baseTitle)
        {
            Slot = slot;
            Background = background;
            Title = title;
            BaseTitle = baseTitle;
        }

        /// <summary>Имя слота.</summary>
        public string Slot { get; }

        /// <summary>Фон плитки.</summary>
        public Image Background { get; }

        /// <summary>Заголовок плитки.</summary>
        public Text Title { get; }

        /// <summary>Заголовок без пометки выбора.</summary>
        public string BaseTitle { get; }

        /// <summary>Создаёт плитку с абсолютной позицией.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="parent">Родитель.</param>
        /// <param name="objectName">Имя объекта в иерархии.</param>
        /// <param name="slot">Имя слота.</param>
        /// <param name="info">Метаданные слота или <c>null</c> для пустой ячейки.</param>
        /// <param name="number">Номер ячейки или 0 для старого слота.</param>
        /// <param name="x">Отступ слева.</param>
        /// <param name="y">Отступ сверху.</param>
        /// <param name="tile">Сторона квадрата.</param>
        /// <param name="selected">Плитка выбрана.</param>
        /// <param name="markSelected">Помечать выбор знаком <c>&gt;</c>.</param>
        /// <param name="onClick">Действие по нажатию; <c>null</c> — плитка неинтерактивна.</param>
        /// <param name="isNew">Плитка создания нового сохранения: подпись «Новое сохранение».</param>
        /// <param name="onDelete">Удаление ячейки; <c>null</c> — кнопки удаления нет.</param>
        /// <returns>Созданная плитка.</returns>
        public static SaveSlotTile Create(
            IUiHost host,
            Transform parent,
            string objectName,
            string slot,
            SaveSlotInfo? info,
            int number,
            float x,
            float y,
            float tile,
            bool selected,
            bool markSelected,
            Action? onClick,
            bool isNew = false,
            Action? onDelete = null)
        {
            var background = UiWidgets.Box(
                host,
                parent,
                objectName,
                x,
                y,
                tile,
                tile,
                selected ? SelectedColor : UiFactory.ButtonColor);
            if (onClick != null)
            {
                var button = background.gameObject.AddComponent<Button>();
                button.targetGraphic = background;
                button.onClick.AddListener(() => onClick());
            }

            var emptyTitle = number > 0 ? UiText.Format(host.Texts, UiTextKey.SaveSlot_Cell, number) : slot;
            var baseTitle = isNew
                ? UiText.Get(host.Texts, UiTextKey.SaveSlot_New)
                : info != null && info.IsReadable
                    ? string.IsNullOrWhiteSpace(info.Title)
                        ? emptyTitle
                        : info.Title
                    : emptyTitle;

            var title = UiWidgets.Label(
                host,
                background.transform,
                "SlotTitle",
                (markSelected && selected ? UiText.Get(host.Texts, UiTextKey.Common_Selected) : string.Empty) + baseTitle,
                SaveSlotLayout.TitleSize(tile),
                TextAnchor.UpperCenter,
                UiFactory.TextColor,
                12f,
                16f,
                tile - 24f,
                Mathf.Max(30f, tile * 0.24f));
            title.horizontalOverflow = HorizontalWrapMode.Wrap;
            title.verticalOverflow = VerticalWrapMode.Truncate;

            if (info != null && info.IsReadable)
            {
                UiWidgets.Label(
                    host,
                    background.transform,
                    "SlotLevel",
                    UiText.Format(host.Texts, UiTextKey.Common_Level_Cap, info.Level) + " · " + SaveSlotText.Money(host.Texts, info.Money),
                    SaveSlotLayout.LineSize(tile),
                    TextAnchor.MiddleCenter,
                    UiFactory.TextColor,
                    8f,
                    tile * 0.44f,
                    tile - 16f,
                    30f);
                UiWidgets.Label(
                    host,
                    background.transform,
                    "SlotDay",
                    UiText.Format(host.Texts, UiTextKey.Common_Day_Hours_Tile, info.Day, info.Hours.ToString("0.0")),
                    SaveSlotLayout.SmallSize(tile),
                    TextAnchor.MiddleCenter,
                    UiFactory.MutedTextColor,
                    8f,
                    tile * 0.62f,
                    tile - 16f,
                    26f);
            }
            else
            {
                if (isNew)
                {
                    return new SaveSlotTile(slot, background, title, baseTitle);
                }

                UiWidgets.Label(
                    host,
                    background.transform,
                    "SlotNote",
                    info == null ? UiText.Get(host.Texts, UiTextKey.SaveSlot_Empty) : UiText.Get(host.Texts, UiTextKey.SaveSlot_Corrupted),
                    SaveSlotLayout.SmallSize(tile),
                    TextAnchor.MiddleCenter,
                    UiFactory.MutedTextColor,
                    8f,
                    tile * 0.56f,
                    tile - 16f,
                    26f);
            }

            if (onDelete != null)
            {
                AddDeleteButton(host, background.transform, slot, tile, onDelete);
            }

            return new SaveSlotTile(slot, background, title, baseTitle);
        }

        /// <summary>Рисует маленькую кнопку удаления в правом нижнем углу плитки.</summary>
        private static void AddDeleteButton(IUiHost host, Transform tileTransform, string slot, float tile, Action onDelete)
        {
            var size = Mathf.Clamp(tile * 0.16f, 28f, 44f);
            var delete = UiWidgets.Box(
                host,
                tileTransform,
                "DeleteSave_" + slot,
                tile - size - 8f,
                tile - size - 8f,
                size,
                size,
                new Color(0.42f, 0.20f, 0.18f, 1f));
            var button = delete.gameObject.AddComponent<Button>();
            button.targetGraphic = delete;
            button.onClick.AddListener(() => onDelete());
            UiWidgets.Label(
                host,
                delete.transform,
                "DeleteLabel",
                UiText.Get(host.Texts, UiTextKey.Common_Close),
                size >= 36f ? 22 : 18,
                TextAnchor.MiddleCenter,
                UiFactory.TextColor,
                0f,
                0f,
                size,
                size);
        }
    }
}
