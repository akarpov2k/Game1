#nullable enable
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Templates;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;

namespace AnomalyPath.Runtime.Ui.Items
{
    /// <summary>Как предмет выглядит в интерфейсе: подпись и цвет плашки из данных.</summary>
    public static class ItemPresentation
    {
        /// <summary>Возвращает шаблон предмета или <c>null</c>, если его нет в данных.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="item">Экземпляр.</param>
        /// <returns>Шаблон или <c>null</c>.</returns>
        public static ItemTemplate? Template(IUiHost host, ItemInstance item) =>
            host.Items.TryGet(item.ItemId, out var template) ? template : null;

        /// <summary>Подпись предмета: название, количество, прочность.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="item">Экземпляр.</param>
        /// <returns>Текст для слота или клетки.</returns>
        public static string Describe(IUiHost host, ItemInstance item)
        {
            var name = Template(host, item) is { } template ? host.Texts.Get(template.NameKey) : item.ItemId;
            if (item.Stack > 1)
            {
                name += " ×" + item.Stack;
            }

            if (item.Durability.HasValue && item.DurabilityMax.HasValue && item.DurabilityMax.Value > 0)
            {
                name += $" · {item.Durability.Value}/{item.DurabilityMax.Value}";
            }

            return name;
        }

        /// <summary>Цвет предмета по категории.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="item">Экземпляр.</param>
        /// <returns>Цвет плашки.</returns>
        public static Color ColorFor(IUiHost host, ItemInstance item)
        {
            var template = Template(host, item);
            if (template == null)
            {
                return ItemPalette.Other;
            }

            return template.Category switch
            {
                "weapon" => ItemPalette.Weapon,
                "ammo" => ItemPalette.Ammo,
                "armor" => ItemPalette.Armor,
                "helmet" => ItemPalette.Armor,
                "backpack" => ItemPalette.Armor,
                "belt" => ItemPalette.Armor,
                "artifact" => ItemPalette.Artifact,
                _ => ItemPalette.Other,
            };
        }
    }
}
